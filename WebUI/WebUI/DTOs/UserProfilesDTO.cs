using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUI.Models;

namespace WebUI.DTOs
{
    public class UserProfilesDTO
    {

        public System.Guid Id { get; set; }

        public Guid ExternalUserID { get; set; }
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
        public Nullable<System.DateTime> DateofBirth { get; set; }

        public string MobilePhone { get; set; }

        public bool HasViewedWizard { get; set; }

        public byte[] QRCode { get; set; }

        public int IndustryTypesID { get; set; }

        public Address Address { get; set; }

        public Address CurrentAddress { get; set; }

        public WorkHistory WorkHistory { get; set; }

        public byte[] ImageData { get; set; }
        public string AboutMe { get; set; }
        public Guid ExternalShiftBroadCastID { get; set; }
        public Guid ExternalshfitRequestID { get; set; }

        public string Line1 { get; set; }
        public string Suburb { get; set; }
        public string State { get; set; }
        public string Postcode { get; set; }
        public string Line2 { get; set; }
        public string PlaceId { get; set; }
        public bool IsRegisteredExternal { get; set; }
        public Nullable<double> PlaceLatitude { get; set; }
        public Nullable<double> PlaceLongitude { get; set; }

        public Nullable<int> Distance { get; set; }
        public int? IndustryTypeId { get; set; }

        public UserRecommendationsDTO UserRecommendationsDTO { get; set; }
        public UserSkillEndorsementDTO UserSkillEndorsementDTO { get; set; }
        public UserSkillEndorsementDTOList UserSkillEndorsmentListDTO { get; set; }
        // public List<UserRecommendationsDTO> UserBeingRecommended { get; set; }
        public int Count { get; set; }
        public List<DropDownDTO> RoleList { get; set; }
        public List<WorkHistoryDTO> WorkHistoryList { get; set; }
        public List<DropDownDTO> QualificationsList { get; set; }
        public employeeListDTO EmployeeList { get; set; }
        public List<OtherQualificationDTO> OtherQualificationsList { get; set; }
        public List<DropDownDTO> SkillsList { get; set; }
        public Guid BusinessID { get; set; }
        public List<DropDownDTO> Businesses { get; set; }
    }

    public class UserSkillEndorsementDTOList
    {
        public IEnumerable<UserSkillEndorsementDTO> UserSkillEndorsementDTO { get; set; }
        public IEnumerable<Guid> UserBeingSkillsEndorsed { get; set; }
        public IEnumerable<Guid> Skills { get; set; }
        public List<Guid> SkillEndorsingBy { get; set; }
    }

    public class ExternalUserProfileListDTO
    {
        public IEnumerable<UserProfilesDTO> ExternalUserProfile { get; set; }
    }
    public class employeeListDTO
    {
        public List<Guid> EmployeeIDs { get; set; }
    }
}