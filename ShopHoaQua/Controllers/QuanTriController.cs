using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Filters;
using ShopHoaQua.Models;

namespace ShopHoaQua.Controllers
{
    public class DoanhThuNgay
    {
        public DateTime Ngay { get; set; }
        public decimal Tien { get; set; }
        public int SoDon { get; set; }
    }

    public class SanPhamBanChay
    {
        public SanPham SanPham { get; set; }
        public int SoLuong { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class TongQuanViewModel
    {
        public decimal DoanhThu { get; set; }
        public decimal DoanhThuThangNay { get; set; }
        public int SoDon { get; set; }
        public int SoDonChoXuLy { get; set; }
        public int SoDonDangGiao { get; set; }
        public int SoSanPhamDangBan { get; set; }
        public int SoSapHetHang { get; set; }
        public int SoKhachHang { get; set; }
        public int SoKhachMoiThangNay { get; set; }
        public int SoLienHeChuaXuLy { get; set; }
        public List<DoanhThuNgay> DoanhThu7Ngay { get; set; }
        public List<SanPhamBanChay> BanChay { get; set; }
        public List<SanPham> SapHetHang { get; set; }
        public List<DonHang> DonMoi { get; set; }
        public List<TaiKhoan> KhachMoi { get; set; }
    }

    [QuyenAdmin]
    public class QuanTriController : Controller
    {
        public const int NGUONG_SAP_HET_HANG = 10;

        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        // GET: /QuanTri
        public ActionResult Index()
        {
            var homNay = DateTime.Today;
            var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
            var tu7Ngay = homNay.AddDays(-6);

            var donHopLe = db.DonHangs.Where(d => d.TrangThai != TrangThaiDonHang.DaHuy);

            var theoNgay = donHopLe.Where(d => d.NgayDat >= tu7Ngay)
                .GroupBy(d => DbFunctions.TruncateTime(d.NgayDat))
                .Select(g => new { Ngay = g.Key, Tien = g.Sum(d => d.TongTien), SoDon = g.Count() })
                .ToList();
            var doanhThu7Ngay = Enumerable.Range(0, 7).Select(i =>
            {
                var ngay = tu7Ngay.AddDays(i);
                var dong = theoNgay.FirstOrDefault(x => x.Ngay == ngay);
                return new DoanhThuNgay { Ngay = ngay, Tien = dong?.Tien ?? 0, SoDon = dong?.SoDon ?? 0 };
            }).ToList();

            var banChayRaw = db.ChiTietDonHangs
                .Where(ct => ct.DonHang.TrangThai != TrangThaiDonHang.DaHuy)
                .GroupBy(ct => ct.SanPhamId)
                .Select(g => new { SanPhamId = g.Key, SoLuong = g.Sum(ct => ct.SoLuong), DoanhThu = g.Sum(ct => ct.SoLuong * ct.DonGia) })
                .OrderByDescending(x => x.SoLuong)
                .Take(5).ToList();
            var idBanChay = banChayRaw.Select(x => x.SanPhamId).ToList();
            var spBanChay = db.SanPhams.Where(sp => idBanChay.Contains(sp.SanPhamId)).ToDictionary(sp => sp.SanPhamId);

            var model = new TongQuanViewModel
            {
                DoanhThu = donHopLe.Sum(d => (decimal?)d.TongTien) ?? 0,
                DoanhThuThangNay = donHopLe.Where(d => d.NgayDat >= dauThang).Sum(d => (decimal?)d.TongTien) ?? 0,
                SoDon = db.DonHangs.Count(),
                SoDonChoXuLy = db.DonHangs.Count(d => d.TrangThai == TrangThaiDonHang.ChoXuLy),
                SoDonDangGiao = db.DonHangs.Count(d => d.TrangThai == TrangThaiDonHang.DangGiao),
                SoSanPhamDangBan = db.SanPhams.Count(sp => sp.ConHang),
                SoSapHetHang = db.SanPhams.Count(sp => sp.ConHang && sp.SoLuongTon <= NGUONG_SAP_HET_HANG),
                SoKhachHang = db.TaiKhoans.Count(t => t.VaiTro != "Admin"),
                SoKhachMoiThangNay = db.TaiKhoans.Count(t => t.VaiTro != "Admin" && t.NgayDangKy >= dauThang),
                SoLienHeChuaXuLy = db.LienHes.Count(l => !l.DaXuLy),
                DoanhThu7Ngay = doanhThu7Ngay,
                BanChay = banChayRaw.Where(x => spBanChay.ContainsKey(x.SanPhamId))
                    .Select(x => new SanPhamBanChay { SanPham = spBanChay[x.SanPhamId], SoLuong = x.SoLuong, DoanhThu = x.DoanhThu })
                    .ToList(),
                SapHetHang = db.SanPhams.Where(sp => sp.ConHang && sp.SoLuongTon <= NGUONG_SAP_HET_HANG)
                    .OrderBy(sp => sp.SoLuongTon).Take(6).ToList(),
                DonMoi = db.DonHangs.OrderByDescending(d => d.NgayDat).Take(6).ToList(),
                KhachMoi = db.TaiKhoans.Where(t => t.VaiTro != "Admin").OrderByDescending(t => t.NgayDangKy).Take(5).ToList()
            };

            ViewBag.TieuDeTrang = "Tổng quan";
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult DemDonChoXuLy()
        {
            var so = db.DonHangs.Count(d => d.TrangThai == TrangThaiDonHang.ChoXuLy);
            return Content(so > 0 ? so.ToString() : "");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
