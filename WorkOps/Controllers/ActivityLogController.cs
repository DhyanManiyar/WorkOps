using System;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [AdminOnly]
    public class ActivityLogController : Controller
    {
        private readonly WorkOpsDBEntities db = new WorkOpsDBEntities();

        public ActionResult Index(int? days)
        {
            ViewBag.Title = "Activity Logs";
            var spanDays = days ?? 7;
            var from = DateTime.Today.AddDays(-Math.Max(1, Math.Min(90, spanDays)));

            var logs = db.ActivityLogs
                .Where(l => (l.LogDate ?? DateTime.MinValue) >= from)
                .OrderByDescending(l => l.LogDate)
                .Take(500)
                .ToList();

            ViewBag.Days = spanDays;
            return View(logs);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
