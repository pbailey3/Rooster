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
    
    public partial class ShiftTemplate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ShiftTemplate()
        {
            this.Enabled = true;
            this.RecurringShiftChangeRequests = new HashSet<RecurringShiftChangeRequest>();
            this.Shifts = new HashSet<Shift>();
        }
    
        public System.Guid Id { get; set; }
        public System.TimeSpan StartTime { get; set; }
        public System.TimeSpan FinishTime { get; set; }
        public bool FinishNextDay { get; set; }
        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }
        public ShiftFrequency Frequency { get; set; }
        public Nullable<System.DateTime> WeekStarting { get; set; }
        public bool Enabled { get; set; }
    
        public virtual BusinessLocation BusinessLocation { get; set; }
        public virtual Role Role { get; set; }
        public virtual InternalLocation InternalLocation { get; set; }
        public virtual Employee Employee { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecurringShiftChangeRequest> RecurringShiftChangeRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Shift> Shifts { get; set; }
    }
}
