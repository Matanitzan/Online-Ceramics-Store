using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Online_Ceramics_Store.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Online_Ceramics_Store.Controllers
{
    public class Account : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [Route("Login")]
        public IActionResult Login() {
            return View();
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
                return View("showDetails", customer);
            }
            else {
                return View("RegisterCustomer", customer);
            }

        }

    }
}

