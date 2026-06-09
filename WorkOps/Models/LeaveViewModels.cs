using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WorkOps.Models
{
    public class LeaveDashboardViewModel
    {
        public int PendingLeaves { get; set; }
        public int ApprovedLeaves { get; set; }
        public int RejectedLeaves { get; set; }
        public int TotalLeavesThisMonth { get; set; }
        public decimal ApprovalRate { get; set; }
        public List<LeaveRequestViewModel> RecentRequests { get; set; }
        public List<LeaveBalanceAlertViewModel> LowBalanceAlerts { get; set; }
    }

    public class LeaveRequestViewModel
    {
        public int LeaveRequestID { get; set; }

        [Display(Name = "Employee")]
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string Initials { get; set; }

        [Display(Name = "Leave Type")]
        public int LeaveTypeID { get; set; }
        public string LeaveTypeName { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Total Days")]
        public int TotalDays { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500)]
        [Display(Name = "Reason")]
        public string Reason { get; set; }

        [Display(Name = "Status")]
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }

        [Display(Name = "Applied Date")]
        public DateTime AppliedDate { get; set; }

        [Display(Name = "Approved By")]
        public string ApprovedByName { get; set; }

        [Display(Name = "Approved Date")]
        public DateTime? ApprovedDate { get; set; }

        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; }

        public string AttachmentPath { get; set; }

        public SelectList LeaveTypes { get; set; }
        public SelectList Employees { get; set; }
        public SelectList StatusList { get; set; }
    }

    public class LeaveBalanceViewModel
    {
        public int BalanceID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string LeaveType { get; set; }
        public int Year { get; set; }
        public int TotalDays { get; set; }
        public int UsedDays { get; set; }
        public int RemainingDays { get; set; }
        public decimal UsagePercentage { get; set; }
        public string UsageColor => UsagePercentage >= 90 ? "danger" : UsagePercentage >= 75 ? "warning" : "success";
    }

    public class LeaveBalanceAlertViewModel
    {
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string LeaveType { get; set; }
        public int RemainingDays { get; set; }
        public decimal UsagePercentage { get; set; }
    }

    public class LeaveApprovalViewModel
    {
        public int LeaveRequestID { get; set; }

        [Required(ErrorMessage = "Action is required")]
        public string Action { get; set; }

        [Display(Name = "Comments")]
        [StringLength(500)]
        public string Comments { get; set; }
    }

    public class LeaveTypeViewModel
    {
        public int LeaveTypeID { get; set; }

        [Required(ErrorMessage = "Leave type name is required")]
        [StringLength(50)]
        [Display(Name = "Leave Type")]
        public string TypeName { get; set; }

        [StringLength(200)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Max days per year is required")]
        [Range(1, 365)]
        [Display(Name = "Max Days Per Year")]
        public int MaxDaysPerYear { get; set; }

        [Display(Name = "Paid Leave")]
        public bool IsPaid { get; set; }

        [Display(Name = "Requires Document")]
        public bool RequiresDocument { get; set; }
    }
}