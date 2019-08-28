using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProxyServer2.Models;
using ProxyServer2.Repository;

namespace ProxyServer2.Controllers
{
	public class HomeController : Controller
	{
		IHomeRepository _homeRepository;
		public HomeController(IHomeRepository homeRepository)
		{
			_homeRepository = homeRepository;
		}
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}
		public IActionResult Politica()
		{
			return View();
		}
		[HttpGet]
		public async Task<int> GetPrice()
		{
			return await _homeRepository.GetPrice();
		}
		[HttpGet]
		public async Task<int> GetCountProxy()
		{
			return await _homeRepository.GetCountProxy();
		}
		[HttpGet]
		public async Task<List<int>> GetStatsCount()
		{
			return await _homeRepository.GetStatsCount();
		}
		[HttpGet]
		public async Task<List<Preimush>> GetPreimush()
		{
			return await _homeRepository.GetPreimush();
		}
		[HttpGet]
		public async Task<List<Pravila>> GetPravila()
		{
			return await _homeRepository.GetPravila();
		}
		[HttpGet]
		public async Task<List<Politica>> GetPolitica()
		{
			return await _homeRepository.GetPolitica();
		}
		[HttpGet]
		public async Task<List<City>> GetCities()
		{
			return await _homeRepository.GetCities();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
