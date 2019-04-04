using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using WebUI.Common;
using System.Web.Mvc;

namespace WebUI.DTOs
{
    public enum EventFrequencyDTO : int
    {
        Weekly = 0,
        Fortnightly = 1,
        Monthly = 2,
        Annually = 3
    }
    public class CalendarEventDTO
    {
        public Guid Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        [MaxLength(50)]
        public string Location { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }
        [DataType(DataType.Date)]
        public DateTime FinishDate { get; set; }
        [DataType(DataType.Time)]
        public DateTime FinishTime { get; set; }

        //[MustBeSelectedAttribute(ErrorMessage = "Please Select Employee Type details")]
        //public EmployeeTypeDTO Type { get; set; }

    }


}