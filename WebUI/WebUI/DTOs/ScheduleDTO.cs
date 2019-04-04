using Newtonsoft.Json;
using ScheduleWidget.Enums;
using ScheduleWidget.ScheduledEvents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI.DTOs
{
    public enum FrequencyEnum : int
    {
        OnceOff = 0,
        Daily = 1,
        Weekly = 2
    };

    public enum RecurrencePattern : int
    {
        OneTime = 0,
        Repeat = 1
    };

    public class ScheduleOccurrenceDTO
    {
        public int ScheduleID { get; set; }
        public DateTime OccurrenceDate { get; set; }
    }
 
        public class ScheduleDTO
    {
        private int _frequencyChoice;
        private DateTime? _endDate;
       
        public int? Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string Title { get; set; }
        [MaxLength(50)]
        public string Location { get; set; }
        [MaxLength(1000)]
        public string Description { get; set; }
        public RecurrencePattern ScheduleRecurrence { get; set; }

        [Display(Name = "Schedule")]
        public int? FrequencyChoice
        {
            get { return _frequencyChoice; }
            set
            {
                _frequencyChoice = value.GetValueOrDefault();
                CalculateFrequency();
                CalculateWeeklyInterval();
                CalculateRecurrencePattern();
            }
        }

        public int Frequency { get; set; }

        [Display(Name = "Days of week")]
        public int DaysOfWeek { get; set; }

        [Display(Name = "Weekly interval")]
        public int WeeklyInterval { get; set; }
        [Display(Name = "Monthly interval")]
        public int MonthlyInterval { get; set; }
        [Display(Name = "Number of occurrences")]
        public int? NumberOfOccurrences { get; set; }

        [Display(Name = "Start date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [JsonIgnore]
        public DateTime StartDateTime
        {
            get
            {
                return StartDate + StartTime;
            }
            set
            {
                StartDate = value.Date;
                StartTime = value.TimeOfDay;
            }
        }

        [JsonIgnore]
        public DateTime? EndDateTime
        {
            get
            {
                if (Frequency == 0) // one-time only 
                    return (StartDate + EndTime);
                return (_endDate.HasValue) ? _endDate : null;
           
                //return (_endDate.HasValue) ? (_endDate + EndTime) : (EndDate + EndTime);
            }
            set
            {
                _endDate = value;

                var ts = (EndDateTime - StartDate);
                if (!ts.HasValue)
                {
                    return;
                }

                if (ts.Value.Days == 0)
                    Frequency = 0;
            }
        }

        [Display(Name = "Start time")]
        //[Required(ErrorMessage = @"Please provide a start time.")]
        [RegularExpression(@"(20|21|22|23|[01]\d|\d)(([:][0-5]\d){1,2})", ErrorMessage = @"Please provide a valid time.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Display(Name = "End time")]
        //[Required(ErrorMessage = @"Please provide an end time.")]
        [RegularExpression(@"(20|21|22|23|[01]\d|\d)(([:][0-5]\d){1,2})", ErrorMessage = @"Please provide a valid time.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Display(Name = "End date")]
        [DataType(DataType.Date)]
       public DateTime? EndDate { get; set; }

        [JsonIgnore]
        [Display(Name = "Sunday")]
        public bool IsSundaySelected
        {
            get
            {
                return DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Sun);
            }
            set
            {
                if (!value) return;

                if (!DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Sun))
                {
                    DaysOfWeekOptions |= DayOfWeekEnum.Sun;
                }
            }
        }
        [JsonIgnore]
        [Display(Name = "Monday")]
        public bool IsMondaySelected
        {
            get
            {
                return DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Mon);
            }
            set
            {
                if (!value) return;

                if (!DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Mon))
                {
                    DaysOfWeekOptions |= DayOfWeekEnum.Mon;
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "Tuesday")]
        public bool IsTuesdaySelected
        {
            get
            {
                return DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Tue);
            }
            set
            {
                if (!value) return;

                if (!DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Tue))
                {
                    DaysOfWeekOptions |= DayOfWeekEnum.Tue;
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "Wednesday")]
        public bool IsWednesdaySelected
        {
            get
            {
                return DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Wed);
            }
            set
            {
                if (!value) return;

                if (!DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Wed))
                {
                    DaysOfWeekOptions |= DayOfWeekEnum.Wed;
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "Thursday")]
        public bool IsThursdaySelected
        {
            get
            {
                return DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Thu);
            }
            set
            {
                if (!value) return;

                if (!DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Thu))
                {
                    DaysOfWeekOptions |= DayOfWeekEnum.Thu;
                }
            }
        }
        
        [JsonIgnore]
        [Display(Name = "Friday")]
        public bool IsFridaySelected
        {
            get
            {
                return DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Fri);
            }
            set
            {
                if (!value) return;

                if (!DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Fri))
                {
                    DaysOfWeekOptions |= DayOfWeekEnum.Fri;
                }
            }
        }
        
        [JsonIgnore]
        [Display(Name = "Saturday")]
        public bool IsSaturdaySelected
        {
            get
            {
                return DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Sat);
            }
            set
            {
                if (!value) return;

                if (!DaysOfWeekOptions.HasFlag(DayOfWeekEnum.Sat))
                {
                    DaysOfWeekOptions |= DayOfWeekEnum.Sat;
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "First week of month")]
        public bool IsFirstWeekOfMonthSelected
        {
            get
            {
                return MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.First);
            }
            set
            {
                if (!value) return;

                if (!MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.First))
                {
                    MonthlyIntervalOptions |= MonthlyIntervalEnum.First;
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "Second week of month")]
        public bool IsSecondWeekOfMonthSelected
        {
            get
            {
                return MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.Second);
            }
            set
            {
                if (!value) return;

                if (!MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.Second))
                {
                    MonthlyIntervalOptions |= MonthlyIntervalEnum.Second;
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "Third week of month")]
        public bool IsThirdWeekOfMonthSelected
        {
            get
            {
                return MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.Third);
            }
            set
            {
                if (!value) return;

                if (!MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.Third))
                {
                    MonthlyIntervalOptions |= MonthlyIntervalEnum.Third;
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "Fourth week of month")]
        public bool IsFourthWeekOfMonthSelected
        {
            get
            {
                return MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.Fourth);
            }
            set
            {
                if (!value) return;

                if (!MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.Fourth))
                {
                    MonthlyIntervalOptions |= MonthlyIntervalEnum.Fourth;
                }
            }
        }

        [JsonIgnore]
        [Display(Name = "Last week of month")]
        public bool IsLastWeekOfMonthSelected
        {
            get
            {
                return MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.Last);
            }
            set
            {
                if (!value) return;

                if (!MonthlyIntervalOptions.HasFlag(MonthlyIntervalEnum.Last))
                {
                    MonthlyIntervalOptions |= MonthlyIntervalEnum.Last;
                }
            }
        }

        /// <summary>
        /// The frequency expressed as enumeration.
        /// </summary>
        [JsonIgnore]
        public FrequencyTypeEnum FrequencyTypeOptions
        {
            get
            {
                return (FrequencyTypeEnum)Frequency;
            }
            set
            {
                Frequency = (int)value;
            }
        }

        /// <summary>
        /// The monthly interval expressed as enumeration
        /// </summary>
        [JsonIgnore]
        public MonthlyIntervalEnum MonthlyIntervalOptions
        {
            get
            {
                return (MonthlyIntervalEnum)MonthlyInterval;
            }
            set
            {
                MonthlyInterval = (int)value;
            }
        }

        /// <summary>
        /// The days of the week expressed as enumeration.
        /// </summary>
        [JsonIgnore]
        public DayOfWeekEnum DaysOfWeekOptions
        {
            get
            {
                return (DayOfWeekEnum)DaysOfWeek;
            }
            set
            {
                DaysOfWeek = (int)value;
            }
        }

        [JsonIgnore]
        public Schedule Schedule
        {
            get
            {
                return BuildSchedule();
            }
        }

        /// <summary>
        /// Returns a schedule from the ScheduleWidget engine based on the 
        /// properties of this recurring schedule.
        /// </summary>
        /// <returns></returns>
        private Schedule BuildSchedule()
        {
            // create a new instance of each recurring event
            var recurringEvent = new Event()
            {
                ID = Id.GetValueOrDefault(),
                Title = this.Description,
                Frequency = Frequency,
                //WeeklyInterval = WeeklyInterval, //Deprecated: for new property RepeatInterval
                RepeatInterval = WeeklyInterval,
                MonthlyInterval = MonthlyInterval,
                StartDateTime = StartDate,
                EndDateTime = EndDate,
                DaysOfWeek = DaysOfWeek
            };

            if (IsOneTimeEvent())
            {
                recurringEvent.OneTimeOnlyEventDate = StartDate;
            }

            return new Schedule(recurringEvent);
        }

        private bool IsOneTimeEvent()
        {
            if (Frequency == 0 && DaysOfWeek == 0 && MonthlyInterval == 0)
                return true;

            return false;
        }

        private void CalculateFrequency()
        {
            switch (_frequencyChoice)
            {
                case 1:
                    Frequency = (int)FrequencyEnum.Daily; // daily
                    break;

                case 2:
                    Frequency = (int)FrequencyEnum.Weekly; // weekly
                    break;

                case 3:
                    Frequency = 2; // biweekly
                    break;

                case 4:
                    Frequency = 4; // monthly
                    break;

                default:
                    Frequency = (int)FrequencyEnum.OnceOff; // one-time only
                    break;
            }
        }

        private void CalculateWeeklyInterval()
        {
            switch (_frequencyChoice)
            {
                case 2:
                    WeeklyInterval = 1; // weekly
                    break;

                case 3:
                    WeeklyInterval = 2; // biweekly
                    break;

                default:
                    WeeklyInterval = 0;
                    break;
            }
        }

        private void CalculateRecurrencePattern()
        {
            // determine frequency from recurrence pattern
            if (ScheduleRecurrence == RecurrencePattern.OneTime)
            {
                Frequency = 0;
                EndDate = null;
            }
            else // repeat pattern
            {
                if (FrequencyTypeOptions == FrequencyTypeEnum.Daily)
                {
                    DaysOfWeekOptions =
                        DayOfWeekEnum.Sun |
                        DayOfWeekEnum.Mon |
                        DayOfWeekEnum.Tue |
                        DayOfWeekEnum.Wed |
                        DayOfWeekEnum.Thu |
                        DayOfWeekEnum.Fri |
                        DayOfWeekEnum.Sat;
                }
            }
        }
    }
}
