using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class WorkHistoryDTO
    {
        public System.Guid workId { get; set; }

        public string workCompanyName { get; set; }

        public System.DateTime workStartDate { get; set; }

        public System.DateTime workEndDate { get; set; }

        public string UserRole { get; set; }

        public virtual UserProfilesDTO UserProfile { get; set; }

    }
}