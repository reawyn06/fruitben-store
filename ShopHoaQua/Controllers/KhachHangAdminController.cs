using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Filters;
using ShopHoaQua.Models;

namespace ShopHoaQua.Controllers
{
    public class KhachHangDong
    {
        public TaiKhoan TaiKhoan { get; set; }
        public int SoDon { get; set; }
        public decimal TongChiTieu { get; set; }
        public DateTime? DonGanNhat { get; set; }
    }

    [QuyenAdmin]
    public class KhachHangAdminController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        // GET: /KhachHangAdmin?tuKhoa=...&sapXep=chitieu
        public ActionResult Index(string tuKhoa, string sapXep)
        {
            tuKhoa = (tuKhoa ?? "").Trim();
            var query = db.TaiKhoans.Where(t => t.VaiTro != "Admin");
            if (tuKhoa.Length > 0)
                query = query.Where(t => t.HoTen.Contains(tuKhoa) || t.Email.Contains(tuKhoa) || t.SoDienThoai.Contains(tuKhoa));

            var danhSach = query.Select(t => new KhachHangDong
            {
                TaiKhoan = t,
                SoDon = db.DonHangs.Count(d => d.TaiKhoanId == t.TaiKhoanId),
                TongChiTieu = db.DonHangs.Where(d => d.TaiKhoanId == t.TaiKhoanId && d.TrangThai != TrangThaiDonHang.DaHuy)
                                         .Sum(d => (decimal?)d.TongTien) ?? 0,
                DonGanNhat = db.DonHangs.Where(d => d.TaiKhoanId == t.TaiKhoanId).Max(d => (DateTime?)d.NgayDat)
            });

            switch (sapXep)
            {
                case "chitieu": danhSach = danhSach.OrderByDescending(x => x.TongChiTieu); break;
                case "sodon": danhSach = danhSach.OrderByDescending(x => x.SoDon); break;
                default: danhSach = danhSach.OrderByDescending(x => x.TaiKhoan.NgayDangKy); break;
            }

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.SapXep = sapXep;
            ViewBag.TieuDeTrang = "Khách hàng";
            return View(danhSach.ToList());
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
