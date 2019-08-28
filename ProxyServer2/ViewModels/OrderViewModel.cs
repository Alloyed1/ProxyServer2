using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer2.ViewModels
{
	public class OrderViewModel
	{
		public string BitcoinAddress { get; set; }
		public string CountBitcoin { get; set; }
		public string PriceBitcoint { get; set; }

		public float BalanceAdd { get; set; }
	}
}
