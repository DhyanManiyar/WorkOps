using System.Web.Mvc;
using WorkOps.Helpers;

namespace WorkOps.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (SessionHelper.IsUserLoggedIn())
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
                }
            }
            return View();
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult NotFound()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "About WorkOps";
            ViewBag.Message = "Enterprise HR and operations management";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Title = "Contact";
            ViewBag.Message = "Get in touch with our team";
            return View();
        }
    }
}