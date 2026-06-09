using System;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Helpers;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [AdminOnly]
    public class DepartmentController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: Department
        public ActionResult Index()
        {
            ViewBag.Title = "Department Management";
            return View();
        }

        // GET: Department/GetDepartments (for DataTable AJAX)
        public JsonResult GetDepartments()
        {
            var departments = db.Departments
                .Select(d => new DepartmentListViewModel
                {
                    DepartmentID = d.DepartmentID,
                    DepartmentName = d.DepartmentName,
                    DepartmentCode = d.DepartmentCode,
                    Description = d.Description,
                    DepartmentHead = d.Employee != null ? d.Employee.FirstName + " " + d.Employee.LastName : DisplayConstants.Empty,
                    DepartmentHeadID = d.DepartmentHeadID,
                    EmployeeCount = d.Employees.Count(e => e.IsActive == true),
                    IsActive = d.IsActive ?? true,
                    CreatedDate = d.CreatedDate ?? DateTime.Now
                })
                .OrderBy(d => d.DepartmentName)
                .ToList();

            return Json(new { data = departments }, JsonRequestBehavior.AllowGet);
        }

        // GET: Department/Create
        public ActionResult Create()
        {
            ViewBag.Title = "Add New Department";

            var model = new CreateDepartmentViewModel
            {
                Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName")
            };

            return View(model);
        }

        // POST: Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateDepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if department code already exists
                if (db.Departments.Any(d => d.DepartmentCode == model.DepartmentCode))
                {
                    ModelState.AddModelError("DepartmentCode", "Department code already exists");
                    model.Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName");
                    return View(model);
                }

                try
                {
                    var department = new Department
                    {
                        DepartmentName = model.DepartmentName,
                        DepartmentCode = model.DepartmentCode.ToUpper(),
                        Description = model.Description,
                        DepartmentHeadID = model.DepartmentHeadID,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };

                    db.Departments.Add(department);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Department created successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating department: " + ex.Message);
                }
            }

            model.Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName");
            return View(model);
        }

        // GET: Department/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.Title = "Edit Department";

            var department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            var model = new EditDepartmentViewModel
            {
                DepartmentID = department.DepartmentID,
                DepartmentName = department.DepartmentName,
                Description = department.Description,
                DepartmentHeadID = department.DepartmentHeadID,
                IsActive = department.IsActive ?? true,
                Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName", department.DepartmentHeadID)
            };

            return View(model);
        }

        // POST: Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditDepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var department = db.Departments.Find(model.DepartmentID);
                    if (department == null)
                    {
                        return HttpNotFound();
                    }

                    department.DepartmentName = model.DepartmentName;
                    department.Description = model.Description;
                    department.DepartmentHeadID = model.DepartmentHeadID;
                    department.IsActive = model.IsActive;

                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Department updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating department: " + ex.Message);
                }
            }

            model.Employees = new SelectList(db.Employees.Where(e => e.IsActive == true), "EmployeeID", "FirstName", "LastName", model.DepartmentHeadID);
            return View(model);
        }

        // GET: Department/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.Title = "Department Details";

            var department = db.Departments.Find(id);
            if (department == null)
            {
                return HttpNotFound();
            }

            // Get employees in department
            var employees = db.Employees
                .Where(e => e.DepartmentID == id)
                .Select(e => new DepartmentEmployeeViewModel
                {
                    EmployeeID = e.EmployeeID,
                    FullName = e.FirstName + " " + e.LastName,
                    Position = e.Position,
                    Email = e.User.Email,
                    Phone = e.Phone,
                    JoiningDate = e.JoiningDate,
                    IsActive = e.IsActive ?? true,
                    Initials = (e.FirstName.Substring(0, 1) + e.LastName.Substring(0, 1)).ToUpper()
                })
                .ToList();

            // Get recent tasks from this department
            var recentTasks = db.WorkTasks
                .Where(t => t.DepartmentID == id)
                .OrderByDescending(t => t.CreatedDate)
                .Take(5)
                .Select(t => new DepartmentTaskViewModel
                {
                    TaskID = t.TaskID,
                    TaskCode = t.TaskCode,
                    Title = t.Title,
                    AssignedTo = t.Employee.FirstName + " " + t.Employee.LastName,
                    Priority = t.Priority == 1 ? "Low" : t.Priority == 2 ? "Medium" : t.Priority == 3 ? "High" : "Critical",
                    Status = t.Status == 1 ? "Pending" : t.Status == 2 ? "In Progress" : t.Status == 3 ? "Submitted" : t.Status == 4 ? "Approved" : "Completed",
                    DueDate = t.DueDate
                })
                .ToList();

            var model = new DepartmentDetailsViewModel
            {
                DepartmentID = department.DepartmentID,
                DepartmentName = department.DepartmentName,
                DepartmentCode = department.DepartmentCode,
                Description = department.Description,
                DepartmentHead = department.Employee != null ? department.Employee.FirstName + " " + department.Employee.LastName : DisplayConstants.Empty,
                DepartmentHeadID = department.DepartmentHeadID,
                IsActive = department.IsActive ?? true,
                CreatedDate = department.CreatedDate ?? DateTime.Now,
                EmployeeCount = employees.Count,
                Employees = employees,
                RecentTasks = recentTasks
            };

            return View(model);
        }

        // POST: Department/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var department = db.Departments.Find(id);
                if (department != null)
                {
                    // Check if department has employees
                    if (db.Employees.Any(e => e.DepartmentID == id && e.IsActive == true))
                    {
                        return Json(new { success = false, message = "Cannot delete department with active employees. Deactivate employees first." });
                    }

                    db.Departments.Remove(department);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Department deleted successfully" });
                }
                return Json(new { success = false, message = "Department not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Department/ToggleStatus/5
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            try
            {
                var department = db.Departments.Find(id);
                if (department != null)
                {
                    department.IsActive = !department.IsActive;
                    db.SaveChanges();
                    string status = department.IsActive == true ? "activated" : "deactivated";
                    return Json(new { success = true, message = $"Department {status} successfully", isActive = department.IsActive });
                }
                return Json(new { success = false, message = "Department not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Export to CSV
        public ActionResult Export()
        {
            var departments = db.Departments
                .Select(d => new
                {
                    DepartmentName = d.DepartmentName,
                    DepartmentCode = d.DepartmentCode,
                    Description = d.Description,
                    DepartmentHead = d.Employee != null ? d.Employee.FirstName + " " + d.Employee.LastName : DisplayConstants.Empty,
                    EmployeeCount = d.Employees.Count(e => e.IsActive == true),
                    Status = d.IsActive == true ? "Active" : "Inactive",
                    //CreatedDate = d.CreatedDate.ToString("dd/MM/yyyy")
                    CreatedDate = d.CreatedDate.HasValue ? d.CreatedDate.Value.ToString("dd/MM/yyyy") : ""
                })
                .ToList();

            var csvBytes = ExportHelper.ToCsvDynamic(departments, new string[] { "DepartmentName", "DepartmentCode", "Description", "DepartmentHead", "EmployeeCount", "Status", "CreatedDate" });
            return File(csvBytes, "text/csv", $"Departments_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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