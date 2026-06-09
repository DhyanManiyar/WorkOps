using System.Web.Mvc;
using WorkOps.Filters;

namespace WorkOps
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new NoCacheForAuthenticatedAttribute());
        }
    }
}
