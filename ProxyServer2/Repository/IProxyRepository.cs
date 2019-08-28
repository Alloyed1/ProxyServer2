using Dapper;
using Microsoft.Extensions.Configuration;
using ProxyServer2.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ProxyServer2.Repository
{
	public interface IProxyRepository
	{
		Task UpdateProxyList();
		Task SendNotify();
		Task UpdateCount();
	}
	public class ProxyRepository : IProxyRepository
	{
		private string connectionString;
		IEmailRepositorycs _emailRepositorycs;
		public ProxyRepository(IConfiguration configuration, IEmailRepositorycs emailRepositorycs)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
			_emailRepositorycs = emailRepositorycs;
		}
		public async Task UpdateCount()
		{
			using (var db = new SqlConnection(connectionString))
			{
				int counProxy = await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT (Id) FROM IPs");
				await db.ExecuteAsync("INSERT INTO CountProxyDays (Date, Count) VALUES(@Date, @Count) ", new { Date = DateTime.Now, Count = counProxy });
			}
		}
		public async Task SendNotify()
		{
			DateTime time = DateTime.Now;
			DateTime timeDayPlus = time.AddDays(1);

			using (var db = new SqlConnection(connectionString))
			{
				List<Subscribe> subscribes = (await db.QueryAsync<Subscribe>("SELECT * FROM Subscribes WHERE EndTime < @timeDayPlus AND IsAvaible = 'True'", new { timeDayPlus })).ToList();
				foreach (var sub in subscribes)
				{
					string userEmail = await db.QueryFirstOrDefaultAsync<string>("SELECT Email FROM AspNetUsers  WHERE Id = @userId", new { userId = sub.UserId });
					if (sub.EndTime < time)
					{
						await db.ExecuteAsync("UPDATE Subscribes SET IsAvaible = 'False' WHERE Id = @id", new { id = sub.Id });
						await _emailRepositorycs.SendEmailAsync(userEmail, "Окончание подписки", "Здравствуйте!\n\rК сожалению, ваша подписка на нашем сайте была окончена\n\rВы можете продлить её в любое время.");
					}
					else
					{
						await _emailRepositorycs.SendEmailAsync(userEmail, "Скороее окончание подписки", "Здравствуйте!\n\rК сожалению, ваша подписка на нашем сайте закончиться в течении суток\n\rВы можете продлить её в любое время.");
					}
				}
			}



		}
		public async Task UpdateProxyList()
		{
			string html;
			using (WebClient client = new WebClient())
			{
				html = await client.DownloadStringTaskAsync("http://proxybroker.ru/s5/underbunges-dh394-DK39f8-DKe3f9fk.txt");
			}

			List<string> ips = html.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();


			using (var db = new SqlConnection(connectionString))
			{
				await db.ExecuteAsync(@"UPDATE Cities SET CityIpCount = 0 WHERE Id != 1 AND Id != 19 AND Id != 13 AND Id != 22 AND Id != 29 AND Id != 34 AND Id != 40 AND Id != 45 AND Id != 47 AND Id != 48 AND Id != 65;
				UPDATE Cities SET CityIpCount = 5 WHERE Id = 1;
				UPDATE Cities SET CityIpCount = 5 WHERE Id = 13;
				UPDATE Cities SET CityIpCount = 150 WHERE Id = 19;
				UPDATE Cities SET CityIpCount = 33 WHERE Id = 22;
				UPDATE Cities SET CityIpCount = 25 WHERE Id = 29;
				UPDATE Cities SET CityIpCount = 200 WHERE Id = 34;
				UPDATE Cities SET CityIpCount = 90 WHERE Id = 40;
				UPDATE Cities SET CityIpCount = 15 WHERE Id = 45;
				UPDATE Cities SET CityIpCount = 240 WHERE Id = 47;
				UPDATE Cities SET CityIpCount = 470 WHERE Id = 48;
				UPDATE Cities SET CityIpCount = 200 WHERE Id = 65;");

				int countCity = await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT (CityIpCount) FROM Cities");
				int sumCity = await db.QueryFirstOrDefaultAsync<int>("SELECT SUM (CityIpCount) FROM Cities");

				Dictionary<int, int> citiesDictionary = new Dictionary<int, int>();
				List<City> cities = (await db.QueryAsync<City>("SELECT * FROM Cities")).ToList();
				foreach (var cit in cities)
				{
					citiesDictionary.Add(cit.Id, cit.CityIpCount);
				}

				for (int j = 0; j < ips.Count() - sumCity; j++)
				{
					int random = new Random().Next(1, countCity) + 1;
					citiesDictionary[random] = citiesDictionary[random] + 1;
				}
				foreach (var cit in citiesDictionary)
				{
					await db.ExecuteAsync("UPDATE Cities SET CityIpCount = @citcount WHERE Id = @id", new { citcount = cit.Value, id = cit.Key });
				}
				int count = ips.Count();
				await db.ExecuteAsync("DELETE FROM IPs");
				for (int i = 0; i <= count / 1000; i++)
				{
					string quyery = "INSERT INTO IPs (Ip) VALUES";
					if (ips.Count() >= 1000)
					{
						foreach (string ip in ips.Take(1000))
						{
							if (ip == ips.Take(1000).ToList().TakeLast(1).FirstOrDefault())
							{
								quyery = quyery + $"('{ip}') ";
							}
							else
							{
								quyery = quyery + $"('{ip}'), ";
							}

						}
						ips.RemoveRange(0, 1000);
					}
					else
					{
						foreach (string ip in ips.Take(ips.Count()))
						{
							if (ip == ips.Take(ips.Count()).TakeLast(1).First())
							{
								quyery = quyery + $"('{ip}') ";
							}
							else
							{
								quyery = quyery + $"('{ip}'), ";
							}

						}
						ips.RemoveRange(0, ips.Count());
					}

					await db.ExecuteAsync(quyery);
				}


			}
		}
	}
}
