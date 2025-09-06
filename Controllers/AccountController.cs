using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Book_Store_1.Models;

namespace E_Book_Store_1.Controllers
{
    public class AccountController : Controller
    {
        private Model1 db = new Model1();

        public ActionResult Register()
        {
            ViewBag.Title = "Register";
            return View();
        }

        // POST: Register
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Register(string Name, string Address, string Email, string Phone, string Username, string Password)
        {
            // Check if username or email already exists
            if (db.Customers.Any(c => c.username == Username || c.email == Email))
            {
                ViewBag.ErrorMessage = "Username or Email already exists. Please try again.";
                return View();
            }

            // Create a new customer object
            var newCustomer = new Customer
            {
                name = Name,
                address = Address,
                email = Email,
                phone_number = Phone,
                username = Username,
                password = Password, // Consider hashing for security
                registered_on = DateTime.Now
            };

            // Save to the database
            db.Customers.Add(newCustomer);
            db.SaveChanges();

            // Redirect to login or success page
            return RedirectToAction("Login", "Account");
        }

        public ActionResult RegisterAdmin()
        {
            ViewBag.Title = "Register Admin";
            return View();
        }
    }
}