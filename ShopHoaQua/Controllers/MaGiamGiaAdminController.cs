using System;
using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Filters;
using ShopHoaQua.Models;
using ShopHoaQua.Services;

namespace ShopHoaQua.Controllers
{
    [QuyenAdmin]
    public class MaGiamGiaAdminController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        // GET: /MaGiamGiaAdmin
        public ActionResult Index()
        {
            ViewBag.TieuDeTrang = "Mã giảm giá";
            return View(db.MaGiamGias.OrderByDescending(m => m.KichHoat).ThenByDescending(m => m.NgayTao).ToList());
        }

        // GET: /MaGiamGiaAdmin/Them
        public ActionResult Them()
        {
            ViewBag.TieuDeTrang = "Thêm mã giảm giá";
            return View(new MaGiamGia { Loai = MaGiamGia.LOAI_PHAN_TRAM, KichHoat = true });
        }

        // POST: /MaGiamGiaAdmin/Them
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Them(MaGiamGia model)
        {
            ChuanHoa(model);
            KiemTra(model, null);
            if (!ModelState.IsValid)
            {
                ViewBag.TieuDeTrang = "Thêm mã giảm giá";
                return View(model);
            }

            model.DaDung = 0;
            model.NgayTao = DateTime.Now;
            db.MaGiamGias.Add(model);
            db.SaveChanges();
            TempData["ThongBao"] = $"Đã tạo mã {model.Ma}.";
            return RedirectToAction("Index");
        }

        // GET: /MaGiamGiaAdmin/Sua/id
        public ActionResult Sua(int id)
        {
            var ma = db.MaGiamGias.Find(id);
            if (ma == null) return HttpNotFound();
            ViewBag.TieuDeTrang = "Sửa mã " + ma.Ma;
            return View(ma);
        }

        // POST: /MaGiamGiaAdmin/Sua/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua(MaGiamGia model)
        {
            var maTrongDb = db.MaGiamGias.Find(model.MaGiamGiaId);
            if (maTrongDb == null) return HttpNotFound();

            ChuanHoa(model);
            KiemTra(model, maTrongDb.MaGiamGiaId);
            if (model.SoLuong.HasValue && model.SoLuong.Value < maTrongDb.DaDung)
                ModelState.AddModelError("SoLuong", $"Mã đã được dùng {maTrongDb.DaDung} lần, số lượt không được nhỏ hơn.");

            if (!ModelState.IsValid)
            {
                model.DaDung = maTrongDb.DaDung;
                ViewBag.TieuDeTrang = "Sửa mã " + maTrongDb.Ma;
                return View(model);
            }

            maTrongDb.Ma = model.Ma;
            maTrongDb.MoTa = model.MoTa;
            maTrongDb.Loai = model.Loai;
            maTrongDb.GiaTri = model.GiaTri;
            maTrongDb.DonToiThieu = model.DonToiThieu;
            maTrongDb.GiamToiDa = model.GiamToiDa;
            maTrongDb.SoLuong = model.SoLuong;
            maTrongDb.NgayBatDau = model.NgayBatDau;
            maTrongDb.NgayKetThuc = model.NgayKetThuc;
            maTrongDb.KichHoat = model.KichHoat;
            db.SaveChanges();

            TempData["ThongBao"] = $"Đã cập nhật mã {maTrongDb.Ma}.";
            return RedirectToAction("Index");
        }

        // POST: /MaGiamGiaAdmin/BatTat/5 - bật/tắt nhanh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BatTat(int id)
        {
            var ma = db.MaGiamGias.Find(id);
            if (ma != null)
            {
                ma.KichHoat = !ma.KichHoat;
                db.SaveChanges();
                TempData["ThongBao"] = ma.KichHoat ? $"Đã bật mã {ma.Ma}." : $"Đã tắt mã {ma.Ma}.";
            }
            return RedirectToAction("Index");
        }

        // POST: /MaGiamGiaAdmin/Xoa/5 - mã chưa ai dùng thì xoá hẳn, đã dùng thì chỉ tắt (giữ lịch sử đơn)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Xoa(int id)
        {
            var ma = db.MaGiamGias.Find(id);
            if (ma == null) return RedirectToAction("Index");

            if (ma.DaDung > 0)
            {
                ma.KichHoat = false;
                TempData["ThongBao"] = $"Mã {ma.Ma} đã được dùng {ma.DaDung} lần nên chỉ tắt, không xoá để giữ lịch sử đơn hàng.";
            }
            else
            {
                db.MaGiamGias.Remove(ma);
                TempData["ThongBao"] = $"Đã xoá mã {ma.Ma}.";
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // ===================== HỖ TRỢ =====================

        private void ChuanHoa(MaGiamGia model)
        {
            model.Ma = DonHangService.ChuanHoaMa(model.Ma);
            ModelState.Remove("Ma"); // kiểm tra lại sau khi đã viết hoa
            TryValidateModel(model);
            if (model.Loai == MaGiamGia.LOAI_SO_TIEN) model.GiamToiDa = null;
        }

        private void KiemTra(MaGiamGia model, int? idDangSua)
        {
            if (model.Loai != MaGiamGia.LOAI_PHAN_TRAM && model.Loai != MaGiamGia.LOAI_SO_TIEN)
                ModelState.AddModelError("Loai", "Loại giảm không hợp lệ.");

            if (model.Loai == MaGiamGia.LOAI_PHAN_TRAM && model.GiaTri > 100)
                ModelState.AddModelError("GiaTri", "Giảm theo phần trăm tối đa 100%.");

            if (model.NgayBatDau.HasValue && model.NgayKetThuc.HasValue && model.NgayKetThuc < model.NgayBatDau)
                ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu.");

            if (!string.IsNullOrEmpty(model.Ma) &&
                db.MaGiamGias.Any(m => m.Ma == model.Ma && (idDangSua == null || m.MaGiamGiaId != idDangSua)))
                ModelState.AddModelError("Ma", "Mã này đã tồn tại.");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
