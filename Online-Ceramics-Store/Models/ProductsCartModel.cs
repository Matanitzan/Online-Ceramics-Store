using System;
using System.Collections;
using System.Collections.Generic;
using System.Web;


namespace Online_Ceramics_Store.Models
{
	public class ProductsCartModel
	{
        public int? userID { get; set; } = null;
        public Dictionary<int, int> productsDetailCart { get; set; }
        public ProductsCartModel()
		{
            productsDetailCart = new Dictionary<int, int>();
        }
    }
} 