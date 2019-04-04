using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using ScheduleWidget.ScheduledEvents;
using ScheduleWidget.Enums;

namespace WebUI.DTOs
{
    public enum ShiftFrequencyDTO : int
    {
        Weekly = 0,
        Fortnight1 = 1,
        Fortnight2 = 2,
        Monthly = 3,
        OneOff = 4
    }
   
    public class ShiftTemplateDTO
    {
        public Guid Id { get; set; }
        public Guid BusinessId { get; set; }
        public Guid BusinessLocationId { get; set; }

        [Display(Name = "Employee")]
        public Guid? EmployeeId { get; set; }
        public String EmployeeFirstName { get; set; }
        public String EmployeeLastName { get; set; }
        //Properties
        [Display(Name = "Employee")]
        public string EmployeeFullName { get { return this.EmployeeFirstName + " " + this.EmployeeLastName; } }

        public string BusinessName { get; set; }
        public string BusinessLocationName { get; set; }

        [DataType(DataType.Time)]
        [Display(Name = "Start")]
        [DisplayFormat(DataFormatString = Common.Constants.FormatTimeSpan, ApplyFormatInEditMode = true)]
        public TimeSpan StartTime { get; set; }
       
        [DataType(DataType.Time)]
        [Display(Name = "Finish")]
        [DisplayFormat(DataFormatString = Common.Constants.FormatTimeSpan, ApplyFormatInEditMode=true)]
        public TimeSpan FinishTime { get; set; }

        [Display(Name = "Next day finish")]
        public bool FinishNextDay { get; set; }

        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        public ShiftFrequencyDTO Frequency { get; set; }

        public DateTime? WeekStarting { get; set; }

        [Required]
        [Display(Name = "Role")]
        public Guid? RoleId { get; set; }
        [Display(Name = "Role")]
       
        public String RoleName { get; set; }

        [Display(Name = "Internal Location")]
        public Guid? InternalLocationId { get; set; }
        [Display(Name = "Location")]
        public String InternalLocationName { get; set; }

        public bool Enabled { get; set; }

        [Display(Name = "Length")]
        public TimeSpan ShiftLength
        {
            get
            {
                return FinishTime - StartTime;
            }
        }

        public Schedule GetSchedule()
        {
            DayOfWeekEnum daysOfWeekOptions =  (this.Monday ? DayOfWeekEnum.Mon: 0) | 
                                    (this.Tuesday ? DayOfWeekEnum.Tue: 0) | 
                                    (this.Wednesday ? DayOfWeekEnum.Wed : 0) | 
                                    (this.Thursday ? DayOfWeekEnum.Thu : 0) | 
                                    (this.Friday ? DayOfWeekEnum.Fri : 0)  | 
                                    (this.Saturday ? DayOfWeekEnum.Sat : 0) | 
                                    (this.Sunday ? DayOfWeekEnum.Sun : 0);
           int frequency = 0;
           if (this.Frequency == ShiftFrequencyDTO.Weekly || this.Frequency == ShiftFrequencyDTO.Fortnight1 || this.Frequency == ShiftFrequencyDTO.Fortnight2)
                frequency = 2;
            else if (this.Frequency == ShiftFrequencyDTO.Monthly)
                frequency = 4;

           int weeklyInterval = 0;
           if (this.Frequency == ShiftFrequencyDTO.Weekly)
                weeklyInterval = 1;
           else if ( this.Frequency == ShiftFrequencyDTO.Fortnight1 || this.Frequency == ShiftFrequencyDTO.Fortnight2)
               weeklyInterval = 2;

            var aEvent = new Event
            {
                FrequencyTypeOptions = ScheduleWidget.Enums.FrequencyTypeEnum.Weekly,
                DaysOfWeekOptions = daysOfWeekOptions,
                Frequency = frequency,
                RepeatInterval = weeklyInterval
            };

           return new Schedule(aEvent);
        }
    }
    public class RecurringShiftChangeActionDTO
    {
        public Guid Id { get; set; }
        public string Reason { get; set; }
        public DateTime OccurenceDate { get; set; } 
    }
   
}