
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
    
public partial class ShiftBlock
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public ShiftBlock()
    {

        this.FinishNextDay = false;

    }


    public System.Guid Id { get; set; }

    public System.TimeSpan StartTime { get; set; }

    public System.TimeSpan FinishTime { get; set; }

    public bool FinishNextDay { get; set; }



    public virtual Role Role { get; set; }

    public virtual BusinessLocation BusinessLocation { get; set; }

}

}