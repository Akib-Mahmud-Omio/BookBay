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

        // GET: Discussion/Details/5 -> dedicated thread page
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var discussion = await db.Discussions
                                     .Include(d => d.Book)
                                     .Include(d => d.CreatedByCustomer)
                                     .Include(d => d.DiscussionMessages.Select(m => m.Customer))
                                     .FirstOrDefaultAsync(d => d.discussion_id == id.Value);

            if (discussion == null) return HttpNotFound();

            // ensure messages are ordered oldest -> newest
            discussion.DiscussionMessages = discussion.DiscussionMessages.OrderBy(m => m.posted_on).ToList();

            ViewBag.CanPost = SessionHelper.GetCustomerId() != null;
            return View(discussion);
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

        // POST: Discussion/PostMessage (AJAX friendly)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> PostMessage(int discussionId, int? parentId, string messageText)
        {
            if (string.IsNullOrWhiteSpace(messageText))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Message is empty");

            var custId = SessionHelper.GetCustomerId();
            if (custId == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Login required to post");

            var discussion = await db.Discussions.FindAsync(discussionId);
            if (discussion == null) return HttpNotFound();

            var message = new DiscussionMessage
            {
                discussion_id = discussionId,
                parent_message_id = parentId,
                customer_id = custId,
                message_text = messageText,
                posted_on = DateTime.Now
            };

            db.DiscussionMessages.Add(message);
            await db.SaveChangesAsync();

            // load the Customer nav prop so partial can show author name
            db.Entry(message).Reference(m => m.Customer).Load();

            var html = RenderPartialViewToString("_MessagePartial", message);
            return Content(html);
        }

        // POST: Discussion/Delete (only owner can delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete([Bind(Prefix = "id")] int id)
        {
            var discussion = await db.Discussions.FindAsync(id);
            if (discussion == null)
                return HttpNotFound();

            var currentUserId = SessionHelper.GetCustomerId();
            if (discussion.created_by != currentUserId)
                return new HttpStatusCodeResult(403, "You are not allowed to delete this discussion");

            db.Discussions.Remove(discussion);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }

        // helper: render partial view to string
        private string RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new System.IO.StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var ctx = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(ctx, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
