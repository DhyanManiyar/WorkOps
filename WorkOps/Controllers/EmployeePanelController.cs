using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Helpers;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [EmployeeOnly]
    public class EmployeePanelController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: EmployeePanel/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.Title = "My Dashboard";
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            var employee = db.Employees.Find(employeeId.Value);
            var today = DateTime.Today;
            var tasks = db.WorkTasks.Where(t => t.AssignedTo == employeeId.Value).ToList();
            var todayAtt = db.Attendances.FirstOrDefault(a => a.EmployeeID == employeeId.Value && a.AttendanceDate == today);

            var model = new EmployeeDashboardViewModel
            {
                EmployeeName = employee != null ? employee.FirstName + " " + employee.LastName : "Employee",
                DepartmentName = employee != null && employee.Department != null ? employee.Department.DepartmentName : "",
                Position = employee != null ? employee.Position : "",
                TotalTasks = tasks.Count,
                PendingTasks = tasks.Count(t => t.Status == 1),
                InProgressTasks = tasks.Count(t => t.Status == 2),
                SubmittedTasks = tasks.Count(t => t.Status == 3),
                CompletedTasks = tasks.Count(t => t.Status == 5),
                OverdueTasks = tasks.Count(t => t.DueDate < today && t.Status < 5),
                AttendanceRateMonth = GetMonthAttendanceRate(employeeId.Value),
                TodayCheckIn = todayAtt?.CheckInTime,
                TodayCheckOut = todayAtt?.CheckOutTime,
                CanCheckIn = todayAtt == null || !todayAtt.CheckInTime.HasValue,
                CanCheckOut = todayAtt != null && todayAtt.CheckInTime.HasValue && !todayAtt.CheckOutTime.HasValue,
                PendingLeaveRequests = db.LeaveRequests.Count(l => l.EmployeeID == employeeId.Value && l.Status == 1),
                LeaveBalances = GetLeaveBalanceItems(employeeId.Value),
                RecentTasks = tasks
                    .OrderByDescending(t => t.LastUpdatedDate ?? t.CreatedDate)
                    .Take(5)
                    .Select(t => MapTaskList(t, today))
                    .ToList()
            };

            model.TotalLeaveRemaining = model.LeaveBalances != null ? model.LeaveBalances.Sum(b => b.RemainingDays) : 0;
            SetTodayAttendanceDisplay(model, todayAtt);
            return View(model);
        }

        // GET: EmployeePanel/Tasks
        public ActionResult Tasks(int? status)
        {
            ViewBag.Title = "My Tasks";
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            ViewBag.CurrentStatus = status;
            var today = DateTime.Today;
            var query = db.WorkTasks.Where(t => t.AssignedTo == employeeId.Value);
            if (status.HasValue && status.Value > 0)
                query = query.Where(t => t.Status == status.Value);

            var model = query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList()
                .Select(t => MapTaskList(t, today))
                .ToList();

            return View(model);
        }

        // GET: EmployeePanel/TaskDetails/5
        public ActionResult TaskDetails(int id)
        {
            ViewBag.Title = "Task Details";
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            if (!IsMyTask(id, employeeId.Value))
                return new HttpUnauthorizedResult();

            var task = db.WorkTasks
                .Include("Employee")
                .Include("Employee1")
                .Include("Department")
                .FirstOrDefault(t => t.TaskID == id);

            if (task == null)
                return HttpNotFound();

            var today = DateTime.Today;
            var comments = db.TaskComments
                .Where(c => c.TaskID == id)
                .Include("Employee")
                .OrderByDescending(c => c.CreatedDate)
                .ToList()
                .Select(c => new TaskCommentViewModel
                {
                    CommentID = c.CommentID,
                    TaskID = c.TaskID,
                    EmployeeID = c.EmployeeID,
                    EmployeeName = c.Employee != null ? c.Employee.FirstName + " " + c.Employee.LastName : "Unknown",
                    EmployeeAvatar = c.Employee != null ? (c.Employee.FirstName.Substring(0, 1) + c.Employee.LastName.Substring(0, 1)).ToUpper() : "??",
                    CommentText = c.CommentText,
                    CreatedDate = c.CreatedDate ?? DateTime.Now,
                    IsEdited = c.IsEdited ?? false,
                    EditedDate = c.EditedDate
                }).ToList();

            var model = new TaskDetailsViewModel
            {
                TaskID = task.TaskID,
                TaskCode = task.TaskCode,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                PriorityText = GetPriorityText(task.Priority),
                PriorityColor = GetPriorityColor(task.Priority),
                Status = task.Status,
                StatusText = GetStatusText(task.Status),
                StatusColor = GetStatusColor(task.Status),
                AssignedTo = task.Employee != null ? task.Employee.FirstName + " " + task.Employee.LastName : "",
                AssignedToID = task.AssignedTo,
                AssignedBy = task.Employee1 != null ? task.Employee1.FirstName + " " + task.Employee1.LastName : "",
                Department = task.Department != null ? task.Department.DepartmentName : "",
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                CompletedDate = task.CompletedDate,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours,
                CreatedDate = task.CreatedDate ?? DateTime.Now,
                LastUpdatedDate = task.LastUpdatedDate,
                IsOverdue = task.DueDate < today && task.Status < 5,
                DaysRemaining = (task.DueDate - today).Days,
                Comments = comments,
                Documents = new List<TaskDocumentViewModel>()
            };

            ViewBag.CanStart = task.Status == 1;
            ViewBag.CanSubmit = task.Status == 2;
            return View(model);
        }

        // POST: EmployeePanel/UpdateTaskStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTaskStatus(int id, int newStatus)
        {
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            if (!IsMyTask(id, employeeId.Value))
                return new HttpUnauthorizedResult();

            var task = db.WorkTasks.Find(id);
            if (task == null)
                return HttpNotFound();

            bool valid = (task.Status == 1 && newStatus == 2) || (task.Status == 2 && newStatus == 3);
            if (!valid)
            {
                TempData["ErrorMessage"] = "Invalid status change for this task.";
                return RedirectToAction("TaskDetails", new { id });
            }

            task.Status = newStatus;
            task.LastUpdatedDate = DateTime.Now;
            TrackActivity(
                newStatus == 2 ? "Task Started" : "Task Submitted",
                newStatus == 2 ? "Employee started working on task." : "Employee submitted task for approval.",
                "WorkTasks",
                task.TaskID
            );
            if (task.Employee1 != null && task.Employee1.UserID > 0)
            {
                Tracker().NotifyUser(
                    task.Employee1.UserID,
                    newStatus == 2 ? "Task Started" : "Task Submitted",
                    newStatus == 2
                        ? "Task \"" + (task.Title ?? "Untitled") + "\" is now in progress."
                        : "Task \"" + (task.Title ?? "Untitled") + "\" was submitted for your approval.",
                    newStatus == 2 ? 1 : 2,
                    Url.Action("Approvals", "Manager")
                );
            }
            db.SaveChanges();
            TempData["SuccessMessage"] = newStatus == 2 ? "Task started." : "Task submitted for approval.";
            return RedirectToAction("TaskDetails", new { id });
        }

        // POST: EmployeePanel/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AddComment(int taskId, string comment)
        {
            try
            {
                var employeeId = RequireEmployeeId();
                if (!employeeId.HasValue || !IsMyTask(taskId, employeeId.Value))
                    return Json(new { success = false, message = "Access denied" });

                if (string.IsNullOrWhiteSpace(comment))
                    return Json(new { success = false, message = "Comment cannot be empty" });

                var taskComment = new TaskComment
                {
                    TaskID = taskId,
                    EmployeeID = employeeId.Value,
                    CommentText = comment.Trim(),
                    CreatedDate = DateTime.Now,
                    IsEdited = false
                };
                db.TaskComments.Add(taskComment);
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Comment added",
                    data = new
                    {
                        employeeName = db.Employees.Find(employeeId.Value)?.FirstName,
                        commentText = comment,
                        createdDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: EmployeePanel/Attendance
        public ActionResult Attendance()
        {
            ViewBag.Title = "My Attendance";
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var todayAtt = db.Attendances.FirstOrDefault(a => a.EmployeeID == employeeId.Value && a.AttendanceDate == today);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var history = db.Attendances
                .Where(a => a.EmployeeID == employeeId.Value && a.AttendanceDate >= monthStart && a.AttendanceDate <= today)
                .OrderByDescending(a => a.AttendanceDate)
                .ToList()
                .Select(a => new EmployeeAttendanceHistoryViewModel
                {
                    Date = a.AttendanceDate,
                    DayName = a.AttendanceDate.ToString("dddd"),
                    CheckIn = a.CheckInTime,
                    CheckOut = a.CheckOutTime,
                    TotalHours = a.TotalHours,
                    StatusText = GetAttendanceStatusText(a.Status),
                    StatusColor = GetAttendanceStatusColor(a.Status),
                    IsWeekend = DateTimeHelper.IsWeekend(a.AttendanceDate)
                }).ToList();

            var model = new EmployeeAttendanceViewModel
            {
                CurrentDate = today,
                CheckInTime = todayAtt?.CheckInTime,
                CheckOutTime = todayAtt?.CheckOutTime,
                TotalHoursToday = todayAtt?.TotalHours,
                LateMinutes = todayAtt?.LateMinutes,
                CanCheckIn = todayAtt == null || !todayAtt.CheckInTime.HasValue,
                CanCheckOut = todayAtt != null && todayAtt.CheckInTime.HasValue && !todayAtt.CheckOutTime.HasValue,
                MonthAttendanceRate = GetMonthAttendanceRate(employeeId.Value),
                PresentDaysThisMonth = db.Attendances.Count(a =>
                    a.EmployeeID == employeeId.Value &&
                    a.AttendanceDate >= monthStart &&
                    a.AttendanceDate <= today &&
                    a.Status == 1),
                History = history
            };

            if (todayAtt != null)
            {
                model.StatusText = GetAttendanceStatusText(todayAtt.Status);
                model.StatusColor = GetAttendanceStatusColor(todayAtt.Status);
            }
            else
            {
                model.StatusText = "Not Marked";
                model.StatusColor = "secondary";
            }

            return View(model);
        }

        // POST: EmployeePanel/CheckIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckIn()
        {
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var now = DateTime.Now;
            var existing = db.Attendances.FirstOrDefault(a => a.EmployeeID == employeeId.Value && a.AttendanceDate == today);

            if (existing != null && existing.CheckInTime.HasValue)
            {
                TempData["ErrorMessage"] = "You have already checked in today.";
                return RedirectToAction("Attendance");
            }

            int lateMinutes = 0;
            if (now.TimeOfDay > new TimeSpan(9, 30, 0))
                lateMinutes = (int)(now.TimeOfDay - new TimeSpan(9, 30, 0)).TotalMinutes;

            if (existing != null)
            {
                existing.CheckInTime = now;
                existing.Status = 1;
                existing.LateMinutes = lateMinutes;
            }
            else
            {
                db.Attendances.Add(new Attendance
                {
                    EmployeeID = employeeId.Value,
                    AttendanceDate = today,
                    CheckInTime = now,
                    Status = 1,
                    LateMinutes = lateMinutes,
                    CreatedDate = now
                });
            }

            db.SaveChanges();
            TempData["SuccessMessage"] = "Checked in at " + now.ToString("hh:mm tt") + ".";
            return RedirectToAction("Attendance");
        }

        // POST: EmployeePanel/CheckOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckOut()
        {
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var now = DateTime.Now;
            var existing = db.Attendances.FirstOrDefault(a => a.EmployeeID == employeeId.Value && a.AttendanceDate == today);

            if (existing == null || !existing.CheckInTime.HasValue)
            {
                TempData["ErrorMessage"] = "Please check in first.";
                return RedirectToAction("Attendance");
            }

            if (existing.CheckOutTime.HasValue)
            {
                TempData["ErrorMessage"] = "You have already checked out today.";
                return RedirectToAction("Attendance");
            }

            existing.CheckOutTime = now;
            existing.TotalHours = (decimal)(now - existing.CheckInTime.Value).TotalHours;
            if (existing.TotalHours > 8)
                existing.OvertimeHours = existing.TotalHours - 8;

            db.SaveChanges();
            TempData["SuccessMessage"] = "Checked out at " + now.ToString("hh:mm tt") + ".";
            return RedirectToAction("Attendance");
        }

        // GET: EmployeePanel/Leaves
        public ActionResult Leaves()
        {
            ViewBag.Title = "My Leaves";
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            var model = new EmployeeLeavesViewModel
            {
                MyRequests = db.LeaveRequests
                    .Where(l => l.EmployeeID == employeeId.Value)
                    .OrderByDescending(l => l.AppliedDate)
                    .ToList()
                    .Select(MapLeaveRequest)
                    .ToList(),
                Balances = GetLeaveBalanceItems(employeeId.Value)
            };

            return View(model);
        }

        // GET: EmployeePanel/ApplyLeave
        public ActionResult ApplyLeave()
        {
            ViewBag.Title = "Apply for Leave";
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            var model = new EmployeeApplyLeaveViewModel
            {
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1),
                LeaveTypes = new SelectList(db.LeaveTypes, "LeaveTypeID", "TypeName")
            };

            return View(model);
        }

        // POST: EmployeePanel/ApplyLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyLeave(EmployeeApplyLeaveViewModel model)
        {
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                if (model.EndDate < model.StartDate)
                {
                    ModelState.AddModelError("EndDate", "End date must be on or after start date.");
                }
                else
                {
                    int totalDays = DateTimeHelper.GetWorkingDays(model.StartDate, model.EndDate);
                    var balance = db.LeaveBalances.FirstOrDefault(lb =>
                        lb.EmployeeID == employeeId.Value &&
                        lb.LeaveTypeID == model.LeaveTypeID &&
                        lb.Year == model.StartDate.Year);

                    int remaining = balance?.RemainingDays ?? 0;
                    if (remaining < totalDays)
                    {
                        ModelState.AddModelError("", "Insufficient leave balance. Available: " + remaining + " day(s).");
                    }
                    else
                    {
                        try
                        {
                            db.LeaveRequests.Add(new LeaveRequest
                            {
                                EmployeeID = employeeId.Value,
                                LeaveTypeID = model.LeaveTypeID,
                                StartDate = model.StartDate,
                                EndDate = model.EndDate,
                                TotalDays = totalDays,
                                Reason = model.Reason,
                                Status = 1,
                                AppliedDate = DateTime.Now
                            });
                            db.SaveChanges();
                            TempData["SuccessMessage"] = "Leave request submitted successfully.";
                            return RedirectToAction("Leaves");
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "Error: " + ex.Message);
                        }
                    }
                }
            }

            model.LeaveTypes = new SelectList(db.LeaveTypes, "LeaveTypeID", "TypeName", model.LeaveTypeID);
            return View(model);
        }

        // GET: EmployeePanel/Profile
        public ActionResult Profile()
        {
            ViewBag.Title = "My Profile";
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return RedirectToAction("Login", "Account");

            var employee = db.Employees.Find(employeeId.Value);
            if (employee == null)
                return HttpNotFound();

            string managerName = "";
            if (employee.ManagerID.HasValue)
            {
                var mgr = db.Employees.Find(employee.ManagerID.Value);
                if (mgr != null)
                    managerName = mgr.FirstName + " " + mgr.LastName;
            }

            var model = new EmployeeProfileViewModel
            {
                EmployeeID = employee.EmployeeID,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = GetEmployeeEmail(employee),
                Department = employee.Department != null ? employee.Department.DepartmentName : "",
                Position = employee.Position,
                ManagerName = managerName,
                Phone = employee.Phone,
                Address = employee.Address,
                EmergencyContact = employee.EmergencyContact,
                EmergencyPhone = employee.EmergencyPhone,
                JoiningDate = employee.JoiningDate,
                ProfilePicture = employee.ProfilePicture
            };

            return View(model);
        }

        // POST: EmployeePanel/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(EmployeeProfileViewModel model)
        {
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue || model.EmployeeID != employeeId.Value)
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                var employee = db.Employees.Find(employeeId.Value);
                if (employee != null)
                {
                    employee.Phone = model.Phone;
                    employee.Address = model.Address;
                    employee.EmergencyContact = model.EmergencyContact;
                    employee.EmergencyPhone = model.EmergencyPhone;
                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Profile updated successfully.";
                    return RedirectToAction("Profile");
                }
            }

            return View(model);
        }

        // POST: EmployeePanel/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix password validation errors.";
                return RedirectToAction("Profile");
            }

            var userId = SessionHelper.GetCurrentUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var user = db.Users.Find(userId.Value);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Profile");
            }

            if (!PasswordHelper.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            string passwordError;
            if (!PasswordHelper.IsPasswordValid(model.NewPassword, out passwordError))
            {
                TempData["ErrorMessage"] = passwordError;
                return RedirectToAction("Profile");
            }

            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            db.SaveChanges();
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }

        // GET: EmployeePanel/GetLeaveBalance (AJAX)
        public JsonResult GetLeaveBalance(int leaveTypeId, int year)
        {
            var employeeId = RequireEmployeeId();
            if (!employeeId.HasValue)
                return Json(new { success = false, message = "Not authenticated" }, JsonRequestBehavior.AllowGet);

            var balance = db.LeaveBalances.FirstOrDefault(lb =>
                lb.EmployeeID == employeeId.Value && lb.LeaveTypeID == leaveTypeId && lb.Year == year);

            return Json(new
            {
                success = true,
                remaining = balance?.RemainingDays ?? 0,
                total = balance?.TotalDays ?? 0
            }, JsonRequestBehavior.AllowGet);
        }

        #region Helpers

        private int? RequireEmployeeId()
        {
            return SessionHelper.GetCurrentEmployeeId();
        }

        private string GetEmployeeEmail(Employee employee)
        {
            if (employee == null) return SessionHelper.GetCurrentUserEmail();
            var user = db.Users.Find(employee.UserID);
            return user != null ? user.Email : SessionHelper.GetCurrentUserEmail();
        }

        private bool IsMyTask(int taskId, int employeeId)
        {
            var task = db.WorkTasks.Find(taskId);
            return task != null && task.AssignedTo == employeeId;
        }

        private decimal GetMonthAttendanceRate(int employeeId)
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            int workingDays = DateTimeHelper.GetWorkingDays(monthStart, today);
            if (workingDays == 0) return 0;

            int present = db.Attendances.Count(a =>
                a.EmployeeID == employeeId &&
                a.AttendanceDate >= monthStart &&
                a.AttendanceDate <= today &&
                (a.Status == 1 || a.Status == 3));

            return Math.Round((decimal)present / workingDays * 100, 1);
        }

        private List<EmployeeLeaveBalanceItemViewModel> GetLeaveBalanceItems(int employeeId)
        {
            int year = DateTime.Now.Year;
            return db.LeaveBalances
                .Where(lb => lb.EmployeeID == employeeId && lb.Year == year)
                .ToList()
                .Select(lb => new EmployeeLeaveBalanceItemViewModel
                {
                    LeaveType = lb.LeaveType != null ? lb.LeaveType.TypeName : "Unknown",
                    TotalDays = lb.TotalDays,
                    RemainingDays = lb.RemainingDays ?? 0,
                    UsagePercentage = lb.TotalDays > 0
                        ? Math.Round((decimal)(lb.UsedDays ?? 0) / lb.TotalDays * 100, 1)
                        : 0
                })
                .ToList();
        }

        private void SetTodayAttendanceDisplay(EmployeeDashboardViewModel model, Attendance todayAtt)
        {
            if (todayAtt == null)
            {
                model.TodayAttendanceStatus = "Not Marked";
                model.TodayAttendanceColor = "secondary";
                return;
            }

            model.TodayAttendanceStatus = GetAttendanceStatusText(todayAtt.Status);
            model.TodayAttendanceColor = GetAttendanceStatusColor(todayAtt.Status);
            if (todayAtt.CheckInTime.HasValue && todayAtt.Status == 1 && !todayAtt.CheckOutTime.HasValue)
            {
                model.TodayAttendanceStatus = "Present (In)";
                model.TodayAttendanceColor = "success";
            }
        }

        private TaskListViewModel MapTaskList(WorkTask t, DateTime today)
        {
            return new TaskListViewModel
            {
                TaskID = t.TaskID,
                TaskCode = t.TaskCode ?? "N/A",
                Title = t.Title ?? "Untitled",
                Priority = t.Priority,
                PriorityText = GetPriorityText(t.Priority),
                PriorityColor = GetPriorityColor(t.Priority),
                Status = t.Status,
                StatusText = GetStatusText(t.Status),
                StatusColor = GetStatusColor(t.Status),
                AssignedToID = t.AssignedTo,
                DueDate = t.DueDate,
                IsOverdue = t.DueDate < today && t.Status < 5,
                DaysRemaining = (t.DueDate - today).Days
            };
        }

        private LeaveRequestViewModel MapLeaveRequest(LeaveRequest l)
        {
            return new LeaveRequestViewModel
            {
                LeaveRequestID = l.LeaveRequestID,
                EmployeeID = l.EmployeeID,
                LeaveTypeName = l.LeaveType != null ? l.LeaveType.TypeName : "Unknown",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                Reason = l.Reason,
                Status = l.Status,
                StatusText = l.Status == 1 ? "Pending" : l.Status == 2 ? "Approved" : l.Status == 3 ? "Rejected" : "Cancelled",
                StatusColor = l.Status == 1 ? "warning" : l.Status == 2 ? "success" : l.Status == 3 ? "danger" : "secondary",
                AppliedDate = l.AppliedDate ?? DateTime.Now,
                RejectionReason = l.RejectionReason
            };
        }

        private ActivityNotificationService Tracker()
        {
            return new ActivityNotificationService(db, Request);
        }

        private void TrackActivity(string type, string description, string affectedTable, int? affectedRecordId)
        {
            var userId = SessionHelper.GetCurrentUserId() ?? 1;
            Tracker().LogActivity(userId, type, description, affectedTable, affectedRecordId);
        }

        private string GetPriorityText(int priority)
        {
            switch (priority)
            {
                case 1: return "Low";
                case 2: return "Medium";
                case 3: return "High";
                default: return "Critical";
            }
        }

        private string GetPriorityColor(int priority)
        {
            switch (priority)
            {
                case 1: return "success";
                case 2: return "info";
                case 3: return "warning";
                default: return "danger";
            }
        }

        private string GetStatusText(int status)
        {
            switch (status)
            {
                case 1: return "Pending";
                case 2: return "In Progress";
                case 3: return "Submitted";
                case 4: return "Approved";
                default: return "Completed";
            }
        }

        private string GetStatusColor(int status)
        {
            switch (status)
            {
                case 1: return "secondary";
                case 2: return "primary";
                case 3: return "info";
                case 4: return "warning";
                default: return "success";
            }
        }

        private string GetAttendanceStatusText(int status)
        {
            switch (status)
            {
                case 1: return "Present";
                case 2: return "Absent";
                case 3: return "Half Day";
                case 4: return "On Leave";
                case 5: return "Holiday";
                default: return "Unknown";
            }
        }

        private string GetAttendanceStatusColor(int status)
        {
            switch (status)
            {
                case 1: return "success";
                case 2: return "danger";
                case 3: return "warning";
                case 4: return "info";
                default: return "secondary";
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
