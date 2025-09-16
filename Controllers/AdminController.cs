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

        // GET: Admin/Books
        public ActionResult Books()
        {
            var books = db.Books.ToList();
            return View(books);
        }

        // Add Book (POST)
        [HttpPost]
        public JsonResult AddBook(Book book)
        {
            // Validate book
            if (string.IsNullOrEmpty(book.title))
            {
                return Json("Title is required.");
            }
            if (string.IsNullOrEmpty(book.author))
            {
                return Json("Author is required.");
            }
            if (string.IsNullOrEmpty(book.genre))
            {
                return Json("Genre is required.");
            }
            if (book.stock < 0)
            {
                return Json("Stock must be greater than or equal to 0.");
            }
            if (book.price <= 0)
            {
                return Json("Price must be greater than 0.");
            }

            if (string.IsNullOrEmpty(book.image))
            {
                book.image = "https://defaultimage.com/default.jpg"; // Default image if the user didn't provide one
            }

            // Save the book to the database
            db.Books.Add(book);
            db.SaveChanges();

            return Json("Book Added Successfully!");
        }

        // Get Book by ID (GET)
        [HttpGet]
        public JsonResult GetBook(int id)
        {
            var book = db.Books.FirstOrDefault(b => b.book_id == id);
            if (book != null)
            {
                return Json(book, JsonRequestBehavior.AllowGet);
            }
            return Json(null, JsonRequestBehavior.AllowGet);
        }

        // Edit Book (POST)
        [HttpPost]
        public JsonResult EditBook(Book updatedBook)
        {
            var existingBook = db.Books.FirstOrDefault(b => b.book_id == updatedBook.book_id);

            if (existingBook != null)
            {
                // Update properties
                existingBook.title = updatedBook.title;
                existingBook.author = updatedBook.author;
                existingBook.genre = updatedBook.genre;
                existingBook.description = updatedBook.description;
                existingBook.price = updatedBook.price;
                existingBook.stock = updatedBook.stock;
                existingBook.published_date = updatedBook.published_date;
                existingBook.region = updatedBook.region;
                existingBook.age = updatedBook.age;

                existingBook.image = string.IsNullOrEmpty(updatedBook.image) ? existingBook.image : updatedBook.image;

                // Save changes
                db.SaveChanges();
                return Json("Book Updated Successfully!");
            }

            return Json("Book not found!");
        }



        // Delete Book (POST)
        [HttpPost]
        public JsonResult DeleteBook(int id)
        {
            var book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
            return Json("Book Deleted Successfully!");
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

       public ActionResult Customers()
        {
            var customers = db.Customers.ToList();
            return View(customers);
        }

        public ActionResult GetCustomer(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            return Json(new
            {
                customer_id = customer.customer_id,
                name = customer.name,
                email = customer.email,
                phone_number = customer.phone_number,
                address = customer.address
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveCustomer(Customer customer)
        {
            var existingCustomer = db.Customers.Find(customer.customer_id); // Find existing customer by ID
            if (existingCustomer != null)
            {
                // Update only the fields that were modified
                if (!string.IsNullOrEmpty(customer.name)) existingCustomer.name = customer.name;
                if (!string.IsNullOrEmpty(customer.email)) existingCustomer.email = customer.email;
                if (!string.IsNullOrEmpty(customer.phone_number)) existingCustomer.phone_number = customer.phone_number;
                if (!string.IsNullOrEmpty(customer.address)) existingCustomer.address = customer.address;
            }
            else
            {
                // Add new customer if it doesn't exist
                db.Customers.Add(customer);
            }

            db.SaveChanges();
            return Json("Customer updated successfully!");
        }


        [HttpPost]
        public ActionResult DeleteCustomer(int id)
        {
            var customer = db.Customers.Find(id);
            if (customer == null) return HttpNotFound();
            db.Customers.Remove(customer);
            db.SaveChanges();
            return Json("Customer deleted successfully!");
        }


        public ActionResult Reviews()
        {
            ViewBag.Title = "Manage Reviews";
            return View();
        }
    }
    }
