using System;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Helpers;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [EmployeeOnly]
    public class NotificationController : Controller
    {
        private readonly WorkOpsDBEntities db = new WorkOpsDBEntities();

        [HttpGet]
        public JsonResult GetUnread(int take = 8)
        {
            var userId = SessionHelper.GetCurrentUserId();
            if (!userId.HasValue)
                return Json(new { success = false, count = 0, items = new object[0] }, JsonRequestBehavior.AllowGet);

            var items = db.Notifications
                .Where(n => n.UserID == userId.Value && (n.IsRead == null || n.IsRead == false))
                .OrderByDescending(n => n.CreatedDate)
                .Take(Math.Max(1, Math.Min(20, take)))
                .Select(n => new
                {
                    n.NotificationID,
                    n.Title,
                    n.Message,
                    n.Link,
                    CreatedDate = n.CreatedDate
                })
                .ToList()
                .Select(n => new
                {
                    n.NotificationID,
                    title = n.Title,
                    message = n.Message,
                    link = n.Link,
                    timeAgo = FormatTimeAgo(n.CreatedDate)
                })
                .ToList();

            var count = db.Notifications.Count(n => n.UserID == userId.Value && (n.IsRead == null || n.IsRead == false));
            return Json(new { success = true, count, items }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MarkAllRead()
        {
            var userId = SessionHelper.GetCurrentUserId();
            if (!userId.HasValue)
                return Json(new { success = false });

            var unread = db.Notifications
                .Where(n => n.UserID == userId.Value && (n.IsRead == null || n.IsRead == false))
                .ToList();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadDate = DateTime.Now;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        private static string FormatTimeAgo(DateTime? date)
        {
            if (!date.HasValue) return "Recently";
            var span = DateTime.Now - date.Value;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return Math.Floor(span.TotalMinutes) + " min ago";
            if (span.TotalHours < 24) return Math.Floor(span.TotalHours) + " hours ago";
            return Math.Floor(span.TotalDays) + " days ago";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
