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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Formats.Asn1.AsnWriter;
using System.ComponentModel.DataAnnotations;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Online_Ceramics_Store.Controllers
{
    public class ShoppingCartController : Controller
    {
        private IConfiguration _configuraion;
        private readonly string _connectionString = "";
        public CartModel cartModel = new CartModel();

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

                string sqlQuery = "SELECT p.name, p.price, p.item_id, p.percent, IFNULL(pd.quantity, 0) AS quantity " +
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
                    Console.WriteLine(sqlQuery);
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
            if (productsTable == null || productsTable.Rows.Count == 0)
            {
                TempData["ErrorMessage"] = "Your shopping cart is empty. Please add items to proceed to checkout.";
            }
            foreach (DataRow row in productsTable.Rows)
            {
                if (Convert.ToDecimal(row["percent"]) == 0)
                {
                    subtotal += Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]);
                }
                else
                {
                    subtotal += Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]) - (Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]) * Convert.ToDecimal(row["percent"]) / 100);
                }
            }
            cartModel.Products = productsTable;
            cartModel.Subtotal = subtotal;
            //TempData["CartModel"] = cartModel;
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
            int userId = HttpContext.Session.GetInt32("cust_id") ?? -1;
            // Fetch user details from the database based on the user ID
            Customer userDetails=new Customer();
            if (userId != -1)
            {
                
                try
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        string query = "SELECT * FROM USERS WHERE cust_id = @cust_id";
                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@cust_id", userId);
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    // Map the reader data to the Customer model

                                    userDetails.full_name = reader["full_name"].ToString();
                                    userDetails.email = reader["email"].ToString();
                                    userDetails.phone = reader["phone"].ToString();
                                    userDetails.city = reader["city"].ToString();
                                    userDetails.address = reader["address"].ToString();
                                    userDetails.password = "1111";
                                    
                                }
                                else
                                {
                                    // User not found in the database
                                    TempData["ErrorMessage"] = "User details not found.";
                                    return RedirectToAction("cart");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while fetching user details: " + ex.Message;
                    return RedirectToAction("cart");
                }
            }

            DataTable productsTable = GetProductsFromCart();
            decimal subtotal = 0;
            
            if (productsTable==null || productsTable.Rows.Count==0)
            {
                TempData["ErrorMessage"] = "Your shopping cart is empty. Please add items to proceed to checkout.";
                return RedirectToAction("cart");
                //return View(cartModel);

            }
            foreach (DataRow row in productsTable.Rows)
            {
                // Assuming "Total" is a column in the DataTable representing the total price for each product
                if (Convert.ToDecimal(row["percent"]) == 0)
                {
                    subtotal += Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]);
                }
                else
                {
                    subtotal += Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"])-(Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"])* Convert.ToDecimal(row["percent"])/100);
                }
            }
            cartModel.userDetails = userDetails;
            cartModel.Products = productsTable;
            cartModel.Subtotal = subtotal;

            //TempData["try"] = cartModel;

            return View(cartModel);
        }


        [HttpPost]
        public IActionResult completeOrder(CartModel cartModel1)
        {
            cartModel1.userDetails.password = "1111";
            DataTable productsTable = GetProductsFromCart();
            decimal subtotal = 0;
            if (productsTable == null || productsTable.Rows.Count == 0)
            {
                TempData["ErrorMessage"] = "Your shopping cart is empty. Please add items to proceed to checkout.";
            }
            foreach (DataRow row in productsTable.Rows)
            {
                if (Convert.ToDecimal(row["percent"]) == 0)
                {
                    subtotal += Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]);
                }
                else
                {
                    subtotal += Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]) - (Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]) * Convert.ToDecimal(row["percent"]) / 100);
                }
            }
            cartModel.Products = productsTable;
            cartModel.Subtotal = subtotal;
            cartModel.userDetails = cartModel1.userDetails;
            TryValidateModel(cartModel1.userDetails);
            var customerModelState = new ModelStateDictionary();
            var customerContext = new ValidationContext(cartModel1.userDetails, null, null);
            var customerValidationResults = new List<ValidationResult>();

            bool customerIsValid = Validator.TryValidateObject(cartModel1.userDetails, customerContext, customerValidationResults, true);

            if (!customerIsValid) //true
            {

                //CartModel cartModel = TempData["try"] as CartModel;

                //cartModel.userDetails = cartModel1.userDetails;
                //TempData["try"] = cartModel;

                // Model validation failed, return the checkout view with validation errors
                return View("checkout", cartModel);
            }
            else
            {
                int orderId;
                string orderDate = DateTime.Now.ToString("yyyy-MM-dd");
                string shipDate = DateTime.Now.AddDays(14).ToString("yyyy-MM-dd");
                //string insertOrderQuery = "INSERT INTO ORDERS (cust_id, order_date, ship_date, total_amount, status) VALUES (@cust_id, @order_date, @ship_date, @total_amount, @status)";
                string insertOrderQuery = $@"INSERT INTO ORDERS (cust_id, order_date, ship_date, total_amount, status) 
                                     VALUES ({cartModel.userDetails.cust_id}, '{orderDate}', '{shipDate}', {cartModel.Subtotal}, 'received')";

                using (var connection = new MySqlConnection(_connectionString))
                {
                    MySqlCommand command = new MySqlCommand(insertOrderQuery, connection);
                    connection.Open();
                    command.ExecuteNonQuery();


                    // Construct the SQL query to retrieve shopping cart data for the specified customer ID
                    string sqlQuery = "SELECT MAX(order_id) FROM ORDERS";

                    // Create a command object with the SQL query and connection
                    using (MySqlCommand command1 = new MySqlCommand(sqlQuery, connection))
                    {
                        using (var reader = command1.ExecuteReader())
                        {
                            reader.Read();
                            orderId = reader.GetInt32(0);
                        }
                    }

                    foreach (DataRow row in cartModel.Products.Rows)
                    {
                        int itemId = Convert.ToInt32(row["item_id"]);
                        int quantity = Convert.ToInt32(row["quantity"]);
                        decimal price;
                        if (Convert.ToDecimal(row["percent"]) == 0)
                        {
                            price= Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]);
                        }
                        else
                        {
                            price= Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]) - (Convert.ToDecimal(row["quantity"]) * Convert.ToDecimal(row["price"]) * Convert.ToDecimal(row["percent"]) / 100);
                        }
                        string insertOrderItemQuery = $@"INSERT INTO ORDER_ITEMS (order_id, item_id, quantity, price) 
                                                       VALUES ({orderId}, {itemId}, {quantity}, {price})";
                        using (MySqlCommand command2 = new MySqlCommand(insertOrderItemQuery, connection))
                        {
                            command2.ExecuteNonQuery();
                        }
                    }
                }
                TempData["OrderId"] = orderId;
                TempData["TotalPrice"] = cartModel.Subtotal;
                TempData["CustomerName"] = cartModel.userDetails.full_name;
                TempData["orderDate"] = orderDate;
                TempData["shipDate"] = shipDate;
                orderSummary();

                return View("orderSummary"); 
            }
        }

        [HttpPost]
        public IActionResult orderSummary()
        {
            HttpContext.Session.Remove("ShoppingCart");

            if (HttpContext.Session.GetString("ShoppingCart") != null)
            {
                Console.WriteLine("problem with the remove session");
            }
            return View("orderSummary");
        }
    }
}

