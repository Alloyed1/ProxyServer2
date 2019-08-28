using Dapper;
using Microsoft.Extensions.Configuration;
using ProxyServer2.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer2.Repository
{
	public interface IHomeRepository
	{
		Task<int> GetPrice();
		Task<int> GetCountProxy();
		Task<List<City>> GetCities();
		Task<List<Pravila>> GetPravila();
		Task<List<Politica>> GetPolitica();
		Task<List<Preimush>> GetPreimush();
		Task<List<int>> GetStatsCount();
	}
	public class HomeRepository : IHomeRepository
	{
		private string connectionString;
		public HomeRepository(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}
		public async Task<List<Preimush>> GetPreimush()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<Preimush>("SELECT * FROM Preimush")).ToList();
			}
		}
		public async Task<List<int>> GetStatsCount()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<int>("SELECT TOP (7) Count  FROM CountProxyDays ORDER BY Date DESC")).ToList();
			}
		}
		public async Task<List<Pravila>> GetPravila()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<Pravila>("SELECT * FROM Pravila")).ToList();
			}
		}
		public async Task<List<Politica>> GetPolitica()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<Politica>("SELECT * FROM Politica")).ToList();
			}
		}
		public async Task<int> GetPrice()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return await db.QueryFirstOrDefaultAsync<int>("SELECT TOP(1) SubPrice FROM AppSettings");
			}
		}
		public async Task<int> GetCountProxy()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT(Id) FROM IPs");
			}

		}
		public async Task<List<City>> GetCities()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<City>("SELECT * FROM Cities")).ToList();
			}
		}
	}
}
