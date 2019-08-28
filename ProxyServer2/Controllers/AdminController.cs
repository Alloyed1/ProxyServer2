using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using ProxyServer2.Models;
using ProxyServer2.Repository;
using ProxyServer2.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ProxyServer2.Controllers
{
	public class AdminController : Controller
	{
		private IAdminRepository _adminRepository;
		private IProxyRepository _proxyRepository;
		//private UserManager<User> _userManager;
		public AdminController(IAdminRepository adminRepository, IProxyRepository proxyRepository)
		{
			_adminRepository = adminRepository;
			_proxyRepository = proxyRepository;
		}
		[HttpGet]
		[Obsolete]
		public IActionResult Index()
		{
			RecurringJob.AddOrUpdate(
				() => _proxyRepository.UpdateProxyList(),
				Cron.MinuteInterval(10));
			RecurringJob.AddOrUpdate(
				() => _proxyRepository.SendNotify(),
				Cron.Daily);
			RecurringJob.AddOrUpdate(
				() => _proxyRepository.UpdateCount(),
				Cron.Daily);
			return View();
		}
		[HttpGet]
		public async Task<AdminViewModel> GetAdminViewModel()
		{
			return await _adminRepository.GetAdminViewModel();
		}
		[HttpGet]
		public async Task<int> GetPrice()
		{
			return await _adminRepository.GetPrice();
		}
		[HttpGet]
		public async Task<List<Preimush>> GetPreimush()
		{
			return await _adminRepository.GetPreimush();
		}
		[HttpPut]
		public async Task<bool> AddBitcoinPayment(string payment)
		{
			return await _adminRepository.AddBitcoinPayment(payment);
		}
		[HttpPut]
		public async Task UpdatePreimush(int index, string name, string body)
		{
			await _adminRepository.UpdatePreimush(index, name, body);
		}
		[HttpPut]
		public async Task SetPravila(List<string> pravila)
		{
			await _adminRepository.SetPravila(pravila);
		}
		[HttpPut]
		public async Task SetPolitica(List<string> politica)
		{
			await _adminRepository.SetPolitica(politica);
		}
		[HttpGet]
		public async Task<string> GetUrl()
		{
			return await _adminRepository.GetUrl();
		}
		[HttpPut]
		public async Task SetUrl(string url)
		{
			await _adminRepository.SetUrl(url);
		}

		[HttpPut]
		public async Task SetPrice(int price)
		{
			await _adminRepository.SetPrice(price);
		}
	}
}
