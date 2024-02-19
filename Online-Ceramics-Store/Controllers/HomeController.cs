using System.Diagnostics;
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

        public async Task<IActionResult> Index()
        {
            List<Product> products = new List<Product>();

            // Database interaction code
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("SELECT * FROM products;", connection);
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                Product product = new Product
                {
                    // Assuming Product class properties match the columns in the "products" table
                    Name = reader.GetString("name"),
                    ProId = reader.GetInt32("proid"),
                    Dep = reader.GetString("dep"),
                    Brand = reader.GetString("brand")
                    // Map other properties accordingly
                };

                products.Add(product);
            }

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}