using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WorkOps.Models
{
    public class TaskListViewModel
    {
        public int TaskID { get; set; }
        public string TaskCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public string PriorityText { get; set; }
        public string PriorityColor { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public string AssignedTo { get; set; }
        public int AssignedToID { get; set; }
        public string AssignedBy { get; set; }
        public string Department { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysRemaining { get; set; }
    }

    public class CreateTaskViewModel
    {
        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
        [Display(Name = "Task Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        [StringLength(5000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Assigned to is required")]
        [Display(Name = "Assign To")]
        public int AssignedTo { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentID { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Estimated Hours")]
        [Range(0, 999, ErrorMessage = "Hours must be between 0 and 999")]
        public decimal? EstimatedHours { get; set; }

        public SelectList Employees { get; set; }
        public SelectList Departments { get; set; }
        public SelectList Priorities { get; set; }
    }

    public class EditTaskViewModel
    {
        public int TaskID { get; set; }

        [Required(ErrorMessage = "Task title is required")]
        [StringLength(200, MinimumLength = 3)]
        [Display(Name = "Task Title")]
        public string Title { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [Display(Name = "Priority")]
        public int Priority { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        public int Status { get; set; }

        [Required(ErrorMessage = "Assigned to is required")]
        [Display(Name = "Assign To")]
        public int AssignedTo { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentID { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Estimated Hours")]
        public decimal? EstimatedHours { get; set; }

        [Display(Name = "Actual Hours")]
        public decimal? ActualHours { get; set; }

        public SelectList Employees { get; set; }
        public SelectList Departments { get; set; }
        public SelectList Priorities { get; set; }
        public SelectList Statuses { get; set; }
    }

    public class TaskDetailsViewModel
    {
        public int TaskID { get; set; }
        public string TaskCode { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public string PriorityText { get; set; }
        public string PriorityColor { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string StatusColor { get; set; }
        public string AssignedTo { get; set; }
        public int AssignedToID { get; set; }
        public string AssignedToAvatar { get; set; }
        public string AssignedBy { get; set; }
        public string Department { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public decimal? EstimatedHours { get; set; }
        public decimal? ActualHours { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysRemaining { get; set; }
        public List<TaskCommentViewModel> Comments { get; set; }
        public List<TaskDocumentViewModel> Documents { get; set; }
    }

    public class TaskCommentViewModel
    {
        public int CommentID { get; set; }
        public int TaskID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeAvatar { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedDate { get; set; }
    }

    public class TaskDocumentViewModel
    {
        public int DocumentID { get; set; }
        public int TaskID { get; set; }
        public int EmployeeID { get; set; }
        public string EmployeeName { get; set; }
        public string DocumentName { get; set; }
        public string FilePath { get; set; }
        public long? FileSize { get; set; }
        public string FileType { get; set; }
        public string FileSizeFormatted { get; set; }
        public DateTime UploadDate { get; set; }
        public string Description { get; set; }
    }

    public class KanbanTaskViewModel
    {
        public int TaskID { get; set; }
        public string TaskCode { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public int Priority { get; set; }
        public string PriorityText { get; set; }
        public string PriorityColor { get; set; }
        public string AssignedTo { get; set; }
        public string AssignedToAvatar { get; set; }
        public DateTime DueDate { get; set; }
        public string DueDateIso { get; set; }
        public string DueDateDisplay { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class TaskFilterViewModel
    {
        [Display(Name = "Department")]
        public int? DepartmentID { get; set; }

        [Display(Name = "Assigned To")]
        public int? AssignedTo { get; set; }

        [Display(Name = "Priority")]
        public int? Priority { get; set; }

        [Display(Name = "Status")]
        public int? Status { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        public SelectList Departments { get; set; }
        public SelectList Employees { get; set; }
        public SelectList Priorities { get; set; }
        public SelectList Statuses { get; set; }
    }

    public class TaskStatisticsViewModel
    {
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int SubmittedTasks { get; set; }
        public int ApprovedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int HighPriorityTasks { get; set; }
        public int CriticalPriorityTasks { get; set; }
        public decimal CompletionRate { get; set; }
        public List<TaskStatusCount> StatusCounts { get; set; }
        public List<TaskPriorityCount> PriorityCounts { get; set; }
    }

    public class TaskStatusCount
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public string Color { get; set; }
    }

    public class TaskPriorityCount
    {
        public string Priority { get; set; }
        public int Count { get; set; }
        public string Color { get; set; }
    }
}