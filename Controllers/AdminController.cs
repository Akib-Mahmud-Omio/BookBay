using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Book_Store_1.Models;


namespace E_Book_Store_1.Controllers
{
    public class AdminController : Controller
    {
        private Model1 db = new Model1();

        public ActionResult Dashboard()
        {
            var totalCustomers = db.Customers.Count();
            // Pass data to the view
            var model = new Dictionary<string, object>
        {
            { "TotalCustomers", totalCustomers },
        };
            return View(model);
        }

        //This function executes that the unregistered/ logged out users can not access the admin page
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            

            if (Session["AdminId"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Account");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
