using System;
using System.Data;

namespace Online_Ceramics_Store.Models
{
	public class CartModel
	{
        public DataTable Products { get; set; }
        public decimal Subtotal { get; set; }
        public Customer userDetails { get; set; } = null; 
    }
}