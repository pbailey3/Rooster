using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class ShiftBlockDTO
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid BusinessLocationId { get; set; }
        public Guid RosterId { get; set; }

        [Display(Name = "Role")]
        public Guid RoleId { get; set; }
        [Display(Name = "Role")]
        public String RoleName { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Start")]
        public TimeSpan StartTime { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Finish")]
        public TimeSpan FinishTime { get; set; }

        [Display(Name = "Next day finish")]
        public bool FinishNextDay { get; set; }
    }
}