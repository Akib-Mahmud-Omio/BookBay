using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using E_Book_Store_1.Models;
using System.IO;

namespace E_Book_Store_1.Controllers
{
    public class AdminEBookController : Controller
    {
        private Model1 db = new Model1();

        // List all EBooks
        public ActionResult Index()
        {
            return View(db.EBooks.ToList());
        }

        // Add new EBook (GET)
        public ActionResult Create()
        {
            return View();
        }

        // Add new EBook (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EBook ebook, HttpPostedFileBase file, HttpPostedFileBase cover)
        {
            if (ModelState.IsValid)
            {
                var ebookDir = Server.MapPath("~/Uploads/EBooks/");
                var coverDir = Server.MapPath("~/Uploads/Covers/");
                if (!Directory.Exists(ebookDir)) Directory.CreateDirectory(ebookDir);
                if (!Directory.Exists(coverDir)) Directory.CreateDirectory(coverDir);

                // Save PDF
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(ebookDir, fileName);
                    file.SaveAs(path);
                    ebook.file_path = "/Uploads/EBooks/" + fileName;
                }

                // Save Cover
                if (cover != null && cover.ContentLength > 0)
                {
                    var coverName = Path.GetFileName(cover.FileName);
                    var coverPath = Path.Combine(coverDir, coverName);
                    cover.SaveAs(coverPath);
                    ebook.cover_image = "/Uploads/Covers/" + coverName;
                }

                db.EBooks.Add(ebook);
                db.SaveChanges();
                TempData["Success"] = "EBook added successfully!";
                return RedirectToAction("Index");
            }

            return View(ebook);
        }

        // ========================
        // EDIT EBOOK
        // ========================

        // GET: AdminEBook/Edit/5
        public ActionResult Edit(int id)
        {
            var ebook = db.EBooks.Find(id);
            if (ebook == null) return HttpNotFound();
            return View(ebook);
        }

        // POST: AdminEBook/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EBook ebook, HttpPostedFileBase file, HttpPostedFileBase cover)
        {
            var existing = db.EBooks.Find(ebook.ebook_id);
            if (existing == null) return HttpNotFound();

            if (ModelState.IsValid)
            {
                existing.title = ebook.title;
                existing.author = ebook.author;
                existing.description = ebook.description;

                var ebookDir = Server.MapPath("~/Uploads/EBooks/");
                var coverDir = Server.MapPath("~/Uploads/Covers/");
                if (!Directory.Exists(ebookDir)) Directory.CreateDirectory(ebookDir);
                if (!Directory.Exists(coverDir)) Directory.CreateDirectory(coverDir);

                // Update PDF
                if (file != null && file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(ebookDir, fileName);
                    file.SaveAs(path);
                    existing.file_path = "/Uploads/EBooks/" + fileName;
                }

                // Update Cover
                if (cover != null && cover.ContentLength > 0)
                {
                    var coverName = Path.GetFileName(cover.FileName);
                    var coverPath = Path.Combine(coverDir, coverName);
                    cover.SaveAs(coverPath);
                    existing.cover_image = "/Uploads/Covers/" + coverName;
                }

                db.SaveChanges();
                TempData["Success"] = "EBook updated successfully!";
                return RedirectToAction("Index");
            }

            return View(ebook);
        }

        // ========================
        // DELETE EBOOK
        // ========================
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var ebook = db.EBooks.Find(id);
            if (ebook == null)
            {
                return Json(new { success = false, message = "E-Book not found!" });
            }

            // Delete files (optional)
            try
            {
                if (!string.IsNullOrEmpty(ebook.file_path))
                {
                    var fullFile = Server.MapPath(ebook.file_path);
                    if (System.IO.File.Exists(fullFile)) System.IO.File.Delete(fullFile);
                }

                if (!string.IsNullOrEmpty(ebook.cover_image))
                {
                    var fullCover = Server.MapPath(ebook.cover_image);
                    if (System.IO.File.Exists(fullCover)) System.IO.File.Delete(fullCover);
                }
            }
            catch
            {
                // Ignore file deletion errors
            }

            db.EBooks.Remove(ebook);
            db.SaveChanges();

            return Json(new { success = true, message = "E-Book deleted successfully!" });
        }

        // ========================
        // SUBSCRIPTIONS
        // ========================
        public ActionResult Subscriptions()
        {
            var subs = db.Subscriptions
                         .OrderByDescending(s => s.start_date)
                         .ToList();
            return View(subs);
        }
    }
}
