//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebUI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Schedule
    {
        public short Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public System.DateTime StartDate { get; set; }
        public System.TimeSpan StartTime { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public System.TimeSpan EndTime { get; set; }
        public short Frequency { get; set; }
        public ScheduleRecurrenceType ScheduleRecurrence { get; set; }
        public Nullable<short> NumberOfOccurrences { get; set; }
        public short MonthlyInterval { get; set; }
        public short DaysOfWeek { get; set; }
        public short WeeklyInterval { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
    }
}
