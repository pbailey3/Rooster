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
    
    public partial class WorkHistory
    {
        public System.Guid workId { get; set; }
        public string workCompanyName { get; set; }
        public System.DateTime workStartDate { get; set; }
        public System.DateTime workEndDate { get; set; }
        public System.Guid UserProfileId { get; set; }
        public string UserRole { get; set; }
    
        public virtual UserProfile UserProfile { get; set; }
    }
}
