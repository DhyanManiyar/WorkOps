using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace WorkOps.Models
{
    
    public class EmployeeListViewModel
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Manager { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsLocked { get; set; }
        public string ProfilePicture { get; set; }
        public string Initials { get; set; }
    }

    public class CreateEmployeeViewModel
    {
        // Personal Information
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        // Professional Information
        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(100)]
        [Display(Name = "Position")]
        public string Position { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerID { get; set; }

        [Required(ErrorMessage = "Joining date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; }

        [Display(Name = "Salary")]
        [DataType(DataType.Currency)]
        public decimal? Salary { get; set; }

        // Emergency Contact
        [Display(Name = "Emergency Contact Name")]
        [StringLength(100)]
        public string EmergencyContact { get; set; }

        [Display(Name = "Emergency Phone")]
        [Phone]
        public string EmergencyPhone { get; set; }

        // Account Information
        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public int RoleID { get; set; }

        [Display(Name = "Generate Password")]
        public bool GeneratePassword { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        // Profile Picture
        public HttpPostedFileBase ProfilePicture { get; set; }

        // Dropdown Lists
        public SelectList Departments { get; set; }
        public SelectList Managers { get; set; }
        public SelectList Roles { get; set; }
    }

    public class EditEmployeeViewModel
    {
        public int EmployeeID { get; set; }
        public int UserID { get; set; }

        // Personal Information
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Address")]
        [StringLength(500)]
        public string Address { get; set; }

        // Professional Information
        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public int DepartmentID { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(100)]
        [Display(Name = "Position")]
        public string Position { get; set; }

        [Display(Name = "Manager")]
        public int? ManagerID { get; set; }

        [Required(ErrorMessage = "Joining date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Joining Date")]
        public DateTime JoiningDate { get; set; }

        [Display(Name = "Salary")]
        [DataType(DataType.Currency)]
        public decimal? Salary { get; set; }

        // Emergency Contact
        [Display(Name = "Emergency Contact Name")]
        [StringLength(100)]
        public string EmergencyContact { get; set; }

        [Display(Name = "Emergency Phone")]
        [Phone]
        public string EmergencyPhone { get; set; }

        // Account Information
        [Required(ErrorMessage = "Role is required")]
        [Display(Name = "Role")]
        public int RoleID { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }

        // Profile Picture
        public string ExistingProfilePicture { get; set; }
        public HttpPostedFileBase ProfilePicture { get; set; }

        // Dropdown Lists
        public SelectList Departments { get; set; }
        public SelectList Managers { get; set; }
        public SelectList Roles { get; set; }
    }

    public class EmployeeDetailsViewModel
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string Manager { get; set; }
        public DateTime JoiningDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public string EmergencyPhone { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string ProfilePicture { get; set; }
        public string Initials { get; set; }

        // Statistics
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
        public int TotalLeavesTaken { get; set; }
        public decimal AttendanceRate { get; set; }
    }
}