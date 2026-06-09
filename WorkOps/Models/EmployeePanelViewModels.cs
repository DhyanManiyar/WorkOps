using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WorkOps.Models
{
    public class EmployeeDashboardViewModel
    {
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public string Position { get; set; }
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int SubmittedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal AttendanceRateMonth { get; set; }
        public string TodayAttendanceStatus { get; set; }
        public string TodayAttendanceColor { get; set; }
        public DateTime? TodayCheckIn { get; set; }
        public DateTime? TodayCheckOut { get; set; }
        public bool CanCheckIn { get; set; }
        public bool CanCheckOut { get; set; }
        public int TotalLeaveRemaining { get; set; }
        public int PendingLeaveRequests { get; set; }
        public List<EmployeeLeaveBalanceItemViewModel> LeaveBalances { get; set; }
        public List<TaskListViewModel> RecentTasks { get; set; }
    }

    public class EmployeeLeaveBalanceItemViewModel
    {
        public string LeaveType { get; set; }
        public int RemainingDays { get; set; }
        public int TotalDays { get; set; }
        public decimal UsagePercentage { get; set; }
    }

    public class EmployeeAttendanceViewModel
    {
        public DateTime CurrentDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? TotalHoursToday { get; set; }
        public int? LateMinutes { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public bool CanCheckIn { get; set; }
        public bool CanCheckOut { get; set; }
        public decimal MonthAttendanceRate { get; set; }
        public int PresentDaysThisMonth { get; set; }
        public List<EmployeeAttendanceHistoryViewModel> History { get; set; }
    }

    public class EmployeeAttendanceHistoryViewModel
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalHours { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public bool IsWeekend { get; set; }
    }

    public class EmployeeLeavesViewModel
    {
        public List<LeaveRequestViewModel> MyRequests { get; set; }
        public List<EmployeeLeaveBalanceItemViewModel> Balances { get; set; }
    }

    public class EmployeeApplyLeaveViewModel
    {
        [Required]
        [Display(Name = "Leave Type")]
        public int LeaveTypeID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Reason")]
        public string Reason { get; set; }

        [Display(Name = "Available Balance")]
        public int AvailableBalance { get; set; }

        public SelectList LeaveTypes { get; set; }
    }

    public class EmployeeProfileViewModel
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string ManagerName { get; set; }

        [Phone]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [StringLength(500)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "Emergency Contact")]
        public string EmergencyContact { get; set; }

        [Display(Name = "Emergency Phone")]
        public string EmergencyPhone { get; set; }

        public DateTime JoiningDate { get; set; }
        public string ProfilePicture { get; set; }
    }
}
