using System;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Models;

namespace ShopHoaQua.Controllers
{
    public class LienHeController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        // GET: /LienHe
        public ActionResult Index()
        {
            return View();
        }

        // POST: /LienHe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LienHe lienHe)
        {
           
            if (ModelState.IsValid)
            {
                lienHe.NgayGui = DateTime.Now;
                lienHe.DaXuLy = false; 

                db.LienHes.Add(lienHe);
                db.SaveChanges();

                TempData["ThongBaoLienHe"] = "Cảm ơn bạn đã liên hệ! FruitBen sẽ phản hồi trong vòng 24h.";
                return RedirectToAction("Index");
            }

           
            return View(lienHe);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}