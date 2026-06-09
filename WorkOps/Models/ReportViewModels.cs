using System.Collections.Generic;

namespace WorkOps.Models
{
    public class ReportsIndexViewModel
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalTasks { get; set; }
        public int PendingLeaves { get; set; }
        public int TodayPresent { get; set; }
        public int TodayAbsent { get; set; }
        public decimal MonthAttendanceRate { get; set; }
        public int[] TaskStatusCounts { get; set; }
        public List<ReportDepartmentSummary> Departments { get; set; }
    }

    public class ReportDepartmentSummary
    {
        public string DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
        public int ActiveTasks { get; set; }
    }
}
