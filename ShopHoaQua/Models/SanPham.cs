using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace ShopHoaQua.Models
{
    public class SanPham
    {
        public const int SO_NGAY_HANG_MOI = 30;

        [Key]
        public int SanPhamId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(150, ErrorMessage = "Tên sản phẩm không được vượt quá 150 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string TenSanPham { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        [StringLength(50)]
        [Display(Name = "Loại sản phẩm")]
        public string Loai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        [Range(1000, 100000000, ErrorMessage = "Giá phải từ 1.000 đến 100.000.000 VNĐ")]
        [Display(Name = "Giá / Kg (VNĐ)")]
        [Column(TypeName = "decimal")]
        public decimal Gia { get; set; }

        [Range(0, 100000000, ErrorMessage = "Giá cũ phải từ 0 đến 100.000.000 VNĐ")]
        [Display(Name = "Giá cũ (trước khi giảm)")]
        [Column(TypeName = "decimal")]
        public decimal? GiaCu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng tồn kho")]
        [Display(Name = "Số lượng tồn kho")]
        [Range(0, 1000000, ErrorMessage = "Số lượng tồn kho phải từ 0 đến 1.000.000")]
        public int SoLuongTon { get; set; }

        [StringLength(300)]
        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Display(Name = "Đang bán")]
        public bool ConHang { get; set; } = true;

        [JsonIgnore]
        public virtual ICollection<HinhAnhSanPham> HinhAnhPhu { get; set; } = new List<HinhAnhSanPham>();

        [NotMapped]
        public bool DangGiamGia => GiaCu.HasValue && GiaCu.Value > Gia;

        [NotMapped]
        public int PhanTramGiam => DangGiamGia
            ? (int)Math.Round((GiaCu.Value - Gia) * 100 / GiaCu.Value)
            : 0;

        [NotMapped]
        public bool LaHangMoi => NgayTao >= DateTime.Now.AddDays(-SO_NGAY_HANG_MOI);
    }

    public class HinhAnhSanPham
    {
        [Key]
        public int HinhAnhSanPhamId { get; set; }

        public int SanPhamId { get; set; }

        [ForeignKey("SanPhamId")]
        [JsonIgnore]
        public virtual SanPham SanPham { get; set; }

        [Required]
        [StringLength(300)]
        public string TenFile { get; set; }

        public int ThuTu { get; set; }
    }
}
