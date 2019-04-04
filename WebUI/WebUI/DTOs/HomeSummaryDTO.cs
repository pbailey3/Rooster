using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.DTOs
{
    public class HomeSummaryDTO
    {
        public dynamic UpcomingShifts { get; set; }

        public bool HasViewedWizard { get; set; }

        public List<BusinessLocationSummaryDTO> Employers { get; set; }

        public List<ExternalBroadcastDTO> ExternalShiftBroadCast { get; set; }
        public ExternalShiftEmployeeIndexDTO ExternalShiftBroadCastEmpoyee { get; set; }

        public OpenShiftsEmployeeIndexDTO OpenShiftsEmployee { get; set; }

        public List<DropDownDTO> IndustryType { get; set; }
        public List<DropDownDTO> SiteSearch { get; set; }

        public UserProfilesDTO Address { get; set; }

        public List<DropDownDTO> LocationRange { get; set; }
    }

    public enum RequestTypeDTO : int
    {
        Employer = 0,
        ShiftCancel = 1,
        TakeOpenShift = 2,
        RecurringShiftCancel = 3,
        Employee = 4,
        TakeExternalShiftBroadCast = 5
    }

    public class RequestDTO
    {
        public Guid Id { get; set; }
        public Guid ShiftId { get; set; }
        public Guid ExternalShiftBroadCastID { get; set; }
        public RequestTypeDTO RequestType { get; set; }
        public string RequesterName { get; set; }
        public string BusinessName { get; set; }
        public string BusinessLocationName { get; set; }
        public Guid BusinessLocationId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime FinishDateTime { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Reason { get; set; }
    }
}