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
        //[Route("")]
        public async Task<IActionResult> Index()
        {
            // Database interaction code
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand("SELECT * FROM ITEMS;", connection);
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
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}