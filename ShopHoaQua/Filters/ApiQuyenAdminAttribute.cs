using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using ShopHoaQua.Helpers;

namespace ShopHoaQua.Filters
{
    public class ApiQuyenAdminAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var current = HttpContext.Current;
            if (current == null || current.Session == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, "Không đọc được phiên đăng nhập.");
                return;
            }

            var httpContext = new HttpContextWrapper(current);
            PhienDangNhap.KhoiPhucNeuCan(httpContext);

            if (!PhienDangNhap.DaDangNhap(httpContext.Session))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Unauthorized, "Vui lòng đăng nhập bằng tài khoản quản trị.");
            }
            else if (!PhienDangNhap.LaAdmin(httpContext.Session))
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Forbidden, "Tài khoản không có quyền quản trị.");
            }
        }
    }
}
