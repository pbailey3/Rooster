
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
    
public partial class Messages
{

    public System.Guid Id { get; set; }

    public string Message { get; set; }

    public Nullable<System.DateTime> DateSent { get; set; }



    public virtual UserProfile UserProfilesMessageFrom { get; set; }

    public virtual UserProfile UserProfileMessageTo { get; set; }

}

}