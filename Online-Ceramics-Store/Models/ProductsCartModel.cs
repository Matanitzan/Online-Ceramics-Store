using System;
using System.Collections;
using System.Collections.Generic;

namespace Online_Ceramics_Store.Models
{
	public class ProductsCartModel
	{
        public int? userID { get; set; } = null;
        Dictionary <int, int> productsDetailCart = new Dictionary<int, int>(); //key=itemID, value=quantity per item
        public ProductsCartModel()
		{
        }
    }
}

