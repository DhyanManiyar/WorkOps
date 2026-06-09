using System.Web.Mvc;
using WorkOps.Helpers;

namespace WorkOps.Filters
{
    public class ManagerOnlyAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (!SessionHelper.IsUserLoggedIn())
                return false;

            return SessionHelper.IsAdmin() || SessionHelper.IsManager();
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!SessionHelper.IsUserLoggedIn())
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
            }
            else
            {
                filterContext.Result = new RedirectResult("~/Home/AccessDenied");
            }
        }
    }
}