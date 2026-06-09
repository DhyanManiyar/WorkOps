using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WorkOps.Filters;
using WorkOps.Models;

namespace WorkOps.Controllers
{
    [AdminOnly]
    public class AttendanceController : Controller
    {
        private WorkOpsDBEntities db = new WorkOpsDBEntities();

        // GET: Attendance
        public ActionResult Index()
        {
            ViewBag.Title = "Attendance Dashboard";

            var today = DateTime.Today;
            var model = new AttendanceDashboardViewModel
            {
                CurrentDate = today,
                TotalEmployees = db.Employees.Count(e => e.IsActive == true)
            };

            // Get today's attendance
            var todayAttendance = db.Attendances
                .Where(a => a.AttendanceDate == today)
                .ToList();

            model.PresentCount = todayAttendance.Count(a => a.Status == 1);
            model.AbsentCount = todayAttendance.Count(a => a.Status == 2);
            model.LeaveCount = todayAttendance.Count(a => a.Status == 4);
            model.HalfDayCount = todayAttendance.Count(a => a.Status == 3);

            model.AttendancePercentage = model.TotalEmployees > 0
                ? Math.Round((decimal)(model.PresentCount + model.HalfDayCount * 0.5m) / model.TotalEmployees * 100, 1)
                : 0;

            // Get today's detailed attendance
            var allEmployees = db.Employees.Where(e => e.IsActive == true).ToList();
            var attendanceRecords = new System.Collections.Generic.List<AttendanceRecordViewModel>();

            foreach (var emp in allEmployees)
            {
                var attendance = todayAttendance.FirstOrDefault(a => a.EmployeeID == emp.EmployeeID);

                attendanceRecords.Add(new AttendanceRecordViewModel
                {
                    AttendanceID = attendance?.AttendanceID ?? 0,
                    EmployeeID = emp.EmployeeID,
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    Department = emp.Department.DepartmentName,
                    Initials = (emp.FirstName.Substring(0, 1) + emp.LastName.Substring(0, 1)).ToUpper(),
                    AttendanceDate = today,
                    CheckInTime = attendance?.CheckInTime,
                    CheckOutTime = attendance?.CheckOutTime,
                    TotalHours = attendance?.TotalHours,
                    Status = attendance?.Status ?? 2, // Default to Absent if no record
                    LateMinutes = attendance?.LateMinutes ?? 0,
                    OvertimeHours = attendance?.OvertimeHours,
                    Notes = attendance?.Notes
                });
            }

            model.TodayAttendance = attendanceRecords.OrderBy(a => a.EmployeeName).ToList();

            return View(model);
        }

        // GET: Attendance/Mark
        public ActionResult Mark(int? employeeId, DateTime? date)
        {
            ViewBag.Title = "Mark Attendance";

            var attendanceDate = (date ?? DateTime.Today).Date;
            var model = new MarkAttendanceViewModel
            {
                AttendanceDate = attendanceDate,
                StatusOptions = new SelectList(new[]
                {
                    new { Value = "1", Text = "Present" },
                    new { Value = "2", Text = "Absent" },
                    new { Value = "3", Text = "Half Day" },
                    new { Value = "4", Text = "On Leave" }
                }, "Value", "Text")
            };

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                model.EmployeeID = employeeId.Value;
                var existing = db.Attendances
                    .FirstOrDefault(a => a.EmployeeID == employeeId.Value && a.AttendanceDate == attendanceDate);
                if (existing != null)
                {
                    model.CheckInTime = existing.CheckInTime?.TimeOfDay;
                    model.CheckOutTime = existing.CheckOutTime?.TimeOfDay;
                    model.Status = existing.Status;
                    model.Notes = existing.Notes;
                }
                else
                {
                    model.Status = 1;
                }
            }
            else
            {
                model.Status = 1;
            }

            model.Employees = GetEmployeeSelectList(model.EmployeeID);
            return View(model);
        }

        // POST: Attendance/Mark
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Mark(MarkAttendanceViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    SaveAttendanceRecord(
                        model.EmployeeID,
                        model.AttendanceDate,
                        model.Status,
                        model.CheckInTime,
                        model.CheckOutTime,
                        model.Notes);
                    TempData["SuccessMessage"] = "Attendance marked successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            model.Employees = GetEmployeeSelectList(model.EmployeeID);
            model.StatusOptions = new SelectList(new[]
            {
                new { Value = "1", Text = "Present" },
                new { Value = "2", Text = "Absent" },
                new { Value = "3", Text = "Half Day" },
                new { Value = "4", Text = "On Leave" }
            }, "Value", "Text", model.Status);

            return View(model);
        }

        // GET: Attendance/BulkMark
        public ActionResult BulkMark(DateTime? date)
        {
            ViewBag.Title = "Bulk Mark Attendance";
            var attendanceDate = (date ?? DateTime.Today).Date;
            var model = BuildBulkMarkViewModel(attendanceDate, null);
            return View(model);
        }

