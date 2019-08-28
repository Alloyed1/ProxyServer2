using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ProxyServer2.Models
{
	public class Subscribe
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int Days { get; set; }

		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }

		public int CountProxy { get; set; }
		public bool IsAvaible { get; set; }

		public bool Notify { get; set; }

		public string UserId { get; set; }
		public User User { get; set; }
	}
}
