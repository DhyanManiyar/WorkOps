using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Helpers;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [AdminOnly]
    public class TaskController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: Task
        public ActionResult Index()
        {
            ViewBag.Title = "Task Management";
            return View();
        }

        //// GET: Task/GetTasks (for DataTable AJAX)
        //public JsonResult GetTasks(TaskFilterViewModel filter)
        //{
        //    var query = db.WorkTasks.AsQueryable();

        //    if (filter.DepartmentID.HasValue)
        //        query = query.Where(t => t.DepartmentID == filter.DepartmentID);
        //    if (filter.AssignedTo.HasValue)
        //        query = query.Where(t => t.AssignedTo == filter.AssignedTo);
        //    if (filter.Priority.HasValue)
        //        query = query.Where(t => t.Priority == filter.Priority);
        //    if (filter.Status.HasValue)
        //        query = query.Where(t => t.Status == filter.Status);
        //    if (filter.StartDate.HasValue)
        //        query = query.Where(t => t.DueDate >= filter.StartDate);
        //    if (filter.EndDate.HasValue)
        //        query = query.Where(t => t.DueDate <= filter.EndDate);

        //    var tasks = query
        //        .Select(t => new TaskListViewModel
        //        {
        //            TaskID = t.TaskID,
        //            TaskCode = t.TaskCode,
        //            Title = t.Title,
        //            Priority = t.Priority,
        //            PriorityText = t.Priority == 1 ? "Low" : t.Priority == 2 ? "Medium" : t.Priority == 3 ? "High" : "Critical",
        //            PriorityColor = t.Priority == 1 ? "success" : t.Priority == 2 ? "info" : t.Priority == 3 ? "warning" : "danger",
        //            Status = t.Status,
        //            StatusText = t.Status == 1 ? "Pending" : t.Status == 2 ? "In Progress" : t.Status == 3 ? "Submitted" : t.Status == 4 ? "Approved" : "Completed",
        //            AssignedTo = t.Employee.FirstName + " " + t.Employee.LastName,
        //            AssignedToID = t.AssignedTo,
        //            Department = t.Department.DepartmentName,
        //            DueDate = t.DueDate,
        //            IsOverdue = t.DueDate < DateTime.Today && t.Status < 4,
        //            DaysRemaining = (t.DueDate - DateTime.Today).Days
        //        })
        //        .OrderByDescending(t => t.Priority)
        //        .ThenBy(t => t.DueDate)
        //        .ToList();

        //    return Json(new { data = tasks }, JsonRequestBehavior.AllowGet);
        //}

        // GET: Task/GetTasks (for DataTable AJAX)
        //public JsonResult GetTasks(TaskFilterViewModel filter)
        //{
        //    try
        //    {
        //        var query = db.WorkTasks.AsQueryable();

        //        if (filter.DepartmentID.HasValue && filter.DepartmentID.Value > 0)
        //            query = query.Where(t => t.DepartmentID == filter.DepartmentID.Value);
        //        if (filter.AssignedTo.HasValue && filter.AssignedTo.Value > 0)
        //            query = query.Where(t => t.AssignedTo == filter.AssignedTo.Value);
        //        if (filter.Priority.HasValue && filter.Priority.Value > 0)
        //            query = query.Where(t => t.Priority == filter.Priority.Value);
        //        if (filter.Status.HasValue && filter.Status.Value > 0)
        //            query = query.Where(t => t.Status == filter.Status.Value);
        //        if (filter.StartDate.HasValue)
        //            query = query.Where(t => t.DueDate >= filter.StartDate.Value);
        //        if (filter.EndDate.HasValue)
        //            query = query.Where(t => t.DueDate <= filter.EndDate.Value);

        //        var tasks = query
        //            .Select(t => new TaskListViewModel
        //            {
        //                TaskID = t.TaskID,
        //                TaskCode = t.TaskCode ?? "N/A",
        //                Title = t.Title ?? "Untitled",
        //                Priority = t.Priority,
        //                PriorityText = t.Priority == 1 ? "Low" : t.Priority == 2 ? "Medium" : t.Priority == 3 ? "High" : "Critical",
        //                PriorityColor = t.Priority == 1 ? "success" : t.Priority == 2 ? "info" : t.Priority == 3 ? "warning" : "danger",
        //                Status = t.Status,
        //                StatusText = t.Status == 1 ? "Pending" : t.Status == 2 ? "In Progress" : t.Status == 3 ? "Submitted" : t.Status == 4 ? "Approved" : "Completed",
        //                AssignedTo = t.Employee != null ? t.Employee.FirstName + " " + t.Employee.LastName : "Unassigned",
        //                AssignedToID = t.AssignedTo,
        //                Department = t.Department != null ? t.Department.DepartmentName : "Unknown",
        //                DueDate = t.DueDate,
        //                IsOverdue = t.DueDate < DateTime.Today && t.Status < 4,
        //                DaysRemaining = (t.DueDate - DateTime.Today).Days
        //            })
        //            .OrderByDescending(t => t.Priority)
        //            .ThenBy(t => t.DueDate)
        //            .ToList();

        //        return Json(new { data = tasks }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Error in GetTasks: " + ex.Message);
        //        return Json(new { data = new List<TaskListViewModel>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public JsonResult GetTasks(TaskFilterViewModel filter)
        {
            try
            {
                var query = db.WorkTasks.AsQueryable();

                if (filter.DepartmentID.HasValue && filter.DepartmentID.Value > 0)
                    query = query.Where(t => t.DepartmentID == filter.DepartmentID.Value);
                if (filter.AssignedTo.HasValue && filter.AssignedTo.Value > 0)
                    query = query.Where(t => t.AssignedTo == filter.AssignedTo.Value);
                if (filter.Priority.HasValue && filter.Priority.Value > 0)
                    query = query.Where(t => t.Priority == filter.Priority.Value);
                if (filter.Status.HasValue && filter.Status.Value > 0)
                    query = query.Where(t => t.Status == filter.Status.Value);

                var tasks = query.ToList();

                var result = tasks.Select(t => new
                {
                    t.TaskID,
                    t.TaskCode,
                    t.Title,
                    Priority = t.Priority,
                    PriorityText = t.Priority == 1 ? "Low" : t.Priority == 2 ? "Medium" : t.Priority == 3 ? "High" : "Critical",
                    Status = t.Status,
                    StatusText = t.Status == 1 ? "Pending" : t.Status == 2 ? "In Progress" : t.Status == 3 ? "Submitted" : t.Status == 4 ? "Approved" : "Completed",
                    AssignedTo = t.Employee != null ? t.Employee.FirstName + " " + t.Employee.LastName : "Unassigned",
                    Department = t.Department != null ? t.Department.DepartmentName : "Unknown",
                    DueDate = t.DueDate,
                    DueDateIso = t.DueDate.ToString("yyyy-MM-dd"),
                    IsOverdue = t.DueDate < DateTime.Today && t.Status < 4
                }).ToList();

                return Json(new { data = result }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ERROR in GetTasks: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack Trace: " + ex.StackTrace);
                return Json(new { data = new List<object>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Task/Board (Kanban)
        public ActionResult Board()
        {
            ViewBag.Title = "Kanban Board";
            return View("Kanban");
        }

        // GET: Task/GetKanbanTasks
        public JsonResult GetKanbanTasks()
        {
            var tasks = db.WorkTasks
                .Select(t => new KanbanTaskViewModel
                {
                    TaskID = t.TaskID,
                    TaskCode = t.TaskCode,
                    Title = t.Title,
                    Status = t.Status,
                    StatusText = t.Status == 1 ? "Pending" : t.Status == 2 ? "In Progress" : t.Status == 3 ? "Submitted" : t.Status == 4 ? "Approved" : "Completed",
                    Priority = t.Priority,
                    PriorityText = t.Priority == 1 ? "Low" : t.Priority == 2 ? "Medium" : t.Priority == 3 ? "High" : "Critical",
                    PriorityColor = t.Priority == 1 ? "success" : t.Priority == 2 ? "info" : t.Priority == 3 ? "warning" : "danger",
                    AssignedTo = t.Employee.FirstName + " " + t.Employee.LastName,
                    AssignedToAvatar = (t.Employee.FirstName.Substring(0, 1) + t.Employee.LastName.Substring(0, 1)).ToUpper(),
                    DueDate = t.DueDate,
                    IsOverdue = t.DueDate < DateTime.Today && t.Status < 4
                })
                .ToList()
                .Select(t =>
                {
                    t.DueDateIso = t.DueDate.ToString("yyyy-MM-dd");
                    t.DueDateDisplay = t.DueDate.ToString("dd/MM/yyyy");
                    return t;
                })
                .ToList();

            var grouped = new
            {
                pending = tasks.Where(t => t.Status == 1).ToList(),
                inProgress = tasks.Where(t => t.Status == 2).ToList(),
                submitted = tasks.Where(t => t.Status == 3).ToList(),
                approved = tasks.Where(t => t.Status == 4).ToList(),
                completed = tasks.Where(t => t.Status == 5).ToList()
            };

            return Json(grouped, JsonRequestBehavior.AllowGet);
        }

        // POST: Task/UpdateStatus (for Kanban drag & drop)
        [HttpPost]
        public JsonResult UpdateStatus(int taskId, int newStatus)
        {
            try
            {
                var task = db.WorkTasks.Find(taskId);
                if (task != null)
                {
                    task.Status = newStatus;
                    if (newStatus == 5)
                        task.CompletedDate = DateTime.Now;
                    task.LastUpdatedDate = DateTime.Now;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Task not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Task/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Create New Task";

            var model = new CreateTaskViewModel
            {
                Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName"),
                Departments = new SelectList(db.Departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName"),
                Priorities = new SelectList(new[]
                {
                    new { Value = "1", Text = "Low" },
                    new { Value = "2", Text = "Medium" },
                    new { Value = "3", Text = "High" },
                    new { Value = "4", Text = "Critical" }
                }, "Value", "Text"),
                DueDate = DateTime.Today.AddDays(7),
                StartDate = DateTime.Today
            };

            return View(model);
        }

        // POST: Task/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Generate unique task code
                    string taskCode = GenerateTaskCode();

                    var task = new WorkTask
                    {
                        TaskCode = taskCode,
                        Title = model.Title,
                        Description = model.Description,
                        Priority = model.Priority,
                        Status = 1, // Pending
                        AssignedTo = model.AssignedTo,
                        AssignedBy = SessionHelper.GetCurrentEmployeeId() ?? 1,
                        DepartmentID = model.DepartmentID,
                        StartDate = model.StartDate,
                        DueDate = model.DueDate,
                        EstimatedHours = model.EstimatedHours,
                        CreatedDate = DateTime.Now,
                        LastUpdatedDate = DateTime.Now
                    };

                    db.WorkTasks.Add(task);
                    db.SaveChanges();
                    var tracker = new ActivityNotificationService(db, Request);
                    var userId = SessionHelper.GetCurrentUserId() ?? 1;
                    tracker.LogActivity(
                        userId,
                        "Task Created",
                        "Created task \"" + (task.Title ?? "Untitled") + "\".",
                        "WorkTasks",
                        task.TaskID
                    );
                    tracker.NotifyEmployeeById(
                        task.AssignedTo,
                        "New Task Assigned",
                        "Task \"" + (task.Title ?? "Untitled") + "\" was assigned to you.",
                        1,
                        Url.Action("TaskDetails", "EmployeePanel", new { id = task.TaskID })
                    );
                    db.SaveChanges();

                    TempData["SuccessMessage"] = $"Task created successfully! Task Code: {taskCode}";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating task: " + ex.Message);
                }
            }

            PopulateDropdowns(model);
            return View(model);
        }

        // GET: Task/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Edit Task";

            var task = db.WorkTasks.Find(id);
            if (task == null)
            {
                return HttpNotFound();
            }

            var model = new EditTaskViewModel
            {
                TaskID = task.TaskID,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                Status = task.Status,
                AssignedTo = task.AssignedTo,
                DepartmentID = task.DepartmentID,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours
            };

            PopulateDropdowns(model);
            return View(model);
        }

        // POST: Task/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditTaskViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var task = db.WorkTasks.Find(model.TaskID);
                    if (task == null)
                    {
                        return HttpNotFound();
                    }

                    task.Title = model.Title;
                    task.Description = model.Description;
                    task.Priority = model.Priority;
                    task.Status = model.Status;
                    task.AssignedTo = model.AssignedTo;
                    task.DepartmentID = model.DepartmentID;
                    task.StartDate = model.StartDate;
                    task.DueDate = model.DueDate;
                    task.EstimatedHours = model.EstimatedHours;
                    task.ActualHours = model.ActualHours;
                    task.LastUpdatedDate = DateTime.Now;

                    if (model.Status == 5 && task.CompletedDate == null)
                        task.CompletedDate = DateTime.Now;
                    else if (model.Status != 5)
                        task.CompletedDate = null;

                    var tracker = new ActivityNotificationService(db, Request);
                    var userId = SessionHelper.GetCurrentUserId() ?? 1;
                    tracker.LogActivity(
                        userId,
                        "Task Updated",
                        "Updated task \"" + (task.Title ?? "Untitled") + "\".",
                        "WorkTasks",
                        task.TaskID
                    );
                    tracker.NotifyEmployeeById(
                        task.AssignedTo,
                        "Task Updated",
                        "Task \"" + (task.Title ?? "Untitled") + "\" has new updates.",
                        1,
                        Url.Action("TaskDetails", "EmployeePanel", new { id = task.TaskID })
                    );
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Task updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating task: " + ex.Message);
                }
            }

            PopulateDropdowns(model);
            return View(model);
        }

        // GET: Task/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Task Details";

            var task = db.WorkTasks
                .Include("Employee")
                .Include("Employee1")
                .Include("Department")
                .FirstOrDefault(t => t.TaskID == id);

            if (task == null)
            {
                return HttpNotFound();
            }

            // Get comments
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
                    EmployeeName = c.Employee.FirstName + " " + c.Employee.LastName,
                    EmployeeAvatar = (c.Employee.FirstName.Substring(0, 1) + c.Employee.LastName.Substring(0, 1)).ToUpper(),
                    CommentText = c.CommentText,
                    CreatedDate = c.CreatedDate ?? DateTime.Now,
                    IsEdited = c.IsEdited ?? false,
                    EditedDate = c.EditedDate
                }).ToList();

            // Get documents
            var documents = db.TaskDocuments
                .Where(d => d.TaskID == id)
                .Include("Employee")
                .ToList()
                .Select(d => new TaskDocumentViewModel
                {
                    DocumentID = d.DocumentID,
                    TaskID = d.TaskID,
                    EmployeeID = d.EmployeeID,
                    EmployeeName = d.Employee.FirstName + " " + d.Employee.LastName,
                    DocumentName = d.DocumentName,
                    FilePath = d.FilePath,
                    FileSize = d.FileSize,
                    FileType = d.FileType,
                    FileSizeFormatted = FormatFileSize(d.FileSize ?? 0),
                    UploadDate = d.UploadDate ?? DateTime.Now,
                    Description = d.Description
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
                AssignedTo = task.Employee.FirstName + " " + task.Employee.LastName,
                AssignedToID = task.AssignedTo,
                AssignedToAvatar = (task.Employee.FirstName.Substring(0, 1) + task.Employee.LastName.Substring(0, 1)).ToUpper(),
                AssignedBy = task.Employee1.FirstName + " " + task.Employee1.LastName,
                Department = task.Department.DepartmentName,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                CompletedDate = task.CompletedDate,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours,
                CreatedDate = task.CreatedDate ?? DateTime.Now,
                LastUpdatedDate = task.LastUpdatedDate,
                IsOverdue = task.DueDate < DateTime.Today && task.Status < 4,
                DaysRemaining = (task.DueDate - DateTime.Today).Days,
                Comments = comments,
                Documents = documents
            };

            return View(model);
        }

        // POST: Task/AddComment
        [HttpPost]
        public JsonResult AddComment(int taskId, string comment)
        {
            try
            {
                var employeeId = SessionHelper.GetCurrentEmployeeId() ?? 1;

                var taskComment = new TaskComment
                {
                    TaskID = taskId,
                    EmployeeID = employeeId,
                    CommentText = comment,
                    CreatedDate = DateTime.Now,
                    IsEdited = false
                };

                db.TaskComments.Add(taskComment);
                db.SaveChanges();

                var employee = db.Employees.Find(employeeId);

                return Json(new
                {
                    success = true,
                    commentId = taskComment.CommentID,
                    employeeName = employee.FirstName + " " + employee.LastName,
                    employeeAvatar = (employee.FirstName.Substring(0, 1) + employee.LastName.Substring(0, 1)).ToUpper(),
                    createdDate = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt"),
                    commentText = comment
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Task/UploadDocument
        [HttpPost]
        public JsonResult UploadDocument(int taskId, string description)
        {
            try
            {
                var file = Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    // Validate file size (10MB max)
                    if (file.ContentLength > 10485760)
                    {
                        return Json(new { success = false, message = "File size exceeds 10MB limit" });
                    }

                    // Validate file type
                    string extension = Path.GetExtension(file.FileName).ToLower();
                    string[] allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" };
                    if (!allowedExtensions.Contains(extension))
                    {
                        return Json(new { success = false, message = "File type not allowed" });
                    }

                    string uploadFolder = Server.MapPath("~/App_Data/Uploads/TaskDocuments/");
                    if (!Directory.Exists(uploadFolder))
                        Directory.CreateDirectory(uploadFolder);

                    string fileName = Guid.NewGuid().ToString() + extension;
                    string filePath = Path.Combine(uploadFolder, fileName);
                    file.SaveAs(filePath);

                    var taskDocument = new TaskDocument
                    {
                        TaskID = taskId,
                        EmployeeID = SessionHelper.GetCurrentEmployeeId() ?? 1,
                        DocumentName = file.FileName,
                        FilePath = "~/App_Data/Uploads/TaskDocuments/" + fileName,
                        FileSize = file.ContentLength,
                        FileType = extension.TrimStart('.'),
                        UploadDate = DateTime.Now,
                        Description = description
                    };

                    db.TaskDocuments.Add(taskDocument);
                    db.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        documentId = taskDocument.DocumentID,
                        documentName = file.FileName,
                        fileSize = FormatFileSize(file.ContentLength),
                        uploadDate = DateTime.Now.ToString("dd MMM yyyy"),
                        description = description
                    });
                }
                return Json(new { success = false, message = "No file selected" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Task/DownloadDocument/5
        public FileResult DownloadDocument(int id)
        {
            var document = db.TaskDocuments.Find(id);
            if (document != null)
            {
                string filePath = Server.MapPath(document.FilePath);
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, document.DocumentName);
            }
            return null;
        }

        // POST: Task/DeleteDocument/5
        [HttpPost]
        public JsonResult DeleteDocument(int id)
        {
            try
            {
                var document = db.TaskDocuments.Find(id);
                if (document != null)
                {
                    string filePath = Server.MapPath(document.FilePath);
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);

                    db.TaskDocuments.Remove(document);
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Document not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Task/GetStatistics
        public JsonResult GetStatistics()
        {
            var totalTasks = db.WorkTasks.Count();
            var pendingTasks = db.WorkTasks.Count(t => t.Status == 1);
            var inProgressTasks = db.WorkTasks.Count(t => t.Status == 2);
            var submittedTasks = db.WorkTasks.Count(t => t.Status == 3);
            var approvedTasks = db.WorkTasks.Count(t => t.Status == 4);
            var completedTasks = db.WorkTasks.Count(t => t.Status == 5);
            var overdueTasks = db.WorkTasks.Count(t => t.DueDate < DateTime.Today && t.Status < 4);
            var highPriorityTasks = db.WorkTasks.Count(t => t.Priority == 3);
            var criticalPriorityTasks = db.WorkTasks.Count(t => t.Priority == 4);
            var completionRate = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

            var statusCounts = new[]
            {
                new { Status = "Pending", Count = pendingTasks, Color = "#6c757d" },
                new { Status = "In Progress", Count = inProgressTasks, Color = "#007bff" },
                new { Status = "Submitted", Count = submittedTasks, Color = "#17a2b8" },
                new { Status = "Approved", Count = approvedTasks, Color = "#ffc107" },
                new { Status = "Completed", Count = completedTasks, Color = "#28a745" }
            };

            var priorityCounts = new[]
            {
                new { Priority = "Low", Count = db.WorkTasks.Count(t => t.Priority == 1), Color = "#28a745" },
                new { Priority = "Medium", Count = db.WorkTasks.Count(t => t.Priority == 2), Color = "#007bff" },
                new { Priority = "High", Count = highPriorityTasks, Color = "#fd7e14" },
                new { Priority = "Critical", Count = criticalPriorityTasks, Color = "#dc3545" }
            };

            return Json(new
            {
                totalTasks,
                pendingTasks,
                inProgressTasks,
                submittedTasks,
                approvedTasks,
                completedTasks,
                overdueTasks,
                highPriorityTasks,
                criticalPriorityTasks,
                completionRate = Math.Round(completionRate, 1),
                statusCounts,
                priorityCounts
            }, JsonRequestBehavior.AllowGet);
        }

        // GET: Task/GetFilterOptions
        public JsonResult GetFilterOptions()
        {
            var departments = db.Departments.Where(d => d.IsActive == true)
                .Select(d => new { id = d.DepartmentID, name = d.DepartmentName }).ToList();
            var employees = db.Employees.Where(e => e.IsActive == true)
                .Select(e => new { id = e.EmployeeID, name = e.FirstName + " " + e.LastName }).ToList();

            return Json(new { departments, employees }, JsonRequestBehavior.AllowGet);
        }

        // Helper Methods
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
                if (int.TryParse(lastNumber, out int num))
                    nextNumber = num + 1;
            }

            return $"TASK-{yearMonth}-{nextNumber:D4}";
        }

        private string GetPriorityText(int priority)
        {
            return priority == 1 ? "Low" : priority == 2 ? "Medium" : priority == 3 ? "High" : "Critical";
        }

        private string GetPriorityColor(int priority)
        {
            return priority == 1 ? "success" : priority == 2 ? "info" : priority == 3 ? "warning" : "danger";
        }

        private string GetStatusText(int status)
        {
            return status == 1 ? "Pending" : status == 2 ? "In Progress" : status == 3 ? "Submitted" : status == 4 ? "Approved" : "Completed";
        }

        private string GetStatusColor(int status)
        {
            return status == 1 ? "secondary" : status == 2 ? "primary" : status == 3 ? "info" : status == 4 ? "warning" : "success";
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1048576) return (bytes / 1024.0).ToString("F1") + " KB";
            return (bytes / 1048576.0).ToString("F1") + " MB";
        }

        private void PopulateDropdowns(CreateTaskViewModel model)
        {
            model.Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName", model.AssignedTo);
            model.Departments = new SelectList(db.Departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName", model.DepartmentID);
            model.Priorities = new SelectList(new[]
            {
                new { Value = "1", Text = "Low" },
                new { Value = "2", Text = "Medium" },
                new { Value = "3", Text = "High" },
                new { Value = "4", Text = "Critical" }
            }, "Value", "Text", model.Priority);
        }

        private void PopulateDropdowns(EditTaskViewModel model)
        {
            model.Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName", model.AssignedTo);
            model.Departments = new SelectList(db.Departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName", model.DepartmentID);
            model.Priorities = new SelectList(new[]
            {
                new { Value = "1", Text = "Low" },
                new { Value = "2", Text = "Medium" },
                new { Value = "3", Text = "High" },
                new { Value = "4", Text = "Critical" }
            }, "Value", "Text", model.Priority);
            model.Statuses = new SelectList(new[]
            {
                new { Value = "1", Text = "Pending" },
                new { Value = "2", Text = "In Progress" },
                new { Value = "3", Text = "Submitted" },
                new { Value = "4", Text = "Approved" },
                new { Value = "5", Text = "Completed" }
            }, "Value", "Text", model.Status);
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