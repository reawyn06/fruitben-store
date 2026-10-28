using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Helpers;
using ShopHoaQua.Models;

namespace ShopHoaQua.Controllers
{
    public class TaiKhoanController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        // GET: /TaiKhoan/DangNhap
        public ActionResult DangNhap(string returnUrl)
        {
            if (PhienDangNhap.DaDangNhap(Session))
                return PhienDangNhap.LaAdmin(Session) ? RedirectToAction("Index", "QuanTri") : RedirectToAction("Index", "Home");

            ViewBag.ReturnUrl = returnUrl;
            return View(new DangNhapViewModel());
        }

        // POST: /TaiKhoan/DangNhap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(DangNhapViewModel model, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid) return View(model);
            var taiKhoan = db.TaiKhoans.FirstOrDefault(t => t.Email == model.Email);

            if (taiKhoan == null || !MatKhauHelper.KiemTra(model.MatKhau, taiKhoan.MatKhau))
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            if (!MatKhauHelper.DaDuocBam(taiKhoan.MatKhau))
            {
                taiKhoan.MatKhau = MatKhauHelper.BamMatKhau(model.MatKhau);
                db.SaveChanges();
            }

            PhienDangNhap.GhiSession(Session, taiKhoan);

            if (model.GhiNho)
            {
                PhienDangNhap.GhiCookieGhiNho(Response, taiKhoan);
            }

            TempData["ThongBaoThanhCong"] = "Xin chào " + taiKhoan.HoTen + ", chúc bạn mua sắm vui vẻ!";

            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (taiKhoan.VaiTro == PhienDangNhap.VAI_TRO_ADMIN)
                return RedirectToAction("Index", "QuanTri");

            return RedirectToAction("Index", "Home");
        }

        // GET: /TaiKhoan/DangKy
        public ActionResult DangKy()
        {
            return View(new DangKyViewModel());
        }

        // POST: /TaiKhoan/DangKy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(DangKyViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (db.TaiKhoans.Any(t => t.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký.");
                return View(model);
            }

            var taiKhoan = new TaiKhoan
            {
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim(),
                SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai) ? null : model.SoDienThoai.Trim(),
                MatKhau = MatKhauHelper.BamMatKhau(model.MatKhau), // chỉ lưu bản băm
                VaiTro = PhienDangNhap.VAI_TRO_KHACH_HANG
            };

            db.TaiKhoans.Add(taiKhoan);
            db.SaveChanges();

            TempData["ThongBao"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("DangNhap");
        }

        // GET: /TaiKhoan/DangXuat
        public ActionResult DangXuat()
        {
            Session.Clear();
            Session.Abandon();
            PhienDangNhap.XoaCookieGhiNho(Response);

            return RedirectToAction("DangNhap");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
