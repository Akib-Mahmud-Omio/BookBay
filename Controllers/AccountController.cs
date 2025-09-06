using E_Book_Store_1.Helpers;
using E_Book_Store_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Razor.Tokenizer;

namespace E_Book_Store_1.Controllers
{
    public class AccountController : Controller
    {
        private Model1 db = new Model1();


        // GET: Account
        public ActionResult Login()
        {
            ViewBag.Title = "Login";
            return View();
        }

        [HttpPost]
        public ActionResult Login(string Username, string Password, bool IsAdmin = false)
        {
            using (var context = new Model1())
            {
                if (IsAdmin)
                {
                    // Admin login
                    var admin = context.Admins
                                       .FirstOrDefault(a => a.username == Username && a.password == Password);

                    if (admin != null)
                    {
                        // Successful admin login
                        Session["AdminId"] = admin.admin_id; // Store admin ID in session


                        // Use helper to store customer_id
                        SessionHelper.SetAdminId(admin.admin_id);


                        TempData["Message"] = "Admin Login Succcess";
                        return RedirectToAction("Dashboard", "Admin"); // Redirect to admin dashboard
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid admin username or password.";
                        return View();
                    }
                }
                else
                {
                    // Customer login
                    var customer = context.Customers
                                          .FirstOrDefault(c => c.username == Username && c.password == Password);

                    if (customer != null)
                    {
                        // Use helper to store customer_id
                        SessionHelper.SetCustomerId(customer.customer_id);
                        Session["CustomerId"] = customer.customer_id;
                        TempData["Message"] = "Login Success, Welocme Back!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid username or password. Please try again.";
                        return View();
                    }
                }
            }
        }

        public ActionResult Logout()
        {
            // Clear the customer session using the SessionHelper
            Helpers.SessionHelper.RemoveCustomerId();

            // Optionally clear other session data if necessary
            Session.Clear();

            TempData["logoutMessage"] = "logout Successfully!";

            // Redirect to the home page or login page
            return RedirectToAction("Index", "Home");
        }

        public ActionResult AdminLogout()
        {
            // Clear the customer session using the SessionHelper
            Helpers.SessionHelper.RemoveAdminId();

            // clear other session data
            Session.Clear();

            TempData["logoutMessage"] = "Admin logout Successfully!";

            // Redirect to the home page or login page
            return RedirectToAction("Login", "Account");
        }



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

        [HttpPost]
        public ActionResult RegisterAdmin(string Username, string Password, string MasterPin)
        {
            // Check if the master pin is correct
            if (MasterPin != "123") // Change this to a secure and hashed comparison in production
            {
                ViewBag.ErrorMessage = "Invalid Master Pin. You are not authorized to register as an admin.";
                return View();
            }

            using (var context = new Model1())
            {
                // Check if the username already exists
                var existingAdmin = context.Admins.FirstOrDefault(a => a.username == Username);
                if (existingAdmin != null)
                {
                    ViewBag.ErrorMessage = "Username already exists. Please choose a different username.";
                    return View();
                }

                // Create a new admin
                var newAdmin = new Admin
                {
                    username = Username,
                    password = Password // Ensure to hash the password in production
                };

                context.Admins.Add(newAdmin);
                context.SaveChanges();

                ViewBag.SuccessMessage = "Admin registered successfully.";
                return RedirectToAction("Login"); // Redirect to login after successful registration
            }
        }



       


    }
}