using System;
namespace Online_Ceramics_Store.Models
{
	public class Order
	{

		
        public string product { get; set; }
        public int price { get; set; }
		public int quantity { get; set; }
        public int total_price { get; set; }
        public int order_id { get; set; }
        public string status { get; set; }
		public Order()
		{
		}
	}
}

