using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using WorkOps.Helpers;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    public class AccountController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: Account/Login
        public ActionResult Login()
        {
            // If user is already logged in, redirect to appropriate panel
            if (SessionHelper.IsUserLoggedIn())
            {
                return RedirectToDashboard();
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var email = (model.Email ?? "").Trim();
                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Check if account is locked
                if (user.IsLocked == true)
                {
                    ModelState.AddModelError("", "Your account has been locked due to multiple failed attempts. Please contact administrator.");
                    return View(model);
                }

                // Check if account is active
                if (user.IsActive == false)
                {
                    ModelState.AddModelError("", "Your account is inactive. Please contact administrator.");
                    return View(model);
                }

                // Verify password
                if (!PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    // Increment failed login attempts
                    user.FailedLoginAttempts = (user.FailedLoginAttempts ?? 0) + 1;

                    // Lock account after 5 failed attempts
                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsLocked = true;
                        db.SaveChanges();
                        ModelState.AddModelError("", "Your account has been locked due to 5 failed login attempts.");
                    }
                    else
                    {
                        db.SaveChanges();
                        ModelState.AddModelError("", "Invalid email or password");
                    }
                    return View(model);
                }

                // Reset failed attempts on successful login
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;
                user.LastLoginDate = DateTime.Now;
                db.SaveChanges();

                // Get role name
                var role = db.Roles.Find(user.RoleID);
                string roleName = role?.RoleName ?? "Employee";

                // Get employee ID
                var employee = db.Employees.FirstOrDefault(e => e.UserID == user.UserID);
                int employeeId = employee?.EmployeeID ?? 0;

                // Set session
                SessionHelper.SetUserSession(user.UserID, user.Email, user.RoleID, roleName, employeeId);

                // Set authentication cookie if remember me
                if (model.RememberMe)
                {
                    FormsAuthentication.SetAuthCookie(user.Email, true);
                }

                // Redirect based on role
                return RedirectToDashboard();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(model);
            }
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            // Check if self-registration is allowed
            bool allowSelfRegistration = System.Configuration.ConfigurationManager.AppSettings["AllowSelfRegistration"] == "true";

            if (!allowSelfRegistration)
            {
                TempData["ErrorMessage"] = "Self-registration is disabled. Please contact your administrator.";
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Register(RegisterViewModel model)
        {
            bool allowSelfRegistration = System.Configuration.ConfigurationManager.AppSettings["AllowSelfRegistration"] == "true";
            if (!allowSelfRegistration)
            {
                TempData["ErrorMessage"] = "Self-registration is disabled. Please contact your administrator.";
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if email already exists
            if (db.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered. Please use a different email.");
                return View(model);
            }

            // Validate password complexity
            string passwordError;
            if (!PasswordHelper.IsPasswordValid(model.Password, out passwordError))
            {
                ModelState.AddModelError("Password", passwordError);
                return View(model);
            }

            try
            {
                // Create User
                var user = new User
                {
                    Email = model.Email,
                    PasswordHash = PasswordHelper.HashPassword(model.Password),
                    RoleID = 3, // Employee role by default
                    IsActive = false, // Inactive until admin approves
                    IsLocked = false,
                    FailedLoginAttempts = 0,
                    CreatedDate = DateTime.Now
                };

                db.Users.Add(user);
                db.SaveChanges();

                // Create Employee
                var employee = new Employee
                {
                    UserID = user.UserID,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    DepartmentID = 2, // Default department (IT)
                    IsActive = false,
                    CreatedDate = DateTime.Now
                };

                db.Employees.Add(employee);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Registration successful! Your account is pending admin approval. You will receive an email when approved.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // GET: Account/Logout
        [HttpGet]
        public ActionResult Logout()
        {
            return PerformLogout();
        }

        // POST: Account/Logout (used by panel dropdown forms)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Logout")]
        public ActionResult LogoutSubmit()
        {
            return PerformLogout();
        }

        private ActionResult PerformLogout()
        {
            Filters.NoCacheForAuthenticatedAttribute.SetNoCacheHeaders(Response);
            SessionHelper.SignOut(HttpContext);
            FormsAuthentication.SignOut();

            if (!string.IsNullOrEmpty(FormsAuthentication.FormsCookieName))
            {
                Response.Cookies.Add(new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, "")
                {
                    Expires = DateTime.Now.AddYears(-1),
                    HttpOnly = true,
                    Path = "/"
                });
            }

            return RedirectToAction("Login");
        }

        // GET: Account/AccessDenied
        public ActionResult AccessDenied()
        {
            return View();
        }

        // Helper method to redirect to appropriate dashboard
        private ActionResult RedirectToDashboard()
        {
            string roleName = SessionHelper.GetCurrentUserRoleName();

            switch (roleName)
            {
                case "Admin":
                    return RedirectToAction("Dashboard", "Admin");
                case "Manager":
                    return RedirectToAction("Dashboard", "Manager");
                case "Employee":
                    return RedirectToAction("Dashboard", "EmployeePanel");
                default:
                    return RedirectToAction("Login");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}