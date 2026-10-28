using System;
using System.Data.Entity;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using ShopHoaQua.Data;

namespace ShopHoaQua
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Database.SetInitializer<ShopHoaQuaContext>(null);
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register); 
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
        protected void Application_PostAuthorizeRequest()
        {
            var path = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;
            if (path != null && path.StartsWith("~/api", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }
        protected void Application_Error()
        {
            var loi = Server.GetLastError() as HttpException;
            if (loi == null || loi.GetHttpCode() != 404) return;

            var path = Request.AppRelativeCurrentExecutionFilePath ?? "";
            if (path.StartsWith("~/Home/Trang404", StringComparison.OrdinalIgnoreCase)) return;

            Server.ClearError();
            Response.Redirect("~/Home/Trang404");
        }
    }
}
