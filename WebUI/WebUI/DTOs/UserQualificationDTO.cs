using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class UserQualificationDTO
    {
        public System.Guid UserQualificationId { get; set; }

        public System.Guid QualificationslookupQualificationId { get; set; }

        public System.Guid UserProfileId { get; set; }



        public virtual QualificationLookupDTO Qualificationslookup { get; set; }

        public virtual UserProfilesDTO UserProfile { get; set; }
    }
}