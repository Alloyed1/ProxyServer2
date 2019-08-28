using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer2.ViewModels
{
	public class AuthViewModel
	{
		[Required]
		public string Bitcoin { get; set; }
		public bool RememberMe { get; set; }
	}
}
