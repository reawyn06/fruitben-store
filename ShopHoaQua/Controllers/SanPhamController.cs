using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Models;

namespace ShopHoaQua.Controllers
{
    public class LocSanPhamViewModel
    {
        public string Loai { get; set; }
        public string TuKhoa { get; set; }
        public decimal? GiaToiDa { get; set; }
        public string SapXep { get; set; }
        public bool GiamGia { get; set; }
        public int Trang { get; set; } = 1;

        public List<SanPham> DanhSach { get; set; } = new List<SanPham>();
        public int TongSo { get; set; }
        public int TongSoTrang { get; set; }
        public decimal GiaCaoNhat { get; set; }
        public List<string> DanhSachLoai { get; set; } = new List<string>();

        public bool DangLoc => !string.IsNullOrEmpty(Loai) || !string.IsNullOrEmpty(TuKhoa) || GiaToiDa.HasValue || GiamGia
                               || (!string.IsNullOrEmpty(SapXep) && SapXep != "macdinh");

        public static readonly Dictionary<string, string> CACH_SAP_XEP = new Dictionary<string, string>
        {
            { "macdinh", "Mặc định" },
            { "moinhat", "Mới nhất" },
            { "banchay", "Bán chạy" },
            { "giatang", "Giá thấp đến cao" },
            { "giagiam", "Giá cao đến thấp" },
            { "giamnhieu", "Giảm giá nhiều" }
        };
    }

    public class SanPhamController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();
        private const int SO_SAN_PHAM_MOI_TRANG = 9;

        // GET: /SanPham
        public ActionResult Index(LocSanPhamViewModel loc)
        {
            loc = loc ?? new LocSanPhamViewModel();
            if (loc.Trang < 1) loc.Trang = 1; 
            loc.TuKhoa = (loc.TuKhoa ?? "").Trim();

            var dangBan = db.SanPhams.Where(sp => sp.ConHang);
            loc.DanhSachLoai = dangBan.Select(sp => sp.Loai).Distinct().OrderBy(l => l).ToList();
            loc.GiaCaoNhat = dangBan.Select(sp => (decimal?)sp.Gia).Max() ?? 0;

            IQueryable<SanPham> query = dangBan;

            if (!string.IsNullOrEmpty(loc.Loai))
            {
                string loaiChuan = loc.Loai.Trim().ToLower();
                query = query.Where(sp => sp.Loai.Trim().ToLower() == loaiChuan);
            }

            if (loc.TuKhoa.Length > 0)
            {
                var k = loc.TuKhoa;
                query = query.Where(sp => sp.TenSanPham.Contains(k) || sp.Loai.Contains(k) || sp.MoTa.Contains(k));
            }

            if (loc.GiaToiDa.HasValue && loc.GiaToiDa.Value < loc.GiaCaoNhat)
            {
                var max = loc.GiaToiDa.Value;
                query = query.Where(sp => sp.Gia <= max);
            }
            else
            {
                loc.GiaToiDa = null; 
            }

            if (loc.GiamGia)
                query = query.Where(sp => sp.GiaCu != null && sp.GiaCu > sp.Gia);

            switch (loc.SapXep)
            {
                case "moinhat": query = query.OrderByDescending(sp => sp.NgayTao).ThenByDescending(sp => sp.SanPhamId); break;
                case "giatang": query = query.OrderBy(sp => sp.Gia); break;
                case "giagiam": query = query.OrderByDescending(sp => sp.Gia); break;
                case "giamnhieu": query = query.OrderByDescending(sp => sp.GiaCu != null && sp.GiaCu > sp.Gia ? (sp.GiaCu - sp.Gia) / sp.GiaCu : 0); break;
                case "banchay":
                    query = query.OrderByDescending(sp => db.ChiTietDonHangs
                                    .Where(ct => ct.SanPhamId == sp.SanPhamId && ct.DonHang.TrangThai != TrangThaiDonHang.DaHuy)
                                    .Sum(ct => (int?)ct.SoLuong) ?? 0)
                                 .ThenBy(sp => sp.SanPhamId);
                    break;
                default:
                    query = loc.TuKhoa.Length > 0
                        ? query.OrderByDescending(sp => sp.TenSanPham.Contains(loc.TuKhoa)).ThenBy(sp => sp.SanPhamId)
                        : query.OrderBy(sp => sp.SanPhamId);
                    break;
            }

            loc.TongSo = query.Count();
            loc.TongSoTrang = (int)Math.Ceiling(loc.TongSo / (double)SO_SAN_PHAM_MOI_TRANG);
            if (loc.TongSoTrang > 0 && loc.Trang > loc.TongSoTrang) loc.Trang = loc.TongSoTrang;

            loc.DanhSach = query.Skip((loc.Trang - 1) * SO_SAN_PHAM_MOI_TRANG).Take(SO_SAN_PHAM_MOI_TRANG).ToList();
            if (!loc.DangLoc && loc.Trang == 1)
            {
                ViewBag.HangMoi = dangBan.OrderByDescending(sp => sp.NgayTao).ThenByDescending(sp => sp.SanPhamId).Take(10).ToList();
                ViewBag.DangGiam = dangBan.Where(sp => sp.GiaCu != null && sp.GiaCu > sp.Gia)
                                         .OrderByDescending(sp => (sp.GiaCu - sp.Gia) / sp.GiaCu).Take(6).ToList();
            }

            ViewBag.TieuDeTrang = loc.TuKhoa.Length > 0 ? "Tìm kiếm: " + loc.TuKhoa : (loc.Loai ?? "Cửa hàng");
            return View(loc);
        }

        // GET: /SanPham/ChiTiet/id
        public ActionResult ChiTiet(int id)
        {
            var sp = db.SanPhams.Include(x => x.HinhAnhPhu).FirstOrDefault(x => x.SanPhamId == id);
            if (sp == null) return RedirectToAction("Trang404", "Home");
            ViewBag.SanPhamLienQuan = db.SanPhams
                .Where(x => x.Loai == sp.Loai && x.SanPhamId != sp.SanPhamId && x.ConHang)
                .OrderByDescending(x => x.GiaCu != null && x.GiaCu > x.Gia)
                .ThenBy(x => Guid.NewGuid())
                .Take(4).ToList();

            ViewBag.DaBan = db.ChiTietDonHangs
                .Where(ct => ct.SanPhamId == id && ct.DonHang.TrangThai != TrangThaiDonHang.DaHuy)
                .Sum(ct => (int?)ct.SoLuong) ?? 0;

            ViewBag.TieuDeTrang = sp.TenSanPham;
            return View(sp);
        }

        // GET: /SanPham/TimKiem?tuKhoa=dâu  
        public ActionResult TimKiem(string tuKhoa)
        {
            return RedirectToAction("Index", new { tuKhoa });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
