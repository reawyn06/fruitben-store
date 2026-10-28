using System.Web.Http;

namespace ShopHoaQua
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Cho phép Web API trả về JSON mặc định (bỏ XML để dễ đọc khi test)
            config.Formatters.Remove(config.Formatters.XmlFormatter);

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
