using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WorkOps.Helpers;

namespace WorkOps.Models
{
    public class AttendanceDashboardViewModel
    {
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LeaveCount { get; set; }
        public int HalfDayCount { get; set; }
        public int TotalEmployees { get; set; }
        public decimal AttendancePercentage { get; set; }
        public DateTime CurrentDate { get; set; }
        public List<AttendanceRecordViewModel> TodayAttendance { get; set; }
    }

    public class AttendanceRecordViewModel
    {
        public int AttendanceID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Initials { get; set; }
        public DateTime AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? TotalHours { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public int LateMinutes { get; set; }
        public decimal? OvertimeHours { get; set; }
        public string Notes { get; set; }
        public string CheckInDisplay => CheckInTime?.ToString("hh:mm tt") ?? DisplayConstants.Empty;
        public string CheckOutDisplay => CheckOutTime?.ToString("hh:mm tt") ?? DisplayConstants.Empty;
    }

    public class MarkAttendanceViewModel
    {
        [Required(ErrorMessage = "Employee is required")]
        [Display(Name = "Employee")]
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Attendance Date")]
        public DateTime AttendanceDate { get; set; }

        [Display(Name = "Check In Time")]
        [DataType(DataType.Time)]
        public TimeSpan? CheckInTime { get; set; }

        [Display(Name = "Check Out Time")]
        [DataType(DataType.Time)]
        public TimeSpan? CheckOutTime { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public int Status { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        public SelectList Employees { get; set; }
        public SelectList StatusOptions { get; set; }
    }

    public class BulkAttendanceRowViewModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Initials { get; set; }
        public bool IsSelected { get; set; }
        public int CurrentStatus { get; set; }
        public string CurrentStatusText { get; set; }
        public bool HasExistingRecord { get; set; }
    }

    public class BulkMarkAttendanceViewModel
    {
        [Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Attendance Date")]
        public DateTime AttendanceDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status for selected employees")]
        public int Status { get; set; }

        [Display(Name = "Check In Time")]
        [DataType(DataType.Time)]
        public TimeSpan? CheckInTime { get; set; }

        [Display(Name = "Check Out Time")]
        [DataType(DataType.Time)]
        public TimeSpan? CheckOutTime { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        public List<int> SelectedEmployeeIds { get; set; } = new List<int>();
        public List<BulkAttendanceRowViewModel> Employees { get; set; } = new List<BulkAttendanceRowViewModel>();
        public SelectList StatusOptions { get; set; }
    }

    public class MonthlyAttendanceReportViewModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HalfDays { get; set; }
        public decimal TotalHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public int LateDays { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
}