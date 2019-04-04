using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Http;
using WebUI.Models;

namespace WebUI.Controllers
{
    [RequireHttps]
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            if (User != null && User.Identity.IsAuthenticated)
                return RedirectToAction("ProfileIndex");
            else
                return RedirectToAction("Login", "Account", null);
        }

        [AuthorizeUser]
        public ActionResult ProfileIndex()
        {
            UserProfilesDTO result = new UserProfilesDTO();
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var response = httpClient.GetAsync("api/UserProfileAPI/ProfileInfo").Result;
                result = JsonConvert.DeserializeObject<UserProfilesDTO>(response.Content.ReadAsStringAsync().Result);

                //var RoleList = httpClient.GetAsync("api/UserProfileAPI/GetRoles").Result;
                //result.RoleList = JsonConvert.DeserializeObject<List<DropDownDTO>>(RoleList.Content.ReadAsStringAsync().Result);

                var WorkHistoryList = httpClient.GetAsync("api/UserProfileAPI/GetWorkHistory").Result;
                result.WorkHistoryList = JsonConvert.DeserializeObject<List<WorkHistoryDTO>>(WorkHistoryList.Content.ReadAsStringAsync().Result);

                var QualificationsList = httpClient.GetAsync("api/UserProfileAPI/GetQualificationsByIndustry").Result;
                result.QualificationsList = JsonConvert.DeserializeObject<List<DropDownDTO>>(QualificationsList.Content.ReadAsStringAsync().Result);

                var OtherQualifications = httpClient.GetAsync("api/UserProfileAPI/GetOtherQualifications").Result;
                result.OtherQualificationsList = JsonConvert.DeserializeObject<List<OtherQualificationDTO>>(OtherQualifications.Content.ReadAsStringAsync().Result);

                var Skills = httpClient.GetAsync("api/UserProfileAPI/GetSkillsByIndustry").Result;
                result.SkillsList = JsonConvert.DeserializeObject<List<DropDownDTO>>(Skills.Content.ReadAsStringAsync().Result);
            }

            return View(result);
        }

        #region Profile Image and About Me

        /// <summary>
        /// To Update the Profile Picture of User
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadProfileImage(string UploadedFile, string FileType)
        {

            byte[] ImageBinaryForm;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                UserPreferencesDTO objProfile = new UserPreferencesDTO()
                {
                    ImageData = Convert.FromBase64String(UploadedFile),
                    ImageType = FileType
                };

                var updateProfileImage = httpClient.PostAsJsonAsync<UserPreferencesDTO>("api/UserProfileAPI/UpdateProfilePicture", objProfile).Result;
                ImageBinaryForm = JsonConvert.DeserializeObject<byte[]>(updateProfileImage.Content.ReadAsStringAsync().Result);
            }

            return Json(new { Success = true, FileName = "data:image;base64," + Convert.ToBase64String(ImageBinaryForm) }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To Update the Profile About Me of User
        /// </summary>
        /// <returns></returns>
        public ActionResult UpdateAboutMe(string AboutMe)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                UserProfilesDTO objProfile = new UserProfilesDTO()
                {
                    AboutMe = AboutMe
                };
                var updateAboutMe = httpClient.PostAsJsonAsync<UserProfilesDTO>("api/UserProfileAPI/UpdateAboutMe", objProfile).Result;
                var resultAboutMe = JsonConvert.DeserializeObject<string>(updateAboutMe.Content.ReadAsStringAsync().Result);
                return Json(new { Success = true, AboutMe = resultAboutMe }, JsonRequestBehavior.AllowGet);
            };
        }

        #endregion

        #region Work History

        /// <summary>
        /// To Update Work History
        /// </summary>
        /// <param name="WHDTO"></param>
        /// <returns></returns>
        public ActionResult AddWorkHistory(WorkHistoryDTO WHDTO)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                WorkHistoryDTO objWorkHistory = new WorkHistoryDTO()
                {
                    workId = WHDTO.workId,
                    workCompanyName = WHDTO.workCompanyName,
                    workStartDate = WHDTO.workStartDate,
                    workEndDate = WHDTO.workEndDate,
                    UserRole = WHDTO.UserRole
                };
                var updateWorkHistory = httpClient.PostAsJsonAsync<WorkHistoryDTO>("api/UserProfileAPI/AddEditWorkHistory", objWorkHistory).Result;
                var resultWorkHistory = JsonConvert.DeserializeObject<WorkHistoryDTO>(updateWorkHistory.Content.ReadAsStringAsync().Result);
                return Json(new { Success = true, workHistory = resultWorkHistory }, JsonRequestBehavior.AllowGet);
            };
        }

        /// <summary>
        /// To Delete Work History
        /// </summary>
        /// <param name="workID"></param>
        /// <returns></returns>
        public ActionResult DeleteWorkHistory(Guid workID)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                WorkHistoryDTO objWorkHistory = new WorkHistoryDTO()
                {
                    workId = workID
                };
                var deleteWorkHistory = httpClient.PostAsJsonAsync<WorkHistoryDTO>("api/UserProfileAPI/DeleteWorkHistory", objWorkHistory).Result;
                var resultWorkHistory = JsonConvert.DeserializeObject<WorkHistoryDTO>(deleteWorkHistory.Content.ReadAsStringAsync().Result);
                return Json(new { Success = true, workID = resultWorkHistory.workId }, JsonRequestBehavior.AllowGet);
            };
        }

        /// <summary>
        /// To Edit Work History
        /// </summary>
        /// <param name="workID"></param>
        /// <returns></returns>
        public ActionResult EditWorkHistory(Guid workID)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var editWorkHistory = httpClient.GetAsync("api/UserProfileAPI/workHistorybyId?workID=" + workID).Result;
                var resultWorkHistory = JsonConvert.DeserializeObject<WorkHistoryDTO>(editWorkHistory.Content.ReadAsStringAsync().Result);
                return Json(new { Success = true, workHistory = resultWorkHistory }, JsonRequestBehavior.AllowGet);
            };
        }

        #endregion

        #region Qualification

        /// <summary>
        /// To Add Other Qualification
        /// </summary>
        /// <param name="otherQualification"></param>
        /// <returns></returns>
        public ActionResult AddOtherQualification(OtherQualificationDTO otherQualificationData)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                OtherQualificationDTO objQualification = new OtherQualificationDTO()
                {
                    Id = otherQualificationData.Id,
                    Name = otherQualificationData.Name
                };
                var AddOtherQualification = httpClient.PostAsJsonAsync<OtherQualificationDTO>("api/UserProfileAPI/AddOtherQualification", objQualification).Result;
                var resultOtherQualification = JsonConvert.DeserializeObject<OtherQualificationDTO>(AddOtherQualification.Content.ReadAsStringAsync().Result);
                return Json(new { Success = true, OtherQualification = resultOtherQualification }, JsonRequestBehavior.AllowGet);
            };
        }

        /// <summary>
        /// To Delete Other Qualification
        /// </summary>
        /// <param name="qualificationId"></param>
        /// <returns></returns>
        public ActionResult DeleteOtherQualification(Guid qualificationId)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                OtherQualificationDTO objOtherQualification = new OtherQualificationDTO()
                {
                    Id = qualificationId
                };
                var deleteOtherQualification = httpClient.PostAsJsonAsync<OtherQualificationDTO>("api/UserProfileAPI/DeleteOtherQualification", objOtherQualification).Result;
                var resultOtherQualification = JsonConvert.DeserializeObject<OtherQualificationDTO>(deleteOtherQualification.Content.ReadAsStringAsync().Result);
                return Json(new { Success = true, qualificationId = resultOtherQualification.Id }, JsonRequestBehavior.AllowGet);
            };
        }

        /// <summary>
        /// To Edit Other Qualification
        /// </summary>
        /// <param name="qualificationId"></param>
        /// <returns></returns>
        public ActionResult EditOtherQualification(Guid qualificationId)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var editOtherQualification = httpClient.GetAsync("api/UserProfileAPI/OtherQualificationbyId?qualificationId=" + qualificationId).Result;
                var resultOtherQualification = JsonConvert.DeserializeObject<OtherQualificationDTO>(editOtherQualification.Content.ReadAsStringAsync().Result);
                return Json(new { Success = true, OtherQualification = resultOtherQualification }, JsonRequestBehavior.AllowGet);
            };
        }

        /// <summary>
        /// To Add Other Qualification
        /// </summary>
        /// <param name="otherQualification"></param>
        /// <returns></returns>
        public ActionResult AddQualification(Guid[] QualificationList)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var AddQualification = httpClient.PostAsJsonAsync<Guid[]>("api/UserProfileAPI/AddQualification", QualificationList).Result;
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            };
        }

        /// <summary>
        /// To Get Saved Qualification List
        /// </summary>
        /// <param name="Qualification"></param>
        /// <returns></returns>
        public ActionResult GetSavedQualification()
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var userQualification = httpClient.GetAsync("api/UserProfileAPI/GetQualificationsSaved").Result;
                var QualificationsList = JsonConvert.DeserializeObject<List<UserQualificationDTO>>(userQualification.Content.ReadAsStringAsync().Result);

                var userSkills = httpClient.GetAsync("api/UserProfileAPI/GetSavedSkills").Result;
                var UserSkillList = JsonConvert.DeserializeObject<List<UserSkillsDTO>>(userSkills.Content.ReadAsStringAsync().Result);

                return Json(new { Success = true, Qualification = QualificationsList, Skills = UserSkillList }, JsonRequestBehavior.AllowGet);
            };
        }

        #endregion

        #region Skills

        /// <summary>
        /// To Add Skills
        /// </summary>
        /// <param name="SkillsList"></param>
        /// <returns></returns>
        public ActionResult AddSkills(Guid[] SkillsList)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var AddSkills = httpClient.PostAsJsonAsync<Guid[]>("api/UserProfileAPI/AddSkills", SkillsList).Result;
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            };
        }

        #endregion
    }
}