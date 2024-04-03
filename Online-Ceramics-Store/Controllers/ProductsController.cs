using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Online_Ceramics_Store.Models;
using System.Data;
using System.Text.Json;

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
        public IActionResult detailByName(string nameDetail)
        {
            Product product = null;
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM ITEMS WHERE name = @nameDetail";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nameDetail", nameDetail);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                            if(reader.Read())
                            {
                                product = new Product()
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
                            }
                            
                        
                    }
                }
                connection.CloseAsync();
            }
            return RedirectToAction("detail", product);
            

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
                connection.CloseAsync();
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
                string query = "SELECT * FROM ITEMS";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@filterPriceMin", filterPriceMin);
                    command.Parameters.AddWithValue("@filterPriceMax", filterPriceMax);
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
                            
                            if ((product.insale == 0 && product.price >= filterPriceMin && product.price <= filterPriceMax) ||  (product.insale == 1 && CalculateNewPrice(product) >= filterPriceMin && CalculateNewPrice(product) <= filterPriceMax)||filterPriceMin==0 && filterPriceMax==0)
                            {
                                allfillterProducts.Add(product);
                            }
                        }
                    }
                }
                connection.CloseAsync();
            }

            // Pass the filtered products to the view
            return View("shop", allfillterProducts);
        }
        // Method to calculate new price
        private int CalculateNewPrice(Product product)
        {
            return (int)(product.price - (product.price * product.percent / 100));
        }
        public IActionResult fillterBypopular()
        {
            List<Product> allfillterProducts = new List<Product>();
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM ITEMS";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                   
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
                                quantity_purchased= reader.GetInt32("quantity_purchased"),
                                category_id = reader.GetInt32("category_id"),
                                insale = reader.GetInt32("insale"),
                                percent = reader.GetInt32("percent"),
                                price = reader.GetInt32("price"),
                            };

                            allfillterProducts = allfillterProducts.OrderByDescending(p => p.quantity_purchased).ToList();
                            allfillterProducts.Add(product);
                            
                        }
                    }
                }
                connection.CloseAsync();
            }

            // Pass the filtered products to the view
            return View("shop", allfillterProducts);
        }


        public IActionResult fillterbydecreaseprice()
        {
            List<Product> allfillterProducts = new List<Product>();
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM ITEMS";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
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
                                quantity_purchased= reader.GetInt32("quantity_purchased"),
                                category_id = reader.GetInt32("category_id"),
                                insale = reader.GetInt32("insale"),
                                percent = reader.GetInt32("percent"),
                                price = reader.GetDouble("price"),
                            };

                            // Update price if the product is on sale
                            if (product.insale == 1)
                            {
                                product.UpdatedPrice = (product.price - (product.price * product.percent / 100));
                            }
                            else
                            {
                                product.UpdatedPrice = product.price; // Use original price
                            }

                            // Add the product to the list without sorting
                            allfillterProducts.Add(product);
                        }
                    }
                }
                connection.CloseAsync();
            }

            // Sort the list by updated price in descending order
            List<Product> sortedProducts = allfillterProducts.OrderByDescending(p => p.UpdatedPrice).ToList();

            // Pass the sorted products to the view
            return View("shop", sortedProducts);
        }
        public IActionResult Filterbyincreaseprice()
        {
            List<Product> allfillterProducts = new List<Product>();
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM ITEMS";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
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
                                quantity_purchased = reader.GetInt32("quantity_purchased"),
                                category_id = reader.GetInt32("category_id"),
                                insale = reader.GetInt32("insale"),
                                percent = reader.GetInt32("percent"),
                                price = reader.GetDouble("price"),
                            };

                            // Update price if the product is on sale
                            if (product.insale == 1)
                            {
                                  product.UpdatedPrice =(int)(product.price - (product.price * product.percent / 100));
                            }
                            else
                            {
                                product.UpdatedPrice = product.price; // Use original price
                            }

                            // Add the product to the list without sorting
                            allfillterProducts.Add(product);
                        }
                    }
                }
                connection.CloseAsync();
            }

            // Sort the list by price in ascending order after updating the prices
            List<Product> sortedProducts = allfillterProducts.OrderBy(p => p.UpdatedPrice).ToList();
            // Pass the sorted products to the view
            return View("shop", sortedProducts);
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
                connection.CloseAsync();
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
                connection.CloseAsync();
                return View("shop",allProducts);

            }
            
        }
        public IActionResult BuyNow(Product product)
        {
            var json = HttpContext.Session.GetString("ShoppingCart");
            var cartModel = new ProductsCartModel();
            if (json != null)
            {
                cartModel = JsonSerializer.Deserialize<ProductsCartModel>(json);
            }
            if (cartModel.productsDetailCart.ContainsKey(product.item_id))
            {
                cartModel.productsDetailCart[product.item_id]++;
            }
            else
            {
                cartModel.productsDetailCart[product.item_id] = 1;
            }
            var json1 = JsonSerializer.Serialize(cartModel);
            HttpContext.Session.SetString("ShoppingCart", json1);

            return RedirectToAction("checkout", "ShoppingCart");
        }
    }
}

