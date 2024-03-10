using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Online_Ceramics_Store.Controllers
{
    public class ProductsController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult detail()
        {
            return View();
        }
        public IActionResult shop()
        {
            int cust_id = HttpContext.Session.GetInt32("cust_id") ?? 0;
            ViewBag.CustId = cust_id;
            return View();
        }
    }
}

