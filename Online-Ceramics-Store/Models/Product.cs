﻿using System;
using System.ComponentModel.DataAnnotations;
namespace Online_Ceramics_Store.Models
{
	public class Product
	{
		public Product()
		{
		}
		public int item_id { get; set; }
		public string name { get; set; }
        public string description { get; set; }
        public string information { get; set; }
		public int quantity_purchased { get; set; }
        public double UpdatedPrice { get; set; }

        public int stock_quantity { get; set; }
        public int category_id { get; set; }
        public int insale { get; set; }
        public int percent { get; set; }
        public double price { get; set; }


	}
}

