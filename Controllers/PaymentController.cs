using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using E_Book_Store_1.Models;
using E_Book_Store_1.Helpers;
using System.Data.Entity;
using System;

namespace BookBay.Controllers
{
    public class PaymentController : Controller
    {
        private Model1 db = new Model1();

        [HttpGet]
        public ActionResult CheckoutFromCart()
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var cart = db.Carts.Include("Cart_Item.Book").FirstOrDefault(c => c.customer_id == customerId);
            if (cart == null || !cart.Cart_Item.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("Cart", "Customer");
            }

            var domain = "http://localhost:50524"; // adjust for production
            StripeConfiguration.ApiKey = ConfigurationManager.AppSettings["StripeSecretKey"];

            var lineItems = new List<SessionLineItemOptions>();
            foreach (var item in cart.Cart_Item)
            {
                lineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Book.price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Book.title
                        }
                    },
                    Quantity = item.quantity
                });
            }

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = domain + "/Payment/SuccessFromCart",
                CancelUrl = domain + "/Payment/Cancel"
            };

            var service = new SessionService();
            var session = service.Create(options);

            return Redirect(session.Url);
        }

        public ActionResult SuccessFromCart()
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var cart = db.Carts.Include("Cart_Item.Book").FirstOrDefault(c => c.customer_id == customerId);
            if (cart == null || !cart.Cart_Item.Any())
            {
                TempData["Error"] = "Cart is empty.";
                return RedirectToAction("Cart", "Customer");
            }

            var order = new Order
            {
                customer_id = customerId.Value,
                order_date = DateTime.Now,
                status = "Paid",
                total_price = cart.Cart_Item.Sum(i => i.quantity * i.Book.price)
            };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in cart.Cart_Item.ToList())
            {
                db.Order_Item.Add(new Order_Item
                {
                    order_id = order.order_id,
                    book_id = item.book_id.Value,
                    quantity = item.quantity,
                    price = item.Book.price
                });

                item.Book.stock -= item.quantity;
            }

            db.Cart_Item.RemoveRange(cart.Cart_Item);
            db.SaveChanges();

            TempData["Success"] = "Payment successful! Your order has been placed.";
            return RedirectToAction("OrderHistory", "Customer");
        }

        public ActionResult Cancel()
        {
            ViewBag.Message = "Payment cancelled. Please try again.";
            return View();
        }
    }
}
