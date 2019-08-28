using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProxyServer2.Models;
using ProxyServer2.Repository;
using ProxyServer2.ViewModels;
using reCAPTCHA.AspNetCore;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProxyServer2.Controllers
{
	public class AccountController : Controller
	{
		private SignInManager<User> signInManager;
		//private IUserRepository userRepository;
		private UserManager<User> userManager;
		private RoleManager<IdentityRole> roleManager;
		private IRecaptchaService _recaptcha;
		ApplicationContext context;
		IProfileRepository _profileRepository;
		public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IRecaptchaService recaptcha, ApplicationContext context, IProfileRepository profileRepository)
		{
			this.signInManager = signInManager;
			this.userManager = userManager;
			this.roleManager = roleManager;
			_recaptcha = recaptcha;
			this.context = context;
			_profileRepository = profileRepository;
		}
		[HttpGet]
		public IActionResult Register()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var recaptcha = await _recaptcha.Validate(Request);
				if (recaptcha.success)
				{
					var bitcoin = context.BitcoinPayments
						.Where(w => w.UserId == null)
						.FirstOrDefault();

					if (bitcoin == null)
					{
						ModelState.AddModelError("", "На данный момент доступных адерсов для регистрации нет");
					}
					else
					{
						User user = new User { Email = model.Email, UserName = model.Email, FirstName = model.FirstName, Bitcoin = bitcoin.Payment };

						// добавляем пользователя
						var result = await userManager.CreateAsync(user, "defaultpassword");
						if (result.Succeeded)
						{
							bitcoin.UserId = user.Id;
							await context.SaveChangesAsync();
							await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
							if (user.Email == "jenya.moxov@gmail.com")
								await userManager.AddToRoleAsync(user, "Admin");

							// установка куки
							await signInManager.SignInAsync(user, model.RememberMe);
							return RedirectToAction("GetInfo", "Account");
						}
						else
						{
							foreach (var error in result.Errors)
							{
								ModelState.AddModelError(string.Empty, error.Description);
							}
						}
					}


				}
				else
				{
					ModelState.AddModelError("", "Ошибка при вводе каптчи");
				}

			}
			return View(model);
		}
		[HttpGet]
		public IActionResult Auth()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Auth(AuthViewModel model)
		{
			User user = context.Users.Where(w => w.Bitcoin == model.Bitcoin).FirstOrDefault();
			if (user != null && ModelState.IsValid)
			{
				var result =
					await signInManager.PasswordSignInAsync(user.Email, "defaultpassword", model.RememberMe, false);
				return RedirectToAction("Index", "Home");
			}
			else
			{
				ModelState.AddModelError("", "Ключ неверен");
			}
			return View(model);
		}
		public IActionResult GetInfo()
		{
			ViewBag.Bitcoin = context.Users.Where(w => w.Email == User.Identity.Name).Select(s => s.Bitcoin).FirstOrDefault();
			return View();
		}
		[HttpGet]
		public async Task<string> CheckKey(string email, string userEmail)
		{
			if (email != userEmail)
			{
				return null;
			}
			else
			{
				return await _profileRepository.CheckKey(email, userEmail);
			}
		}
		public IActionResult Index()
		{
			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> LogOff()
		{
			// удаляем аутентификационные куки
			await signInManager.SignOutAsync();
			return RedirectToAction("Index", "Home");
		}
	}
}
