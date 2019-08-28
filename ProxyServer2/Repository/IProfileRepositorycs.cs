using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Dapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using ProxyServer2.Models;
using ProxyServer2.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ProxyServer2.Repository
{
	public interface IProfileRepository
	{
		Task<string> CheckKey(string email, string userEmail);
		Task<List<OrderViewModel>> CheckPay(string email);
		Task<List<Subscribe>> GetSubscribes(string email);
		Task<List<string>> GetSub(string key);
		Task<int> GetCountProxy();
		Task<List<City>> GetCities();
		Task<List<int>> GetStatsCount();
	}
	public class ProfileRepository : IProfileRepository
	{
		private string connectionString;
		IEmailRepositorycs _emailRepositorycs;
		public ProfileRepository(IConfiguration configuration, IEmailRepositorycs emailRepositorycs)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
			_emailRepositorycs = emailRepositorycs;
		}
		public async Task<List<int>> GetStatsCount()
		{
			using(var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<int>("SELECT TOP (7) Count  FROM CountProxyDays ORDER BY Date DESC")).ToList();
			}
		}
		public async Task<List<City>> GetCities()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return (await db.QueryAsync<City>("SELECT * FROM Cities")).ToList();
			}
		}
		public async Task<string> CheckKey(string email, string userEmail)
		{
			using(var db = new SqlConnection(connectionString))
			{
				string query = "SELECT Bitcoin FROM AspNetUsers WHERE Email = @userEmail";
				return await db.QueryFirstOrDefaultAsync<string>(query, new { userEmail });
			}
		}
		public async Task<int> GetCountProxy()
		{
			using (var db = new SqlConnection(connectionString))
			{
				return await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT(Id) FROM IPs");
			}

		}
		public async Task<List<string>> GetSub(string key)
		{
			using (var db = new SqlConnection(connectionString))
			{
				string userId = await db.QueryFirstOrDefaultAsync<string>("SELECT Id FROM AspNetUsers WHERE Bitcoin = @key", new { key });
				List<IP> iP = (await db.QueryAsync<IP>("SELECT * FROM IPs WHERE ReservedBy = @userId", new { userId })).ToList();
				if(iP.Count() == 0)
				{
					int count = await db.QueryFirstOrDefaultAsync<int>("SELECT CountProxy FROM Subscribes WHERE UserId = @userId AND IsAvaible = 'True'", new { userId });
					iP = (await db.QueryAsync<IP>("SELECT TOP "+ count +" IP FROM IPs WHERE ReservedBy IS NULL ORDER BY NEWID()")).ToList();

					int countIP = iP.Count();
					List<IP> dfg = iP.Take(1000).ToList();
					for (int i = 0; i <= countIP / 1000; i++)
					{
						string quyery = "UPDATE IPs SET ReservedBy = @userId WHERE  ";
						foreach (string ip in iP.Select(s => s.Ip).Take(1000))
						{
							if (ip == iP.Take(1000).Select(s => s.Ip).TakeLast(1).FirstOrDefault())
							{
								quyery = quyery + $" Ip = '{ip}' ";
							}
							else
							{
								quyery = quyery + $" Ip = '{ip}' OR ";
							}

						}
						if (iP.Count() > 1000)
						{
							iP.RemoveRange(0, 1000);
						}
						else
						{
							iP.RemoveRange(0, iP.Count());
						}
						
						await db.ExecuteAsync(quyery, new { userId });
					}
				}
				if (iP.Count() == 0)
				{
					return (await db.QueryAsync<IP>("SELECT * FROM IPs WHERE ReservedBy = @userId", new { userId })).Select(s => s.Ip).ToList();
				}
				else
				{
					return iP.Select(s => s.Ip).ToList();
				}
				
			}
		}
		public async Task<List<Subscribe>> GetSubscribes(string email)
		{
			using(var db = new SqlConnection(connectionString))
			{
				string userId = await db.QueryFirstOrDefaultAsync<string>("SELECT Id FROM AspNetUsers WHERE Email = @email", new { email });
				return (await db.QueryAsync<Subscribe>("SELECT * FROM Subscribes WHERE UserId = @userId", new { userId })).ToList();

			}
		}
		public async Task<List<OrderViewModel>> CheckPay(string email)
		{
			using (var db = new SqlConnection(connectionString))
			{
				string address = await db.QueryFirstOrDefaultAsync<string>("SELECT Bitcoin FROM AspNetUsers WHERE Email = @email", new { email });
				string html;
				string jsonBitcoinPrice;
				using (WebClient client = new WebClient())
				{
					html = await client.DownloadStringTaskAsync("https://www.blockchain.com/btc/address/"+ address +"?filter=2");
					jsonBitcoinPrice = await client.DownloadStringTaskAsync("https://api.coindesk.com/v1/bpi/currentprice/btc.json");
				}
				var parser = new HtmlParser();
				var document = await parser.ParseDocumentAsync(html);

				IEnumerable<IElement> elements = document.GetElementsByClassName("txdiv");

				List<string> txList = new List<string>();
				List<string> priceList = new List<string>();
				foreach (var el in elements)
				{
					string tx = el.GetElementsByClassName("hash-link").Select(s => s.TextContent).FirstOrDefault();
					int? idResult = await db.QueryFirstOrDefaultAsync<int>("SELECT Id FROM Orders WHERE txHash = @tx", new { tx });
					if (idResult == 0)
					{
						txList.Add(tx);
						priceList.Add(el.GetElementsByClassName("pull-right hidden-phone").FirstOrDefault().GetElementsByTagName("span").Select(s => s.TextContent).FirstOrDefault().Trim(new char[] { ' ', 'B', 'T', 'C' }));
					}

				}
				int countProxy = 0;
				List<OrderViewModel> returnList = new List<OrderViewModel>();
				if (txList.Count() != 0)
				{

					JObject o = JObject.Parse(jsonBitcoinPrice);
					float price = float.Parse(o.Last.First.First.First.Last.Last.ToString());
					float userBalace = await db.QueryFirstOrDefaultAsync<float>("SELECT Balance FROM AspNetUsers WHERE Email = @email", new { email });



					for (int i = 0; i < txList.Count(); i++)
					{

						int priceSub = await db.QueryFirstOrDefaultAsync<int>("SELECT TOP(1) SubPrice FROM AppSettings");
						float pr = (float.Parse(priceList[i], CultureInfo.InvariantCulture) * price + userBalace) * 1000 / priceSub;
						float pr2 = pr;

						int count = 0;
						for(int j = 0; pr > 1000; j++)
						{
							count++;
							pr -= 1000;
						}
						pr = 1000 * count;
						pr2 = pr2 - pr;

						await db.ExecuteAsync("UPDATE AspNetUsers SET Balance = @balance WHERE Email = @email", new { balance = pr2 * priceSub / 1000, email });

						countProxy = countProxy + (int)Math.Round(pr, 0);
						returnList.Add(new OrderViewModel { PriceBitcoint = price.ToString(), CountBitcoin = priceList[i], BitcoinAddress = countProxy.ToString(), BalanceAdd = pr2 * priceSub / 1000 });

						string txHash = txList[i];
						string countBitcoin = priceList[i];

						await db.ExecuteAsync("INSERT INTO Orders (txHash, BitcoinAddress, CountBitcoin, PriceBitcoint) VALUES(@txHash, @address, @countBitcoin, @price)", new { txHash, address, countBitcoin, price });
					}
					string userId = await db.QueryFirstOrDefaultAsync<string>("SELECT Id FROM AspNetUsers WHERE Email = @email", new { email });
					Subscribe subscribe = new Subscribe { Days = 7, StartTime = DateTime.Now, EndTime = DateTime.Now.AddDays(7), UserId = userId, CountProxy = countProxy, IsAvaible = true, Notify = false };
					await db.ExecuteAsync("INSERT INTO Subscribes (Days, StartTime, EndTime, UserId, CountProxy, IsAvaible, Notify) Values(@Days, @StartTime, @EndTime, @UserId, @CountProxy, @IsAvaible, @Notify)", new { subscribe.Days, subscribe.StartTime, subscribe.EndTime, subscribe.UserId, subscribe.CountProxy, subscribe.IsAvaible, subscribe.Notify });
					await _emailRepositorycs.SendEmailAsync(email, "Успешная покупка", $"Здравствуйте!\nВы успешно приобрели подписку на нашем сайте.\nКоличество прокси: {countProxy}\nДата окончания: {subscribe.EndTime}\n\nСпасибо за покупку!");

				}

				return returnList;
			}

			
		}
	}
}
