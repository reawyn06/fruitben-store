using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using ShopHoaQua.Data;
using ShopHoaQua.Helpers;
using ShopHoaQua.Models;
using ShopHoaQua.Services;

namespace ShopHoaQua.Controllers
{
    public class DonHangController : Controller
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        private const string SESSION_DON_VUA_DAT = "DonHangVuaDat";

        private GioHang LayGioHang()
        {
            return Session[GioHangController.SESSION_KEY] as GioHang;
        }

        private bool DongBoGia(GioHang gioHang, out string loi)
        {
            loi = null;
            var dsId = gioHang.Items.Select(i => i.SanPhamId).ToList();
            var sanPhamTrongDb = db.SanPhams.Where(sp => dsId.Contains(sp.SanPhamId)).ToDictionary(sp => sp.SanPhamId);

            bool coThayDoi = false;
            foreach (var item in gioHang.Items)
            {
                SanPham sp;
                if (!sanPhamTrongDb.TryGetValue(item.SanPhamId, out sp) || !sp.ConHang || sp.SoLuongTon < item.SoLuong)
                {
                    loi = $"Rất tiếc, sản phẩm {item.TenSanPham} đã hết hoặc không đủ số lượng để giao. Vui lòng kiểm tra lại giỏ hàng.";
                    return coThayDoi;
                }
                if (item.Gia != sp.Gia)
                {
                    item.Gia = sp.Gia;
                    coThayDoi = true;
                }
                item.GiaCu = sp.DangGiamGia ? sp.GiaCu : null;
            }
            Session[GioHangController.SESSION_KEY] = gioHang;
            return coThayDoi;
        }

        private ActionResult HienFormThanhToan(DatHangViewModel model, GioHang gioHang)
        {
            ViewBag.GioHang = gioHang;
            ViewBag.TinhTien = new DonHangService(db).TinhTien(gioHang.TongTien, model.TinhThanh, model.MaGiamGia);

            var homNay = DateTime.Today;
            ViewBag.MaGoiY = db.MaGiamGias
                .Where(m => m.KichHoat
                            && (m.NgayBatDau == null || m.NgayBatDau <= homNay)
                            && (m.NgayKetThuc == null || m.NgayKetThuc >= homNay)
                            && (m.SoLuong == null || m.DaDung < m.SoLuong))
                .OrderBy(m => m.DonToiThieu).Take(4).ToList();
            return View("ThanhToan", model);
        }

        // GET: /DonHang/ThanhToan
        public ActionResult ThanhToan()
        {
            var gioHang = LayGioHang();
            if (gioHang == null || gioHang.Items.Count == 0)
            {
                TempData["ThongBaoLoi"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "GioHang");
            }

            string loi;
            DongBoGia(gioHang, out loi);
            if (loi != null)
            {
                TempData["ThongBaoLoi"] = loi;
                return RedirectToAction("Index", "GioHang");
            }
            var model = new DatHangViewModel();
            var taiKhoanId = PhienDangNhap.LayTaiKhoanId(Session);
            if (taiKhoanId != null)
            {
                var taiKhoan = db.TaiKhoans.Find(taiKhoanId.Value);
                if (taiKhoan != null)
                {
                    model.HoTenNguoiNhan = taiKhoan.HoTen;
                    model.Email = taiKhoan.Email;
                    model.SoDienThoai = taiKhoan.SoDienThoai;
                }

                var donGanNhat = db.DonHangs.Where(d => d.TaiKhoanId == taiKhoanId && d.TinhThanh != null)
                                            .OrderByDescending(d => d.NgayDat).FirstOrDefault();
                if (donGanNhat != null)
                {
                    model.SoDienThoai = model.SoDienThoai ?? donGanNhat.SoDienThoai;
                    model.TinhThanh = donGanNhat.TinhThanh;
                    model.PhuongXa = donGanNhat.PhuongXa;
                    model.DiaChiGiao = donGanNhat.DiaChiGiao;
                }
            }

            return HienFormThanhToan(model, gioHang);
        }

        // GET: /DonHang/TinhTien?tinh=...&ma=...
        [HttpGet]
        public JsonResult TinhTien(string tinh, string ma)
        {
            var gioHang = LayGioHang();
            if (gioHang == null || gioHang.Items.Count == 0)
                return Json(new { ok = false }, JsonRequestBehavior.AllowGet);

            var kq = new DonHangService(db).TinhTien(gioHang.TongTien, tinh, ma);
            return Json(new
            {
                ok = true,
                tamTinh = kq.TamTinh,
                phiVanChuyen = kq.PhiVanChuyen,
                chuaChonTinh = kq.ChuaChonTinh,
                giamGia = kq.GiamGia,
                tongTien = kq.TongTien,
                coNhapMa = kq.CoNhapMa,
                maHopLe = kq.MaApDung != null,
                thongBaoMa = kq.ThongBaoMa,
                conThieuMienPhiShip = kq.ConThieuMienPhiShip
            }, JsonRequestBehavior.AllowGet);
        }

