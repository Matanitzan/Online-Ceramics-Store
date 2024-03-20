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
using static System.Net.Mime.MediaTypeNames;

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

                string sqlQuery = "SELECT p.name, p.price, p.item_id, IFNULL(pd.quantity, 0) AS quantity " +
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
                    sqlQuery += ") AS temp) AS pd ON p.item_id = pd.item_id having quantity!=0";

                    // Creating a SqlDataAdapter to execute the SQL query
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(sqlQuery, connection))
                    {
                        Console.WriteLine(sqlQuery);
                        // Opening the connection and filling the DataTable with the result
                        connection.Open();
                        adapter.Fill(productsTable);
                    }
                }
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

        [HttpPost]
        public ActionResult UpdateCart(int itemId, string operation)
        {
            int flag = 0;
            // Retrieve the ProductsCartModel from session
            var json = HttpContext.Session.GetString("ShoppingCart");
            var cartModel = JsonSerializer.Deserialize<ProductsCartModel>(json);

            // Update quantity in ProductsCartModel
            if (operation == "plus")
            {
                // Increment quantity
                if (cartModel.productsDetailCart.ContainsKey(itemId))
                {
                    cartModel.productsDetailCart[itemId]++;
                }
                // Add new item to cart if it doesn't exist
                else
                {
                    cartModel.productsDetailCart[itemId] = 1;
                }
            }
            else if (operation == "minus")
            {
                // Decrement quantity
                if (cartModel.productsDetailCart.ContainsKey(itemId) && cartModel.productsDetailCart[itemId] > 1)
                {
                    cartModel.productsDetailCart[itemId]--;
                }
                // Remove item if quantity is 1 or less
                else if (cartModel.productsDetailCart.ContainsKey(itemId) && cartModel.productsDetailCart[itemId] == 1)
                {
                    flag = 1;
                    cartModel.productsDetailCart.Remove(itemId);
                }
            }
            else if (operation == "remove")
            {
                if (cartModel.productsDetailCart.ContainsKey(itemId))
                {
                    flag = 1;
                    cartModel.productsDetailCart.Remove(itemId);
                }
            }

            // Update session variable
            var json1 = JsonSerializer.Serialize(cartModel);
            // Store the JSON string in the session
            HttpContext.Session.SetString("ShoppingCart", json1);

            // Update database only if the user is registered
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    // SQL query to update quantity in CART_PROD table
                    string query = "UPDATE CART_PROD SET quantity = @quantity WHERE item_id = @item_id and cust_id=0";
                    if (flag == 1)
                    {
                        query = "DELETE FROM CART_PROD WHERE item_id = @item_id and cust_id=0";
                    }
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        if (flag == 0)
                        {
                            command.Parameters.AddWithValue("@quantity", cartModel.productsDetailCart[itemId]);
                        }
                        // Add parameters to the SQL query
                        command.Parameters.AddWithValue("@item_id", itemId);

                        // Execute the SQL command
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {                            
                            // Update successful
                            TempData["Message"] = "Quantity updated successfully.";
                        }
                        else
                        {
                            // No rows affected, possibly product not found
                            TempData["ErrorMessage"] = "Failed to update quantity. Product may not exist.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while updating quantity: " + ex.Message;
            }
            int updatedQuantity = cartModel.productsDetailCart.ContainsKey(itemId)
        ? cartModel.productsDetailCart[itemId]
        : 0;
            //return RedirectToAction("cart", "ShoppingCart");
            return Json(new { success = true, updatedQuantity });
        }


        [HttpGet]
        public ActionResult<int> GetStockQuantity(int itemId)
        {
            int stockQuantity = 0;
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT stock_quantity FROM ITEMS WHERE item_id = @itemId";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@itemId", itemId);
                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            stockQuantity = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving stock quantity: " + ex.Message);
            }
            return stockQuantity;
        }


        public IActionResult checkout()
        {
            return View();
        }
    }
}

