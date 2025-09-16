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
       
    }
}
