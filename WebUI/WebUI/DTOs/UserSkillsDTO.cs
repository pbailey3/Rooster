using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class UserSkillsDTO
    {
        public System.Guid UserSkillId { get; set; }

        public System.Guid IndustrySkillsId { get; set; }

        public System.Guid UserProfileId { get; set; }



        public virtual IndustrySkillsDTO IndustrySkill { get; set; }

        public virtual UserProfilesDTO UserProfile { get; set; }
    }
}