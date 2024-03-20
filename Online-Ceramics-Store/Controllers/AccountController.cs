using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Online_Ceramics_Store.Models;


// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Online_Ceramics_Store.Controllers

{
    public class Account : Controller
    {
        private IConfiguration _configuraion;
        private readonly string _connectionString = "";
        static int numofUsers = 0;

        // GET: /<controller>/

        public Account(IConfiguration configuration)
        {
            _configuraion = configuration;
            _connectionString = _configuraion.GetConnectionString("Default");
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Logout()
        {
            int? custId = HttpContext.Session.GetInt32("cust_id");

            if (custId!=null|| custId != -1)
            {
                //custId = -1;
                HttpContext.Session.Remove("full_name");
                HttpContext.Session.Remove("cust_id");
            }
            HttpContext.Session.Clear();
            return RedirectToAction("Index","Home");
        }

        [Route("Login")]
        public IActionResult Login() {
            return View();
        }
        [HttpPost] // This action handles POST requests
        [Route("LoginCustomers")] // Specifies the route for this action
        public IActionResult LoginCustomers(string email, string password)
        {
            // Validate email and password
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                // Handle invalid input (e.g., show error message)
                return View();
            }

            // Perform database query to check if the user exists
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                string query = "SELECT cust_id,full_name FROM USERS WHERE email = @email AND password = @password";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.Parameters.AddWithValue("@password", password);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // User exists, store CUST_ID and full_name in session
                            int custId = reader.GetInt32("cust_id");
                            HttpContext.Session.SetInt32("cust_id", custId);
                            string fullName = reader.GetString("full_name");
                            HttpContext.Session.SetString("full_name", fullName);
                            return RedirectToAction("shop", "Products");
                        }
                        else
                        {
                            // User does not exist or invalid credentials, handle accordingly
                            // For example, display error message and return to login page
                            ViewData["ErrorMessage"] = "Invalid email or password";
                            return View("Login");
                        }
                    }
                    
                }
            }
        }


        [Route("RegisterCustomer")]
        public IActionResult RegisterCustomer()
        {
            Customer customer = new Customer();
            return View("RegisterCustomer",customer);
        }

        [Route("AddCustomer")]
        public IActionResult AddCustomer(Customer customer)
        {
            if (ModelState.IsValid)
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Retrieve the current value of numofUsers from the database
                    string query = "SELECT numofUsers FROM NumberOfUsers WHERE id = 1";
                    int currentNumOfUsers;

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        currentNumOfUsers = (int)command.ExecuteScalar();
                    }

                    // Insert the new customer with the current value of numofUsers
                    query = "INSERT INTO USERS (cust_id, full_Name, email, password, phone, city, address) VALUES (@cust_id, @full_Name, @email, @password, @phone, @city, @address)";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@cust_id", currentNumOfUsers);
                        command.Parameters.AddWithValue("@full_Name", customer.full_name);
                        command.Parameters.AddWithValue("@email", customer.email);
                        command.Parameters.AddWithValue("@password", customer.password);
                        command.Parameters.AddWithValue("@phone", customer.phone);
                        command.Parameters.AddWithValue("@city", customer.city);
                        command.Parameters.AddWithValue("@address", customer.address);
                        int numRowEffected = command.ExecuteNonQuery();
                        if (numRowEffected > 0)
                        {
                            // Update the value of numofUsers in the database
                            query = "UPDATE NumberOfUsers SET numofUsers = @numofUsers WHERE id = 1";
                            using (MySqlCommand updateCommand = new MySqlCommand(query, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@numofUsers", currentNumOfUsers + 1);
                                updateCommand.ExecuteNonQuery();
                            }

                            // Redirect to the login page or any other page
                            return View("Login", customer);
                        }
                        else
                        {
                            return View("RegisterCustomer", customer);
                        }
                    }
                }
            }
            else
            {
                return View("RegisterCustomer", customer);
            }
        }


    }
}

