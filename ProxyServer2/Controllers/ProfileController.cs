using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProxyServer2.Models;
using ProxyServer2.Repository;
using ProxyServer2.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProxyServer2.Controllers
{
	public class ProfileController : Controller
	{
		ApplicationContext _context;
		IProfileRepository _profileRepository;
		IEmailRepositorycs _emailRepositorycs;
		public ProfileController(ApplicationContext context, IProfileRepository profileRepository, IEmailRepositorycs emailRepositorycs)
		{
			_context = context;
			_profileRepository = profileRepository;
			_emailRepositorycs = emailRepositorycs;
		}
		[Authorize]
		[HttpGet]
		public IActionResult Index()
		{
			return View();
		}
		[Authorize]
		[HttpGet]
		public IActionResult ResetKey()
		{
			return View();
		}
		[Authorize]
		[HttpGet]
		public IActionResult Subscribes()
		{
			return View();
		}
		[Authorize]
		[HttpGet]
		public async Task<List<OrderViewModel>> CheckPay()
		{
			return await _profileRepository.CheckPay(User.Identity.Name);
		}
		[Authorize]
		[HttpGet]
		public async Task<List<int>> GetStatsCount()
		{
			return await _profileRepository.GetStatsCount();
		}
		[Authorize]
		[HttpGet]
		public IActionResult About()
		{
			return View();
		}

		[Authorize]
		[HttpGet]
		public string GetKey(string email)
		{
			return _context.Users.Where(w => w.Email == email).Select(s => s.Bitcoin).FirstOrDefault();
		}
		[HttpGet]
		public async Task<IActionResult> GetSub(string key)
		{
			return View(await _profileRepository.GetSub(key));
		}
		[Authorize]
		[HttpGet]
		public async Task<int> GetCountProxy()
		{
			return await _profileRepository.GetCountProxy();
		}
		[Authorize]
		[HttpGet]
		public async Task<List<Subscribe>> GetSubscribes()
		{
			return await _profileRepository.GetSubscribes(User.Identity.Name);
		}
		[Authorize]
		[HttpGet]
		public async Task<List<City>> GetCities()
		{
			return await _profileRepository.GetCities();
		}
	}
}
