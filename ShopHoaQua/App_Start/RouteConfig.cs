using System.Web.Mvc;
using System.Web.Routing;

namespace ShopHoaQua
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Chương 1.2.3 Routing trong ASP.NET MVC
            routes.MapRoute(
                name: "SanPhamChiTiet",
                url: "san-pham/{id}",
                defaults: new { controller = "SanPham", action = "ChiTiet" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
