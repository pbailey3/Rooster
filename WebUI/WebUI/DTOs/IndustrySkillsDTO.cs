using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class IndustrySkillsDTO
    {
        public System.Guid Id { get; set; }

        public string Name { get; set; }

        public int IndustryTypesID { get; set; }



        public virtual IndustryTypeDTO IndustryType { get; set; }

        public virtual ICollection<UserSkillsDTO> UserSkills { get; set; }
    }
}