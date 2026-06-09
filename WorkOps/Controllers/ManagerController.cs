using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Helpers;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [ManagerOnly]
    public class ManagerController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: Manager/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.Title = "Manager Dashboard";
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
            {
                TempData["ErrorMessage"] = "Manager profile not found.";
                return RedirectToAction("Login", "Account");
            }

            var teamIds = GetTeamMemberIds(managerId.Value);
            var manager = db.Employees.Find(managerId.Value);
            var today = DateTime.Today;

            var model = new ManagerDashboardViewModel
            {
                ManagerName = manager != null ? manager.FirstName + " " + manager.LastName : "Manager",
                DepartmentName = manager != null && manager.Department != null ? manager.Department.DepartmentName : DisplayConstants.Empty,
                TeamSize = teamIds.Count,
                PendingTasks = CountTeamTasks(teamIds, t => t.Status == 1 || t.Status == 2),
                OverdueTasks = CountTeamTasks(teamIds, t => t.DueDate < today && t.Status < 5),
                SubmittedTasks = CountTeamTasks(teamIds, t => t.Status == 3),
                PendingLeaves = db.LeaveRequests.Count(l => teamIds.Contains(l.EmployeeID) && l.Status == 1),
                PresentToday = db.Attendances.Count(a => teamIds.Contains(a.EmployeeID) && a.AttendanceDate == today && a.Status == 1),
                OnLeaveToday = db.Attendances.Count(a => teamIds.Contains(a.EmployeeID) && a.AttendanceDate == today && a.Status == 4),
                TeamMembers = BuildTeamMemberList(managerId.Value, teamIds, today),
                RecentActivities = BuildRecentActivities(teamIds),
                TaskStatusCounts = GetTeamTaskStatusCounts(teamIds)
            };

            return View(model);
        }

        // GET: Manager/Team
        public ActionResult Team()
        {
            ViewBag.Title = "My Team";
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Login", "Account");

            var teamIds = GetTeamMemberIds(managerId.Value);
            var model = BuildTeamMemberList(managerId.Value, teamIds, DateTime.Today);
            return View(model);
        }

        // GET: Manager/Tasks
        public ActionResult Tasks(int? status)
        {
            ViewBag.Title = "Team Tasks";
            ViewBag.CurrentStatus = status;

            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Login", "Account");

            var teamIds = GetTeamMemberIds(managerId.Value);
            var model = GetTeamTasks(teamIds, status);
            return View(model);
        }

        // GET: Manager/AssignTask
        public ActionResult AssignTask()
        {
            ViewBag.Title = "Assign Task";
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Login", "Account");

            var manager = db.Employees.Find(managerId.Value);
            var teamIds = GetTeamMemberIds(managerId.Value);

            var model = new ManagerAssignTaskViewModel
            {
                DueDate = DateTime.Today.AddDays(7),
                StartDate = DateTime.Today,
                DepartmentID = manager != null ? manager.DepartmentID : 0
            };
            PopulateAssignTaskDropdowns(model, teamIds, manager);
            return View(model);
        }

        // POST: Manager/AssignTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignTask(ManagerAssignTaskViewModel model)
        {
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Login", "Account");

            var teamIds = GetTeamMemberIds(managerId.Value);

            if (!teamIds.Contains(model.AssignedTo))
            {
                ModelState.AddModelError("AssignedTo", "You can only assign tasks to your team members.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var task = new WorkTask
                    {
                        TaskCode = GenerateTaskCode(),
                        Title = model.Title,
                        Description = model.Description,
                        Priority = model.Priority,
                        Status = 1,
                        AssignedTo = model.AssignedTo,
                        AssignedBy = managerId.Value,
                        DepartmentID = model.DepartmentID,
                        StartDate = model.StartDate ?? DateTime.Today,
                        DueDate = model.DueDate,
                        EstimatedHours = model.EstimatedHours,
                        CreatedDate = DateTime.Now,
                        LastUpdatedDate = DateTime.Now
                    };

                    db.WorkTasks.Add(task);
                    db.SaveChanges();
                    TrackActivity(
                        "Task Assigned",
                        "Assigned task \"" + (task.Title ?? "Untitled") + "\" to team member.",
                        "WorkTasks",
                        task.TaskID
                    );
                    Tracker().NotifyEmployeeById(
                        task.AssignedTo,
                        "New Task Assigned",
                        "You have been assigned task \"" + (task.Title ?? "Untitled") + "\".",
                        1,
                        Url.Action("TaskDetails", "EmployeePanel", new { id = task.TaskID })
                    );
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Task assigned successfully! Code: " + task.TaskCode;
                    return RedirectToAction("Tasks");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error assigning task: " + ex.Message);
                }
            }

            var manager = db.Employees.Find(managerId.Value);
            PopulateAssignTaskDropdowns(model, teamIds, manager);
            return View(model);
        }

        // GET: Manager/Approvals
        public ActionResult Approvals()
        {
            ViewBag.Title = "Approvals";
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Login", "Account");

            var teamIds = GetTeamMemberIds(managerId.Value);

            var model = new ManagerApprovalsViewModel
            {
                PendingTaskApprovals = GetTeamTasks(teamIds, 3),
                PendingLeaveApprovals = db.LeaveRequests
                    .Where(l => teamIds.Contains(l.EmployeeID) && l.Status == 1)
                    .OrderByDescending(l => l.AppliedDate)
                    .ToList()
                    .Select(MapLeaveRequest)
                    .ToList()
            };

            return View(model);
        }

        // POST: Manager/ApproveTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveTask(int id)
        {
            return UpdateTeamTaskStatus(id, 4, "Task approved successfully.");
        }

        // POST: Manager/RejectTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectTask(int id)
        {
            return UpdateTeamTaskStatus(id, 2, "Task sent back for revision.");
        }

        // POST: Manager/CompleteTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CompleteTask(int id)
        {
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Approvals");

            var teamIds = GetTeamMemberIds(managerId.Value);
            var task = db.WorkTasks.Find(id);
            if (task == null || !teamIds.Contains(task.AssignedTo))
            {
                TempData["ErrorMessage"] = "Task not found or access denied.";
                return RedirectToAction("Approvals");
            }

            if (task.Status == 4)
            {
                task.Status = 5;
                task.CompletedDate = DateTime.Now;
                task.LastUpdatedDate = DateTime.Now;
                TrackActivity("Task Completed", "Manager marked task as completed.", "WorkTasks", task.TaskID);
                Tracker().NotifyEmployeeById(
                    task.AssignedTo,
                    "Task Marked Completed",
                    "Your task \"" + (task.Title ?? "Untitled") + "\" was marked completed by manager.",
                    2,
                    Url.Action("TaskDetails", "EmployeePanel", new { id = task.TaskID })
                );
                db.SaveChanges();
                TempData["SuccessMessage"] = "Task marked as completed.";
            }

            return RedirectToAction("Approvals");
        }

        // GET: Manager/ApproveLeave/5
        public ActionResult ApproveLeave(int id)
        {
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Approvals");

            var leave = db.LeaveRequests.Find(id);
            if (leave == null || !IsTeamMember(leave.EmployeeID, managerId.Value))
            {
                TempData["ErrorMessage"] = "Leave request not found.";
                return RedirectToAction("Approvals");
            }

            var balance = db.LeaveBalances
                .FirstOrDefault(lb => lb.EmployeeID == leave.EmployeeID && lb.LeaveTypeID == leave.LeaveTypeID && lb.Year == leave.StartDate.Year);

            if (balance != null && balance.RemainingDays < leave.TotalDays)
            {
                TempData["ErrorMessage"] = "Cannot approve: insufficient leave balance.";
                return RedirectToAction("Approvals");
            }

            leave.Status = 2;
            leave.ApprovedDate = DateTime.Now;
            leave.ApprovedBy = managerId.Value;

            if (balance != null)
                balance.UsedDays = (balance.UsedDays ?? 0) + leave.TotalDays;

            TrackActivity("Leave Approved", "Manager approved leave request #" + leave.LeaveRequestID + ".", "LeaveRequests", leave.LeaveRequestID);
            Tracker().NotifyEmployeeById(
                leave.EmployeeID,
                "Leave Approved",
                "Your leave request from " + leave.StartDate.ToString("dd MMM") + " to " + leave.EndDate.ToString("dd MMM") + " has been approved.",
                2,
                Url.Action("Leaves", "EmployeePanel")
            );
            db.SaveChanges();
            TempData["SuccessMessage"] = "Leave request approved.";
            return RedirectToAction("Approvals");
        }

        // GET: Manager/RejectLeave/5
        public ActionResult RejectLeave(int id, string reason)
        {
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Approvals");

            var leave = db.LeaveRequests.Find(id);
            if (leave == null || !IsTeamMember(leave.EmployeeID, managerId.Value))
            {
                TempData["ErrorMessage"] = "Leave request not found.";
                return RedirectToAction("Approvals");
            }

            leave.Status = 3;
            leave.ApprovedDate = DateTime.Now;
            leave.ApprovedBy = managerId.Value;
            leave.RejectionReason = reason ?? "Rejected by manager";

            TrackActivity("Leave Rejected", "Manager rejected leave request #" + leave.LeaveRequestID + ".", "LeaveRequests", leave.LeaveRequestID);
            Tracker().NotifyEmployeeById(
                leave.EmployeeID,
                "Leave Rejected",
                "Your leave request was rejected. Reason: " + (leave.RejectionReason ?? "Not specified"),
                3,
                Url.Action("Leaves", "EmployeePanel")
            );
            db.SaveChanges();
            TempData["SuccessMessage"] = "Leave request rejected.";
            return RedirectToAction("Approvals");
        }

        // GET: Manager/Reports
        public ActionResult Reports()
        {
            ViewBag.Title = "Team Reports";
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Login", "Account");

            var teamIds = GetTeamMemberIds(managerId.Value);
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var teamTasks = db.WorkTasks.Where(t => teamIds.Contains(t.AssignedTo)).ToList();
            int totalTasks = teamTasks.Count;
            int completed = teamTasks.Count(t => t.Status == 5);
            int pending = teamTasks.Count(t => t.Status < 5);
            int overdue = teamTasks.Count(t => t.DueDate < today && t.Status < 5);

            var memberPerformance = new List<TeamMemberPerformanceViewModel>();
            foreach (var empId in teamIds)
            {
                var emp = db.Employees.Find(empId);
                if (emp == null) continue;

                var empTasks = teamTasks.Where(t => t.AssignedTo == empId).ToList();
                int empTotal = empTasks.Count;
                int empDone = empTasks.Count(t => t.Status == 5);

                int workingDays = DateTimeHelper.GetWorkingDays(monthStart, today);
                int presentDays = db.Attendances.Count(a =>
                    a.EmployeeID == empId &&
                    a.AttendanceDate >= monthStart &&
                    a.AttendanceDate <= today &&
                    a.Status == 1);

                memberPerformance.Add(new TeamMemberPerformanceViewModel
                {
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    TotalTasks = empTotal,
                    CompletedTasks = empDone,
                    CompletionRate = empTotal > 0 ? Math.Round((decimal)empDone / empTotal * 100, 1) : 0,
                    AttendanceRate = workingDays > 0 ? Math.Round((decimal)presentDays / workingDays * 100, 1) : 0
                });
            }

            var chartMembers = memberPerformance.Take(8).ToList();

            var model = new ManagerReportsViewModel
            {
                TeamSize = teamIds.Count,
                TotalTeamTasks = totalTasks,
                CompletedTasks = completed,
                PendingTasks = pending,
                OverdueTasks = overdue,
                TaskCompletionRate = totalTasks > 0 ? Math.Round((decimal)completed / totalTasks * 100, 1) : 0,
                TeamAttendanceRate = memberPerformance.Count > 0 ? Math.Round(memberPerformance.Average(m => m.AttendanceRate), 1) : 0,
                ApprovedLeavesThisMonth = db.LeaveRequests.Count(l =>
                    teamIds.Contains(l.EmployeeID) && l.Status == 2 &&
                    l.ApprovedDate >= monthStart && l.ApprovedDate <= monthEnd),
                PendingLeaves = db.LeaveRequests.Count(l => teamIds.Contains(l.EmployeeID) && l.Status == 1),
                MemberPerformance = memberPerformance.OrderByDescending(m => m.CompletionRate).ToList(),
                ChartLabels = chartMembers.Select(m => m.EmployeeName.Split(' ').FirstOrDefault() ?? m.EmployeeName).ToArray(),
                ChartCompleted = chartMembers.Select(m => m.CompletedTasks).ToArray(),
                ChartPending = chartMembers.Select(m => m.TotalTasks - m.CompletedTasks).ToArray()
            };

            return View(model);
        }

        // GET: Manager/GetTeamTasks (AJAX)
        public JsonResult GetTeamTasksJson(int? status)
        {
            try
            {
                var managerId = GetManagerEmployeeId();
                if (!managerId.HasValue)
                    return Json(new { success = false, message = "Not authenticated" }, JsonRequestBehavior.AllowGet);

                var teamIds = GetTeamMemberIds(managerId.Value);
                var tasks = GetTeamTasks(teamIds, status);
                return Json(new { success = true, data = tasks }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #region Helpers

        private int? GetManagerEmployeeId()
        {
            return SessionHelper.GetCurrentEmployeeId();
        }

        private List<int> GetTeamMemberIds(int managerId)
        {
            return db.Employees
                .Where(e => e.ManagerID == managerId && e.IsActive == true)
                .Select(e => e.EmployeeID)
                .ToList();
        }

        private bool IsTeamMember(int employeeId, int managerId)
        {
            return db.Employees.Any(e => e.EmployeeID == employeeId && e.ManagerID == managerId);
        }

        private int CountTeamTasks(List<int> teamIds, Func<WorkTask, bool> predicate)
        {
            if (teamIds.Count == 0) return 0;
            return db.WorkTasks.Where(t => teamIds.Contains(t.AssignedTo)).ToList().Count(predicate);
        }

        private int[] GetTeamTaskStatusCounts(List<int> teamIds)
        {
            if (teamIds.Count == 0)
                return new int[5];

            var tasks = db.WorkTasks.Where(t => teamIds.Contains(t.AssignedTo));
            return new[]
            {
                tasks.Count(t => t.Status == 1),
                tasks.Count(t => t.Status == 2),
                tasks.Count(t => t.Status == 3),
                tasks.Count(t => t.Status == 4),
                tasks.Count(t => t.Status == 5)
            };
        }

        private List<TeamMemberViewModel> BuildTeamMemberList(int managerId, List<int> teamIds, DateTime today)
        {
            return db.Employees
                .Where(e => teamIds.Contains(e.EmployeeID))
                .ToList()
                .Select(e =>
                {
                    var attendance = db.Attendances.FirstOrDefault(a => a.EmployeeID == e.EmployeeID && a.AttendanceDate == today);
                    string attText = "Not Marked";
                    string attColor = "secondary";
                    if (attendance != null)
                    {
                        switch (attendance.Status)
                        {
                            case 1: attText = "Present"; attColor = "success"; break;
                            case 2: attText = "Absent"; attColor = "danger"; break;
                            case 3: attText = "Half Day"; attColor = "warning"; break;
                            case 4: attText = "On Leave"; attColor = "info"; break;
                            case 5: attText = "Holiday"; attColor = "secondary"; break;
                        }
                    }

                    var tasks = db.WorkTasks.Where(t => t.AssignedTo == e.EmployeeID).ToList();
                    return new TeamMemberViewModel
                    {
                        EmployeeID = e.EmployeeID,
                        FullName = e.FirstName + " " + e.LastName,
                        Email = e.User != null ? e.User.Email : "",
                        Position = e.Position,
                        Department = e.Department != null ? e.Department.DepartmentName : "",
                        Initials = (e.FirstName.Substring(0, 1) + e.LastName.Substring(0, 1)).ToUpper(),
                        ActiveTasks = tasks.Count(t => t.Status < 5),
                        CompletedTasks = tasks.Count(t => t.Status == 5),
                        OverdueTasks = tasks.Count(t => t.DueDate < today && t.Status < 5),
                        TodayAttendance = attText,
                        TodayAttendanceColor = attColor,
                        ProfilePicture = e.ProfilePicture,
                        IsActive = e.IsActive == true
                    };
                })
                .OrderBy(m => m.FullName)
                .ToList();
        }

        private List<ManagerTaskViewModel> GetTeamTasks(List<int> teamIds, int? status)
        {
            if (teamIds.Count == 0)
                return new List<ManagerTaskViewModel>();

            var query = db.WorkTasks.Where(t => teamIds.Contains(t.AssignedTo));
            if (status.HasValue && status.Value > 0)
                query = query.Where(t => t.Status == status.Value);

            var today = DateTime.Today;
            return query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToList()
                .Select(t => MapTask(t, today))
                .ToList();
        }

        private ManagerTaskViewModel MapTask(WorkTask t, DateTime today)
        {
            return new ManagerTaskViewModel
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
                AssignedToName = t.Employee != null ? t.Employee.FirstName + " " + t.Employee.LastName : "Unknown",
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
                EmployeeName = l.Employee != null ? l.Employee.FirstName + " " + l.Employee.LastName : "Unknown",
                Department = l.Employee != null && l.Employee.Department != null ? l.Employee.Department.DepartmentName : "",
                Initials = l.Employee != null ? (l.Employee.FirstName.Substring(0, 1) + l.Employee.LastName.Substring(0, 1)).ToUpper() : "??",
                LeaveTypeName = l.LeaveType != null ? l.LeaveType.TypeName : "Unknown",
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                Reason = l.Reason,
                Status = l.Status,
                StatusText = l.Status == 1 ? "Pending" : l.Status == 2 ? "Approved" : l.Status == 3 ? "Rejected" : "Cancelled",
                StatusColor = l.Status == 1 ? "warning" : l.Status == 2 ? "success" : l.Status == 3 ? "danger" : "secondary",
                AppliedDate = l.AppliedDate ?? DateTime.Now
            };
        }

        private List<ManagerRecentActivityViewModel> BuildRecentActivities(List<int> teamIds)
        {
            var activities = new List<ManagerRecentActivityViewModel>();
            if (teamIds.Count == 0) return activities;

            var recentTasks = db.WorkTasks
                .Where(t => teamIds.Contains(t.AssignedTo))
                .OrderByDescending(t => t.LastUpdatedDate ?? t.CreatedDate)
                .Take(3)
                .ToList();

            foreach (var t in recentTasks)
            {
                activities.Add(new ManagerRecentActivityViewModel
                {
                    Type = "task",
                    Message = string.Format("Task \"{0}\" - {1}", t.Title, GetStatusText(t.Status)),
                    TimeAgo = FormatTimeAgo(t.LastUpdatedDate ?? t.CreatedDate)
                });
            }

            var recentLeaves = db.LeaveRequests
                .Where(l => teamIds.Contains(l.EmployeeID))
                .OrderByDescending(l => l.AppliedDate)
                .Take(3)
                .ToList();

            foreach (var l in recentLeaves)
            {
                var name = l.Employee != null ? l.Employee.FirstName + " " + l.Employee.LastName : "Employee";
                activities.Add(new ManagerRecentActivityViewModel
                {
                    Type = "leave",
                    Message = string.Format("{0} - leave {1}", name, l.Status == 1 ? "pending" : l.Status == 2 ? "approved" : "updated"),
                    TimeAgo = FormatTimeAgo(l.AppliedDate)
                });
            }

            return activities.OrderByDescending(a => a.TimeAgo).Take(6).ToList();
        }

        private string FormatTimeAgo(DateTime? date)
        {
            if (!date.HasValue) return "Recently";
            var span = DateTime.Now - date.Value;
            if (span.TotalMinutes < 60) return Math.Max(1, (int)span.TotalMinutes) + " min ago";
            if (span.TotalHours < 24) return (int)span.TotalHours + " hours ago";
            return (int)span.TotalDays + " days ago";
        }

        private ActionResult UpdateTeamTaskStatus(int taskId, int newStatus, string successMessage)
        {
            var managerId = GetManagerEmployeeId();
            if (!managerId.HasValue)
                return RedirectToAction("Approvals");

            var teamIds = GetTeamMemberIds(managerId.Value);
            var task = db.WorkTasks.Find(taskId);
            if (task == null || !teamIds.Contains(task.AssignedTo))
            {
                TempData["ErrorMessage"] = "Task not found or access denied.";
                return RedirectToAction("Approvals");
            }

            if (task.Status != 3)
            {
                TempData["ErrorMessage"] = "Only submitted tasks can be approved or rejected.";
                return RedirectToAction("Approvals");
            }

            task.Status = newStatus;
            task.LastUpdatedDate = DateTime.Now;
            if (newStatus == 5)
                task.CompletedDate = DateTime.Now;

            TrackActivity(
                newStatus == 4 ? "Task Approved" : "Task Sent Back",
                newStatus == 4 ? "Manager approved submitted task." : "Manager sent submitted task back for revision.",
                "WorkTasks",
                task.TaskID
            );
            Tracker().NotifyEmployeeById(
                task.AssignedTo,
                newStatus == 4 ? "Task Approved" : "Task Sent Back",
                newStatus == 4
                    ? "Your submitted task \"" + (task.Title ?? "Untitled") + "\" has been approved."
                    : "Your submitted task \"" + (task.Title ?? "Untitled") + "\" was sent back for revision.",
                newStatus == 4 ? 2 : 3,
                Url.Action("TaskDetails", "EmployeePanel", new { id = task.TaskID })
            );

            db.SaveChanges();
            TempData["SuccessMessage"] = successMessage;
            return RedirectToAction("Approvals");
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

        private void PopulateAssignTaskDropdowns(ManagerAssignTaskViewModel model, List<int> teamIds, Employee manager)
        {
            model.TeamMembers = new SelectList(
                db.Employees.Where(e => teamIds.Contains(e.EmployeeID))
                    .ToList()
                    .Select(e => new { e.EmployeeID, Name = e.FirstName + " " + e.LastName }),
                "EmployeeID", "Name", model.AssignedTo);

            if (manager != null)
            {
                model.Departments = new SelectList(
                    db.Departments.Where(d => d.DepartmentID == manager.DepartmentID && d.IsActive == true),
                    "DepartmentID", "DepartmentName", model.DepartmentID);
            }

            model.Priorities = new SelectList(new[]
            {
                new { Value = "1", Text = "Low" },
                new { Value = "2", Text = "Medium" },
                new { Value = "3", Text = "High" },
                new { Value = "4", Text = "Critical" }
            }, "Value", "Text", model.Priority);
        }

        private string GenerateTaskCode()
        {
            string yearMonth = DateTime.Now.ToString("yyyyMM");
            var lastTask = db.WorkTasks
                .Where(t => t.TaskCode.StartsWith("TASK-" + yearMonth))
                .OrderByDescending(t => t.TaskCode)
                .FirstOrDefault();

            int nextNumber = 1;
            if (lastTask != null)
            {
                string lastNumber = lastTask.TaskCode.Split('-').Last();
                int num;
                if (int.TryParse(lastNumber, out num))
                    nextNumber = num + 1;
            }

            return string.Format("TASK-{0}-{1:D4}", yearMonth, nextNumber);
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

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
