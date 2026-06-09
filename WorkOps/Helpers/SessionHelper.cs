using System;
using System.Web;

namespace WorkOps.Helpers
{
    public static class SessionHelper
    {
        // Session keys
        private const string USER_ID_KEY = "UserID";
        private const string USER_EMAIL_KEY = "UserEmail";
        private const string USER_ROLE_ID_KEY = "RoleID";
        private const string USER_ROLE_NAME_KEY = "RoleName";
        private const string USER_EMPLOYEE_ID_KEY = "EmployeeID";
        private const string LOGIN_TIME_KEY = "LoginTime";

        // Set user session after successful login
        public static void SetUserSession(int userId, string email, int roleId, string roleName, int employeeId)
        {
            HttpContext.Current.Session[USER_ID_KEY] = userId;
            HttpContext.Current.Session[USER_EMAIL_KEY] = email;
            HttpContext.Current.Session[USER_ROLE_ID_KEY] = roleId;
            HttpContext.Current.Session[USER_ROLE_NAME_KEY] = roleName;
            HttpContext.Current.Session[USER_EMPLOYEE_ID_KEY] = employeeId;
            HttpContext.Current.Session[LOGIN_TIME_KEY] = DateTime.Now;
        }

        // Get current user ID
        public static int? GetCurrentUserId()
        {
            return HttpContext.Current.Session[USER_ID_KEY] as int?;
        }

        // Get current user email
        public static string GetCurrentUserEmail()
        {
            return HttpContext.Current.Session[USER_EMAIL_KEY] as string;
        }

        // Get current user role ID
        public static int? GetCurrentUserRoleId()
        {
            return HttpContext.Current.Session[USER_ROLE_ID_KEY] as int?;
        }

        // Get current user role name
        public static string GetCurrentUserRoleName()
        {
            return HttpContext.Current.Session[USER_ROLE_NAME_KEY] as string;
        }

        // Get current employee ID
        public static int? GetCurrentEmployeeId()
        {
            return HttpContext.Current.Session[USER_EMPLOYEE_ID_KEY] as int?;
        }

        // Check if user is logged in
        public static bool IsUserLoggedIn()
        {
            return GetCurrentUserId() != null;
        }

        // Check if user is Admin
        public static bool IsAdmin()
        {
            var roleName = GetCurrentUserRoleName();
            return roleName == "Admin";
        }

        // Check if user is Manager
        public static bool IsManager()
        {
            var roleName = GetCurrentUserRoleName();
            return roleName == "Manager";
        }

        // Check if user is Employee
        public static bool IsEmployee()
        {
            var roleName = GetCurrentUserRoleName();
            return roleName == "Employee";
        }

        // Get session login time
        public static DateTime? GetLoginTime()
        {
            return HttpContext.Current.Session[LOGIN_TIME_KEY] as DateTime?;
        }

        // Clear user session (logout)
        public static void ClearSession()
        {
            var context = HttpContext.Current;
            if (context == null)
                return;

            SignOut(new HttpContextWrapper(context));
        }

        /// <summary>
        /// Clears session data and expires the session cookie (required for reliable logout on all panels).
        /// </summary>
        public static void SignOut(HttpContextBase context)
        {
            if (context?.Session == null)
                return;

            context.Session.Clear();
            context.Session.Abandon();

            var path = string.IsNullOrEmpty(context.Request.ApplicationPath)
                || context.Request.ApplicationPath == "/"
                ? "/"
                : context.Request.ApplicationPath;

            context.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId", "")
            {
                Expires = DateTime.Now.AddYears(-1),
                HttpOnly = true,
                Path = path
            });
        }

        // Check if session has expired
        public static bool IsSessionExpired()
        {
            var loginTime = GetLoginTime();
            if (!loginTime.HasValue)
                return true;

            var hoursSinceLogin = (DateTime.Now - loginTime.Value).TotalHours;
            return hoursSinceLogin >= 8; // 8-hour session timeout
        }
    }
}