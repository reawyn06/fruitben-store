using System;
using System.Web;
using System.Web.Mvc;
using ShopHoaQua.Helpers;

namespace ShopHoaQua.Filters
{
    public class KhoiPhucDangNhapAttribute : ActionFilterAttribute
    {
        private const string COOKIE_CU = "TaiKhoanId"; 

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.IsChildAction) return;

            var httpContext = filterContext.HttpContext;

            if (httpContext.Request.Cookies[COOKIE_CU] != null)
            {
                httpContext.Response.Cookies.Add(new HttpCookie(COOKIE_CU, "") { Expires = DateTime.Now.AddDays(-1) });
            }

            PhienDangNhap.KhoiPhucNeuCan(httpContext);
        }
    }
}
