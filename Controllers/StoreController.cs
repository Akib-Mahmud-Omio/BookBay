using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Book_Store_1.Models;
using E_Book_Store_1.Helpers;
using System.Data.Entity;

namespace E_Book_Store_1.Controllers
{
    public class StoreController : Controller
    {
        private Model1 db = new Model1();
        // GET: Store
        // GET: Store
        public ActionResult Index(string searchQuery, decimal? minPrice, decimal? maxPrice, string genre)
        {
            // Default Book Query
            var books = db.Books.AsQueryable();

            // Apply search query filter
            if (!string.IsNullOrEmpty(searchQuery))
            {
                books = books.Where(b => b.title.Contains(searchQuery) || b.author.Contains(searchQuery));
            }

            // Apply price range filter
            if (minPrice.HasValue)
            {
                books = books.Where(b => b.price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                books = books.Where(b => b.price <= maxPrice.Value);
            }

            if (minPrice.HasValue && maxPrice.HasValue)
            {
                books = books.Where(b => b.price >= minPrice.Value && b.price <= maxPrice.Value);
            }

            // Apply genre filter
            if (!string.IsNullOrEmpty(genre) && genre != "All Books")
            {
                books = books.Where(b => b.genre.Equals(genre, StringComparison.OrdinalIgnoreCase));
            }

            // Get the available categories (genres)
            ViewBag.Genres = db.Books.Select(b => b.genre).Distinct().ToList();

            // Pass books to the view
            return View(books.ToList());
        }

        //public ActionResult BookDetails()
        //{
        //    return View();
        //}

        public ActionResult BookDetails(int id)
        {
            // Fetch the book details using the provided ID
            var book = db.Books.FirstOrDefault(b => b.book_id == id);
            if (book == null)
            {
                return HttpNotFound(); // Handle case where the book doesn't exist
            }
            return View(book);
        }

        public ActionResult AddToCart(int bookId)
        {
            // Get the customer ID from the session
            var customerId = SessionHelper.GetCustomerId(); // This method gets the logged-in user's customer ID

            if (customerId == null)
            {
                return Json(new { success = false, message = "Please log in to add books to the cart." });
            }

            db.SaveChanges();

            return Json(new { success = true, message = "Book added to cart successfully!" });
        }


    }
}