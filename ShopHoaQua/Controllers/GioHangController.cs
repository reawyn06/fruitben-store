using ShopHoaQua.Data;
using ShopHoaQua.Models;
using System.Linq;
using System.Web.Mvc;

namespace ShopHoaQua.Controllers
{
    public class GioHangController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        public const string SESSION_KEY = "GioHang";

        private GioHang LayGioHang()
        {
            var gioHang = Session[SESSION_KEY] as GioHang;
            if (gioHang == null)
            {
                gioHang = new GioHang();
                Session[SESSION_KEY] = gioHang;
            }
            return gioHang;
        }

        private ActionResult QuayLaiTrangTruoc()
        {
            return Redirect(Request.UrlReferrer?.ToString() ?? Url.Action("Index", "SanPham"));
        }

        // GET: /GioHang 
        public ActionResult Index()
        {
            return View(LayGioHang());
        }

        // POST: /GioHang/ThemVaoGio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemVaoGio(int sanPhamId, int soLuong = 1, bool muaNgay = false)
        {
            if (soLuong < 1)
            {
                TempData["ThongBaoLoi"] = "Số lượng phải lớn hơn 0.";
                return QuayLaiTrangTruoc();
            }

            var sp = db.SanPhams.Find(sanPhamId);
            if (sp == null || !sp.ConHang)
            {
                TempData["ThongBaoLoi"] = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.";
                return QuayLaiTrangTruoc();
            }

            var gioHang = LayGioHang();

            var itemTrongGio = gioHang.Items.FirstOrDefault(i => i.SanPhamId == sanPhamId);
            int soLuongDaCo = itemTrongGio != null ? itemTrongGio.SoLuong : 0;
            int tongSoLuongYeuCau = soLuong + soLuongDaCo;
            if (sp.SoLuongTon < tongSoLuongYeuCau)
            {
                TempData["ThongBaoLoi"] = $"Rất tiếc, {sp.TenSanPham} chỉ còn {sp.SoLuongTon} sản phẩm trong kho" +
                                          (soLuongDaCo > 0 ? $" (giỏ hàng của bạn đã có {soLuongDaCo})." : ".");
                return QuayLaiTrangTruoc();
            }

            gioHang.ThemSanPham(sp, soLuong);
            Session[SESSION_KEY] = gioHang;

            if (muaNgay) return RedirectToAction("ThanhToan", "DonHang");

            TempData["ThongBaoThanhCong"] = $"Đã thêm {soLuong} kg {sp.TenSanPham} vào giỏ hàng!";
            TempData["HienNutGioHang"] = true;
            return QuayLaiTrangTruoc();
        }

        // POST: /GioHang/CapNhat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhat(int sanPhamId, int soLuong)
        {
            var gioHang = LayGioHang();

            if (soLuong > 0)
            {
                var sp = db.SanPhams.Find(sanPhamId);
                if (sp == null || !sp.ConHang || sp.SoLuongTon == 0)
                {
                    gioHang.XoaSanPham(sanPhamId);
                    Session[SESSION_KEY] = gioHang;
                    TempData["ThongBaoLoi"] = "Sản phẩm đã hết hàng hoặc ngừng kinh doanh nên đã được xoá khỏi giỏ.";
                    return RedirectToAction("Index");
                }

                if (soLuong > sp.SoLuongTon)
                {
                    TempData["ThongBaoLoi"] = $"{sp.TenSanPham} chỉ còn {sp.SoLuongTon} sản phẩm, số lượng đã được điều chỉnh về {sp.SoLuongTon}.";
                    soLuong = sp.SoLuongTon;
                }
            }
            gioHang.CapNhatSoLuong(sanPhamId, soLuong);
            Session[SESSION_KEY] = gioHang;
            return RedirectToAction("Index");
        }

        // POST: /GioHang/Xoa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int sanPhamId)
        {
            var gioHang = LayGioHang();
            gioHang.XoaSanPham(sanPhamId);
            Session[SESSION_KEY] = gioHang;
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