        // POST: Attendance/BulkMark
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkMark(BulkMarkAttendanceViewModel model)
        {
            ViewBag.Title = "Bulk Mark Attendance";
            model.AttendanceDate = model.AttendanceDate.Date;

            if (model.SelectedEmployeeIds == null || !model.SelectedEmployeeIds.Any())
            {
                ModelState.AddModelError("", "Select at least one employee.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var saved = 0;
                    foreach (var employeeId in model.SelectedEmployeeIds.Distinct())
                    {
                        SaveAttendanceRecord(
                            employeeId,
                            model.AttendanceDate,
                            model.Status,
                            model.CheckInTime,
                            model.CheckOutTime,
                            model.Notes);
                        saved++;
                    }

                    TempData["SuccessMessage"] = saved + " attendance record(s) saved successfully.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error: " + ex.Message);
                }
            }

            model = BuildBulkMarkViewModel(model.AttendanceDate, model);
            return View(model);
        }

        // GET: Attendance/Report
        public ActionResult Report(int? year, int? month)
        {
            ViewBag.Title = "Attendance Report";

            int selectedYear = year ?? DateTime.Now.Year;
            int selectedMonth = month ?? DateTime.Now.Month;

            var startDate = new DateTime(selectedYear, selectedMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var employees = db.Employees.Where(e => e.IsActive == true).ToList();
            var reportData = new System.Collections.Generic.List<MonthlyAttendanceReportViewModel>();
            var workingDaysForMonth = CalculateWorkingDaysExcludingHolidays(startDate, endDate);

            foreach (var emp in employees)
            {
                var attendances = db.Attendances
                    .Where(a => a.EmployeeID == emp.EmployeeID &&
                               a.AttendanceDate >= startDate &&
                               a.AttendanceDate <= endDate)
                    .ToList();

                reportData.Add(new MonthlyAttendanceReportViewModel
                {
                    EmployeeID = emp.EmployeeID,
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    Department = emp.Department.DepartmentName,
                    Year = selectedYear,
                    Month = selectedMonth,
                    PresentDays = attendances.Count(a => a.Status == 1),
                    AbsentDays = attendances.Count(a => a.Status == 2),
                    LeaveDays = attendances.Count(a => a.Status == 4),
                    HalfDays = attendances.Count(a => a.Status == 3),
                    TotalHours = attendances.Sum(a => a.TotalHours ?? 0),
                    OvertimeHours = attendances.Sum(a => a.OvertimeHours ?? 0),
                    LateDays = attendances.Count(a => a.LateMinutes > 0),
                    AttendancePercentage = workingDaysForMonth > 0
                        ? Math.Round(
                            (decimal)attendances.Count(a => a.Status == 1 || a.Status == 3)
                            / workingDaysForMonth * 100, 1)
                        : 0
                });
            }

            ViewBag.Year = selectedYear;
            ViewBag.Month = selectedMonth;
            ViewBag.Months = new SelectList(
                Enumerable.Range(1, 12).Select(m => new
                {
                    Value = m,
                    Text = new DateTime(2000, m, 1).ToString("MMMM")
                }),
                "Value",
                "Text",
                selectedMonth
            );
            ViewBag.Years = new SelectList(
                Enumerable.Range(2020, 10).Select(y => new { Value = y, Text = y.ToString() }),
                "Value",
                "Text",
                selectedYear
            );

            return View(reportData.OrderBy(r => r.EmployeeName).ToList());
        }

        // POST: Attendance/Delete/5
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var attendance = db.Attendances.Find(id);
                if (attendance != null)
                {
                    db.Attendances.Remove(attendance);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Attendance record deleted" });
                }
                return Json(new { success = false, message = "Record not found" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Export to CSV
        public ActionResult Export(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var data = db.Attendances
                .Where(a => a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
                .Select(a => new
                {
                    Employee = a.Employee.FirstName + " " + a.Employee.LastName,
                    Department = a.Employee.Department.DepartmentName,
                    Date = a.AttendanceDate,
                    CheckIn = a.CheckInTime,
                    CheckOut = a.CheckOutTime,
                    TotalHours = a.TotalHours,
                    Status = a.Status == 1 ? "Present" : a.Status == 2 ? "Absent" : a.Status == 3 ? "Half Day" : "On Leave",
                    LateMinutes = a.LateMinutes,
                    Overtime = a.OvertimeHours
                })
                .ToList();

            var csvBytes = System.Text.Encoding.UTF8.GetBytes("Employee,Department,Date,CheckIn,CheckOut,TotalHours,Status,LateMinutes,Overtime\n" +
                string.Join("\n", data.Select(d =>
                    $"{d.Employee},{d.Department},{d.Date:dd/MM/yyyy},{d.CheckIn?.ToString("hh:mm tt")},{d.CheckOut?.ToString("hh:mm tt")},{d.TotalHours},{d.Status},{d.LateMinutes},{d.Overtime}")));

            return File(csvBytes, "text/csv", $"Attendance_{year}_{month:00}.csv");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private BulkMarkAttendanceViewModel BuildBulkMarkViewModel(DateTime attendanceDate, BulkMarkAttendanceViewModel posted)
        {
            var selectedIds = posted?.SelectedEmployeeIds ?? new List<int>();
            var status = posted?.Status ?? 1;
            var todayRecords = db.Attendances
                .Where(a => a.AttendanceDate == attendanceDate)
                .ToList();

            var employees = db.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList()
                .Select(e =>
                {
                    var record = todayRecords.FirstOrDefault(a => a.EmployeeID == e.EmployeeID);
                    var currentStatus = record?.Status ?? 2;
                    return new BulkAttendanceRowViewModel
                    {
                        EmployeeID = e.EmployeeID,
                        EmployeeName = e.FirstName + " " + e.LastName,
                        Department = e.Department.DepartmentName,
                        Initials = (e.FirstName.Substring(0, 1) + e.LastName.Substring(0, 1)).ToUpper(),
                        IsSelected = selectedIds.Contains(e.EmployeeID),
                        CurrentStatus = currentStatus,
                        CurrentStatusText = currentStatus == 1 ? "Present" : currentStatus == 2 ? "Absent" : currentStatus == 3 ? "Half Day" : "On Leave",
                        HasExistingRecord = record != null
                    };
                })
                .ToList();

            return new BulkMarkAttendanceViewModel
            {
                AttendanceDate = attendanceDate,
                Status = status,
                CheckInTime = posted?.CheckInTime ?? new TimeSpan(9, 0, 0),
                CheckOutTime = posted?.CheckOutTime ?? new TimeSpan(18, 0, 0),
                Notes = posted?.Notes,
                SelectedEmployeeIds = selectedIds,
                Employees = employees,
                StatusOptions = new SelectList(new[]
                {
                    new { Value = "1", Text = "Present" },
                    new { Value = "2", Text = "Absent" },
                    new { Value = "3", Text = "Half Day" },
                    new { Value = "4", Text = "On Leave" }
                }, "Value", "Text", status)
            };
        }

        private void SaveAttendanceRecord(int employeeId, DateTime attendanceDate, int status, TimeSpan? checkInTime, TimeSpan? checkOutTime, string notes)
        {
            var existing = db.Attendances
                .FirstOrDefault(a => a.EmployeeID == employeeId && a.AttendanceDate == attendanceDate);

            DateTime? checkIn = null;
            DateTime? checkOut = null;

            if (checkInTime.HasValue)
                checkIn = attendanceDate.Date.Add(checkInTime.Value);
            if (checkOutTime.HasValue)
                checkOut = attendanceDate.Date.Add(checkOutTime.Value);

            decimal? totalHours = null;
            if (checkIn.HasValue && checkOut.HasValue)
                totalHours = (decimal)(checkOut.Value - checkIn.Value).TotalHours;

            if (existing != null)
            {
                existing.CheckInTime = checkIn;
                existing.CheckOutTime = checkOut;
                existing.TotalHours = totalHours;
                existing.Status = status;
                existing.Notes = notes;
                existing.LateMinutes = checkIn.HasValue && checkIn.Value.TimeOfDay > new TimeSpan(9, 30, 0)
                    ? (int)(checkIn.Value.TimeOfDay - new TimeSpan(9, 30, 0)).TotalMinutes
                    : 0;
            }
            else
            {
                var attendance = new Attendance
                {
                    EmployeeID = employeeId,
                    AttendanceDate = attendanceDate,
                    CheckInTime = checkIn,
                    CheckOutTime = checkOut,
                    TotalHours = totalHours,
                    Status = status,
                    Notes = notes,
                    CreatedDate = DateTime.Now,
                    LateMinutes = checkIn.HasValue && checkIn.Value.TimeOfDay > new TimeSpan(9, 30, 0)
                        ? (int)(checkIn.Value.TimeOfDay - new TimeSpan(9, 30, 0)).TotalMinutes
                        : 0
                };
                db.Attendances.Add(attendance);
            }

            db.SaveChanges();
        }

        private SelectList GetEmployeeSelectList(int? selectedEmployeeId)
        {
            var employeeItems = db.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new
                {
                    e.EmployeeID,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();

            return new SelectList(employeeItems, "EmployeeID", "FullName", selectedEmployeeId);
        }

        private int CalculateWorkingDaysExcludingHolidays(DateTime startDate, DateTime endDate)
        {
            var oneTimeHolidays = db.Holidays
                .Where(h => !(h.IsRecurring ?? false) && h.HolidayDate >= startDate && h.HolidayDate <= endDate)
                .Select(h => h.HolidayDate)
                .ToList();

            var recurringHolidayMd = db.Holidays
                .Where(h => h.IsRecurring == true)
                .Select(h => new { h.HolidayDate.Month, h.HolidayDate.Day })
                .ToList();

            int workingDays = 0;
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var isOneTimeHoliday = oneTimeHolidays.Any(h => h == date);
                var isRecurringHoliday = recurringHolidayMd.Any(h => h.Month == date.Month && h.Day == date.Day);
                if (isOneTimeHoliday || isRecurringHoliday)
                    continue;

                workingDays++;
            }
            return workingDays;
        }
    }
}