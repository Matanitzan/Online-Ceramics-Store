using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Online_Ceramics_Store.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Text.Json;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Online_Ceramics_Store.Controllers
{
    public class ShoppingCartController : Controller
    {
        private IConfiguration _configuraion;
        private readonly string _connectionString = "";

        public ShoppingCartController(IConfiguration configuration)
        {
            _configuraion = configuration;
            _connectionString = _configuraion.GetConnectionString("Default");
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public DataTable GetProductsFromCart()
        {
            var productsDetailCart = new Dictionary<int, int>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                // Construct the SQL query to retrieve shopping cart data for the specified customer ID
                string sqlQuery = "SELECT item_id, quantity FROM CART_PROD WHERE cust_id = 0";

                // Create a command object with the SQL query and connection
                using (var command = new MySqlCommand(sqlQuery, connection))
                {

                    // Open the database connection
                    connection.Open();

                    // Execute the SQL query and retrieve the result using a reader
                    using (var reader = command.ExecuteReader())
                    {
                        // Iterate through the result set and populate the productsDetailCart dictionary
                        while (reader.Read())
                        {
                            int itemId = reader.GetInt32("item_id");
                            int quantity = reader.GetInt32("quantity");

                            // Add item_id and quantity to the dictionary
                            productsDetailCart.Add(itemId, quantity);
                        }
                    }
                }
            }

            var test = new ProductsCartModel
            {
                userID = 0,
                productsDetailCart = productsDetailCart
            };
            

            var json1 = JsonSerializer.Serialize(test);

            // Store the JSON string in the session
            HttpContext.Session.SetString("ShoppingCart", json1);

            var json = HttpContext.Session.GetString("ShoppingCart");

            if (json == null)
            {
                // Return an empty DataTable if the shopping cart is empty
                return new DataTable();
            }
            // Deserialize the JSON string into your object type
            var cart = JsonSerializer.Deserialize<ProductsCartModel>(json);

            // Creating a DataTable to store the result
            DataTable productsTable = new DataTable();

            // Opening a connection to the database
            using (var connection = new MySqlConnection(_connectionString))
            {
                // SQL command to select name, price, and calculate quantity from products table and the dictionary

                string sqlQuery = "SELECT p.name, p.price, IFNULL(pd.quantity, 0) AS quantity " +
                                  "FROM ITEMS p " +
                                  "LEFT JOIN (SELECT * FROM (";

                // Adding values from the dictionary to the SQL query
                List<string> values = new List<string>();
                foreach (var kvp in cart.productsDetailCart)
                {
                    values.Add($"SELECT {kvp.Key} AS item_id, {kvp.Value} AS quantity");
                }

                if (values.Count > 0)
                {
                    sqlQuery += string.Join(" UNION ALL ", values);

                    // Completing the SQL query
                    sqlQuery += ") AS temp) AS pd ON p.item_id = pd.item_id";

                    // Creating a SqlDataAdapter to execute the SQL query
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(sqlQuery, connection))
                    {
                        // Opening the connection and filling the DataTable with the result
                        connection.Open();
                        adapter.Fill(productsTable);
                    }
                }


                //sqlQuery += string.Join(" UNION ALL ", values);


                // Completing the SQL query
                //sqlQuery += ") AS temp) AS pd ON p.item_id = pd.item_id";


                // Creating a SqlDataAdapter to execute the SQL query
                //using (MySqlDataAdapter adapter = new MySqlDataAdapter(sqlQuery, connection))
                //{
                //    // Opening the connection and filling the DataTable with the result
                //    connection.Open();
                //    adapter.Fill(productsTable);
                //}
            }

            // Returning the DataTable as a result
            return productsTable;
        }

        public IActionResult cart()
        {
            // Retrieve the DataTable with product data
            DataTable productsTable = GetProductsFromCart();
            decimal subtotal = 0;
            foreach (DataRow row in productsTable.Rows)
            {
                // Assuming "Total" is a column in the DataTable representing the total price for each product
                subtotal += Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]);
            }
            // Create an instance of the ViewModel and populate it with the DataTable
            CartModel cartModel = new CartModel
            {
                Products = productsTable,
                Subtotal = subtotal

            };
            return View(cartModel);
        }

        public IActionResult checkout()
        {
            return View();
        }
    }
}

