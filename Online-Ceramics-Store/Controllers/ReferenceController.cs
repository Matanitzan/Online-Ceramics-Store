using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using Online_Ceramics_Store.Models;
using System.Reflection;



// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Online_Ceramics_Store.Controllers
{
    public class ReferenceController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult contact()
        {
            ContactViewModel contact = new ContactViewModel();
            int? cust_id = HttpContext.Session.GetInt32("cust_id");
            string? full_name = HttpContext.Session.GetString("full_name");
            ViewBag.CustId = cust_id;
            ViewBag.FullName = full_name;
            return View("contact", contact);
        }


        [HttpPost]
        public IActionResult SendMessage(ContactViewModel contact)
        {
            return View("contact", contact);

            // If validation fails, return to the same view with validation errors
        }
        public IActionResult faqs()
        {
            int? cust_id = HttpContext.Session.GetInt32("cust_id");
            string? full_name = HttpContext.Session.GetString("full_name");
            ViewBag.CustId = cust_id;
            ViewBag.FullName = full_name;
            return View();
        }
    }

}

