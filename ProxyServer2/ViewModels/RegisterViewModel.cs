using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer2.ViewModels
{
	public class RegisterViewModel
	{
		[Required]
		public string Email { get; set; }

		[Required]
		public string FirstName { get; set; }

		public bool RememberMe { get; set; }
	}
}
