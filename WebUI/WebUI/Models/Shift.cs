
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
    
public partial class Shift
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Shift()
    {

        this.ShiftChangeRequests = new HashSet<ShiftChangeRequest>();

    }


    public System.Guid Id { get; set; }

    public System.DateTime StartTime { get; set; }

    public System.DateTime FinishTime { get; set; }

    public bool IsPublished { get; set; }

    public Nullable<System.Guid> ExternalShiftBroadcastId { get; set; }



    public virtual Roster Roster { get; set; }

    public virtual Employee Employee { get; set; }

    public virtual Role Role { get; set; }

    public virtual InternalLocation InternalLocation { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<ShiftChangeRequest> ShiftChangeRequests { get; set; }

    public virtual ShiftTemplate ShiftTemplate { get; set; }

    public virtual TimeCard TimeCard { get; set; }

    public virtual ExternalShiftBroadcast ExternalShiftBroadcast { get; set; }

}

}