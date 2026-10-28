using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    public class SanPhamAdminController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        private const string THU_MUC_ANH = "~/assets/img/products/";
        private static readonly string[] LOAI_MAC_DINH = { "Trái cây nội địa", "Trái cây nhập khẩu" };

        // GET: /SanPhamAdmin?tuKhoa=...&loai=...&tinhTrang=dangban|ngung|saphet|giamgia
        public ActionResult Index(string tuKhoa, string loai, string tinhTrang)
        {
            tuKhoa = (tuKhoa ?? "").Trim();
            IQueryable<SanPham> query = db.SanPhams;

            if (tuKhoa.Length > 0) query = query.Where(sp => sp.TenSanPham.Contains(tuKhoa));
            if (!string.IsNullOrEmpty(loai)) query = query.Where(sp => sp.Loai == loai);

            int nguong = QuanTriController.NGUONG_SAP_HET_HANG;
            switch (tinhTrang)
            {
                case "dangban": query = query.Where(sp => sp.ConHang); break;
                case "ngung": query = query.Where(sp => !sp.ConHang); break;
                case "saphet": query = query.Where(sp => sp.ConHang && sp.SoLuongTon <= nguong); break;
                case "giamgia": query = query.Where(sp => sp.GiaCu != null && sp.GiaCu > sp.Gia); break;
            }

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.Loai = loai;
            ViewBag.TinhTrang = tinhTrang;
            ViewBag.DanhSachLoai = db.SanPhams.Select(sp => sp.Loai).Distinct().OrderBy(l => l).ToList();
            ViewBag.SoDangBan = db.SanPhams.Count(sp => sp.ConHang);
            ViewBag.SoNgung = db.SanPhams.Count(sp => !sp.ConHang);
            ViewBag.SoSapHet = db.SanPhams.Count(sp => sp.ConHang && sp.SoLuongTon <= nguong);
            ViewBag.SoGiamGia = db.SanPhams.Count(sp => sp.GiaCu != null && sp.GiaCu > sp.Gia);
            ViewBag.TieuDeTrang = "Sản phẩm";

            return View(query.OrderByDescending(sp => sp.NgayTao).ThenByDescending(sp => sp.SanPhamId).ToList());
        }

        // GET: /SanPhamAdmin/Them
        public ActionResult Them()
        {
            NapDanhSachLoai(null);
            return View(new SanPham { SoLuongTon = 10, ConHang = true });
        }

        // POST: /SanPhamAdmin/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(SanPham sanPham, HttpPostedFileBase fileHinhAnh, IEnumerable<HttpPostedFileBase> fileAnhPhu)
        {
            KiemTraFileAnh(fileHinhAnh);
            KiemTraAnhPhu(fileAnhPhu);
            KiemTraGiaCu(sanPham);

            if (ModelState.IsValid)
            {
                var tenFile = LuuFileAnh(fileHinhAnh, sanPham.TenSanPham);
                if (tenFile != null) sanPham.HinhAnh = tenFile;

                sanPham.NgayTao = DateTime.Now;
                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                LuuAnhPhu(sanPham, fileAnhPhu);
                TempData["ThongBao"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            NapDanhSachLoai(sanPham.Loai);
            return View(sanPham);
        }

        // GET: /SanPhamAdmin/Sua/id
        public ActionResult Sua(int id)
        {
            var sp = db.SanPhams.Include(x => x.HinhAnhPhu).FirstOrDefault(x => x.SanPhamId == id);
            if (sp == null) return HttpNotFound();

            NapDanhSachLoai(sp.Loai);
            return View(sp);
        }

        // POST: /SanPhamAdmin/Sua/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(SanPham sanPham, HttpPostedFileBase fileHinhAnh, IEnumerable<HttpPostedFileBase> fileAnhPhu, int[] xoaAnhPhu)
        {
            var spTrongDb = db.SanPhams.Include(sp => sp.HinhAnhPhu).FirstOrDefault(sp => sp.SanPhamId == sanPham.SanPhamId);
            if (spTrongDb == null) return HttpNotFound();

            KiemTraFileAnh(fileHinhAnh);
            KiemTraAnhPhu(fileAnhPhu);
            KiemTraGiaCu(sanPham);

            if (ModelState.IsValid)
            {
                spTrongDb.TenSanPham = sanPham.TenSanPham;
                spTrongDb.Loai = sanPham.Loai;
                spTrongDb.Gia = sanPham.Gia;
                spTrongDb.GiaCu = sanPham.GiaCu;
                spTrongDb.SoLuongTon = sanPham.SoLuongTon;
                spTrongDb.MoTa = sanPham.MoTa;
                spTrongDb.ConHang = sanPham.ConHang;
                var tenFile = LuuFileAnh(fileHinhAnh, sanPham.TenSanPham);
                spTrongDb.HinhAnh = tenFile ?? sanPham.HinhAnh;
                if (xoaAnhPhu != null)
                {
                    foreach (var anh in spTrongDb.HinhAnhPhu.Where(a => xoaAnhPhu.Contains(a.HinhAnhSanPhamId)).ToList())
                        db.HinhAnhSanPhams.Remove(anh);
                }

                db.SaveChanges();
                LuuAnhPhu(spTrongDb, fileAnhPhu);
                TempData["ThongBao"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }

            sanPham.HinhAnhPhu = spTrongDb.HinhAnhPhu.OrderBy(a => a.ThuTu).ToList();
            NapDanhSachLoai(sanPham.Loai);
            return View(sanPham);
        }

        // GET: /SanPhamAdmin/Xoa/id 
        public ActionResult Xoa(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();
            return View(sp);
        }

        // POST: /SanPhamAdmin/Xoa/id
        [HttpPost, ActionName("Xoa")]
        [ValidateAntiForgeryToken]
        public ActionResult XoaXacNhan(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp != null)
            {
                sp.ConHang = false;
                db.SaveChanges();
                TempData["ThongBao"] = $"Đã ngừng kinh doanh sản phẩm \"{sp.TenSanPham}\".";
            }
            return RedirectToAction("Index");
        }

        // ===================== HÀM HỖ TRỢ =====================
        private void NapDanhSachLoai(string loaiDangChon)
        {
            var danhSach = db.SanPhams.Select(sp => sp.Loai).Distinct().ToList();
            foreach (var loai in LOAI_MAC_DINH)
            {
                if (!danhSach.Contains(loai)) danhSach.Add(loai);
            }
            ViewBag.DanhSachLoai = new SelectList(danhSach.OrderBy(l => l), loaiDangChon);
        }

        private void KiemTraFileAnh(HttpPostedFileBase file)
        {
            string loi;
            if (!UploadAnhHelper.HopLe(file, out loi))
            {
                ModelState.AddModelError("fileHinhAnh", loi);
            }
        }

        private void KiemTraGiaCu(SanPham sanPham)
        {
            if (sanPham.GiaCu.HasValue && sanPham.GiaCu.Value == 0) sanPham.GiaCu = null;
            if (sanPham.GiaCu.HasValue && sanPham.GiaCu.Value <= sanPham.Gia)
                ModelState.AddModelError("GiaCu", "Giá cũ phải lớn hơn giá bán. Để trống nếu sản phẩm không giảm giá.");
        }

        private const int SO_ANH_PHU_TOI_DA = 6;

        private void KiemTraAnhPhu(IEnumerable<HttpPostedFileBase> files)
        {
            var coFile = (files ?? Enumerable.Empty<HttpPostedFileBase>()).Where(UploadAnhHelper.CoFile).ToList();
            if (coFile.Count > SO_ANH_PHU_TOI_DA)
                ModelState.AddModelError("fileAnhPhu", $"Mỗi lần chỉ tải tối đa {SO_ANH_PHU_TOI_DA} ảnh phụ.");
            foreach (var f in coFile)
            {
                string loi;
                if (!UploadAnhHelper.HopLe(f, out loi)) { ModelState.AddModelError("fileAnhPhu", f.FileName + ": " + loi); break; }
            }
        }

        private void LuuAnhPhu(SanPham sanPham, IEnumerable<HttpPostedFileBase> files)
        {
            var coFile = (files ?? Enumerable.Empty<HttpPostedFileBase>()).Where(UploadAnhHelper.CoFile).ToList();
            if (!coFile.Any()) return;

            int thuTu = db.HinhAnhSanPhams.Where(a => a.SanPhamId == sanPham.SanPhamId).Select(a => (int?)a.ThuTu).Max() ?? 0;
            foreach (var f in coFile)
            {
                db.HinhAnhSanPhams.Add(new HinhAnhSanPham
                {
                    SanPhamId = sanPham.SanPhamId,
                    TenFile = LuuFileAnh(f, sanPham.TenSanPham),
                    ThuTu = ++thuTu
                });
            }
            db.SaveChanges();
        }

        private string LuuFileAnh(HttpPostedFileBase file, string tenSanPham)
        {
            return UploadAnhHelper.Luu(file, Server.MapPath(THU_MUC_ANH), tenSanPham);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
