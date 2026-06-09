using System;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [AdminOnly]
    public class LeaveController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: Leave
        public ActionResult Index()
        {
            ViewBag.Title = "Leave Management";

            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;

            var model = new LeaveDashboardViewModel
            {
                PendingLeaves = db.LeaveRequests.Count(l => l.Status == 1),
                ApprovedLeaves = db.LeaveRequests.Count(l => l.Status == 2),
                RejectedLeaves = db.LeaveRequests.Count(l => l.Status == 3),
                TotalLeavesThisMonth = db.LeaveRequests.Count(l => l.StartDate.Month == currentMonth && l.StartDate.Year == currentYear)
            };

            model.ApprovalRate = (model.ApprovedLeaves + model.RejectedLeaves) > 0
                ? Math.Round((decimal)model.ApprovedLeaves / (model.ApprovedLeaves + model.RejectedLeaves) * 100, 1)
                : 0;

            // Recent requests
            model.RecentRequests = db.LeaveRequests
                .OrderByDescending(l => l.AppliedDate)
                .Take(10)
                .ToList()
                .Select(l => new LeaveRequestViewModel
                {
                    LeaveRequestID = l.LeaveRequestID,
                    EmployeeID = l.EmployeeID,
                    EmployeeName = l.Employee != null ? l.Employee.FirstName + " " + l.Employee.LastName : "Unknown",
                    Department = l.Employee != null && l.Employee.Department != null ? l.Employee.Department.DepartmentName : "Unknown",
                    Initials = l.Employee != null ? (l.Employee.FirstName.Substring(0, 1) + l.Employee.LastName.Substring(0, 1)).ToUpper() : "??",
                    LeaveTypeName = l.LeaveType != null ? l.LeaveType.TypeName : "Unknown",
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    TotalDays = l.TotalDays,
                    Reason = l.Reason,
                    Status = l.Status,
                    StatusText = l.Status == 1 ? "Pending" : l.Status == 2 ? "Approved" : l.Status == 3 ? "Rejected" : "Cancelled",
                    StatusColor = l.Status == 1 ? "warning" : l.Status == 2 ? "success" : l.Status == 3 ? "danger" : "secondary",
                    AppliedDate = l.AppliedDate ?? DateTime.Now,
                    ApprovedByName = l.Employee1 != null ? l.Employee1.FirstName + " " + l.Employee1.LastName : ""
                }).ToList();

            // Low balance alerts
            var lowBalances = db.LeaveBalances
                .Where(lb => lb.Year == currentYear && lb.RemainingDays <= 2 && lb.RemainingDays > 0)
                .ToList()
                .Select(lb => new LeaveBalanceAlertViewModel
                {
                    EmployeeID = lb.EmployeeID,
                    EmployeeName = lb.Employee != null ? lb.Employee.FirstName + " " + lb.Employee.LastName : "Unknown",
                    Department = lb.Employee != null && lb.Employee.Department != null ? lb.Employee.Department.DepartmentName : "Unknown",
                    LeaveType = lb.LeaveType != null ? lb.LeaveType.TypeName : "Unknown",
                    RemainingDays = lb.RemainingDays ?? 0,
                    UsagePercentage = lb.TotalDays > 0 ? Math.Round((decimal)(lb.UsedDays ?? 0) / lb.TotalDays * 100, 1) : 0
                })
                .Take(5)
                .ToList();

            model.LowBalanceAlerts = lowBalances;

            return View(model);
        }

        // GET: Leave/Requests
        public ActionResult Requests(int? status)
        {
            ViewBag.Title = "Leave Requests";

            var query = db.LeaveRequests.AsQueryable();
            if (status.HasValue && status.Value > 0)
                query = query.Where(l => l.Status == status.Value);

            var requests = query
                .OrderByDescending(l => l.AppliedDate)
                .ToList()
                .Select(l => new LeaveRequestViewModel
                {
                    LeaveRequestID = l.LeaveRequestID,
                    EmployeeID = l.EmployeeID,
                    EmployeeName = l.Employee != null ? l.Employee.FirstName + " " + l.Employee.LastName : "Unknown",
                    Department = l.Employee != null && l.Employee.Department != null ? l.Employee.Department.DepartmentName : "Unknown",
                    Initials = l.Employee != null ? (l.Employee.FirstName.Substring(0, 1) + l.Employee.LastName.Substring(0, 1)).ToUpper() : "??",
                    LeaveTypeName = l.LeaveType != null ? l.LeaveType.TypeName : "Unknown",
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    TotalDays = l.TotalDays,
                    Reason = l.Reason,
                    Status = l.Status,
                    StatusText = l.Status == 1 ? "Pending" : l.Status == 2 ? "Approved" : l.Status == 3 ? "Rejected" : "Cancelled",
                    StatusColor = l.Status == 1 ? "warning" : l.Status == 2 ? "success" : l.Status == 3 ? "danger" : "secondary",
                    AppliedDate = l.AppliedDate ?? DateTime.Now,
                    ApprovedByName = l.Employee1 != null ? l.Employee1.FirstName + " " + l.Employee1.LastName : "",
                    ApprovedDate = l.ApprovedDate,
                    RejectionReason = l.RejectionReason
                }).ToList();

            ViewBag.CurrentStatus = status;
            return View(requests);
        }

        // GET: Leave/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Leave Request Details";

            var leave = db.LeaveRequests.Find(id);
            if (leave == null)
                return HttpNotFound();

            var model = new LeaveRequestViewModel
            {
                LeaveRequestID = leave.LeaveRequestID,
                EmployeeID = leave.EmployeeID,
                EmployeeName = leave.Employee != null ? leave.Employee.FirstName + " " + leave.Employee.LastName : "Unknown",
                Department = leave.Employee != null && leave.Employee.Department != null ? leave.Employee.Department.DepartmentName : "Unknown",
                LeaveTypeName = leave.LeaveType != null ? leave.LeaveType.TypeName : "Unknown",
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                TotalDays = leave.TotalDays,
                Reason = leave.Reason,
                Status = leave.Status,
                AppliedDate = leave.AppliedDate ?? DateTime.Now,
                ApprovedByName = leave.Employee1 != null ? leave.Employee1.FirstName + " " + leave.Employee1.LastName : "",
                ApprovedDate = leave.ApprovedDate,
                RejectionReason = leave.RejectionReason
            };

            // Get available balance
            var balance = db.LeaveBalances
                .FirstOrDefault(lb => lb.EmployeeID == leave.EmployeeID && lb.LeaveTypeID == leave.LeaveTypeID && lb.Year == leave.StartDate.Year);
            ViewBag.AvailableBalance = balance?.RemainingDays ?? 0;

            return View(model);
        }

        // GET: Leave/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Create Leave Request";

            var model = new LeaveRequestViewModel
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                LeaveTypes = new SelectList(db.LeaveTypes, "LeaveTypeID", "TypeName"),
                Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName")
            };

            return View(model);
        }

        // POST: Leave/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LeaveRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Calculate total days (excluding weekends)
                    int totalDays = CalculateWorkingDays(model.StartDate, model.EndDate);

                    var leaveRequest = new LeaveRequest
                    {
                        EmployeeID = model.EmployeeID,
                        LeaveTypeID = model.LeaveTypeID,
                        StartDate = model.StartDate,
                        EndDate = model.EndDate,
                        TotalDays = totalDays,
                        Reason = model.Reason,
                        Status = 1, // Pending
                        AppliedDate = DateTime.Now
                    };

                    db.LeaveRequests.Add(leaveRequest);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Leave request created successfully!";
                    return RedirectToAction("Requests");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            model.LeaveTypes = new SelectList(db.LeaveTypes, "LeaveTypeID", "TypeName", model.LeaveTypeID);
            model.Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName", model.EmployeeID);
            return View(model);
        }

        // GET: Leave/Approve/5
        public ActionResult Approve(int id)
        {
            var leave = db.LeaveRequests.Find(id);
            if (leave == null)
                return HttpNotFound();

            // Check balance
            var balance = db.LeaveBalances
                .FirstOrDefault(lb => lb.EmployeeID == leave.EmployeeID && lb.LeaveTypeID == leave.LeaveTypeID && lb.Year == leave.StartDate.Year);

            if (balance != null && balance.RemainingDays < leave.TotalDays)
            {
                TempData["ErrorMessage"] = $"Cannot approve: Insufficient balance. Available: {balance.RemainingDays} days";
                return RedirectToAction("Requests");
            }

            leave.Status = 2; // Approved
            leave.ApprovedDate = DateTime.Now;
            // Get current admin employee ID (simplified - you may want to get actual logged in user)
            leave.ApprovedBy = 1; // Admin employee ID

            // Update balance
            if (balance != null)
                balance.UsedDays = (balance.UsedDays ?? 0) + leave.TotalDays;

            db.SaveChanges();
            TempData["SuccessMessage"] = "Leave request approved successfully!";
            return RedirectToAction("Requests");
        }

        // GET: Leave/Reject/5
        public ActionResult Reject(int id)
        {
            var leave = db.LeaveRequests.Find(id);
            if (leave == null)
                return HttpNotFound();

            leave.Status = 3; // Rejected
            leave.ApprovedDate = DateTime.Now;
            leave.ApprovedBy = 1; // Admin employee ID

            db.SaveChanges();
            TempData["SuccessMessage"] = "Leave request rejected.";
            return RedirectToAction("Requests");
        }

        // GET: Leave/Balances
        public ActionResult Balances(int? year)
        {
            ViewBag.Title = "Leave Balances";

            int selectedYear = year ?? DateTime.Now.Year;

            var balances = db.LeaveBalances
                .Where(lb => lb.Year == selectedYear)
                .ToList()
                .Select(lb => new LeaveBalanceViewModel
                {
                    BalanceID = lb.BalanceID,
                    EmployeeID = lb.EmployeeID,
                    EmployeeName = lb.Employee != null ? lb.Employee.FirstName + " " + lb.Employee.LastName : "Unknown",
                    Department = lb.Employee != null && lb.Employee.Department != null ? lb.Employee.Department.DepartmentName : "Unknown",
                    LeaveType = lb.LeaveType != null ? lb.LeaveType.TypeName : "Unknown",
                    Year = lb.Year,
                    TotalDays = lb.TotalDays,
                    UsedDays = lb.UsedDays ?? 0,
                    RemainingDays = lb.RemainingDays ?? 0,
                    UsagePercentage = lb.TotalDays > 0 ? Math.Round((decimal)(lb.UsedDays ?? 0) / lb.TotalDays * 100, 1) : 0
                })
                .OrderBy(lb => lb.EmployeeName)
                .ThenBy(lb => lb.LeaveType)
                .ToList();

            ViewBag.SelectedYear = selectedYear;
            ViewBag.Years = new SelectList(
                Enumerable.Range(2020, 10).Select(y => new { Value = y, Text = y.ToString() }),
                "Value",
                "Text",
                selectedYear
            );

            return View(balances);
        }

        // GET: Leave/Types
        public ActionResult Types()
        {
            ViewBag.Title = "Leave Types";

            var leaveTypes = db.LeaveTypes
                .ToList()
                .Select(lt => new LeaveTypeViewModel
                {
                    LeaveTypeID = lt.LeaveTypeID,
                    TypeName = lt.TypeName,
                    Description = lt.Description,
                    MaxDaysPerYear = lt.MaxDaysPerYear,
                    IsPaid = lt.IsPaid ?? true,
                    RequiresDocument = lt.RequiresDocument ?? false
                })
                .ToList();

            return View(leaveTypes);
        }

        // Helper Methods
        private int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            var oneTimeHolidays = db.Holidays
                .Where(h => !(h.IsRecurring ?? false) && h.HolidayDate >= startDate && h.HolidayDate <= endDate)
                .Select(h => h.HolidayDate)
                .ToList();
            var recurringHolidayMd = db.Holidays
                .Where(h => h.IsRecurring == true)
                .Select(h => new { h.HolidayDate.Month, h.HolidayDate.Day })
                .ToList();

            int totalDays = 0;
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var isOneTimeHoliday = oneTimeHolidays.Any(h => h == date.Date);
                var isRecurringHoliday = recurringHolidayMd.Any(h => h.Month == date.Month && h.Day == date.Day);
                if (isOneTimeHoliday || isRecurringHoliday)
                    continue;

                totalDays++;
            }
            return totalDays;
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