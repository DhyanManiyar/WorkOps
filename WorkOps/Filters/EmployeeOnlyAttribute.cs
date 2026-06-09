using System.Web.Mvc;
using WorkOps.Helpers;

namespace WorkOps.Filters
{
    public class EmployeeOnlyAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            return SessionHelper.IsUserLoggedIn();
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Result = new RedirectResult("~/Account/Login");
        }
    }
}