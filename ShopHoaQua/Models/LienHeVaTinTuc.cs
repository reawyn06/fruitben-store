using System;
using System.ComponentModel.DataAnnotations;

namespace ShopHoaQua.Models
{
    public class LienHe
    {
        [Key]
        public int LienHeId { get; set; }

        // 1. Bắt buộc nhập Họ tên
        [Required(ErrorMessage = "Vui lòng nhập Họ và Tên!")]
        [Display(Name = "Họ và Tên")]
        public string HoTen { get; set; }

        // 2. Bắt buộc nhập Email & Kiểm tra định dạng Email (vd: abc@gmail.com)
        [Required(ErrorMessage = "Vui lòng nhập Email!")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng (ví dụ: abc@gmail.com)!")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // 3. Bắt buộc nhập SĐT & Phải đúng 10 chữ số, bắt đầu bằng số 0
        [Required(ErrorMessage = "Vui lòng nhập Số điện thoại!")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại phải đủ 10 chữ số và bắt đầu bằng số 0!")]
        [Display(Name = "Số Điện Thoại")]
        public string SoDienThoai { get; set; }

        // 4. Bắt buộc nhập Chủ đề
        [Required(ErrorMessage = "Vui lòng nhập Chủ đề!")]
        [Display(Name = "Chủ Đề")]
        public string ChuDe { get; set; }

        // 5. Bắt buộc nhập Tin nhắn
        [Required(ErrorMessage = "Vui lòng nhập nội dung Tin nhắn!")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Tin Nhắn")]
        public string TinNhan { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;

        public bool DaXuLy { get; set; } = false;
    }

    public class TinTuc
    {
        [Key]
        public int TinTucId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string TieuDe { get; set; }

        [StringLength(300)]
        [Display(Name = "Hình ảnh")]
        public string HinhAnh { get; set; }

        [Display(Name = "Tóm tắt")]
        public string TomTat { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; }

        [Display(Name = "Người đăng")]
        public string NguoiDang { get; set; } = "Admin";

        [Display(Name = "Ngày đăng")]
        public DateTime NgayDang { get; set; } = DateTime.Now;
    }
}
