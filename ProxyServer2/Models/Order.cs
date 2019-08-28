using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer2.Models
{
	public class Order
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public string txHash { get; set; }
		public string BitcoinAddress { get; set; }
		public string CountBitcoin { get; set; }
		public string PriceBitcoint { get; set; }
	}
}
