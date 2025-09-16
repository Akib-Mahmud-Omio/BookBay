using System;
using System.Linq;
using System.Web.Mvc;
using E_Book_Store_1.Models;
using E_Book_Store_1.Helpers;
using Stripe;
using Stripe.Checkout;

// Alias to avoid conflict between Stripe.Subscription and our EF model
using DbSubscription = E_Book_Store_1.Models.Subscription;

namespace E_Book_Store_1.Controllers
{
    public class EBookController : Controller
    {
        private Model1 db = new Model1();

        // -----------------------------
        // List all EBooks
        // -----------------------------
        public ActionResult Index(string searchQuery, string genre, decimal? minPrice, decimal? maxPrice)
        {
            var ebooks = db.EBooks.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                ebooks = ebooks.Where(e => e.title.Contains(searchQuery) || e.author.Contains(searchQuery));

            if (!string.IsNullOrEmpty(genre))
                ebooks = ebooks.Where(e => e.genre == genre);

            if (minPrice.HasValue)
                ebooks = ebooks.Where(e => e.price >= minPrice.Value);

            if (maxPrice.HasValue)
                ebooks = ebooks.Where(e => e.price <= maxPrice.Value);

            return View(ebooks.ToList());
        }

        // -----------------------------
        // Read an EBook
        // -----------------------------
        public ActionResult Read(int id)
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var ebook = db.EBooks.Find(id);
            if (ebook == null)
                return HttpNotFound();

            // Check site-level subscription (not per-ebook)
            bool isSubscribed = db.Subscriptions.Any(s =>
                s.customer_id == customerId.Value &&
                s.end_date > DateTime.Now &&
                s.is_active == true
            );

            ViewBag.IsSubscribed = isSubscribed;
            ViewBag.ShowSubscribeSuccess = TempData["Subscribed"] ?? false;

            return View(ebook);
        }

        // -----------------------------
        // AJAX: Check if current user has active subscription
        // Returns JSON: { isAuthenticated: bool, isSubscribed: bool }
        // -----------------------------
        [HttpGet]
        public JsonResult IsUserSubscribed()
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
            {
                return Json(new { isAuthenticated = false, isSubscribed = false }, JsonRequestBehavior.AllowGet);
            }

            bool isSubscribed = db.Subscriptions.Any(s =>
                s.customer_id == customerId.Value &&
                s.end_date > DateTime.Now &&
                s.is_active == true
            );

            return Json(new { isAuthenticated = true, isSubscribed = isSubscribed }, JsonRequestBehavior.AllowGet);
        }

        // -----------------------------
        // Subscribe to a site-level plan via Stripe Checkout
        // planMonths is 1, 6 or 12 (posted from the view)
        // -----------------------------
        [HttpPost]
        public ActionResult Subscribe(int ebookId, int planMonths)
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var ebook = db.EBooks.Find(ebookId);
            if (ebook == null)
                return HttpNotFound();

            // Simple plan pricing (adjust as necessary)
            decimal planPrice = 0m;
            string planName = planMonths + " month";
            switch (planMonths)
            {
                case 1:
                    planPrice = 5.00m;
                    planName = "1 Month";
                    break;
                case 6:
                    planPrice = 25.00m;
                    planName = "6 Months";
                    break;
                case 12:
                    planPrice = 45.00m;
                    planName = "12 Months";
                    break;
                default:
                    planMonths = 1;
                    planPrice = 5.00m;
                    planName = "1 Month";
                    break;
            }

            // Stripe API Key
            StripeConfiguration.ApiKey = System.Configuration.ConfigurationManager.AppSettings["StripeSecretKey"];

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new System.Collections.Generic.List<string> { "card" },
                LineItems = new System.Collections.Generic.List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmountDecimal = planPrice * 100, // Stripe expects cents
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "Subscription - " + planName,
                                Description = $"Site subscription for {planName}"
                            },
                        },
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = Url.Action("SubscribeSuccess", "EBook", new { ebookId = ebookId, planMonths = planMonths }, protocol: Request.Url.Scheme),
                CancelUrl = Url.Action("Read", "EBook", new { id = ebookId }, protocol: Request.Url.Scheme)
            };

            var service = new SessionService();
            var session = service.Create(options);

            // Optionally: store session.Id for webhook verification later

            return Redirect(session.Url);
        }

        // -----------------------------
        // Stripe payment success
        // -----------------------------
        public ActionResult SubscribeSuccess(int ebookId, int planMonths)
        {
            int? customerId = SessionHelper.GetCustomerId();
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            // determine planName and price (should match Subscribe)
            decimal planPrice = 0m;
            string planName = planMonths + " Month";
            switch (planMonths)
            {
                case 1:
                    planPrice = 5.00m;
                    planName = "1 Month";
                    break;
                case 6:
                    planPrice = 25.00m;
                    planName = "6 Months";
                    break;
                case 12:
                    planPrice = 45.00m;
                    planName = "12 Months";
                    break;
                default:
                    planMonths = 1;
                    planPrice = 5.00m;
                    planName = "1 Month";
                    break;
            }

            // Check existing active subscription for this customer
            var subscription = db.Subscriptions
                .FirstOrDefault(s => s.customer_id == customerId.Value && s.is_active == true && s.end_date > DateTime.Now);

            if (subscription == null)
            {
                // Create new subscription
                db.Subscriptions.Add(new DbSubscription
                {
                    customer_id = customerId.Value,
                    plan_months = planMonths,
                    plan_name = planName,
                    price = planPrice,
                    start_date = DateTime.Now,
                    end_date = DateTime.Now.AddMonths(planMonths),
                    is_active = true
                });
            }
            else
            {
                // Extend active subscription: add planMonths to the end_date
                subscription.is_active = true;
                subscription.plan_months = planMonths; // optional overwrite
                subscription.plan_name = planName;
                subscription.price = planPrice;
                if (subscription.end_date > DateTime.Now)
                {
                    subscription.end_date = subscription.end_date.AddMonths(planMonths);
                }
                else
                {
                    subscription.end_date = DateTime.Now.AddMonths(planMonths);
                    subscription.start_date = DateTime.Now;
                }
                db.Entry(subscription).State = System.Data.Entity.EntityState.Modified;
            }

            db.SaveChanges();

            // Flag for success in the view
            TempData["Subscribed"] = true;

            // Redirect to requested ebook read page
            return RedirectToAction("Read", new { id = ebookId });
        }
    }
}
