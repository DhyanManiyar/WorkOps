using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WorkOps.Models
{
    public class ManagerDashboardViewModel
    {
        public string ManagerName { get; set; }
        public string DepartmentName { get; set; }
        public int TeamSize { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int SubmittedTasks { get; set; }
        public int PendingLeaves { get; set; }
        public int PresentToday { get; set; }
        public int OnLeaveToday { get; set; }
        public List<TeamMemberViewModel> TeamMembers { get; set; }
        public List<ManagerRecentActivityViewModel> RecentActivities { get; set; }
        public int[] TaskStatusCounts { get; set; }
    }

    public class TeamMemberViewModel
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Initials { get; set; }
        public int ActiveTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public string TodayAttendance { get; set; }
        public string TodayAttendanceColor { get; set; }
        public string ProfilePicture { get; set; }
        public bool IsActive { get; set; }
    }

    public class ManagerTaskViewModel
    {
        public int TaskID { get; set; }
        public string TaskCode { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public string PriorityText { get; set; }
        public string PriorityColor { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public string AssignedToName { get; set; }
        public int AssignedToID { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class ManagerApprovalsViewModel
    {
        public List<ManagerTaskViewModel> PendingTaskApprovals { get; set; }
        public List<LeaveRequestViewModel> PendingLeaveApprovals { get; set; }
    }

    public class ManagerAssignTaskViewModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(1, 4)]
        public int Priority { get; set; }

        [Required]
        public int AssignedTo { get; set; }

        [Required]
        public int DepartmentID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        public decimal? EstimatedHours { get; set; }

        public SelectList TeamMembers { get; set; }
        public SelectList Departments { get; set; }
        public SelectList Priorities { get; set; }
    }

    public class ManagerReportsViewModel
    {
        public int TeamSize { get; set; }
        public int TotalTeamTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal TaskCompletionRate { get; set; }
        public decimal TeamAttendanceRate { get; set; }
        public int ApprovedLeavesThisMonth { get; set; }
        public int PendingLeaves { get; set; }
        public List<TeamMemberPerformanceViewModel> MemberPerformance { get; set; }
        public string[] ChartLabels { get; set; }
        public int[] ChartCompleted { get; set; }
        public int[] ChartPending { get; set; }
    }

    public class TeamMemberPerformanceViewModel
    {
        public string EmployeeName { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AttendanceRate { get; set; }
    }

    public class ManagerRecentActivityViewModel
    {
        public string Type { get; set; }
        public string Message { get; set; }
        public string TimeAgo { get; set; }
    }
}
