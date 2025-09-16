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
        public ActionResult Dashboard()
        {
            var totalOrders = db.Orders.Count();
            var totalRevenue = db.Orders.Sum(o => (decimal?)o.total_price) ?? 0;
            var totalCustomers = db.Customers.Count();
            var totalBooks = db.Books.Count();
            var recentBooks = db.Books.OrderByDescending(b => b.published_date).Take(4).ToList();

            // Recent Reviews
            var recentReviews = db.Feedbacks
                                  .Include(f => f.Book)
                                  .Include(f => f.Customer)
                                  .OrderByDescending(f => f.date)
                                  .Take(3)
                                  .ToList();

            // Revenue Graph Data (Last 6 months)
            var revenueGraphData = db.Orders
                .GroupBy(o => new { Month = o.order_date.Value.Month, Year = o.order_date.Value.Year })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalRevenue = g.Sum(o => (decimal?)o.total_price) ?? 0
                })
                .OrderBy(g => g.Year).ThenBy(g => g.Month)
                .ToList();

            var revenueLabels = revenueGraphData.Select(g => $"{g.Month}/{g.Year}").ToArray();
            var revenueValues = revenueGraphData.Select(g => g.TotalRevenue).ToArray();

            // Pass data to the view
            var model = new Dictionary<string, object>
        {
            { "TotalOrders", totalOrders },
            { "TotalRevenue", totalRevenue },
            { "TotalCustomers", totalCustomers },
            { "TotalBooks", totalBooks },
            { "RecentBooks", recentBooks },
            { "RecentReviews", recentReviews },
            { "RevenueGraphData", new { labels = revenueLabels, values = revenueValues } }
        };

            // Controller - Fetch low stock books
            var lowStockBooks = db.Books
                .Where(b => b.stock < 5) // Assuming 'stock' is the field for book quantity
                .ToList(); // Get the full Book model for low stock books

            // Pass the low stock books to the view
            model.Add("LowStockBooks", lowStockBooks);

            // Assuming you have an 'Order_Item' model that links orders with books.
            // Query to get the top 3 books with the most stock
            var topStockBooks = db.Books
                .OrderByDescending(b => b.stock) // Sort by stock in descending order
                .Take(3) // Get the top 3 books with the highest stock
                .ToList();

            // Pass the data to the view
            ViewData["TopStockBooks"] = topStockBooks;


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