        // POST: /DonHang/XacNhanDatHang
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanDatHang(DatHangViewModel model)
        {
            var gioHang = LayGioHang();
            if (gioHang == null || gioHang.Items.Count == 0)
            {
                return RedirectToAction("Index", "GioHang");
            }

            if (model.PhuongThucThanhToan != PhuongThucThanhToan.COD && model.PhuongThucThanhToan != PhuongThucThanhToan.QR)
                ModelState.AddModelError("PhuongThucThanhToan", "Vui lòng chọn phương thức thanh toán.");

            var service = new DonHangService(db);

            var maNhap = DonHangService.ChuanHoaMa(model.MaGiamGia);
            model.MaGiamGia = maNhap;
            if (maNhap.Length > 0)
            {
                var loiMa = DonHangService.KiemTraMa(service.TimMa(maNhap), gioHang.TongTien);
                if (loiMa != null) ModelState.AddModelError("MaGiamGia", loiMa);
            }

            if (!ModelState.IsValid)
            {
                return HienFormThanhToan(model, gioHang);
            }

            // 1. ĐỐI CHIẾU GIỎ HÀNG VỚI CSDL (giá có thể đã đổi, sản phẩm có thể đã hết)
            string loi;
            bool coGiaThayDoi = DongBoGia(gioHang, out loi);
            if (loi != null)
            {
                TempData["ThongBaoLoi"] = loi;
                return RedirectToAction("Index", "GioHang");
            }
            if (coGiaThayDoi)
            {
                TempData["ThongBaoLoi"] = "Giá một số sản phẩm vừa được cập nhật. Vui lòng kiểm tra lại tổng tiền rồi bấm Đặt hàng lần nữa.";
                return HienFormThanhToan(model, gioHang);
            }

            // 2. TÍNH TIỀN Ở SERVER (không lấy số tiền nào từ form)
            var tinhTien = service.TinhTien(gioHang.TongTien, model.TinhThanh, model.MaGiamGia);

            var donHang = new DonHang
            {
                HoTenNguoiNhan = model.HoTenNguoiNhan.Trim(),
                SoDienThoai = model.SoDienThoai.Trim(),
                Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
                TinhThanh = model.TinhThanh,
                PhuongXa = model.PhuongXa,
                DiaChiGiao = model.DiaChiGiao.Trim(),
                GhiChu = model.GhiChu,
                PhuongThucThanhToan = model.PhuongThucThanhToan,
                TaiKhoanId = PhienDangNhap.LayTaiKhoanId(Session), // null nếu khách vãng lai
                NgayDat = DateTime.Now,
                TrangThai = TrangThaiDonHang.ChoXuLy,
                TamTinh = tinhTien.TamTinh,
                PhiVanChuyen = tinhTien.PhiVanChuyen,
                GiamGia = tinhTien.GiamGia,
                MaGiamGiaApDung = tinhTien.MaApDung?.Ma,
                TongTien = tinhTien.TongTien,
                ChiTietDonHangs = new List<ChiTietDonHang>()
            };

            // 3. TRỪ KHO + TRỪ LƯỢT MÃ + LƯU ĐƠN TRONG CÙNG 1 TRANSACTION
            using (var giaoDich = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in gioHang.Items)
                    {
                        int soDong = db.Database.ExecuteSqlCommand(
                            "UPDATE SanPhams SET SoLuongTon = SoLuongTon - @p0 " +
                            "WHERE SanPhamId = @p1 AND ConHang = 1 AND SoLuongTon >= @p0",
                            item.SoLuong, item.SanPhamId);

                        if (soDong == 0)
                        {
                            giaoDich.Rollback();
                            TempData["ThongBaoLoi"] = $"Rất tiếc, sản phẩm {item.TenSanPham} vừa hết hàng. Vui lòng kiểm tra lại giỏ hàng.";
                            return RedirectToAction("Index", "GioHang");
                        }

                        donHang.ChiTietDonHangs.Add(new ChiTietDonHang
                        {
                            SanPhamId = item.SanPhamId,
                            SoLuong = item.SoLuong,
                            DonGia = item.Gia
                        });
                    }

                    if (tinhTien.MaApDung != null)
                    {
                        int soDong = db.Database.ExecuteSqlCommand(
                            "UPDATE MaGiamGias SET DaDung = DaDung + 1 " +
                            "WHERE MaGiamGiaId = @p0 AND KichHoat = 1 AND (SoLuong IS NULL OR DaDung < SoLuong)",
                            tinhTien.MaApDung.MaGiamGiaId);

                        if (soDong == 0)
                        {
                            giaoDich.Rollback();
                            ModelState.AddModelError("MaGiamGia", "Mã giảm giá vừa hết lượt sử dụng.");
                            return HienFormThanhToan(model, gioHang);
                        }
                    }

                    db.DonHangs.Add(donHang);
                    db.SaveChanges();
                    giaoDich.Commit();
                }
                catch
                {
                    giaoDich.Rollback();
                    throw;
                }
            }

            Session[GioHangController.SESSION_KEY] = new GioHang();
            Session[SESSION_DON_VUA_DAT] = donHang.DonHangId;

            return RedirectToAction("DatHangThanhCong", new { id = donHang.DonHangId });
        }

        private bool DuocXemDon(DonHang donHang)
        {
            bool laDonVuaDat = Session[SESSION_DON_VUA_DAT] is int idVuaDat && idVuaDat == donHang.DonHangId;
            var taiKhoanId = PhienDangNhap.LayTaiKhoanId(Session);
            bool laChuDon = taiKhoanId != null && donHang.TaiKhoanId == taiKhoanId;
            return laDonVuaDat || laChuDon;
        }

        private DonHang LayDonDayDu(int id)
        {
            return db.DonHangs
                .Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham))
                .FirstOrDefault(d => d.DonHangId == id);
        }

        // GET: /DonHang/DatHangThanhCong/id
        public ActionResult DatHangThanhCong(int id)
        {
            var donHang = LayDonDayDu(id);
            if (donHang == null) return RedirectToAction("Index", "Home");

            if (!DuocXemDon(donHang))
            {
                TempData["ThongBaoLoi"] = "Bạn không có quyền xem đơn hàng này.";
                return RedirectToAction("Index", "Home");
            }

            return View(donHang);
        }

        // GET: /DonHang/LichSu
        public ActionResult LichSu(string trangThai)
        {
            var taiKhoanId = PhienDangNhap.LayTaiKhoanId(Session);
            if (taiKhoanId == null)
            {
                TempData["ThongBaoLoi"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng.";
                return RedirectToAction("DangNhap", "TaiKhoan", new { returnUrl = Url.Action("LichSu") });
            }

            var query = db.DonHangs.Include(d => d.ChiTietDonHangs.Select(ct => ct.SanPham))
                                   .Where(d => d.TaiKhoanId == taiKhoanId);

            ViewBag.DemTheoTrangThai = db.DonHangs.Where(d => d.TaiKhoanId == taiKhoanId)
                .GroupBy(d => d.TrangThai)
                .Select(g => new { g.Key, Dem = g.Count() })
                .ToList()
                .ToDictionary(x => x.Key ?? "", x => x.Dem);
            ViewBag.TrangThai = trangThai;

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            return View(query.OrderByDescending(d => d.NgayDat).ToList());
        }

        // GET: /DonHang/ChiTiet/id
        public ActionResult ChiTiet(int id)
        {
            var donHang = LayDonDayDu(id);
            if (donHang == null || !DuocXemDon(donHang))
            {
                TempData["ThongBaoLoi"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn này.";
                return RedirectToAction("LichSu");
            }
            return View(donHang);
        }

        // POST: /DonHang/HuyDon/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HuyDon(int id)
        {
            var donHang = db.DonHangs.Find(id);
            if (donHang == null || !DuocXemDon(donHang))
            {
                TempData["ThongBaoLoi"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("LichSu");
            }

            if (donHang.TrangThai != TrangThaiDonHang.ChoXuLy)
            {
                TempData["ThongBaoLoi"] = "Đơn đã được cửa hàng xử lý nên không thể tự huỷ. Vui lòng liên hệ FruitBen để được hỗ trợ.";
                return RedirectToAction("ChiTiet", new { id });
            }

            var loi = new DonHangService(db).HuyDon(id);
            if (loi != null) TempData["ThongBaoLoi"] = loi;
            else TempData["ThongBaoThanhCong"] = $"Đã huỷ đơn {donHang.MaHienThi}.";

            return RedirectToAction("ChiTiet", new { id });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
