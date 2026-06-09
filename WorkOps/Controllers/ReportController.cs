using System;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [AdminOnly]
    public class ReportController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        public ActionResult Index()
        {
            ViewBag.Title = "Reports & Analytics";

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var totalEmployees = db.Employees.Count();
            var activeEmployees = db.Employees.Count(e => e.IsActive == true);
            var presentToday = db.Attendances.Count(a => a.AttendanceDate == today && a.Status == 1);
            var workingDays = Enumerable.Range(0, today.Day)
                .Count(d => {
                    var dt = monthStart.AddDays(d);
                    return dt.DayOfWeek != DayOfWeek.Saturday && dt.DayOfWeek != DayOfWeek.Sunday;
                });
            var presentMonth = db.Attendances.Count(a =>
                a.AttendanceDate >= monthStart && a.AttendanceDate <= today && a.Status == 1);
            var expectedSlots = workingDays * Math.Max(activeEmployees, 1);
            var monthRate = expectedSlots > 0
                ? Math.Round((decimal)presentMonth / expectedSlots * 100, 1)
                : 0;

            var model = new ReportsIndexViewModel
            {
                TotalEmployees = totalEmployees,
                ActiveEmployees = activeEmployees,
                TotalDepartments = db.Departments.Count(d => d.IsActive == true),
                TotalTasks = db.WorkTasks.Count(),
                PendingLeaves = db.LeaveRequests.Count(l => l.Status == 1),
                TodayPresent = presentToday,
                TodayAbsent = db.Attendances.Count(a => a.AttendanceDate == today && a.Status == 2),
                MonthAttendanceRate = monthRate,
                TaskStatusCounts = new[]
                {
                    db.WorkTasks.Count(t => t.Status == 1),
                    db.WorkTasks.Count(t => t.Status == 2),
                    db.WorkTasks.Count(t => t.Status == 3),
                    db.WorkTasks.Count(t => t.Status == 4),
                    db.WorkTasks.Count(t => t.Status == 5)
                },
                Departments = db.Departments
                    .Where(d => d.IsActive == true)
                    .ToList()
                    .Select(d => new ReportDepartmentSummary
                    {
                        DepartmentName = d.DepartmentName,
                        EmployeeCount = d.Employees.Count(e => e.IsActive == true),
                        ActiveTasks = db.WorkTasks.Count(t =>
                            t.DepartmentID == d.DepartmentID && t.Status != 5)
                    })
                    .OrderByDescending(d => d.EmployeeCount)
                    .ToList()
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
