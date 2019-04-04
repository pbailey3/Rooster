using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class ExternalBroadcastDTO
    {
        public Guid Id { get; set; }
        public Guid RosterId { get; set; }
        public Guid BusinessId { get; set; }
        public string BusinessName { get; set; }
        public Guid BusinessLocationId { get; set; }
        public string BusinessLocationName { get; set; }
        public AddressModelDTO BusinessLocationAddress { get; set; }
        public List<DropDownDTO> Roles { get; set; }
        public Guid Role { get; set; }
        public List<DropDownDTO> Employees { get; set; }
        public Guid Employee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishDate { get; set; }
        public DateTime FinishTime { get; set; }
        [Required]
        public string Description { get; set; }
        public double Distance { get; set; }
        public string EmployeeMessage { get; set; }
        // public Guid QualificationRSA { get; set; }
        //public bool QualificationRCG { get; set; }
        public List<DropDownDTO> Qualification { get; set; }
        public List<DropDownDTO> Skills { get; set; }
        public List<string> Skill { get; set; }
        [Required]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid price")]
        public decimal Wage { get; set; }
        public List<BroadcastedOpenShiftsDTO> BroadcastedOpenShiftList { get; set; }
        public BroadcastedOpenShiftsDTO BroadcastedOpenShift { get; set; }
        public List<ShiftDTO> Shifts { get; set; }
    }

    public class BroadcastedOpenShiftsDTO
    {
        public Guid BroadcastedOpenShiftId { get; set; }
        public BusinessDTO Business { get; set; }
        public DateTime Date { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
    }
}