using ProxyServer2.Models;
using ProxyServer2.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace ProxyServer2.Repository
{
	public interface IAdminRepository
	{
		Task<AdminViewModel> GetAdminViewModel();
		Task<int> GetPrice();
		Task<bool> AddBitcoinPayment(string payment);
		Task SetPrice(int price);
		Task<List<Preimush>> GetPreimush();
		Task UpdatePreimush(int index, string name, string body);
		Task<List<Pravila>> GetPravila();
		Task SetPravila(List<string> pravilas);
		Task<string> GetUrl();
		Task SetUrl(string url);
		Task SetPolitica(List<string> pravilas);
	}
	public class AdminRepository : IAdminRepository
	{
		private string connectionString;
		public AdminRepository(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}
		public async Task UpdatePreimush(int index, string name, string body)
		{
			using (var db = new SqlConnection(connectionString))
			{
				await db.QueryFirstOrDefaultAsync<int>("UPDATE Preimush SET Name = @name, Body = @body WHERE Id = @index", new { name, body, index });
			}
		}
		public async Task<string> GetUrl()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return await db.QueryFirstOrDefaultAsync<string>("SELECT TOP(1) Url FROM AppSettings");
			}
		}
		public async Task SetUrl(string url)
		{
			using (var db = new SqlConnection(connectionString))
			{
				await db.ExecuteAsync("UPDATE AppSettings SET Url = @url", new { url });
			}
		}
		public async Task<int> GetPrice()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return await db.QueryFirstOrDefaultAsync<int>("SELECT TOP(1) SubPrice FROM AppSettings");
			}
		}
		public async Task<List<Preimush>> GetPreimush()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<Preimush>("SELECT * FROM Preimush")).ToList();
			}
		}
		public async Task<List<Pravila>> GetPravila()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<Pravila>("SELECT * FROM Pravila")).ToList();
			}
		}
		public async Task SetPravila(List<string> pravilas)
		{
			using (var db = new SqlConnection(connectionString))
			{
				await db.ExecuteAsync("DELETE FROM Pravila");
				foreach (string item in pravilas)
				{
					await db.ExecuteAsync("INSERT INTO Pravila (Body) VALUES(@item)", new { item });
				}

			}
		}
		public async Task SetPrice(int price)
		{
			using (var db = new SqlConnection(connectionString))
			{
				await db.ExecuteAsync("UPDATE AppSettings SET SubPrice = @price", new { price });
			}
		}
		public async Task<AdminViewModel> GetAdminViewModel()
		{
			AdminViewModel adminViewModel = new AdminViewModel();
			using (var db = new SqlConnection(connectionString))
			{
				adminViewModel.bitcoinPaymentsList = (await db.QueryAsync<BitcoinPayment>("SELECT * FROM BitcoinPayments")).ToList();
				adminViewModel.AvaibleBitcountPayment = adminViewModel.bitcoinPaymentsList.Where(s => s.UserId == null).Count();
			}
			return adminViewModel;
		}
		public async Task SetPolitica(List<string> pravilas)
		{
			using (var db = new SqlConnection(connectionString))
			{
				await db.ExecuteAsync("DELETE FROM Politica");
				foreach (string item in pravilas)
				{
					await db.ExecuteAsync("INSERT INTO Politica (Body) VALUES(@item)", new { item });
				}

			}
		}
		public async Task<bool> AddBitcoinPayment(string payment)
		{
			using (var db = new SqlConnection(connectionString))
			{
				BitcoinPayment bitcoinPayment = await db.QueryFirstOrDefaultAsync<BitcoinPayment>("SELECT Id FROM BitcoinPayments WHERE Payment = @payment", new { payment });
				if (bitcoinPayment != null)
				{
					return false;
				}
				else
				{
					await db.ExecuteAsync("INSERT INTO BitcoinPayments (Payment) VALUES(@payment)", new { payment });
					return true;
				}
			}
		}
	}
}
