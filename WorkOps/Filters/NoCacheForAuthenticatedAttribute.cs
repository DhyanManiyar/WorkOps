using System;
using System.Web;
using System.Web.Mvc;
using WorkOps.Helpers;

namespace WorkOps.Filters
{
    /// <summary>
    /// Prevents browser back/forward cache from showing protected pages after logout.
    /// </summary>
    public class NoCacheForAuthenticatedAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!SessionHelper.IsUserLoggedIn())
                return;

            SetNoCacheHeaders(filterContext.HttpContext.Response);
        }

        public static void SetNoCacheHeaders(HttpResponseBase response)
        {
            var cache = response.Cache;
            cache.SetCacheability(HttpCacheability.NoCache);
            cache.SetNoStore();
            cache.SetExpires(DateTime.UtcNow.AddYears(-1));
            cache.SetValidUntilExpires(false);
            cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            response.AppendHeader("Pragma", "no-cache");
        }
    }
}
