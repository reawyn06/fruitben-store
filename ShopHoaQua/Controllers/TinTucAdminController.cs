using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Filters;
using ShopHoaQua.Helpers;
using ShopHoaQua.Models;

namespace ShopHoaQua.Controllers
{
    [QuyenAdmin]
    public class TinTucAdminController : Controller
    {
        private readonly ShopHoaQuaContext db = new ShopHoaQuaContext();
        private const string ThuMucAnh = "~/assets/img/tintuc/";

        public ActionResult Index(string tuKhoa)
        {
            tuKhoa = (tuKhoa ?? "").Trim();
            ViewBag.TuKhoa = tuKhoa;
            var query = db.TinTucs.AsQueryable();
            if (tuKhoa.Length > 0) query = query.Where(t => t.TieuDe.Contains(tuKhoa));
            return View(query.OrderByDescending(t => t.NgayDang).ThenByDescending(t => t.TinTucId).ToList());
        }

        public ActionResult Them() { return View(new TinTuc()); }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Them([Bind(Include = "TieuDe,TomTat,NoiDung")] TinTuc tin, HttpPostedFileBase fileHinhAnh)
        {
            KiemTra(tin, fileHinhAnh);
            if (!ModelState.IsValid) return View(tin);
            tin.HinhAnh = UploadAnhHelper.Luu(fileHinhAnh, Server.MapPath(ThuMucAnh), tin.TieuDe);
            tin.NguoiDang = Session["HoTen"] as string ?? "Quản trị viên";
            tin.NgayDang = DateTime.Now;
            db.TinTucs.Add(tin);
            db.SaveChanges();
            TempData["ThongBao"] = "Đã thêm tin tức. Bài viết đã hiển thị ngoài cửa hàng.";
            return RedirectToAction("Index");
        }

        public ActionResult Sua(int id)
        {
            var tin = db.TinTucs.Find(id);
            if (tin == null) return HttpNotFound();
            return View(tin);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Sua([Bind(Include = "TinTucId,TieuDe,TomTat,NoiDung")] TinTuc tin, HttpPostedFileBase fileHinhAnh)
        {
            var cu = db.TinTucs.Find(tin.TinTucId);
            if (cu == null) return HttpNotFound();
            KiemTra(tin, fileHinhAnh);
            if (!ModelState.IsValid)
            {
                tin.HinhAnh = cu.HinhAnh;
                return View(tin);
            }
            cu.TieuDe = tin.TieuDe;
            cu.TomTat = tin.TomTat;
            cu.NoiDung = tin.NoiDung;
            var anh = UploadAnhHelper.Luu(fileHinhAnh, Server.MapPath(ThuMucAnh), tin.TieuDe);
            if (anh != null) cu.HinhAnh = anh;
            db.SaveChanges();
            TempData["ThongBao"] = "Đã cập nhật tin tức.";
            return RedirectToAction("Index");
        }

        public ActionResult Xoa(int id)
        {
            var tin = db.TinTucs.Find(id);
            if (tin == null) return HttpNotFound();
            return View(tin);
        }

        [HttpPost, ActionName("Xoa"), ValidateAntiForgeryToken]
        public ActionResult XoaXacNhan(int id)
        {
            var tin = db.TinTucs.Find(id);
            if (tin == null) return HttpNotFound();
            db.TinTucs.Remove(tin);
            db.SaveChanges();
            TempData["ThongBao"] = "Đã xóa tin tức.";
            return RedirectToAction("Index");
        }

        private void KiemTra(TinTuc tin, HttpPostedFileBase file)
        {
            if (string.IsNullOrWhiteSpace(tin.NoiDung))
                ModelState.AddModelError("NoiDung", "Vui lòng nhập nội dung tin tức.");
            string loi;
            if (!UploadAnhHelper.HopLe(file, out loi)) ModelState.AddModelError("fileHinhAnh", loi);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
