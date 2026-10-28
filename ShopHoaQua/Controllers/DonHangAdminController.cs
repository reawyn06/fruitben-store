using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Filters;
using ShopHoaQua.Models;
using ShopHoaQua.Services;

namespace ShopHoaQua.Controllers
{
    public class LocDonHangViewModel
    {
        public string TrangThai { get; set; }
        public string TuKhoa { get; set; }
        public int? TaiKhoanId { get; set; }
        public int Trang { get; set; } = 1;

        public List<DonHang> DanhSach { get; set; } = new List<DonHang>();
        public Dictionary<string, int> DemTheoTrangThai { get; set; } = new Dictionary<string, int>();
        public int TongSo { get; set; }
        public int TongSoTrang { get; set; }
        public string TenKhachHang { get; set; }
    }

    [QuyenAdmin]
    public class DonHangAdminController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();
        private const int SO_DON_MOI_TRANG = 15;

        // GET: /DonHangAdmin?trangThai=Chờ xử lý&tuKhoa=0901&trang=1
        public ActionResult Index(LocDonHangViewModel loc)
        {
            loc = loc ?? new LocDonHangViewModel();
            if (loc.Trang < 1) loc.Trang = 1;
            loc.TuKhoa = (loc.TuKhoa ?? "").Trim();

            IQueryable<DonHang> query = db.DonHangs;

            if (loc.TaiKhoanId.HasValue)
            {
                var id = loc.TaiKhoanId.Value;
                query = query.Where(d => d.TaiKhoanId == id);
                loc.TenKhachHang = db.TaiKhoans.Where(t => t.TaiKhoanId == id).Select(t => t.HoTen).FirstOrDefault();
            }

            if (loc.TuKhoa.Length > 0)
            {
                var k = loc.TuKhoa;
                int maDon;
                var soTrongMa = k.StartsWith("FB", StringComparison.OrdinalIgnoreCase) ? k.Substring(2) : k;
                bool laMa = int.TryParse(soTrongMa, out maDon);
                query = query.Where(d => (laMa && d.DonHangId == maDon) || d.HoTenNguoiNhan.Contains(k) || d.SoDienThoai.Contains(k));
            }

            loc.DemTheoTrangThai = query.GroupBy(d => d.TrangThai)
                .Select(g => new { g.Key, Dem = g.Count() }).ToList()
                .ToDictionary(x => x.Key ?? "", x => x.Dem);

            if (!string.IsNullOrEmpty(loc.TrangThai))
                query = query.Where(d => d.TrangThai == loc.TrangThai);

            loc.TongSo = query.Count();
            loc.TongSoTrang = (int)Math.Ceiling(loc.TongSo / (double)SO_DON_MOI_TRANG);
            if (loc.TongSoTrang > 0 && loc.Trang > loc.TongSoTrang) loc.Trang = loc.TongSoTrang;

            loc.DanhSach = query.Include(d => d.ChiTietDonHangs)
                .OrderByDescending(d => d.NgayDat)
                .Skip((loc.Trang - 1) * SO_DON_MOI_TRANG).Take(SO_DON_MOI_TRANG)
                .ToList();

            ViewBag.TieuDeTrang = "Đơn hàng";
            return View(loc);
        }

        // GET: /DonHangAdmin/ChiTiet/id
        public ActionResult ChiTiet(int id)
        {
            var dh = db.DonHangs.Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham)).FirstOrDefault(d => d.DonHangId == id);
            if (dh == null) return HttpNotFound();

            if (dh.TaiKhoanId.HasValue)
            {
                ViewBag.TaiKhoan = db.TaiKhoans.Find(dh.TaiKhoanId.Value);
                ViewBag.SoDonCuaKhach = db.DonHangs.Count(d => d.TaiKhoanId == dh.TaiKhoanId);
            }
            ViewBag.TrangThaiTiepTheo = DonHangService.TrangThaiTiepTheo(dh.TrangThai);
            ViewBag.TieuDeTrang = "Đơn " + dh.MaHienThi;
            return View(dh);
        }

        // POST: /DonHangAdmin/DoiTrangThai
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThai(int id, string trangThaiMoi, string quayVe)
        {
            var loi = new DonHangService(db).DoiTrangThai(id, trangThaiMoi);
            if (loi != null)
            {
                TempData["ThongBaoLoi"] = loi;
            }
            else
            {
                TempData["ThongBao"] = trangThaiMoi == TrangThaiDonHang.DaHuy
                    ? $"Đã huỷ đơn FB{id:D5}, tồn kho và lượt mã giảm giá đã được hoàn lại."
                    : $"Đơn FB{id:D5} chuyển sang \"{trangThaiMoi}\".";
            }

            if (Url.IsLocalUrl(quayVe)) return Redirect(quayVe);
            return RedirectToAction("ChiTiet", new { id });
        }

        // POST: /DonHangAdmin/DaThanhToan/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DaThanhToan(int id)
        {
            var dh = db.DonHangs.Find(id);
            if (dh == null) return HttpNotFound();

            if (dh.TrangThai == TrangThaiDonHang.DaHuy)
            {
                TempData["ThongBaoLoi"] = "Đơn đã huỷ, không thể xác nhận thanh toán.";
            }
            else
            {
                dh.DaThanhToan = true;
                dh.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
                TempData["ThongBao"] = $"Đã xác nhận đơn {dh.MaHienThi} đã thanh toán.";
            }
            return RedirectToAction("ChiTiet", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
