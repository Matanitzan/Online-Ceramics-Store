using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Online_Ceramics_Store.Models;
using System.Data;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Online_Ceramics_Store.Controllers
{
    public class ProductsController : Controller
    {
        private IConfiguration _configuraion;
        private readonly string _connectionString = "";
        public ProductsController(IConfiguration configuration)
        {
            _configuraion = configuration;
            _connectionString = _configuraion.GetConnectionString("Default");
        }
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult detail(Product product)
        {
            int? cust_id = HttpContext.Session.GetInt32("cust_id");
            string? full_name = HttpContext.Session.GetString("full_name");
            ViewBag.CustId = cust_id;
            ViewBag.FullName = full_name;
            return View(product);
        }
        public IActionResult SearchProducts(string searchString)
        {
            List<Product> searchResults = new List<Product>();

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM ITEMS WHERE name LIKE @searchString OR description LIKE @searchString";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@searchString", "%" + searchString + "%");

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Product product = new Product()
                            {
                                item_id = reader.GetInt32("item_id"),
                                name = reader.GetString("name"),
                                description = reader.GetString("description"),
                                information = reader.GetString("information"),
                                stock_quantity = reader.GetInt32("stock_quantity"),
                                category_id = reader.GetInt32("category_id"),
                                insale = reader.GetInt32("insale"),
                                percent = reader.GetInt32("percent"),
                                price = reader.GetInt32("price"),
                            };
                            searchResults.Add(product);
                        }
                    }
                }
            }
            int? cust_id = HttpContext.Session.GetInt32("cust_id");
            string? full_name = HttpContext.Session.GetString("full_name");
            ViewBag.CustId = cust_id;
            ViewBag.FullName = full_name;
            return View("shop", searchResults);
        }
        public IActionResult fillterByPrice(int filterPriceMin, int filterPriceMax)
        {
            List<Product> allfillterProducts = new List<Product>();
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM ITEMS WHERE price>= @filterPriceMin and price<=@filterPriceMax";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@filterPriceMin", filterPriceMin);
                    command.Parameters.AddWithValue("@filterPriceMax", filterPriceMax);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            do
                            {
                                Product product = new Product()
                                {
                                    item_id = reader.GetInt32("item_id"),
                                    name = reader.GetString("name"),
                                    description = reader.GetString("description"),
                                    information = reader.GetString("information"),
                                    stock_quantity = reader.GetInt32("stock_quantity"),
                                    category_id = reader.GetInt32("category_id"),
                                    insale = reader.GetInt32("insale"),
                                    percent = reader.GetInt32("percent"),
                                    price = reader.GetInt32("price"),
                                };
                                allfillterProducts.Add(product);
                            } while (reader.Read());
                        }
                    }
                }
                int? cust_id = HttpContext.Session.GetInt32("cust_id");
                string? full_name = HttpContext.Session.GetString("full_name");
                ViewBag.CustId = cust_id;
                ViewBag.FullName = full_name;
                return View("shop", allfillterProducts);
            }
        }
        public IActionResult fillterProductShop(int category)
        {
            
            List<Product> allfillterProducts = new List<Product>();
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM ITEMS WHERE category_id = @category";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@category", category);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            do
                            {
                                Product product = new Product()
                                {
                                    item_id = reader.GetInt32("item_id"),
                                    name = reader.GetString("name"),
                                    description = reader.GetString("description"),
                                    information = reader.GetString("information"),
                                    stock_quantity = reader.GetInt32("stock_quantity"),
                                    category_id = reader.GetInt32("category_id"),
                                    insale = reader.GetInt32("insale"),
                                    percent = reader.GetInt32("percent"),
                                    price = reader.GetInt32("price"),
                                };
                                allfillterProducts.Add(product);
                            } while (reader.Read());

                        }
                    }
                }

                int? cust_id = HttpContext.Session.GetInt32("cust_id");
                string? full_name = HttpContext.Session.GetString("full_name");
                ViewBag.CustId = cust_id;
                ViewBag.FullName = full_name;
                return View("shop", allfillterProducts);

            }

        }
        public IActionResult shop()
        {
            List<Product> allProducts = new List<Product>();
            using(MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM ITEMS";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            do
                            {
                                Product product = new Product()
                                {
                                    item_id = reader.GetInt32("item_id"),
                                    name = reader.GetString("name"),
                                    description = reader.GetString("description"),
                                    information = reader.GetString("information"),
                                    stock_quantity = reader.GetInt32("stock_quantity"),
                                    category_id = reader.GetInt32("category_id"),
                                    insale = reader.GetInt32("insale"),
                                    percent = reader.GetInt32("percent"),
                                    price = reader.GetInt32("price"),
                                };
                                allProducts.Add(product);
                            } while (reader.Read());

                        }
                    }
                }

                int? cust_id = HttpContext.Session.GetInt32("cust_id");
                string? full_name = HttpContext.Session.GetString("full_name");
                ViewBag.CustId = cust_id;
                ViewBag.FullName = full_name;
                return View("shop",allProducts);

            }
            
        }
    }
}

