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
    
    public partial class Employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Employee()
        {
            this.IsAdmin = false;
            this.IsActive = true;
            this.Roles = new HashSet<Role>();
            this.ActionedRequests = new HashSet<EmployerRequest>();
            this.ShiftTemplate = new HashSet<ShiftTemplate>();
            this.Shifts = new HashSet<Shift>();
            this.ActionedShiftChangeRequests = new HashSet<ShiftChangeRequest>();
            this.ManagerBusinessLocations = new HashSet<BusinessLocation>();
            this.RecurringShiftChangeRequests = new HashSet<RecurringShiftChangeRequest>();
            this.ShiftChangeRequest = new HashSet<ShiftChangeRequest>();
            this.TimeCards = new HashSet<TimeCard>();
            this.TimeCardsUpdated = new HashSet<TimeCard>();
            this.ExternalShiftRequestsActionedBy = new HashSet<ExternalShiftRequest>();
        }
    
        public System.Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MobilePhone { get; set; }
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        public string Email { get; set; }
        public EmployeeType Type { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public byte[] QRCode { get; set; }
        public Nullable<decimal> PayRate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Role> Roles { get; set; }
        public virtual UserProfile UserProfile { get; set; }
        public virtual EmployerRequest EmployerRequest { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmployerRequest> ActionedRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShiftTemplate> ShiftTemplate { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Shift> Shifts { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShiftChangeRequest> ActionedShiftChangeRequests { get; set; }
        public virtual EmployeeRequest EmployeeRequest { get; set; }
        public virtual BusinessLocation BusinessLocation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BusinessLocation> ManagerBusinessLocations { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RecurringShiftChangeRequest> RecurringShiftChangeRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShiftChangeRequest> ShiftChangeRequest { get; set; }
        public virtual PaymentDetails PaymentDetails { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TimeCard> TimeCards { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TimeCard> TimeCardsUpdated { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExternalShiftRequest> ExternalShiftRequestsActionedBy { get; set; }
    }
}
