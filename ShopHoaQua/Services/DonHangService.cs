using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using ShopHoaQua.Data;
using ShopHoaQua.Models;

namespace ShopHoaQua.Services
{
    public static class CauHinhVanChuyen
    {
        public const decimal MIEN_PHI_TU = 500000;     
        public const decimal PHI_NOI_THANH = 20000;    
        public const decimal PHI_TINH_KHAC = 35000;    
        public const string TINH_NOI_THANH = "Hồ Chí Minh";
    }

    public class KetQuaTinhTien
    {
        public decimal TamTinh { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public bool ChuaChonTinh { get; set; }
        public decimal GiamGia { get; set; }
        public decimal TongTien { get; set; }

        public MaGiamGia MaApDung { get; set; }
        public bool CoNhapMa { get; set; }
        public string ThongBaoMa { get; set; }

        public decimal ConThieuMienPhiShip { get; set; }
    }

    public class DonHangService
    {
        private readonly ShopHoaQuaContext _db;

        public DonHangService(ShopHoaQuaContext db)
        {
            _db = db;
        }

        public static decimal TinhPhiVanChuyen(decimal tamTinh, string tinhThanh)
        {
            if (tamTinh >= CauHinhVanChuyen.MIEN_PHI_TU) return 0;
            return LaNoiThanh(tinhThanh) ? CauHinhVanChuyen.PHI_NOI_THANH : CauHinhVanChuyen.PHI_TINH_KHAC;
        }

