using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class QualificationLookupDTO
    {
        public System.Guid QualificationId { get; set; }

        public string QualificationName { get; set; }

        public int IndustryTypesID { get; set; }


        public virtual IndustryTypeDTO IndustryType { get; set; }

        public virtual ICollection<UserQualificationDTO> tblUserQualifications { get; set; }
    }
}