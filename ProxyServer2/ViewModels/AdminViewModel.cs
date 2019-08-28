using ProxyServer2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer2.ViewModels
{
	public class AdminViewModel
	{
		public int AvaibleBitcountPayment { get; set; }
		public List<BitcoinPayment> bitcoinPaymentsList { get; set; }
	}
}
