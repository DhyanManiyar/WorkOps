using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WorkOps.Models
{
    public class DepartmentListViewModel
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string Description { get; set; }
        public string DepartmentHead { get; set; }
        public int? DepartmentHeadID { get; set; }
        public int EmployeeCount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CreateDepartmentViewModel
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [Required(ErrorMessage = "Department code is required")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Department code must be between 2 and 20 characters")]
        [Display(Name = "Department Code")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Department code can only contain uppercase letters and numbers")]
        public string DepartmentCode { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Department Head")]
        public int? DepartmentHeadID { get; set; }

        public SelectList Employees { get; set; }
    }

    public class EditDepartmentViewModel
    {
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 100 characters")]
        [Display(Name = "Department Name")]
        public string DepartmentName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Department Head")]
        public int? DepartmentHeadID { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        public SelectList Employees { get; set; }
    }

    public class DepartmentDetailsViewModel
    {
        public int DepartmentID { get; set; }
        public string DepartmentName { get; set; }
        public string DepartmentCode { get; set; }
        public string Description { get; set; }
        public string DepartmentHead { get; set; }
        public int? DepartmentHeadID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int EmployeeCount { get; set; }
        public List<DepartmentEmployeeViewModel> Employees { get; set; }
        public List<DepartmentTaskViewModel> RecentTasks { get; set; }
    }

    public class DepartmentEmployeeViewModel
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime JoiningDate { get; set; }
        public bool IsActive { get; set; }
        public string Initials { get; set; }
    }

    public class DepartmentTaskViewModel
    {
        public int TaskID { get; set; }
        public string TaskCode { get; set; }
        public string Title { get; set; }
        public string AssignedTo { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
    }
}