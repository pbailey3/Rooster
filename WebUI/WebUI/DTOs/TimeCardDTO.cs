using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public enum CheckInOutEnum
    {
        CheckIn = 0,
        CheckOut = 1
    }

    public class TimeCardDTO
    {
        public Guid Id { get; set; }
        public Guid RosterId { get; set; }
        public Guid TimesheetId { get; set; }
        public System.DateTime ClockIn { get; set; }
        public string ClockInComment { get; set; }
        public DateTime? ClockOut { get; set; }
        public string ClockOutComment { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public Guid EmployeeId { get; set; }
        public String EmployeeFirstName { get; set; }
        public String EmployeeLastName { get; set; }
        public Guid BusinessLocationId { get; set; }
        public Guid LastUpdatedById { get; set; }
        public String ShiftRoleName { get; set; }
        public String ShiftInternalLocationName { get; set; }


        [JsonIgnore]
        [Display(Name = "Clock-In")]
        public TimeSpan ClockInTime
        {
            get
            {
                return this.ClockIn.TimeOfDay;
            }
        }

        [JsonIgnore]
        [Display(Name = "Clock-Out")]
        public TimeSpan? ClockOutTime
        {
            get
            {
                if(this.ClockOut.HasValue)
                    return this.ClockOut.Value.TimeOfDay;
                else
                return null;
            }
        }

        // public TimesheetEntryDTO TimesheetEntry { get; set; }
    }

    public class ClockInOutReponseDTO
    {
        public CheckInOutEnum CheckInOut { get; set; }
        public string Message { get; set; }

        public string ShiftDetails { get; set; }
    }
}