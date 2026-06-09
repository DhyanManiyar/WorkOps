using System;
using System.Collections.Generic;
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
    public class EmployeeController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: Employee
        public ActionResult Index()
        {
            ViewBag.Title = "Employee Management";
            return View();
        }

        // GET: Employee/GetEmployees (for DataTable AJAX)
        public JsonResult GetEmployees()
        {
            var users = db.Users.ToList();
            var userById = users.ToDictionary(u => u.UserID);
            var roles = db.Roles.ToList();
            var roleById = roles.ToDictionary(r => r.RoleID);
            var departments = db.Departments.ToList();
            var deptById = departments.ToDictionary(d => d.DepartmentID);

            var employees = db.Employees.ToList().Select(e =>
            {
                userById.TryGetValue(e.UserID, out var user);
                string roleName = "Employee";
                if (user != null && roleById.TryGetValue(user.RoleID, out var role))
                    roleName = role.RoleName;

                string deptName = "";
                if (deptById.TryGetValue(e.DepartmentID, out var dept))
                    deptName = dept.DepartmentName;

                return new EmployeeListViewModel
                {
                    EmployeeID = e.EmployeeID,
                    FullName = e.FirstName + " " + e.LastName,
                    Email = user != null ? user.Email : "",
                    Phone = e.Phone,
                    Department = deptName,
                    Position = e.Position,
                    Manager = GetManagerName(e.ManagerID),
                    JoiningDate = e.JoiningDate,
                    Role = roleName,
                    IsActive = e.IsActive == true,
                    IsLocked = user != null && user.IsLocked == true,
                    Initials = GetInitials(e.FirstName, e.LastName)
                };
            })
            .OrderBy(e => e.FullName)
            .ToList();

            return Json(new { data = employees }, JsonRequestBehavior.AllowGet);
        }

        // GET: Employee/ProfilePhoto/5 — serves uploaded profile images from App_Data
        [HttpGet]
        public ActionResult ProfilePhoto(int id)
        {
            var employee = db.Employees.Find(id);
            if (employee == null || string.IsNullOrWhiteSpace(employee.ProfilePicture))
                return new HttpStatusCodeResult(404);

            var physicalPath = Server.MapPath(employee.ProfilePicture);
            if (!System.IO.File.Exists(physicalPath))
                return new HttpStatusCodeResult(404);

            return File(physicalPath, MimeMapping.GetMimeMapping(physicalPath));
        }

        // GET: Employee/GetDepartments (department names for filter dropdown)
        public JsonResult GetDepartments()
        {
            var names = db.Departments
                .Where(d => d.IsActive == true)
                .OrderBy(d => d.DepartmentName)
                .Select(d => d.DepartmentName)
                .ToList();

            return Json(new { data = names }, JsonRequestBehavior.AllowGet);
        }

        // Helper method to get manager name safely
        private string GetManagerName(int? managerId)
        {
            if (managerId == null || managerId == 0)
                return DisplayConstants.Empty;

            var manager = db.Employees.Find(managerId);
            if (manager != null)
                return manager.FirstName + " " + manager.LastName;

            return DisplayConstants.Empty;
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Add New Employee";
            ViewBag.AdminRoleId = GetRoleIdByName("Admin");

            var model = new CreateEmployeeViewModel
            {
                Departments = new SelectList(db.Departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName"),
                Managers = GetManagerSelectList(null),
                Roles = new SelectList(db.Roles.Where(r => r.IsActive == true), "RoleID", "RoleName"),
                JoiningDate = DateTime.Today,
                GeneratePassword = true
            };

            return View(model);
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(CreateEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (db.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    PopulateDropdowns(model);
                    ViewBag.AdminRoleId = GetRoleIdByName("Admin");
                    return View(model);
                }

                string password = model.GeneratePassword ? PasswordHelper.GenerateRandomPassword() : model.Password;

                // Validate password if not auto-generated
                if (!model.GeneratePassword)
                {
                    string passwordError;
                    if (!PasswordHelper.IsPasswordValid(password, out passwordError))
                    {
                        ModelState.AddModelError("Password", passwordError);
                        PopulateDropdowns(model);
                        ViewBag.AdminRoleId = GetRoleIdByName("Admin");
                        return View(model);
                    }
                }

                var adminRoleId = GetRoleIdByName("Admin");
                if (adminRoleId.HasValue && model.RoleID == adminRoleId.Value)
                    model.ManagerID = null;

                try
                {
                    // Create User
                    var user = new User
                    {
                        Email = model.Email,
                        PasswordHash = PasswordHelper.HashPassword(password),
                        RoleID = model.RoleID,
                        IsActive = true,
                        IsLocked = false,
                        FailedLoginAttempts = 0,
                        CreatedDate = DateTime.Now
                    };
                    db.Users.Add(user);
                    db.SaveChanges();

                    // Handle Profile Picture
                    string profilePicturePath = null;
                    if (model.ProfilePicture != null && model.ProfilePicture.ContentLength > 0)
                    {
                        string uploadFolder = Server.MapPath("~/App_Data/Uploads/ProfilePictures/");
                        if (!Directory.Exists(uploadFolder))
                            Directory.CreateDirectory(uploadFolder);

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                        string filePath = Path.Combine(uploadFolder, fileName);
                        model.ProfilePicture.SaveAs(filePath);
                        profilePicturePath = "~/App_Data/Uploads/ProfilePictures/" + fileName;
                    }

                    // Create Employee (UserID is int, not nullable)
                    var employee = new Employee
                    {
                        UserID = user.UserID,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Phone = model.Phone,
                        DepartmentID = model.DepartmentID,
                        ManagerID = model.ManagerID,
                        Position = model.Position,
                        JoiningDate = model.JoiningDate,
                        Salary = model.Salary,
                        Address = model.Address,
                        DateOfBirth = model.DateOfBirth,
                        ProfilePicture = profilePicturePath,
                        EmergencyContact = model.EmergencyContact,
                        EmergencyPhone = model.EmergencyPhone,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                    db.Employees.Add(employee);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = $"Employee created successfully! {(model.GeneratePassword ? $"Password: {password}" : "")}";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating employee: " + ex.Message);
                }
            }

            PopulateDropdowns(model);
            ViewBag.AdminRoleId = GetRoleIdByName("Admin");
            return View(model);
        }

        // GET: Employee/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Edit Employee";
            ViewBag.AdminRoleId = GetRoleIdByName("Admin");

            var employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            // Get User and Role safely
            var user = db.Users.Find(employee.UserID);
            int roleId = 3; // Default to Employee
            if (user != null && user.RoleID != null)
            {
                roleId = (int)user.RoleID;
            }

            var model = new EditEmployeeViewModel
            {
                EmployeeID = employee.EmployeeID,
                UserID = employee.UserID,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = user != null ? user.Email : "",
                Phone = employee.Phone,
                DateOfBirth = employee.DateOfBirth,
                Address = employee.Address,
                DepartmentID = employee.DepartmentID,
                Position = employee.Position,
                ManagerID = employee.ManagerID,
                JoiningDate = employee.JoiningDate,
                Salary = employee.Salary,
                EmergencyContact = employee.EmergencyContact,
                EmergencyPhone = employee.EmergencyPhone,
                RoleID = roleId,
                IsActive = (employee.IsActive == true),
                ExistingProfilePicture = employee.ProfilePicture
            };

            PopulateDropdowns(model);
            ViewBag.AdminRoleId = GetRoleIdByName("Admin");
            return View(model);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Email == model.Email && u.UserID != model.UserID))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    PopulateDropdowns(model);
                    ViewBag.AdminRoleId = GetRoleIdByName("Admin");
                    return View(model);
                }

                var adminRoleId = GetRoleIdByName("Admin");
                if (adminRoleId.HasValue && model.RoleID == adminRoleId.Value)
                    model.ManagerID = null;

                try
                {
                    var employee = db.Employees.Find(model.EmployeeID);
                    if (employee == null)
                    {
                        return HttpNotFound();
                    }

                    // Update Employee
                    employee.FirstName = model.FirstName;
                    employee.LastName = model.LastName;
                    employee.Phone = model.Phone;
                    employee.DepartmentID = model.DepartmentID;
                    employee.ManagerID = model.ManagerID;
                    employee.Position = model.Position;
                    employee.JoiningDate = model.JoiningDate;
                    employee.Salary = model.Salary;
                    employee.Address = model.Address;
                    employee.DateOfBirth = model.DateOfBirth;
                    employee.EmergencyContact = model.EmergencyContact;
                    employee.EmergencyPhone = model.EmergencyPhone;
                    employee.IsActive = model.IsActive;

                    // Handle Profile Picture
                    if (model.ProfilePicture != null && model.ProfilePicture.ContentLength > 0)
                    {
                        string uploadFolder = Server.MapPath("~/App_Data/Uploads/ProfilePictures/");
                        if (!Directory.Exists(uploadFolder))
                            Directory.CreateDirectory(uploadFolder);

                        // Delete old picture if exists
                        if (!string.IsNullOrEmpty(employee.ProfilePicture))
                        {
                            string oldPath = Server.MapPath(employee.ProfilePicture);
                            if (System.IO.File.Exists(oldPath))
                                System.IO.File.Delete(oldPath);
                        }

                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                        string filePath = Path.Combine(uploadFolder, fileName);
                        model.ProfilePicture.SaveAs(filePath);
                        employee.ProfilePicture = "~/App_Data/Uploads/ProfilePictures/" + fileName;
                    }

                    // Update User
                    var user = db.Users.Find(employee.UserID);
                    if (user != null)
                    {
                        user.Email = model.Email;
                        user.RoleID = model.RoleID;
                        user.IsActive = model.IsActive;
                    }

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating employee: " + ex.Message);
                }
            }

            PopulateDropdowns(model);
            ViewBag.AdminRoleId = GetRoleIdByName("Admin");
            return View(model);
        }

        // GET: Employee/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Employee Details";

            var employee = db.Employees.Find(id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            // Get User and Role safely
            var user = db.Users.Find(employee.UserID);
            string roleName = "Employee";
            if (user != null && user.Role != null)
            {
                roleName = user.Role.RoleName;
            }

            // Get manager name safely
            string managerName = DisplayConstants.Empty;
            if (employee.ManagerID != null && employee.ManagerID != 0)
            {
                var manager = db.Employees.Find(employee.ManagerID);
                if (manager != null)
                {
                    managerName = manager.FirstName + " " + manager.LastName;
                }
            }

            // Calculate statistics
            int totalTasks = db.WorkTasks.Count(t => t.AssignedTo == id);
            int completedTasks = db.WorkTasks.Count(t => t.AssignedTo == id && t.Status == 5);
            int pendingTasks = db.WorkTasks.Count(t => t.AssignedTo == id && t.Status < 3);
            int totalLeaves = db.LeaveRequests.Count(l => l.EmployeeID == id && l.Status == 2);

            // Calculate attendance rate for current month
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            var attendanceRecords = db.Attendances.Count(a => a.EmployeeID == id && a.AttendanceDate >= firstDayOfMonth && a.AttendanceDate <= today);
            var workingDays = DateTime.DaysInMonth(today.Year, today.Month);
            decimal attendanceRate = workingDays > 0 ? (decimal)attendanceRecords / workingDays * 100 : 0;

            var model = new EmployeeDetailsViewModel
            {
                EmployeeID = employee.EmployeeID,
                FullName = employee.FirstName + " " + employee.LastName,
                Email = user != null ? user.Email : "",
                Phone = employee.Phone,
                Department = employee.Department != null ? employee.Department.DepartmentName : "",
                Position = employee.Position,
                Manager = managerName,
                JoiningDate = employee.JoiningDate,
                DateOfBirth = employee.DateOfBirth,
                Address = employee.Address,
                EmergencyContact = employee.EmergencyContact,
                EmergencyPhone = employee.EmergencyPhone,
                Role = roleName,
                IsActive = (employee.IsActive == true),
                ProfilePicture = employee.ProfilePicture,
                Initials = GetInitials(employee.FirstName, employee.LastName),
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                TotalLeavesTaken = totalLeaves,
                AttendanceRate = attendanceRate
            };

            return View(model);
        }

        // POST: Employee/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var employee = db.Employees.Find(id);
                if (employee != null)
                {
                    // Soft delete - just deactivate
                    employee.IsActive = false;
                    var user = db.Users.Find(employee.UserID);
                    if (user != null)
                        user.IsActive = false;

                    db.SaveChanges();
                    return Json(new { success = true, message = "Employee deactivated successfully" });
                }
                return Json(new { success = false, message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Employee/ResetPassword/5
        [HttpPost]
        public JsonResult ResetPassword(int id)
        {
            try
            {
                var employee = db.Employees.Find(id);
                if (employee != null)
                {
                    string newPassword = PasswordHelper.GenerateRandomPassword();
                    var user = db.Users.Find(employee.UserID);
                    if (user != null)
                    {
                        user.PasswordHash = PasswordHelper.HashPassword(newPassword);
                        user.IsLocked = false;
                        user.FailedLoginAttempts = 0;
                        db.SaveChanges();
                        return Json(new
                        {
                            success = true,
                            message = "Password reset successfully. Copy the temporary password below.",
                            newPassword = newPassword
                        });
                    }
                }
                return Json(new { success = false, message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Employee/UnlockAccount/5
        [HttpPost]
        public JsonResult UnlockAccount(int id)
        {
            try
            {
                var employee = db.Employees.Find(id);
                if (employee == null)
                    return Json(new { success = false, message = "Employee not found" });

                var user = db.Users.Find(employee.UserID);
                if (user == null)
                    return Json(new { success = false, message = "User account not found" });

                user.IsLocked = false;
                user.FailedLoginAttempts = 0;
                db.SaveChanges();

                return Json(new { success = true, message = "Account unlocked successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Employee/ActivateAccount/5
        [HttpPost]
        public JsonResult ActivateAccount(int id)
        {
            try
            {
                var employee = db.Employees.Find(id);
                if (employee == null)
                    return Json(new { success = false, message = "Employee not found" });

                employee.IsActive = true;
                var user = db.Users.Find(employee.UserID);
                if (user != null)
                {
                    user.IsActive = true;
                    user.IsLocked = false;
                    user.FailedLoginAttempts = 0;
                }

                db.SaveChanges();
                return Json(new { success = true, message = "Employee account activated successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Employee/ToggleStatus/5
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            try
            {
                var employee = db.Employees.Find(id);
                if (employee != null)
                {
                    bool newStatus = !(employee.IsActive == true);
                    employee.IsActive = newStatus;
                    var user = db.Users.Find(employee.UserID);
                    if (user != null)
                        user.IsActive = newStatus;

                    db.SaveChanges();
                    string status = newStatus ? "activated" : "deactivated";
                    return Json(new { success = true, message = $"Employee {status} successfully", isActive = newStatus });
                }
                return Json(new { success = false, message = "Employee not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Export to CSV
        public ActionResult Export()
        {
            var employeesList = db.Employees.ToList();
            var employees = employeesList.Select(e => new
            {
                Name = e.FirstName + " " + e.LastName,
                Email = e.User != null ? e.User.Email : "",
                Phone = e.Phone,
                Department = e.Department != null ? e.Department.DepartmentName : "",
                Position = e.Position,
                JoiningDate = e.JoiningDate.ToString("dd/MM/yyyy"),
                Status = (e.IsActive == true) ? "Active" : "Inactive"
            }).ToList();

            var csvBytes = ExportHelper.ToCsvDynamic(employees, new string[] { "Name", "Email", "Phone", "Department", "Position", "JoiningDate", "Status" });
            return File(csvBytes, "text/csv", $"Employees_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private static string GetInitials(string firstName, string lastName)
        {
            var a = !string.IsNullOrEmpty(firstName) ? firstName.Substring(0, 1) : "?";
            var b = !string.IsNullOrEmpty(lastName) ? lastName.Substring(0, 1) : "?";
            return (a + b).ToUpper();
        }

        private int? GetRoleIdByName(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return null;
            var role = db.Roles.FirstOrDefault(r => r.RoleName == roleName);
            return role?.RoleID;
        }

        private SelectList GetManagerSelectList(int? selectedId, int? excludeEmployeeId = null)
        {
            var managerRoleId = GetRoleIdByName("Manager");
            if (!managerRoleId.HasValue)
                return new SelectList(Enumerable.Empty<SelectListItem>());

            var managerUserIds = db.Users
                .Where(u => u.RoleID == managerRoleId.Value)
                .Select(u => u.UserID)
                .ToList();

            var query = db.Employees.Where(e => e.IsActive == true && managerUserIds.Contains(e.UserID));
            if (excludeEmployeeId.HasValue)
                query = query.Where(e => e.EmployeeID != excludeEmployeeId.Value);

            var items = query
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new { e.EmployeeID, Name = e.FirstName + " " + e.LastName })
                .ToList();

            if (selectedId.HasValue && selectedId.Value > 0 && !items.Any(i => i.EmployeeID == selectedId.Value))
            {
                var selected = db.Employees.Find(selectedId.Value);
                if (selected != null)
                {
                    items.Add(new { selected.EmployeeID, Name = selected.FirstName + " " + selected.LastName });
                    items = items.OrderBy(i => i.Name).ToList();
                }
            }

            return new SelectList(items, "EmployeeID", "Name", selectedId);
        }

        private void PopulateDropdowns(CreateEmployeeViewModel model)
        {
            model.Departments = new SelectList(db.Departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName", model.DepartmentID);
            model.Managers = GetManagerSelectList(model.ManagerID);
            model.Roles = new SelectList(db.Roles.Where(r => r.IsActive == true), "RoleID", "RoleName", model.RoleID);
        }

        private void PopulateDropdowns(EditEmployeeViewModel model)
        {
            model.Departments = new SelectList(db.Departments.Where(d => d.IsActive == true), "DepartmentID", "DepartmentName", model.DepartmentID);
            model.Managers = GetManagerSelectList(model.ManagerID, model.EmployeeID);
            model.Roles = new SelectList(db.Roles.Where(r => r.IsActive == true), "RoleID", "RoleName", model.RoleID);
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