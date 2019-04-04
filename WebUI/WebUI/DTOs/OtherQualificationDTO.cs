using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class OtherQualificationDTO
    {
        public System.Guid Id { get; set; }

        public string Name { get; set; }

        public System.Guid UserProfileId { get; set; }



        public virtual UserProfilesDTO UserProfile { get; set; }
    }
}