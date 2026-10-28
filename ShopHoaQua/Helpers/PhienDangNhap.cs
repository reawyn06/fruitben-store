using System;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using ShopHoaQua.Data;
using ShopHoaQua.Models;

namespace ShopHoaQua.Helpers
{
    public static class PhienDangNhap
    {
        public const string KEY_TAI_KHOAN_ID = "TaiKhoanId";
        public const string KEY_HO_TEN = "HoTen";
        public const string KEY_VAI_TRO = "VaiTro";
        public const string COOKIE_GHI_NHO = "FruitBen.GhiNho";

        public const string VAI_TRO_ADMIN = "Admin";
        public const string VAI_TRO_KHACH_HANG = "KhachHang";

        private const int SO_NGAY_GHI_NHO = 30;

        // ===================== SESSION =====================
        public static int? LayTaiKhoanId(HttpSessionStateBase session)
        {
            if (session != null && session[KEY_TAI_KHOAN_ID] is int id && id > 0)
                return id;
            return null;
        }

        public static bool DaDangNhap(HttpSessionStateBase session)
        {
            return LayTaiKhoanId(session) != null;
        }

        public static bool LaAdmin(HttpSessionStateBase session)
        {
            return session != null && (session[KEY_VAI_TRO] as string) == VAI_TRO_ADMIN;
        }

        public static void GhiSession(HttpSessionStateBase session, TaiKhoan taiKhoan)
        {
            session[KEY_TAI_KHOAN_ID] = taiKhoan.TaiKhoanId;
            session[KEY_HO_TEN] = taiKhoan.HoTen;
            session[KEY_VAI_TRO] = taiKhoan.VaiTro;
        }

        // ===================== COOKIE GHI NHỚ =====================
        public static void GhiCookieGhiNho(HttpResponseBase response, TaiKhoan taiKhoan)
        {
            var hetHan = DateTime.Now.AddDays(SO_NGAY_GHI_NHO);
            var ticket = new FormsAuthenticationTicket(
                1,                                   
                taiKhoan.Email,                      
                DateTime.Now,                       
                hetHan,                             
                true,                               
                taiKhoan.TaiKhoanId + "|" + TaoDauVanTay(taiKhoan.MatKhau)); 

            var cookie = new HttpCookie(COOKIE_GHI_NHO, FormsAuthentication.Encrypt(ticket))
            {
                Expires = hetHan,
                HttpOnly = true 
            };
            response.Cookies.Add(cookie);
        }

        public static void XoaCookieGhiNho(HttpResponseBase response)
        {
            response.Cookies.Add(new HttpCookie(COOKIE_GHI_NHO, "")
            {
                Expires = DateTime.Now.AddDays(-1),
                HttpOnly = true
            });
        }

        public static TaiKhoan DocCookieGhiNho(HttpContextBase httpContext, ShopHoaQuaContext db)
        {
            var cookie = httpContext.Request.Cookies[COOKIE_GHI_NHO];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value)) return null;

            FormsAuthenticationTicket ticket = null;
            try
            {
                ticket = FormsAuthentication.Decrypt(cookie.Value);
            }
            catch (Exception) { }

            TaiKhoan taiKhoan = null;
            if (ticket != null && !ticket.Expired && !string.IsNullOrEmpty(ticket.UserData))
            {
                var phan = ticket.UserData.Split('|');
                int taiKhoanId;
                if (phan.Length == 2 && int.TryParse(phan[0], out taiKhoanId))
                {
                    var tk = db.TaiKhoans.Find(taiKhoanId);
                    if (tk != null && tk.Email == ticket.Name && TaoDauVanTay(tk.MatKhau) == phan[1])
                        taiKhoan = tk;
                }
            }

            if (taiKhoan == null) XoaCookieGhiNho(httpContext.Response);
            return taiKhoan;
        }

        public static void KhoiPhucNeuCan(HttpContextBase httpContext)
        {
            var session = httpContext.Session;
            if (session == null || DaDangNhap(session)) return;
            if (httpContext.Request.Cookies[COOKIE_GHI_NHO] == null) return;

            using (var db = new ShopHoaQuaContext())
            {
                var taiKhoan = DocCookieGhiNho(httpContext, db);
                if (taiKhoan != null) GhiSession(session, taiKhoan);
            }
        }

        private static string TaoDauVanTay(string matKhauDaLuu)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(matKhauDaLuu ?? ""));
                return BitConverter.ToString(hash, 0, 8).Replace("-", "");
            }
        }
    }
}
