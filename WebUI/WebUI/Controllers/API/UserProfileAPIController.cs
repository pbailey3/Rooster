using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebUI.Models;
using AutoMapper;
using WebUI.DTOs;
using System.Threading;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.WebApi;
using WebUI.Http;
using WebUI.Common;
using System.IO;
using System.Data.Entity.Validation;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class UserProfileAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/UserProfileAPI
        [Authorize]
        [Route("api/UserProfileAPI/GetPreferredName")]
        public string GetPreferredNameForCurrentUser()
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return userProfile.FirstName;
        }

        // GET api/UserProfileAPI
        [Authorize]
        [Route("api/UserProfileAPI/GetHasViewedTutorial")]
        public bool GetHasViewedTutorialForCurrentUser()
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return userProfile.HasViewedWizard;
        }

        // GET api/UserProfileAPI
        [Authorize]
        [Route("api/UserProfileAPI/ViewedWizard")]
        public bool PostViewedWizard()
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            userProfile.HasViewedWizard = true;

            db.SaveChanges();

            return userProfile.HasViewedWizard;
        }

        // GET api/UserProfileAPI
        [Authorize]
        [Route("api/UserProfileAPI/UpdateLocation")]
        public bool PostUpdateLocation([FromBody]UserProfilesDTO userProfileDTO)
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            userProfile.CurrentAddress.Line1 = !String.IsNullOrEmpty(userProfileDTO.Line1) ? userProfileDTO.Line1.Replace(",", "") : "";
            userProfile.CurrentAddress.Line2 = !String.IsNullOrEmpty(userProfileDTO.Line2) ? userProfileDTO.Line2.Replace(",", "") : "";
            userProfile.CurrentAddress.Suburb = !String.IsNullOrEmpty(userProfileDTO.Suburb) ? userProfileDTO.Suburb.Replace(",", "") : "";
            userProfile.CurrentAddress.State = !String.IsNullOrEmpty(userProfileDTO.State) ? userProfileDTO.State.Replace(",", "") : "";
            userProfile.CurrentAddress.Postcode = !String.IsNullOrEmpty(userProfileDTO.Postcode) ? userProfileDTO.Postcode : "";
            userProfile.CurrentAddress.PlaceLongitude = userProfileDTO.PlaceLongitude;
            userProfile.CurrentAddress.PlaceLatitude = userProfileDTO.PlaceLatitude;

            db.SaveChanges();

            return true;
        }

        // GET api/UserProfileAPI
        [Authorize]
        [Route("api/UserProfileAPI/UpdateDistance")]
        public bool PostUpdateDistance([FromBody]UserProfilesDTO userProfileDTO)
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            userProfile.Distance = userProfileDTO.Distance;
            userProfile.IsRegisteredExternal = true;
            userProfile.IndustryTypesID = userProfileDTO.IndustryTypeId;
            db.SaveChanges();

            return true;
        }

        // GET api/UserProfileAPI
        [Authorize]
        [Route("api/UserProfileAPI/GetQRCode")]
        public byte[] GetQRCodeForCurrentUser()
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            else
            {
                return userProfile.QRCode;
            }
        }

        #region Profile Image and About Me

        // POST api/UserProfileAPI/UpdateProfilePicture
        [Authorize]
        [Route("api/UserProfileAPI/UpdateProfilePicture")]
        public Byte[] PostUpdateProfilePicture([FromBody]UserPreferencesDTO FileName)
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            userProfile.UserPreferences.ImageData = FileName.ImageData;
            userProfile.UserPreferences.ImageType = FileName.ImageType;

            db.SaveChanges();

            return userProfile.UserPreferences.ImageData;
        }

        // GET api/UserProfileAPI
        [Authorize]
        [Route("api/UserProfileAPI/ProfileInfo")]
        public UserProfilesDTO GetProfileInfo()
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfilesDTO userProfile = db.UserProfiles.Where(usr => usr.Email == email).Select(x => new UserProfilesDTO
            {
                FirstName = x.FirstName,
                LastName = x.LastName,
                Line1 = x.CurrentAddress.Line1,
                Line2 = x.CurrentAddress.Line2,
                Suburb = x.CurrentAddress.Suburb,
                State = x.CurrentAddress.State,
                AboutMe = x.AboutMe,
                DateofBirth = x.DateofBirth,
                ImageData = x.UserPreferences.ImageData
            }).FirstOrDefault();

            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return userProfile;
        }

        // POST api/UserProfileAPI/UpdateAboutMe
        [Authorize]
        [Route("api/UserProfileAPI/UpdateAboutMe")]
        public string PostUpdateAboutMe([FromBody]UserProfilesDTO aboutMeData)
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            userProfile.AboutMe = aboutMeData.AboutMe;
            db.SaveChanges();

            return userProfile.AboutMe;
        }

        #endregion

        #region WorkHistory

        // POST api/UserProfileAPI/AddWorkHistory
        [Authorize]
        [Route("api/UserProfileAPI/AddEditWorkHistory")]
        public WorkHistoryDTO PostUpdateWorkHistory([FromBody]WorkHistoryDTO workHistoryData)
        {

            var email = HttpContext.Current.User.Identity.Name;

            WorkHistory workHistory = null;
            if (workHistoryData.workId != Guid.Empty)
            {
                workHistory = db.WorkHistories.Where(x => x.workId == workHistoryData.workId).FirstOrDefault();
            }
            else
            {
                workHistory = new WorkHistory();
                workHistory.workId = Guid.NewGuid();
            }

            workHistory.workCompanyName = workHistoryData.workCompanyName;
            workHistory.workStartDate = workHistoryData.workStartDate;
            workHistory.workEndDate = workHistoryData.workEndDate;
            workHistory.UserProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            workHistory.UserRole = workHistoryData.UserRole;

            if (workHistory == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (workHistoryData.workId == Guid.Empty)
            {
                db.WorkHistories.Add(workHistory);
            }

            db.SaveChanges();
            WorkHistoryDTO employeeDTO = MapperFacade.MapperConfiguration.Map<WorkHistory, WorkHistoryDTO>(workHistory);

            return employeeDTO;
        }

        [HttpPost]
        [Authorize]
        [Route("api/UserProfileAPI/DeleteWorkHistory")]
        public WorkHistoryDTO PostDeleteWorkHistory(WorkHistoryDTO workHistoryData)
        {
            WorkHistory deletedEntry = new WorkHistory();
            deletedEntry = db.WorkHistories.Where(x => x.workId == workHistoryData.workId).FirstOrDefault();
            if (deletedEntry.workId != Guid.Empty)
            {
                db.WorkHistories.Remove(deletedEntry);
                db.SaveChanges();
            }
            return MapperFacade.MapperConfiguration.Map<WorkHistory, WorkHistoryDTO>(deletedEntry);
        }

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/GetRoles")]
        public List<DropDownDTO> GetRoles()
        {
            List<DropDownDTO> getRoles = db.Roles.Select(x => new DropDownDTO
            {
                ValueGuid = x.Id,
                Text = x.Name
            }).ToList();
            return getRoles;
        }

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/workHistorybyId")]
        public WorkHistoryDTO GetworkHistorybyId(Guid workID)
        {
            WorkHistoryDTO getworkHistory = db.WorkHistories.Select(x => new WorkHistoryDTO
            {
                workId = x.workId,
                workCompanyName = x.workCompanyName,
                workStartDate = x.workStartDate,
                workEndDate = x.workEndDate,
                UserRole = x.UserRole
            }).Where(x => x.workId == workID).FirstOrDefault();

            return getworkHistory;
        }

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/GetWorkHistory")]
        public List<WorkHistoryDTO> GetWorkHistory()
        {
            var email = HttpContext.Current.User.Identity.Name;

            List<WorkHistory> getWorkHistoryDTO = db.WorkHistories.Where(x => x.UserProfileId == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Id).ToList();
            List<WorkHistoryDTO> getWorkHistory = MapperFacade.MapperConfiguration.Map<List<WorkHistory>, List<WorkHistoryDTO>>(getWorkHistoryDTO);
            return getWorkHistory;
        }

        #endregion

        #region Qualification

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/GetQualificationsByIndustry")]
        public List<DropDownDTO> GetQualificationsByIndustry()
        {
            var email = HttpContext.Current.User.Identity.Name;

            List<DropDownDTO> getQualification = db.Qualificationslookups.Where(x => x.IndustryTypesID == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).IndustryTypesID).Select(x => new DropDownDTO
            {
                ValueGuid = x.QualificationId,
                Text = x.QualificationName
            }).ToList();

            return getQualification;
        }

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/GetOtherQualifications")]
        public List<OtherQualificationDTO> GetOtherQualifications()
        {
            var email = HttpContext.Current.User.Identity.Name;

            List<OtherQualificationDTO> getOtherQualification = db.OtherQualifications.Where(x => x.UserProfileId == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Id).Select(x => new OtherQualificationDTO
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            return getOtherQualification;
        }

        [HttpPost]
        [Authorize]
        [Route("api/UserProfileAPI/DeleteOtherQualification")]
        public OtherQualificationDTO PostDeleteOtherQualification(OtherQualificationDTO OtherQualificationData)
        {
            OtherQualification deletedEntry = new OtherQualification();
            deletedEntry = db.OtherQualifications.Where(x => x.Id == OtherQualificationData.Id).FirstOrDefault();
            if (deletedEntry.Id != Guid.Empty)
            {
                db.OtherQualifications.Remove(deletedEntry);
                db.SaveChanges();
            }
            return MapperFacade.MapperConfiguration.Map<OtherQualification, OtherQualificationDTO>(deletedEntry);
        }

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/OtherQualificationbyId")]
        public OtherQualificationDTO GetOtherQualificationbyId(Guid qualificationId)
        {
            OtherQualificationDTO getOtherQualification = db.OtherQualifications.Select(x => new OtherQualificationDTO
            {
                Id = x.Id,
                Name = x.Name
            }).Where(x => x.Id == qualificationId).FirstOrDefault();

            return getOtherQualification;
        }

        [HttpPost]
        [Authorize]
        [Route("api/UserProfileAPI/AddOtherQualification")]
        public OtherQualificationDTO PostAddOtherQualification([FromBody]OtherQualificationDTO OtherQualificationModel)
        {
            var email = HttpContext.Current.User.Identity.Name;

            OtherQualification otherQualification = null;
            if (OtherQualificationModel.Id != Guid.Empty)
            {
                otherQualification = db.OtherQualifications.Where(x => x.Id == OtherQualificationModel.Id).FirstOrDefault(); ;
            }
            else
            {
                otherQualification = new OtherQualification();
                otherQualification.Id = Guid.NewGuid();
            }

            otherQualification.Name = OtherQualificationModel.Name;
            otherQualification.UserProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);

            if (otherQualification == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (OtherQualificationModel.Id == Guid.Empty)
            {
                db.OtherQualifications.Add(otherQualification);
            }

            db.SaveChanges();

            OtherQualificationDTO otherQualificationDTO = MapperFacade.MapperConfiguration.Map<OtherQualification, OtherQualificationDTO>(otherQualification);

            return otherQualificationDTO;
        }

        [HttpPost]
        [Authorize]
        [Route("api/UserProfileAPI/AddQualification")]
        public void PostAddQualification([FromBody]Guid[] QualificationLookupid)
        {
            var email = HttpContext.Current.User.Identity.Name;

            db.UserQualifications.RemoveRange(db.UserQualifications.Where(x => x.UserProfileId == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Id));
            db.SaveChanges();

            for (int i = 0; i < QualificationLookupid.Count(); i++)
            {
                UserQualification userQualification = new UserQualification();
                userQualification.UserQualificationId = Guid.NewGuid();
                var id = QualificationLookupid[i];
                userQualification.Qualificationslookup = db.Qualificationslookups.FirstOrDefault(qual => qual.QualificationId == id);
                userQualification.UserProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
                db.UserQualifications.Add(userQualification);
                db.SaveChanges();

                if (userQualification == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/GetQualificationsSaved")]
        public List<UserQualificationDTO> GetQualificationsSaved()
        {
            var email = HttpContext.Current.User.Identity.Name;

            List<UserQualificationDTO> QualificationDTO = db.UserQualifications.Where(x => x.UserProfileId == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Id).Select(x => new UserQualificationDTO
            {
                QualificationslookupQualificationId = x.QualificationslookupQualificationId
            }).ToList();

            return QualificationDTO;
        }

        #endregion

        #region Skills

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/GetSkillsByIndustry")]
        public List<DropDownDTO> GetSkillsByIndustry()
        {
            var email = HttpContext.Current.User.Identity.Name;

            List<DropDownDTO> getSkills = db.IndustrySkills.Where(x => x.IndustryTypesID == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).IndustryTypesID).Select(x => new DropDownDTO
            {
                ValueGuid = x.Id,
                Text = x.Name
            }).ToList();

            return getSkills;
        }

        [HttpPost]
        [Authorize]
        [Route("api/UserProfileAPI/AddSkills")]
        public void PostAddSkills([FromBody]Guid[] SkillsLookupid)
        {
            var email = HttpContext.Current.User.Identity.Name;

            db.UserSkills.RemoveRange(db.UserSkills.Where(x => x.UserProfileId == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Id));
            db.SaveChanges();

            for (int i = 0; i < SkillsLookupid.Count(); i++)
            {
                UserSkills userSkills = new UserSkills();
                userSkills.UserSkillId = Guid.NewGuid();
                var id = SkillsLookupid[i];
                userSkills.IndustrySkill = db.IndustrySkills.FirstOrDefault(skill => skill.Id == id);
                userSkills.UserProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
                db.UserSkills.Add(userSkills);

                if (userSkills == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }

                db.SaveChanges();
            }
        }

        [HttpGet]
        [Authorize]
        [Route("api/UserProfileAPI/GetSavedSkills")]
        public List<UserSkillsDTO> GetSavedSkills()
        {
            var email = HttpContext.Current.User.Identity.Name;

            List<UserSkillsDTO> UserSklDTO = db.UserSkills.Where(x => x.UserProfileId == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Id).Select(x => new UserSkillsDTO
            {
                IndustrySkillsId = x.IndustrySkillsId
            }).ToList();

            return UserSklDTO;
        }

        #endregion


        [Route("api/UserProfileAPI/ExternalUserProfile")]
        [HttpGet]
        public UserProfilesDTO ExternalUserProfile(Guid ID)
        {
            var LoggedInUser = db.UserProfiles.FirstOrDefault(a => a.Email == HttpContext.Current.User.Identity.Name).Id;
            UserProfilesDTO retVal = new UserProfilesDTO();
            Guid userID = new Guid();
            //This is the check if an user finds from uniq search Top bar and if the user is an employee
            var employee = db.Employees.FirstOrDefault(a => a.Id == ID);
            if (employee != null)
            {
                userID = employee.UserProfile.Id;
            }
            else
            {
                userID = ID;
            }

            var result = db.UserProfiles.Where(usr => usr.Id == userID).FirstOrDefault();

            retVal.AboutMe = result.AboutMe;
            retVal.Address = result.Address;
            retVal.CurrentAddress = result.CurrentAddress;
            retVal.DateofBirth = result.DateofBirth;
            retVal.Distance = result.Distance;
            retVal.Email = result.Email;
            retVal.FirstName = result.FirstName;
            retVal.HasViewedWizard = result.HasViewedWizard;
            retVal.ExternalUserID = result.Id;
            retVal.ImageData = result.UserPreferences.ImageData;
            retVal.IndustryTypeId = result.IndustryTypesID;
            retVal.LastName = result.LastName;
            retVal.MobilePhone = result.MobilePhone;
            retVal.QRCode = result.QRCode;
            retVal.Id = LoggedInUser;
            retVal.Businesses = db.Employees.Where(a => a.Email == HttpContext.Current.User.Identity.Name && a.UserProfile.Id != userID).Select(x => new DropDownDTO { Text = x.BusinessLocation.Business.Name, ValueGuid = x.BusinessLocation.Business.Id }).ToList();
            retVal.QualificationsList = result.UserQualifications.Select(a => new DropDownDTO { Text = a.Qualificationslookup.QualificationName, ValueGuid = a.Qualificationslookup.QualificationId }).ToList();
            retVal.SkillsList = result.UserSkills.Select(a => new DropDownDTO { Text = a.IndustrySkill.Name, ValueGuid = a.UserSkillId }).ToList();
            retVal.WorkHistoryList = result.WorkHistories.Select(a => new WorkHistoryDTO { workCompanyName = a.workCompanyName, workEndDate = a.workEndDate, workId = a.workId, workStartDate = a.workStartDate, UserRole = a.UserRole }).ToList();
            retVal.EmployeeList = EpmloyeeList(userID);
            retVal.UserRecommendationsDTO = UserRecommending(userID);
            //retVal.UserRecommendationsDTO = UserRecommending(ID);
            //retVal.UserSkillEndorsementDTO.UserSkillEndorsedBy = skillEndorsedBY.Id;
            retVal.UserSkillEndorsmentListDTO = GetSkillsEndorsedByUsers(ID);
            // retVal.UserSkillEndorsementDTO = SkillsEndorsedUserBeingRecommended(ID);
            retVal.Count = result.UserRecommendationsUserRecommended.Where(a => a.UserProfileUserRecommended.Id == userID).Count();
            // retVal = MapperFacade.MapperConfiguration.Map<UserProfile, UserProfilesDTO>(result);
            return retVal;
        }


        //Getting the Skills Endorsed By logged in User
        //private UserSkillEndorsementDTO SkillsEndorsedByRecommendingUser()
        //{
        //    var LoggedInUser = db.UserProfiles.FirstOrDefault(a => a.Email == HttpContext.Current.User.Identity.Name).Id;
        //    UserSkillEndorsementDTO retVal = new UserSkillEndorsementDTO();
        //    var UserSkillEndorsing = from sk in db.UserSkillEndorsements
        //                             where sk.UserProfileUserRecommendedBY.Id == LoggedInUser
        //                             select sk.UserSkill.UserSkillId;
        //    retVal.UserSkillEndorsing = UserSkillEndorsing.AsEnumerable();
        //    return retVal;
        //}

        //Checking the User Recommending while Manager LogsIn
        private UserRecommendationsDTO UserRecommending(Guid Id)
        {
            UserRecommendationsDTO retVal = new UserRecommendationsDTO();
            //UserProfilesDTO usr = new UserProfilesDTO();
            var RecommendedBy = (from sk in db.UserRecommendations
                                 where sk.UserProfileUserRecommended.Id == Id
                                 select sk.UserProfileUserRecommendedBy.Id).ToList();
            retVal.UserRecommendedByList = RecommendedBy;
            return retVal;
        }

        private UserSkillEndorsementDTOList GetSkillsEndorsedByUsers(Guid id)
        {
            UserSkillEndorsementDTOList retVal = new UserSkillEndorsementDTOList();
            var skill = (from sk in db.UserSkillEndorsements
                         where sk.UserProfileUserBeingRecommended.Id == id
                         select sk).ToList();

            retVal.UserSkillEndorsementDTO = (from s in skill
                                              select new UserSkillEndorsementDTO
                                              {
                                                  UserSkill = s.UserSkill.UserSkillId,
                                                  UserProfileUserRecommendedBY = s.UserProfileUserRecommendedBY.Id,
                                                  id = s.Id
                                              }).ToList();

            //retVal.SkillEndorsingBy = (from u in skill
            //                           select u.UserProfileUserRecommendedBY.Id).ToList();


            //List<UserSkillEndorsementDTO> model = MapperFacade.MapperConfiguration.Map<List<UserSkillEndorsements>, List<UserSkillEndorsementDTO>>(skill);
            //retVal.UserSkillEndorsementDTO = model;

            //var users = (from sk in db.UserSkillEndorsements
            //             where sk.UserProfileUserBeingRecommended.Id == id
            //             select sk.UserProfileUserRecommendedBY.Id).ToList();
            //retVal.SkillEndorsingBy = users.Distinct().ToList();
            return retVal;
        }


        private employeeListDTO EpmloyeeList(Guid Id)
        {
            employeeListDTO retVal = new employeeListDTO();
            //UserProfilesDTO usr = new UserProfilesDTO();
            retVal.EmployeeIDs = (from sk in db.Employees
                                  where sk.UserProfile.Id == Id
                                  select sk.UserProfile.Id).ToList();

            return retVal;
        }

        ////Checking the UserBeingRecommended by Logged in user
        //private UserRecommendationsDTO UserBeingRecommended()
        //{
        //    var LoggedInUser = db.UserProfiles.FirstOrDefault(a => a.Email == HttpContext.Current.User.Identity.Name).Id;
        //    UserRecommendationsDTO retVal = new UserRecommendationsDTO();
        //    //UserProfilesDTO usr = new UserProfilesDTO();
        //    var UserBeingRecommended = from sk in db.UserRecommendations
        //                               where sk.UserProfileUserRecommendedBy.Id == LoggedInUser
        //                               select sk.UserProfileUserRecommended.Id;
        //    retVal.UserBeingRecommendedList = UserBeingRecommended.AsEnumerable();
        //    return retVal;
        //}

        [Route("api/UserProfileAPI/UserRecommendations")]
        [HttpPost]
        public HttpResponseMessage UserRecommendations(Guid ID, UserRecommendationsDTO userRecommendationsDTO)
        {
            try
            {
                UserRecommendations recommendation = new UserRecommendations();
                recommendation.Id = Guid.NewGuid();
                recommendation.UserProfileUserRecommended = db.UserProfiles.FirstOrDefault(usr => usr.Id == ID);
                recommendation.UserProfileUserRecommendedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == HttpContext.Current.User.Identity.Name);
                db.UserRecommendations.Add(recommendation);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [Route("api/UserProfileAPI/UserEndorseASkill")]
        [HttpPost]
        public HttpResponseMessage UserEndorseASkill(Guid ID, UserSkillsDTO usrskill)
        {
            try
            {
                UserSkillEndorsements endorsement = new UserSkillEndorsements();
                endorsement.Id = Guid.NewGuid();
                endorsement.UserProfileUserBeingRecommended = db.UserProfiles.FirstOrDefault(a => a.Id == ID);
                endorsement.UserProfileUserRecommendedBY = db.UserProfiles.FirstOrDefault(usr => usr.Email == HttpContext.Current.User.Identity.Name);
                endorsement.UserSkill = db.UserSkills.FirstOrDefault(a => a.UserSkillId == usrskill.UserSkillId);
                db.UserSkillEndorsements.Add(endorsement);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }
    }
}