using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Models;

namespace ShopHoaQua.Controllers
{
    public class TrangChuViewModel
    {
        public List<SanPham> NoiBat { get; set; }
        public List<SanPham> DangGiam { get; set; }
        public List<TinTuc> TinMoi { get; set; }
        public int SoNoiDia { get; set; }
        public int SoNhapKhau { get; set; }
        public int SoDangGiam { get; set; }
        public int SoSanPham { get; set; }
        public int SoKhachHang { get; set; }
        public int SoDonHoanThanh { get; set; }
        public MaGiamGia MaNoiBat { get; set; }
    }

    public class HomeController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        public const string LOAI_NOI_DIA = "Trái cây nội địa";
        public const string LOAI_NHAP_KHAU = "Trái cây nhập khẩu";

        // GET: / 
        public ActionResult Index()
        {
            ViewBag.TieuDeTrang = "Trang chủ";
            var dangBan = db.SanPhams.Where(sp => sp.ConHang);
            var noiBat = dangBan
                .OrderByDescending(sp => db.ChiTietDonHangs
                    .Where(ct => ct.SanPhamId == sp.SanPhamId && ct.DonHang.TrangThai != TrangThaiDonHang.DaHuy)
                    .Sum(ct => (int?)ct.SoLuong) ?? 0)
                .ThenBy(sp => sp.SanPhamId)
                .Take(8).ToList();

            var model = new TrangChuViewModel
            {
                NoiBat = noiBat,
                DangGiam = dangBan.Where(sp => sp.GiaCu != null && sp.GiaCu > sp.Gia)
                                  .OrderByDescending(sp => (sp.GiaCu - sp.Gia) / sp.GiaCu).Take(4).ToList(),
                TinMoi = db.TinTucs.OrderByDescending(t => t.NgayDang).Take(3).ToList(),
                SoNoiDia = dangBan.Count(sp => sp.Loai == LOAI_NOI_DIA),
                SoNhapKhau = dangBan.Count(sp => sp.Loai == LOAI_NHAP_KHAU),
                SoDangGiam = dangBan.Count(sp => sp.GiaCu != null && sp.GiaCu > sp.Gia),
                SoSanPham = dangBan.Count(),
                SoKhachHang = db.TaiKhoans.Count(t => t.VaiTro != "Admin"),
                SoDonHoanThanh = db.DonHangs.Count(d => d.TrangThai == TrangThaiDonHang.HoanThanh)
            };

            var homNay = System.DateTime.Today;
            model.MaNoiBat = db.MaGiamGias
                .Where(m => m.KichHoat
                            && (m.NgayBatDau == null || m.NgayBatDau <= homNay)
                            && (m.NgayKetThuc == null || m.NgayKetThuc >= homNay)
                            && (m.SoLuong == null || m.DaDung < m.SoLuong))
                .OrderBy(m => m.DonToiThieu).FirstOrDefault();

            return View(model);
        }

        // GET: /Home/GioiThieu  
        public ActionResult GioiThieu()
        {
            ViewBag.TieuDeTrang = "Giới thiệu";
            ViewBag.SoSanPham = db.SanPhams.Count(sp => sp.ConHang);
            return View();
        }
        // GET: /Home/VanChuyen
        public ActionResult VanChuyen()
        {
            ViewBag.TieuDeTrang = "Chính sách vận chuyển";
            return View();
        }

        // GET: /Home/DoiTra
        public ActionResult DoiTra()
        {
            ViewBag.TieuDeTrang = "Chính sách đổi trả";
            return View();
        }

        // GET: /Home/ThanhToan
        public ActionResult ThanhToan()
        {
            ViewBag.TieuDeTrang = "Hướng dẫn thanh toán";
            return View();
        }

        // GET: /Home/Trang404
        public ActionResult Trang404()
        {
            Response.StatusCode = 404;              
            Response.TrySkipIisCustomErrors = true;  
            ViewBag.TieuDeTrang = "Không tìm thấy trang";
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
