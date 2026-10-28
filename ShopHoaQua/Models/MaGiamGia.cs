using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopHoaQua.Models
{
    public class MaGiamGia
    {
        public const string LOAI_PHAN_TRAM = "PhanTram";
        public const string LOAI_SO_TIEN = "SoTien";

        [Key]
        public int MaGiamGiaId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "Mã dài từ 3 đến 30 ký tự")]
        [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "Mã chỉ gồm chữ IN HOA không dấu và số, không có khoảng trắng")]
        [Index(IsUnique = true)]
        [Display(Name = "Mã")]
        public string Ma { get; set; }

        [StringLength(200)]
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Loại giảm")]
        public string Loai { get; set; } = LOAI_PHAN_TRAM;

        [Range(1, 100000000, ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        [Column(TypeName = "decimal")]
        [Display(Name = "Giá trị")]
        public decimal GiaTri { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Đơn tối thiểu không được âm")]
        [Column(TypeName = "decimal")]
        [Display(Name = "Đơn tối thiểu (VNĐ)")]
        public decimal DonToiThieu { get; set; }

        [Range(0, 100000000, ErrorMessage = "Giảm tối đa không được âm")]
        [Column(TypeName = "decimal")]
        [Display(Name = "Giảm tối đa (VNĐ)")]
        public decimal? GiamToiDa { get; set; }

        [Range(1, 1000000, ErrorMessage = "Số lượt phải lớn hơn 0")]
        [Display(Name = "Số lượt sử dụng")]
        public int? SoLuong { get; set; }

        [Display(Name = "Đã dùng")]
        public int DaDung { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime? NgayBatDau { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime? NgayKetThuc { get; set; }

        [Display(Name = "Đang kích hoạt")]
        public bool KichHoat { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [NotMapped]
        public string MoTaGiaTri => Loai == LOAI_PHAN_TRAM
            ? "Giảm " + GiaTri.ToString("0.##") + "%" + (GiamToiDa.HasValue ? " (tối đa " + GiamToiDa.Value.ToString("#,##0") + "đ)" : "")
            : "Giảm " + GiaTri.ToString("#,##0") + "đ";

        [NotMapped]
        public bool HetLuot => SoLuong.HasValue && DaDung >= SoLuong.Value;

        [NotMapped]
        public bool HetHan => NgayKetThuc.HasValue && NgayKetThuc.Value.Date < DateTime.Today;

        [NotMapped]
        public bool ChuaToiNgay => NgayBatDau.HasValue && NgayBatDau.Value.Date > DateTime.Today;
    }
}
