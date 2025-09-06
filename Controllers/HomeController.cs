using E_Book_Store_1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace E_Book_Store_1.Controllers
{
    public class HomeController : Controller
    {
        private Model1 db = new Model1();

        // GET: Home
        public ActionResult Index()
        {
            ViewBag.Title = "Home";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "About";
            return View();
        }
    }
}