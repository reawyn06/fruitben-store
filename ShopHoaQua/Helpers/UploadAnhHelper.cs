using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace ShopHoaQua.Helpers
{
    public static class UploadAnhHelper
    {
        public const int KICH_THUOC_TOI_DA = 2 * 1024 * 1024; // 2 MB
        private static readonly string[] DUOI_HOP_LE = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public static bool CoFile(HttpPostedFileBase file)
        {
            return file != null && file.ContentLength > 0;
        }

        public static bool HopLe(HttpPostedFileBase file, out string loi)
        {
            loi = null;
            if (!CoFile(file)) return true;

            var duoi = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
            if (!DUOI_HOP_LE.Contains(duoi) ||
                file.ContentType == null || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                loi = "Chỉ chấp nhận file ảnh .jpg, .jpeg, .png, .webp hoặc .gif.";
                return false;
            }

            if (file.ContentLength > KICH_THUOC_TOI_DA)
            {
                loi = "Ảnh không được vượt quá 2 MB.";
                return false;
            }

            return true;
        }

        public static string Luu(HttpPostedFileBase file, string thuMucVatLy, string goiYTen)
        {
            if (!CoFile(file)) return null;

            Directory.CreateDirectory(thuMucVatLy);

            var duoi = Path.GetExtension(file.FileName).ToLowerInvariant();
            var tenFile = TaoSlug(goiYTen) + "-" + Guid.NewGuid().ToString("N").Substring(0, 6) + duoi;

            file.SaveAs(Path.Combine(thuMucVatLy, tenFile));
            return tenFile;
        }

        public static string TaoSlug(string chuoi)
        {
            if (string.IsNullOrWhiteSpace(chuoi)) return "anh";

            var chuan = chuoi.Trim().ToLowerInvariant()
                             .Replace('đ', 'd')
                             .Normalize(NormalizationForm.FormD);

            var sb = new StringBuilder();
            foreach (var c in chuan)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
            }

            var slug = Regex.Replace(sb.ToString(), "[^a-z0-9]+", "-").Trim('-');
            if (slug.Length > 50) slug = slug.Substring(0, 50).Trim('-');
            return slug.Length == 0 ? "anh" : slug;
        }
    }
}
