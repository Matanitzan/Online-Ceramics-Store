using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Online_Ceramics_Store.Models;
//using var connection = new MySqlConnection("Server=sql11.freesqldatabase.com;User ID=sql11683721;Password=cwfasjNBlh;Database=sql11683721");

namespace Online_Ceramics_Store.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString = "Server=sql11.freesqldatabase.com;User ID=sql11683721;Password=cwfasjNBlh;Database=sql11683721";


        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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