using System.Data;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Filters;

namespace ShopHoaQua.Controllers
{
    [QuyenAdmin]
    public class ThongKeAdoController : Controller
    {
        private readonly AdoNetHelper _ado = new AdoNetHelper();

        // GET: /ThongKeAdo
        public ActionResult Index()
        {
            DataTable danhSachSanPham = _ado.LayDanhSachSanPham();
            DataSet baoCaoDoanhThu = _ado.ThongKeDoanhThuTheoSanPham();
            ViewBag.TongSoDonHang = _ado.DemTongSoDonHang();
            ViewBag.TongDoanhThu = _ado.TinhTongDoanhThu();
            ViewBag.DanhSachSanPham = danhSachSanPham;
            return View(baoCaoDoanhThu.Tables["DoanhThu"]);
        }
    }
}
