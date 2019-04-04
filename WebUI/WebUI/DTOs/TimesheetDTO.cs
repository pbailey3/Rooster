using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class TimesheetEntryDTO
    {
        public Guid Id { get; set; }
        public Guid TimesheetId { get; set; }

        [Display(Name = "Start date")]
        [DataType(DataType.Date)]
        public DateTime StartDay { get; set; }

        [Display(Name = "Start time")]
        //[Required(ErrorMessage = @"Please provide an end time.")]
        [RegularExpression(@"(20|21|22|23|[01]\d|\d)(([:][0-5]\d){1,2})", ErrorMessage = @"Please provide a valid time.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [JsonIgnore]
        public DateTime StartDateTime
        {
            get
            {
                return StartDay + StartTime;
            }
            set
            {
                this.StartDay = value.Date;
                this.StartTime = value.TimeOfDay;
            }
        }


        [Display(Name = "Finish date")]
        [DataType(DataType.Date)]
        public DateTime? FinishDay { get; set; }

        [Display(Name = "Finish time")]
        //[Required(ErrorMessage = @"Please provide an end time.")]
        [RegularExpression(@"(20|21|22|23|[01]\d|\d)(([:][0-5]\d){1,2})", ErrorMessage = @"Please provide a valid time.")]
        [DataType(DataType.Time)]
        public TimeSpan? FinishTime { get; set; }

        [JsonIgnore]
        public DateTime? FinishDateTime
        {
            get
            {
                if (!FinishDay.HasValue || !FinishTime.HasValue)
                    return null;
                else
                    return FinishDay.Value + FinishTime.Value;
            }
            set
            {
                if (value != null)
                {
                    this.FinishDay = value.Value.Date;
                    this.FinishTime = value.Value.TimeOfDay;
                }
            }
        }

        [JsonIgnore]
        public TimeSpan? ShiftLength
        {
            get
            {
                if (!FinishDay.HasValue || !FinishTime.HasValue)
                    return null;
                else
                    return this.FinishDateTime.Value - this.StartDateTime;
            }
        }
        public decimal PayRate { get; set; }
        public decimal Pay { get; set; }

        public bool Approved { get; set; }
        public bool ContainsErrors { get; set; }
        public DateTime? ApprovedDateTime { get; set; }
        public TimeCardDTO TimeCard { get; set; }
      
    }

    public class TimesheetWeekDTO 
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid BusinessLocationId { get; set; }
        public String BusinessLocationName { get; set; }
        public DateTime WeekStartDate { get; set; }
        public List<TimesheetEntryDTO> TimesheetEntries { get; set; }
        public bool Approved { get; set; }
    }

    public class TimesheetSummaryDTO
    {
        public Guid Id { get; set; }
        public DateTime WeekStartDate { get; set; }
        public double TotalHours{ get; set; }
        public int TotalShifts { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AvgHourlyRate { get; set; }
        public bool AllTimeCardsApproved{ get; set; }
    }

    public class TimesheetDTO
    {

        public Guid Id { get; set; }
        public Guid RosterId { get; set; }

        [Display(Name = "Week start")]
        [DataType(DataType.Date)]
        public DateTime WeekStartDate { get; set; }

        [Display(Name = "Week end")]
        [DataType(DataType.Date)]
        public DateTime WeekEndDate { get; set; }

        [Display(Name = "Total hours")]
        public double TotalHours { get; set; }

        public int TotalTimeSheetEntries { get; set; }

        [Display(Name = "Total cost")]
        public decimal TotalCost { get; set; }

        [Display(Name = "Average wage")]
        public decimal AverageWage { get; set; }

        public bool Approved { get; set; }
        public DateTime? ApprovedDateTime { get; set; }

       // public UserProfile ApprovedBy { get; set; }
       // public Roster Roster { get; set; }
        //
    }
    
}