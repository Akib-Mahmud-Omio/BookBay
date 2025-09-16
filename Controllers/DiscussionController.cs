using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using E_Book_Store_1.Helpers;
using E_Book_Store_1.Models;

namespace E_Book_Store_1.Controllers
{
    public class DiscussionController : Controller
    {
        private readonly Model1 db = new Model1();

        // GET: Discussion (list page)
        public async Task<ActionResult> Index(string q, int page = 1, int pageSize = 20)
        {
            var discussions = db.Discussions.Include(d => d.Book).Include(d => d.CreatedByCustomer).AsQueryable();

            if (!string.IsNullOrEmpty(q))
            {
                q = q.Trim();
                discussions = discussions.Where(d =>
                    d.title.Contains(q) ||
                    (d.book_name != null && d.book_name.Contains(q)) ||
                    (d.Book != null && d.Book.title.Contains(q))
                );
            }

            discussions = discussions.OrderByDescending(d => d.created_on);

            var list = await discussions.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Query = q;
            // pass login flag to view
            ViewBag.IsLogged = SessionHelper.GetCustomerId() != null;
            return View(list);
        }

        

        // POST: Discussion/Create (user-typed book name)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(string BookName, string Title)
        {
            var custId = SessionHelper.GetCustomerId();
            if (custId == null)
            {
                TempData["Error"] = "You must be logged in to start a discussion.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(BookName) || string.IsNullOrWhiteSpace(Title))
            {
                TempData["Error"] = "Please enter both book name and discussion title.";
                return RedirectToAction("Index");
            }

            // try to match existing book (case-insensitive exact match)
            var book = db.Books.FirstOrDefault(b => b.title.Equals(BookName.Trim(), StringComparison.OrdinalIgnoreCase));

            var discussion = new Discussion
            {
                book_id = book?.book_id,
                book_name = BookName.Trim(),
                title = Title.Trim(),
                created_by = custId,
                created_on = DateTime.Now
            };

            db.Discussions.Add(discussion);
            await db.SaveChangesAsync();

            return RedirectToAction("Details", new { id = discussion.discussion_id });
        }

        
    }
}
