using System;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Online_Ceramics_Store.Models;

namespace Online_Ceramics_Store.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuraion;
        private readonly string _connectionString = "";

        public HomeController( IConfiguration configuration)
        {
            _configuraion = configuration;
            _connectionString = _configuraion.GetConnectionString("Default");
        }
        //[Route("")]
        public async Task<IActionResult> Index()
        {
            // Database interaction code
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand("SELECT * FROM items;", connection);
            using var reader = await command.ExecuteReaderAsync();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Route("")]

        public IActionResult HomePage()
        {
            int? cust_id = HttpContext.Session.GetInt32("cust_id");
            string? full_name = HttpContext.Session.GetString("full_name");
            ViewBag.CustId = cust_id;
            ViewBag.FullName = full_name;
            return View();
        }
        public IActionResult AddToCart(int itemId)
        {
            var json = HttpContext.Session.GetString("ShoppingCart");
            var cartModel=new ProductsCartModel();
            if (json != null)
            {
                cartModel = JsonSerializer.Deserialize<ProductsCartModel>(json);
            }
            int userId = HttpContext.Session.GetInt32("cust_id") ?? -1;
            string query;
            // Increment quantity
            if (cartModel.productsDetailCart.ContainsKey(itemId))
            {
                query = $"UPDATE CART_PROD SET quantity = @quantity WHERE item_id = {itemId} and cust_id = {userId}";
                cartModel.productsDetailCart[itemId]++;
            }
            // Add new item to cart if it doesn't exist
            else
            {
                query = $"INSERT INTO CART_PROD (cust_id, item_id, quantity) VALUES({userId}, {itemId}, {1})";
                cartModel.productsDetailCart[itemId] = 1;
            }
            // Update session variable
            var json1 = JsonSerializer.Serialize(cartModel);
            HttpContext.Session.SetString("ShoppingCart", json1);

            if (userId != -1)
            {
                try
                {
                    using (var connection = new MySqlConnection(_connectionString))
                    {
                        connection.Open();
                        using (MySqlCommand command = new MySqlCommand(query, connection))
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                TempData["Message"] = "Quantity updated successfully.";
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Failed to update quantity. Product may not exist.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating quantity: " + ex.Message;
                }
            }
            
            return Json(new { success = true});
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}