
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
    
public partial class UserRecommendations
{

    public System.Guid Id { get; set; }



    public virtual UserProfile UserProfileUserRecommendedBy { get; set; }

    public virtual UserProfile UserProfileUserRecommended { get; set; }

}

}