        public static bool LaNoiThanh(string tinhThanh)
        {
            return !string.IsNullOrWhiteSpace(tinhThanh) &&
                   tinhThanh.IndexOf(CauHinhVanChuyen.TINH_NOI_THANH, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string ChuanHoaMa(string ma)
        {
            return (ma ?? "").Trim().ToUpperInvariant();
        }

        public MaGiamGia TimMa(string ma)
        {
            ma = ChuanHoaMa(ma);
            if (ma.Length == 0) return null;
            return _db.MaGiamGias.FirstOrDefault(m => m.Ma == ma);
        }

        public static string KiemTraMa(MaGiamGia ma, decimal tamTinh)
        {
            if (ma == null) return "Mã giảm giá không tồn tại.";
            if (!ma.KichHoat) return "Mã giảm giá đã ngừng áp dụng.";
            if (ma.ChuaToiNgay) return "Mã giảm giá áp dụng từ ngày " + ma.NgayBatDau.Value.ToString("dd/MM/yyyy") + ".";
            if (ma.HetHan) return "Mã giảm giá đã hết hạn.";
            if (ma.HetLuot) return "Mã giảm giá đã hết lượt sử dụng.";
            if (tamTinh < ma.DonToiThieu)
                return "Đơn hàng cần tối thiểu " + ma.DonToiThieu.ToString("#,##0") + "đ để dùng mã này (còn thiếu "
                       + (ma.DonToiThieu - tamTinh).ToString("#,##0") + "đ).";
            return null;
        }

        public static decimal TinhSoTienGiam(MaGiamGia ma, decimal tamTinh)
        {
            decimal giam = ma.Loai == MaGiamGia.LOAI_PHAN_TRAM
                ? Math.Round(tamTinh * ma.GiaTri / 100 / 1000, MidpointRounding.AwayFromZero) * 1000   // làm tròn nghìn đồng
                : ma.GiaTri;

            if (ma.Loai == MaGiamGia.LOAI_PHAN_TRAM && ma.GiamToiDa.HasValue && giam > ma.GiamToiDa.Value)
                giam = ma.GiamToiDa.Value;

            return Math.Min(giam, tamTinh); 
        }

        public KetQuaTinhTien TinhTien(decimal tamTinh, string tinhThanh, string maGiamGia)
        {
            var kq = new KetQuaTinhTien
            {
                TamTinh = tamTinh,
                ChuaChonTinh = string.IsNullOrWhiteSpace(tinhThanh),
                CoNhapMa = ChuanHoaMa(maGiamGia).Length > 0
            };

            kq.PhiVanChuyen = kq.ChuaChonTinh && tamTinh < CauHinhVanChuyen.MIEN_PHI_TU
                ? 0
                : TinhPhiVanChuyen(tamTinh, tinhThanh);
            kq.ConThieuMienPhiShip = Math.Max(0, CauHinhVanChuyen.MIEN_PHI_TU - tamTinh);

            if (kq.CoNhapMa)
            {
                var ma = TimMa(maGiamGia);
                var loi = KiemTraMa(ma, tamTinh);
                if (loi == null)
                {
                    kq.MaApDung = ma;
                    kq.GiamGia = TinhSoTienGiam(ma, tamTinh);
                    kq.ThongBaoMa = "Đã áp dụng mã " + ma.Ma + ": " + ma.MoTaGiaTri + ".";
                }
                else
                {
                    kq.ThongBaoMa = loi;
                }
            }

            kq.TongTien = Math.Max(0, kq.TamTinh - kq.GiamGia + kq.PhiVanChuyen);
            return kq;
        }

        public static string[] TrangThaiTiepTheo(string hienTai)
        {
            switch (hienTai)
            {
                case TrangThaiDonHang.ChoXuLy: return new[] { TrangThaiDonHang.DangGiao, TrangThaiDonHang.DaHuy };
                case TrangThaiDonHang.DangGiao: return new[] { TrangThaiDonHang.HoanThanh, TrangThaiDonHang.DaHuy };
                default: return new string[0];
            }
        }

        public string DoiTrangThai(int donHangId, string trangThaiMoi)
        {
            var dh = _db.DonHangs.Find(donHangId);
            if (dh == null) return "Không tìm thấy đơn hàng.";
            if (!TrangThaiTiepTheo(dh.TrangThai).Contains(trangThaiMoi))
                return $"Không thể chuyển đơn từ \"{dh.TrangThai}\" sang \"{trangThaiMoi}\".";

            if (trangThaiMoi == TrangThaiDonHang.DaHuy) return HuyDon(donHangId);

            dh.TrangThai = trangThaiMoi;
            dh.NgayCapNhat = DateTime.Now;

            if (trangThaiMoi == TrangThaiDonHang.HoanThanh && dh.PhuongThucThanhToan == PhuongThucThanhToan.COD)
                dh.DaThanhToan = true;

            _db.SaveChanges();
            return null;
        }

        public string HuyDon(int donHangId)
        {
            var dh = _db.DonHangs.Include(d => d.ChiTietDonHangs).FirstOrDefault(d => d.DonHangId == donHangId);
            if (dh == null) return "Không tìm thấy đơn hàng.";
            if (dh.TrangThai != TrangThaiDonHang.ChoXuLy && dh.TrangThai != TrangThaiDonHang.DangGiao)
                return $"Đơn đang ở trạng thái \"{dh.TrangThai}\" nên không huỷ được.";

            using (var giaoDich = _db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var ct in dh.ChiTietDonHangs)
                    {
                        _db.Database.ExecuteSqlCommand(
                            "UPDATE SanPhams SET SoLuongTon = SoLuongTon + @p0 WHERE SanPhamId = @p1",
                            ct.SoLuong, ct.SanPhamId);
                    }

                    if (!string.IsNullOrEmpty(dh.MaGiamGiaApDung))
                    {
                        _db.Database.ExecuteSqlCommand(
                            "UPDATE MaGiamGias SET DaDung = DaDung - 1 WHERE Ma = @p0 AND DaDung > 0",
                            dh.MaGiamGiaApDung);
                    }

                    dh.TrangThai = TrangThaiDonHang.DaHuy;
                    dh.NgayCapNhat = DateTime.Now;
                    _db.SaveChanges();
                    giaoDich.Commit();
                }
                catch
                {
                    giaoDich.Rollback();
                    throw;
                }
            }
            return null;
        }
    }

    public static class QrThanhToan
    {
        public static string MaNganHang => ConfigurationManager.AppSettings["QR_MaNganHang"] ?? "970422";
        public static string SoTaiKhoan => ConfigurationManager.AppSettings["QR_SoTaiKhoan"] ?? "0123456789";
        public static string TenTaiKhoan => ConfigurationManager.AppSettings["QR_TenTaiKhoan"] ?? "FRUITBEN SHOP";
        public static string TenNganHang => ConfigurationManager.AppSettings["QR_TenNganHang"] ?? "MB Bank";

        public const int SO_PHUT_HIEU_LUC = 15;

        public static string TaoLinkAnh(decimal soTien, string noiDung)
        {
            return string.Format("https://img.vietqr.io/image/{0}-{1}-compact2.png?amount={2}&addInfo={3}&accountName={4}",
                MaNganHang, SoTaiKhoan, (long)soTien,
                Uri.EscapeDataString(noiDung), Uri.EscapeDataString(TenTaiKhoan));
        }
    }
}
