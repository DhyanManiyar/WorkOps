using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkOps.Models
{
    public class HolidayListItemViewModel
    {
        public int HolidayID { get; set; }
        public string HolidayName { get; set; }
        public DateTime HolidayDate { get; set; }
        public bool IsRecurring { get; set; }
        public string Description { get; set; }
    }

    public class HolidayIndexViewModel
    {
        public List<HolidayListItemViewModel> Holidays { get; set; }
        public int Year { get; set; }
    }

    public class HolidayFormViewModel
    {
        public int HolidayID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Holiday Name")]
        public string HolidayName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Holiday Date")]
        public DateTime HolidayDate { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string Description { get; set; }

        [Display(Name = "Recurring every year")]
        public bool IsRecurring { get; set; }
    }
}
