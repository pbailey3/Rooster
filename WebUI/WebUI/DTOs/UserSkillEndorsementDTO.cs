using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUI.Models;

namespace WebUI.DTOs
{
    public class UserSkillEndorsementDTO
    {
        public Guid id { get; set; }
        public Guid UserSkill { get; set; }

        public Guid UserProfileUserRecommendedBY { get; set; }

        public Guid UserProfileUserBeingRecommended { get; set; }
    }
}