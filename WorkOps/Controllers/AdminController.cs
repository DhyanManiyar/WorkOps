using System;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.Title = "Dashboard";

            // Get statistics
            ViewBag.TotalEmployees = db.Employees.Count();
            ViewBag.ActiveEmployees = db.Employees.Count(e => e.IsActive == true);
            ViewBag.TotalDepartments = db.Departments.Count(d => d.IsActive == true);
            ViewBag.TotalTasks = db.WorkTasks.Count();
            ViewBag.PendingTasks = db.WorkTasks.Count(t => t.Status == 1);
            ViewBag.PendingLeaves = db.LeaveRequests.Count(l => l.Status == 1);

            // Today's attendance
            var today = System.DateTime.Today;
            var todayAttendance = db.Attendances.Count(a => a.AttendanceDate == today);
            ViewBag.TodayPresent = db.Attendances.Count(a => a.AttendanceDate == today && a.Status == 1);
            ViewBag.TodayAbsent = db.Attendances.Count(a => a.AttendanceDate == today && a.Status == 2);

            // Department distribution for chart
            var deptData = db.Departments
                .Where(d => d.IsActive == true)
                .Select(d => new { d.DepartmentName, Count = d.Employees.Count(e => e.IsActive == true) })
                .ToList();

            ViewBag.DeptNames = deptData.Select(d => d.DepartmentName).ToArray();
            ViewBag.DeptCounts = deptData.Select(d => d.Count).ToArray();

            return View();
        }

        public ActionResult Settings()
        {
            ViewBag.Title = "System Settings";
            return View();
        }

        // GET: Admin/GetDashboardData (for AJAX)
        public JsonResult GetDashboardData()
        {
            try
            {
                var data = new
                {
                    totalEmployees = db.Employees.Count(),
                    activeEmployees = db.Employees.Count(e => e.IsActive == true),
                    totalDepartments = db.Departments.Count(d => d.IsActive == true),
                    totalTasks = db.WorkTasks.Count(),
                    pendingTasks = db.WorkTasks.Count(t => t.Status == 1 || t.Status == 2), 
                    pendingLeaves = db.LeaveRequests.Count(l => l.Status == 1),

                    // Department distribution
                    departmentNames = db.Departments.Where(d => d.IsActive == true).Select(d => d.DepartmentName).ToList(),
                    departmentCounts = db.Departments.Where(d => d.IsActive == true).Select(d => d.Employees.Count(e => e.IsActive == true)).ToList(),

                    // Attendance trend (last 4 weeks)
                    attendanceTrend = GetWeeklyAttendanceTrend(),

                    // Task status distribution
                    taskStatusCounts = new int[] {
                db.WorkTasks.Count(t => t.Status == 1),  // Pending
                db.WorkTasks.Count(t => t.Status == 2),  // In Progress
                db.WorkTasks.Count(t => t.Status == 3),  // Submitted
                db.WorkTasks.Count(t => t.Status == 4),  // Approved
                db.WorkTasks.Count(t => t.Status == 5)   // Completed
            },

                    // Leave status distribution
                    leaveStatusCounts = new int[] {
                db.LeaveRequests.Count(l => l.Status == 2),  // Approved
                db.LeaveRequests.Count(l => l.Status == 1),  // Pending
                db.LeaveRequests.Count(l => l.Status == 3)   // Rejected
            }
                };

                // Recent activities (sample data - you can replace with actual activity log)
                var activities = new[]
                {
            new { type = "employee", message = "New employee John Smith joined IT department", time = "10 minutes ago" },
            new { type = "task", message = "Database migration task completed", time = "1 hour ago" },
            new { type = "leave", message = "Alice Johnson's sick leave approved", time = "2 hours ago" },
            new { type = "attendance", message = "5 employees marked late today", time = "3 hours ago" },
            new { type = "employee", message = "Jane Smith promoted to Senior Developer", time = "5 hours ago" }
        };

                return Json(new { success = true, data = data, activities = activities }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private int[] GetWeeklyAttendanceTrend()
        {
            var trend = new int[4];
            var today = DateTime.Today;

            for (int i = 0; i < 4; i++)
            {
                var weekStart = today.AddDays(-(i * 7 + 7));
                var weekEnd = weekStart.AddDays(6);
                var totalEmployees = db.Employees.Count(e => e.IsActive == true);
                var presentCount = db.Attendances.Count(a => a.AttendanceDate >= weekStart && a.AttendanceDate <= weekEnd && a.Status == 1);

                trend[3 - i] = totalEmployees > 0 ? (presentCount * 100 / totalEmployees) : 0;
            }

            return trend;
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