using System.Web.Mvc;
using ShopHoaQua.Filters;

namespace ShopHoaQua
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());

           
            filters.Add(new KhoiPhucDangNhapAttribute());
        }
    }
}
