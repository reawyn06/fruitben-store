using System.Web.Mvc;
using System.Web.Routing;
using ShopHoaQua.Helpers;

namespace ShopHoaQua.Filters
{
    public class QuyenAdminAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var httpContext = filterContext.HttpContext;
            PhienDangNhap.KhoiPhucNeuCan(httpContext);

            if (PhienDangNhap.LaAdmin(httpContext.Session)) return;

            if (PhienDangNhap.DaDangNhap(httpContext.Session))
            {
                filterContext.Controller.TempData["ThongBaoLoi"] = "Tài khoản của bạn không có quyền truy cập trang quản trị.";
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller", "Home" },
                    { "action", "Index" }
                });
                return;
            }

            filterContext.Controller.TempData["ThongBaoLoi"] = "Vui lòng đăng nhập bằng tài khoản quản trị.";
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
            {
                { "controller", "TaiKhoan" },
                { "action", "DangNhap" },
                { "returnUrl", httpContext.Request.HttpMethod == "GET" ? httpContext.Request.RawUrl : null }
            });
        }
    }
}
