using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using E_Book_Store_1.Models;
using E_Book_Store_1.Helpers;
using System.Data.Entity;

namespace E_Book_Store_1.Controllers
{
    public class CustomerController : Controller
    {

        public ActionResult Profile()
        {
            int? customerId = SessionHelper.GetCustomerId(); // Get logged-in customer ID
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect if not logged in
            }

            using (var db = new Model1())
            {
                var customer = db.Customers.Find(customerId);
                if (customer == null)
                {
                    return HttpNotFound();
                }

                return View(customer);
            }
        }

        // Update Profile
        [HttpPost]
        public ActionResult Profile(Customer updatedCustomer)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedCustomer);
            }

            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new Model1())
            {
                var customer = db.Customers.Find(customerId);
                if (customer == null)
                {
                    return HttpNotFound();
                }

                // Update fields
                customer.name = updatedCustomer.name;
                customer.email = updatedCustomer.email;
                customer.address = updatedCustomer.address;
                customer.phone_number = updatedCustomer.phone_number;
                customer.username = updatedCustomer.username;
                customer.password = updatedCustomer.password;

                db.SaveChanges(); // Save changes to the database
            }

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Profile");
        }
        // View Cart
        public ActionResult Cart()
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            using (var db = new Model1())
            {
                var cart = db.Carts.Include("Cart_Item.Book")
                                   .FirstOrDefault(c => c.customer_id == customerId);

                if (cart == null)
                {
                    cart = new Cart { customer_id = customerId.Value };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }

                var cartItems = cart.Cart_Item.ToList();
                return View(cartItems);
            }
        }

        // Place Order (before Stripe redirect)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(Dictionary<int, int> Quantities)
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            using (var db = new Model1())
            {
                var cart = db.Carts.Include("Cart_Item.Book")
                                   .FirstOrDefault(c => c.customer_id == customerId);
                if (cart == null || !cart.Cart_Item.Any())
                {
                    TempData["Error"] = "Your cart is empty.";
                    return RedirectToAction("Cart");
                }

                foreach (var item in cart.Cart_Item)
                {
                    if (Quantities != null && Quantities.TryGetValue(item.cart_item_id, out int newQuantity))
                    {
                        if (newQuantity <= item.Book.stock && newQuantity > 0)
                            item.quantity = newQuantity;
                        else
                        {
                            TempData["Error"] = $"Not enough stock for {item.Book.title}, or invalid quantity.";
                            return RedirectToAction("Cart");
                        }
                    }
                }
                db.SaveChanges();

                // Redirect to PaymentController
                return RedirectToAction("CheckoutFromCart", "Payment");
            }
        }

        // Remove item from cart
        [HttpPost]
        public JsonResult RemoveFromCart(int cartItemId)
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return Json(new { success = false, message = "User not logged in." });

            using (var db = new Model1())
            {
                var cartItem = db.Cart_Item.Include("Cart")
                                           .FirstOrDefault(ci => ci.cart_item_id == cartItemId && ci.Cart.customer_id == customerId);

                if (cartItem == null)
                    return Json(new { success = false, message = "Item not found." });

                db.Cart_Item.Remove(cartItem);
                db.SaveChanges();
                return Json(new { success = true });
            }
        }
        [HttpGet]
        public ActionResult AddFeedback(int bookId)
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new Model1())
            {
                var book = db.Books.FirstOrDefault(b => b.book_id == bookId);

                // Fetch reviews for the specific book
                var reviews = db.Feedbacks.Where(f => f.book_id == bookId).ToList();

                // Pass book details and reviews to the view
                ViewBag.Reviews = reviews;


                if (book == null)
                {
                    return HttpNotFound();
                }

                // Pass book details and reviews to the view
                ViewBag.Reviews = reviews;

                ViewBag.BookTitle = book.title;
                ViewBag.BookId = bookId;
                return View();
            }
        }

        [HttpPost]
        public ActionResult AddFeedback(int bookId, int rating, string comment)
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            using (var db = new Model1())
            {
                // Add feedback to the database
                var feedback = new Feedback
                {
                    customer_id = customerId.Value,
                    book_id = bookId,
                    rating = rating,
                    comment = comment,
                    date = DateTime.Now
                };
                db.Feedbacks.Add(feedback);
                db.SaveChanges();

                TempData["Success"] = "Thank you for your review!";
                return RedirectToAction("OrderHistory");
            }
        }

        // Order History
        public ActionResult OrderHistory()
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            using (var db = new Model1())
            {
                var orders = db.Orders
                               .Where(o => o.customer_id == customerId)
                               .OrderByDescending(o => o.order_date)
                               .Include(o => o.Order_Item)
                               .Include("Order_Item.Book")
                               .ToList();

                return View(orders);
            }
        }
    }
}
