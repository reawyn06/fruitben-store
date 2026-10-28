using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;

namespace ShopHoaQua.Controllers
{
    public class TinTucController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        // GET: /TinTuc
        public ActionResult Index()
        {
            var danhSach = db.TinTucs.OrderByDescending(t => t.NgayDang).ToList();
            return View(danhSach);
        }

        public ActionResult ChiTiet(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var tin = db.TinTucs.Find(id);
            if (tin == null)
            {
                return RedirectToAction("Trang404", "Home");
            }

            return View(tin);
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