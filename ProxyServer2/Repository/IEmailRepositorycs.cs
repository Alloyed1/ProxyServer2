using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer2.Repository
{
	public interface IEmailRepositorycs
	{
		Task SendEmailAsync(string email, string subject, string message);
	}
	public class EmailRepositorycs : IEmailRepositorycs
	{

		private string connectionString;
		public EmailRepositorycs(IConfiguration configuration)
		{
			connectionString = configuration.GetConnectionString("DefaultConnection");
		}

		public async Task SendEmailAsync(string email, string subject, string message)
		{
			var emailMessage = new MimeMessage();

			emailMessage.From.Add(new MailboxAddress("Администрация сайта", "login@yandex.ru"));
			emailMessage.To.Add(new MailboxAddress("", email));
			emailMessage.Subject = subject;
			emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
			{
				Text = message
			};

			using (var client = new SmtpClient())
			{
				await client.ConnectAsync("smtp.gmail.com", 465, true);
				await client.AuthenticateAsync("proxyserveremailsender@gmail.com", "cw42puQAZ");
				await client.SendAsync(emailMessage);

				await client.DisconnectAsync(true);
			}
		}
	}
}
