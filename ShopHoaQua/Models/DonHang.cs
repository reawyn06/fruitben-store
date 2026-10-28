using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ShopHoaQua.Models
{
    public class DonHang
    {
        [Key]
        public int DonHangId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận!")]
        [StringLength(100)]
        [Display(Name = "Họ tên người nhận")]
        public string HoTenNguoiNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        /// <summary>Số nhà, tên đường (phần chi tiết của địa chỉ).</summary>
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng!")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string DiaChiGiao { get; set; }

        [StringLength(100)]
        [Display(Name = "Tỉnh / Thành phố")]
        public string TinhThanh { get; set; }

        [StringLength(150)]
        [Display(Name = "Phường / Xã")]
        public string PhuongXa { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Display(Name = "Cập nhật lần cuối")]
        public DateTime? NgayCapNhat { get; set; }

        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = TrangThaiDonHang.ChoXuLy;

        [Column(TypeName = "decimal")]
        [Display(Name = "Tạm tính")]
        public decimal TamTinh { get; set; }

        [Column(TypeName = "decimal")]
        [Display(Name = "Phí vận chuyển")]
        public decimal PhiVanChuyen { get; set; }

        [Column(TypeName = "decimal")]
        [Display(Name = "Giảm giá")]
        public decimal GiamGia { get; set; }

        [StringLength(30)]
        [Display(Name = "Mã giảm giá")]
        public string MaGiamGiaApDung { get; set; }

        [Column(TypeName = "decimal")]
        [Display(Name = "Tổng tiền")]
        public decimal TongTien { get; set; }

        [StringLength(20)]
        [Display(Name = "Thanh toán")]
        public string PhuongThucThanhToan { get; set; } = Models.PhuongThucThanhToan.COD;

        [Display(Name = "Đã thanh toán")]
        public bool DaThanhToan { get; set; }

        public int? TaiKhoanId { get; set; }

        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();

        [NotMapped]
        public string DiaChiDayDu => string.Join(", ",
            new[] { DiaChiGiao, PhuongXa, TinhThanh }.Where(x => !string.IsNullOrWhiteSpace(x)));

        [NotMapped]
        public string MaHienThi => "FB" + DonHangId.ToString("D5");

        [NotMapped]
        public string NoiDungChuyenKhoan => "FRUITBEN " + MaHienThi;

        [NotMapped]
        public string TenPhuongThucThanhToan => Models.PhuongThucThanhToan.TenHienThi(PhuongThucThanhToan);
    }

    public static class PhuongThucThanhToan
    {
        public const string COD = "COD";
        public const string QR = "QR";

        public static string TenHienThi(string ma)
        {
            return ma == QR ? "Chuyển khoản QR" : "Thanh toán khi nhận hàng (COD)";
        }
    }

    public class DatHangViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận!")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string HoTenNguoiNhan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại!")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải đủ 10 chữ số và bắt đầu bằng số 0!")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng (ví dụ: abc@gmail.com)!")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tỉnh / thành phố!")]
        [StringLength(100)]
        [Display(Name = "Tỉnh / Thành phố")]
        public string TinhThanh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phường / xã!")]
        [StringLength(150)]
        [Display(Name = "Phường / Xã")]
        public string PhuongXa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số nhà, tên đường!")]
        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string DiaChiGiao { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        [StringLength(30)]
        [Display(Name = "Mã giảm giá")]
        public string MaGiamGia { get; set; }

        [Required]
        [Display(Name = "Phương thức thanh toán")]
        public string PhuongThucThanhToan { get; set; } = Models.PhuongThucThanhToan.COD;
    }
    public class ChiTietDonHang
    {
        [Key]
        public int ChiTietDonHangId { get; set; }

        public int DonHangId { get; set; }
        [ForeignKey("DonHangId")]
        public virtual DonHang DonHang { get; set; }

        public int SanPhamId { get; set; }
        [ForeignKey("SanPhamId")]
        public virtual SanPham SanPham { get; set; }

        [Range(1, 1000)]
        public int SoLuong { get; set; }

        [Column(TypeName = "decimal")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [NotMapped]
        public decimal ThanhTien => SoLuong * DonGia;
    }

    [Serializable]
    public class GioHangItem
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal Gia { get; set; }
        public decimal? GiaCu { get; set; }
        public string Loai { get; set; }
        public int SoLuong { get; set; }

        [NotMapped]
        public decimal ThanhTien => Gia * SoLuong;
    }

    [Serializable]
    public class GioHang
    {
        public List<GioHangItem> Items { get; set; } = new List<GioHangItem>();

        public void ThemSanPham(SanPham sp, int soLuong)
        {
            var item = Items.FirstOrDefault(i => i.SanPhamId == sp.SanPhamId);
            if (item != null)
            {
                item.SoLuong += soLuong;
            }
            else
            {
                Items.Add(new GioHangItem
                {
                    SanPhamId = sp.SanPhamId,
                    TenSanPham = sp.TenSanPham,
                    HinhAnh = sp.HinhAnh,
                    Gia = sp.Gia,
                    GiaCu = sp.DangGiamGia ? sp.GiaCu : null,
                    Loai = sp.Loai,
                    SoLuong = soLuong
                });
            }
        }

        public void CapNhatSoLuong(int sanPhamId, int soLuong)
        {
            var item = Items.FirstOrDefault(i => i.SanPhamId == sanPhamId);
            if (item != null)
            {
                if (soLuong <= 0) Items.Remove(item);
                else item.SoLuong = soLuong;
            }
        }

        public void XoaSanPham(int sanPhamId)
        {
            Items.RemoveAll(i => i.SanPhamId == sanPhamId);
        }

        public void XoaTatCa() => Items.Clear();

        public decimal TongTien => Items.Sum(i => i.ThanhTien);

        public int TongSoLuong => Items.Sum(i => i.SoLuong);
    }
}
