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
    
    public partial class UserProfile
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UserProfile()
        {
            this.security_Roles = new HashSet<security_Role>();
            this.Employees = new HashSet<Employee>();
            this.EmployerRequests = new HashSet<EmployerRequest>();
            this.RecurringCalendarEvents = new HashSet<Schedule>();
            this.EmployeeRequests = new HashSet<EmployeeRequest>();
            this.OAuthMemberships = new HashSet<security_OAuthMembership>();
            this.ApprovedTimesheets = new HashSet<Timesheet>();
            this.TimesheetEntry = new HashSet<TimesheetEntry>();
            this.WorkHistories = new HashSet<WorkHistory>();
            this.UserQualifications = new HashSet<UserQualification>();
            this.OtherQualifications = new HashSet<OtherQualification>();
            this.UserSkills = new HashSet<UserSkills>();
            this.ExternalShiftRequestsCreatedBy = new HashSet<ExternalShiftRequest>();
            this.UserRecommendationUserRecommendedBy = new HashSet<UserRecommendations>();
            this.UserRecommendationsUserRecommended = new HashSet<UserRecommendations>();
            this.UserSkillEndorsementsUserRecommendedBY = new HashSet<UserSkillEndorsements>();
            this.UserSkillEndorsementsUserBeingRecommended = new HashSet<UserSkillEndorsements>();
            this.MessagesFrom = new HashSet<Messages>();
            this.MessagesTo = new HashSet<Messages>();
            this.Address = new Address();
            this.CurrentAddress = new Address();
        }
    
        public System.Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobilePhone { get; set; }
        public bool HasViewedWizard { get; set; }
        public byte[] QRCode { get; set; }
        public Nullable<int> Distance { get; set; }
        public Nullable<int> IndustryTypesID { get; set; }
        public string AboutMe { get; set; }
        public Nullable<System.DateTime> DateofBirth { get; set; }
        public Nullable<bool> IsRegisteredExternal { get; set; }
    
        public Address Address { get; set; }
        public Address CurrentAddress { get; set; }
    
        public virtual security_Membership Membership { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<security_Role> security_Roles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employees { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmployerRequest> EmployerRequests { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Schedule> RecurringCalendarEvents { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<EmployeeRequest> EmployeeRequests { get; set; }
        public virtual UserPreferences UserPreferences { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<security_OAuthMembership> OAuthMemberships { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Timesheet> ApprovedTimesheets { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TimesheetEntry> TimesheetEntry { get; set; }
        public virtual IndustryTypes IndustryType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<WorkHistory> WorkHistories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserQualification> UserQualifications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OtherQualification> OtherQualifications { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserSkills> UserSkills { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExternalShiftRequest> ExternalShiftRequestsCreatedBy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserRecommendations> UserRecommendationUserRecommendedBy { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserRecommendations> UserRecommendationsUserRecommended { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserSkillEndorsements> UserSkillEndorsementsUserRecommendedBY { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserSkillEndorsements> UserSkillEndorsementsUserBeingRecommended { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Messages> MessagesFrom { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Messages> MessagesTo { get; set; }
    }
}
