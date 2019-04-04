using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Thinktecture.IdentityModel;
//using Thinktecture.IdentityModel.Authorization;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
    public class ManagerShiftActionAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        [Authorize]
        [Route("api/ManagerShiftActionAPI/GetShiftChangeRequests")]
        public ShiftChangeRequestListsDTO GetShiftChangeRequests()
        {

            var email = HttpContext.Current.User.Identity.Name;
            var retVal = new ShiftChangeRequestListsDTO();

            //Get list of all pending requests for any businesses that the user is a manager of

            //First get list of businesses which user is a manager of
            var mgrBusIdList = from emp in db.Employees
                               where emp.UserProfile.Email == email && emp.IsAdmin == true
                               select emp.BusinessLocation.Id;

            DateTime datetimeNow = WebUI.Common.Common.DateTimeNowLocal();

            using (SummaryAPIController summaryController = new SummaryAPIController())
            {
                //Get list of shift change Requests linked to busineses which user is a manager of
                var shiftRequestList = summaryController.ShiftChangeRequestList(mgrBusIdList.ToList()).Select(sr => new ShiftChangeRequestDTO
                {
                    Id = sr.Id,
                    ShiftId = sr.Shift.Id,
                    EmployeeName = (sr.Type == ShiftRequestType.TakeOpenShift) ? sr.CreatedBy.FirstName + " " + sr.CreatedBy.LastName : sr.Shift.Employee.FirstName + " " + sr.Shift.Employee.LastName,
                    BusinessLocationName = sr.Shift.InternalLocation.BusinessLocation.Name,
                    BusinessName = sr.Shift.InternalLocation.BusinessLocation.Business.Name,
                    BusinessLocationId = sr.Shift.InternalLocation.BusinessLocation.Id,
                    StartDateTime = sr.Shift.StartTime,
                    FinishDateTime = sr.Shift.FinishTime,
                    Type = (ShiftRequestTypeDTO)sr.Type,
                    Reason = sr.Reason,
                    Status = (RequestStatusDTO)sr.Status,
                    CreatedDate = sr.CreatedDate
                });

                var recurringShiftRequestList = summaryController.RecurringShiftChangeRequestList(mgrBusIdList.ToList()).Select(sr => new RecurringShiftChangeRequestDTO
                {
                    Id = sr.Id,
                    ShiftId = sr.ShiftTemplate.Id,
                    EmployeeName = sr.ShiftTemplate.Employee.FirstName + " " + sr.ShiftTemplate.Employee.LastName,
                    BusinessLocationName = sr.ShiftTemplate.InternalLocation.BusinessLocation.Name,
                    BusinessName = sr.ShiftTemplate.InternalLocation.BusinessLocation.Business.Name,
                    OccurenceDate = sr.OccurenceDate,
                    StartTime = sr.ShiftTemplate.StartTime,
                    FinishTime = sr.ShiftTemplate.FinishTime,
                    Type = (ShiftRequestTypeDTO)sr.Type,
                    Reason = sr.Reason,
                    Status = (RequestStatusDTO)sr.Status,
                    CreatedDate = sr.CreatedDate
                });

                retVal.ShiftChangeRequests = shiftRequestList;
                retVal.RecurringShiftChangeRequests = recurringShiftRequestList;
            }
            return retVal;
        }

        [Route("api/ManagerShiftActionAPI/ExternalUserProfile")]
        [HttpGet]
        public UserProfilesDTO ExternalUserProfile(Guid ID)
        {
            var retVal = new UserProfilesDTO();

            var result = db.ExternalShiftRequests.FirstOrDefault(a => a.Id == ID).CreatedBy;

            retVal.AboutMe = result.AboutMe;
            retVal.Address = result.Address;
            retVal.CurrentAddress = result.CurrentAddress;
            retVal.DateofBirth = result.DateofBirth;
            retVal.Distance = result.Distance;
            retVal.Email = result.Email;
            retVal.FirstName = result.FirstName;
            retVal.HasViewedWizard = result.HasViewedWizard;
            retVal.Id = result.Id;
            retVal.ImageData = result.UserPreferences.ImageData;
            retVal.IndustryTypeId = result.IndustryTypesID;
            retVal.LastName = result.LastName;
            retVal.MobilePhone = result.MobilePhone;
            //retVal.OtherQualificationsList = result.OtherQualifications;
            retVal.QRCode = result.QRCode;
            retVal.QualificationsList = result.UserQualifications.Select(a => new DropDownDTO { Text = a.Qualificationslookup.QualificationName, ValueGuid = a.Qualificationslookup.QualificationId }).ToList();
            retVal.SkillsList = result.UserSkills.Select(a => new DropDownDTO { Text = a.IndustrySkill.Name, ValueGuid = a.IndustrySkill.Id }).ToList();
            retVal.WorkHistoryList = result.WorkHistories.Select(a => new WorkHistoryDTO { workCompanyName = a.workCompanyName, workEndDate = a.workEndDate, workId = a.workId, workStartDate = a.workStartDate, UserRole = a.UserRole }).ToList();
            //retVal = MapperFacade.MapperConfiguration.Map<UserProfile, ExternalUserProfileViewDTO>(result);
            return retVal;
        }


        [Authorize]
        [Route("api/ManagerShiftActionAPI/GetExternalShiftRequests")]
        public ExternalShiftRequestListsDTO GetExternalShiftRequests()
        {
            var email = HttpContext.Current.User.Identity.Name;
            var retVal = new ExternalShiftRequestListsDTO();

            //Get list of all pending requests for any businesses that the user is a manager of

            //First get list of businesses which user is a manager of
            var mgrBusIdList = from emp in db.Employees
                               where emp.UserProfile.Email == email && emp.IsAdmin == true
                               select emp.BusinessLocation.Id;

            DateTime datetimeNow = WebUI.Common.Common.DateTimeNowLocal();

            using (SummaryAPIController summaryController = new SummaryAPIController())
            {
                //Get list of shift change Requests linked to busineses which user is a manager of
                var ExternalShiftRequestList = summaryController.ExternalShiftRequestList().Select(sr => new ExternalShiftRequestDTO
                {
                    Id = sr.Id,
                    ExternalShiftBroadCastID = sr.ExternalShiftBroadcast.Id,
                    UserName = sr.CreatedBy.FirstName + " " + sr.CreatedBy.LastName,
                    ProfileImageData = sr.CreatedBy.UserPreferences.ImageData,
                    Message = sr.ExternalShiftMessage,
                    //BusinessLocationName = sr.Shift.InternalLocation.BusinessLocation.Name,
                    //BusinessName = sr.Shift.InternalLocation.BusinessLocation.Business.Name,
                    //BusinessLocationId = sr.Shift.InternalLocation.BusinessLocation.Id,
                    //StartDateTime = sr.Shift.StartTime,
                    //FinishDateTime = sr.Shift.StartTime,
                    Type = (ExternalShiftRequestTypeDTO)sr.Type,
                    Reason = sr.Reason,
                    Status = (RequestStatusDTO)sr.Status,
                    CreatedDate = (DateTime)sr.CreatedDate
                });

                retVal.ExternalShiftRequests = ExternalShiftRequestList;
            }
            return retVal;

        }

        [HttpPost]
        public HttpResponseMessage RejectShiftChangeRequest(string id, ShiftChangeActionDTO rejectDTO)
        {
            try
            {
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                var scRequest = db.ShiftChangeRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);

                //Ensure user has "Put" manager permissions for the business location which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", scRequest.Shift.Roster.BusinessLocation.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

                if (scRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending shift change request.");

                //Update Request object
                scRequest.Status = RequestStatus.Denied;
                scRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                scRequest.ActionedComment = rejectDTO.Reason;
                //Attach the Employee record which matches the logged in user profile and business id.
                scRequest.ActionedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == scRequest.Shift.Roster.BusinessLocation.Id);

                db.SaveChanges();

                MessagingService.ShiftChangeRequestRejected(scRequest.CreatedBy.Email, scRequest.CreatedBy.FirstName, scRequest.Shift.StartTime.ToString() + " - " + scRequest.Shift.FinishTime.ToString() + " @ " + scRequest.Shift.InternalLocation.BusinessLocation.Name, scRequest.ActionedBy.FirstName + ' ' + scRequest.ActionedBy.LastName, rejectDTO.Reason);


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage RejectRecurringShiftChangeRequest(string id, ShiftChangeActionDTO rejectDTO)
        {
            try
            {
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                var scRequest = db.RecurringShiftChangeRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);

                //Ensure user has "Put" manager permissions for the business location which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", scRequest.ShiftTemplate.BusinessLocation.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

                if (scRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending recurring shift change request.");

                //Update Request object
                scRequest.Status = RequestStatus.Denied;
                scRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                scRequest.ActionedComment = rejectDTO.Reason;
                //Attach the Employee record which matches the logged in user profile and business id.'
                var usrProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
                var employee = usrProfile.Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == scRequest.ShiftTemplate.BusinessLocation.Id);
                if (employee == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find employee profile.");
                scRequest.ActionedBy = employee;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage ApproveShiftChangeRequest(string id, ShiftChangeActionDTO acceptDTO)
        {
            try
            {
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                var scRequest = db.ShiftChangeRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);

                if (scRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending shift change request.");

                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", scRequest.Shift.Roster.BusinessLocation.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");


                //Update Request object
                scRequest.Status = RequestStatus.Approved;
                scRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                scRequest.ActionedComment = acceptDTO.Reason;
                //Attach the Employee record which matches the logged in user profile and business id.
                scRequest.ActionedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == scRequest.Shift.InternalLocation.BusinessLocation.Id);

                if (scRequest.Type == ShiftRequestType.Cancel)
                {
                    //Update shift to be cancelled, ie set to not have an employee assigned and become and open shift
                    scRequest.Shift.Employee.Shifts.Remove(scRequest.Shift);
                }

                //If this is an open shift Take request, then need to reject all other requests for same shift
                // and send notification
                if (scRequest.Type == ShiftRequestType.TakeOpenShift)
                {
                    scRequest.Shift.Employee = scRequest.CreatedBy; //Assign the open shift to the employee
                    foreach (var openShiftRequest in db.ShiftChangeRequests.Where(scr => scr.Shift.Id == scRequest.Shift.Id && scr.Id != scRequest.Id))
                    {
                        openShiftRequest.Status = RequestStatus.Denied;
                        openShiftRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                        openShiftRequest.ActionedComment = acceptDTO.Reason;
                    }

                    MessagingService.OpenShiftRequestAccept(scRequest.CreatedBy.Email, scRequest.CreatedBy.FirstName, scRequest.Shift.StartTime.ToString() + " - " + scRequest.Shift.FinishTime.ToString() + " @ " + scRequest.Shift.InternalLocation.BusinessLocation.Name);

                }

                db.SaveChanges();


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage ApproveExternalShiftRequest(string id, ExternalShiftActionDTO acceptDTO)
        {
            try
            {
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                var user = db.UserProfiles.FirstOrDefault(usr => usr.Id == db.ExternalShiftRequests.FirstOrDefault(a => a.Id == requestId).CreatedBy.Id && usr.IsRegisteredExternal == true);

                var ExternalshiftRequest = db.ExternalShiftRequests.Find(requestId);

                var businessLocation = ExternalshiftRequest.ExternalShiftBroadcast.Shifts.FirstOrDefault().Roster.BusinessLocation;

                //Set the Status of ExternalBroadCastShift pending to Filled.
                ExternalshiftRequest.ExternalShiftBroadcast.Status = ExternalShiftStatus.Filled;
              
                // if user already Existing in table.
                var Externalemp = db.Employees.Where(emp => emp.UserProfile.Id == user.Id).FirstOrDefault();

                if (Externalemp == null)
                {
                    EmployeeDTO empModel = new EmployeeDTO();
                    empModel.FirstName = user.FirstName;
                    empModel.LastName = user.LastName;
                    empModel.DateOfBirth = user.DateofBirth;
                    empModel.Email = user.Email;
                    empModel.IsActive = true;
                    empModel.IsAdmin = false;
                    empModel.MobilePhone = user.MobilePhone;
                    empModel.Type = EmployeeTypeDTO.External;

                    Externalemp = MapperFacade.MapperConfiguration.Map<EmployeeDTO, Employee>(empModel);
                    Externalemp.Id = Guid.NewGuid();
                    Externalemp.UserProfile = user;


                    Externalemp.BusinessLocation = businessLocation;
                    //Create QR code for the employee
                    System.Drawing.Bitmap qrCodeImage = WebUI.Common.Common.GenerateQR(empModel.Id.ToString());

                    using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
                    {
                        qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                        Externalemp.QRCode = memory.ToArray();
                    }

                    db.Employees.Add(Externalemp);
                }
                
                var scRequest = db.ExternalShiftRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);

                if (scRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending External shift request.");

                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", scRequest.ExternalShiftBroadcast.Shifts.First().Roster.BusinessLocation.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");


                //Update Request object
                scRequest.Status = RequestStatus.Approved;
                scRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                scRequest.ActionedComment = acceptDTO.Reason;
                //Attach the Employee record which matches the logged in user profile and business id.
                scRequest.ActionedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == scRequest.ExternalShiftBroadcast.Shifts.First().InternalLocation.BusinessLocation.Id);
                
                //If this is an open shift Take request, then need to reject all other requests for same shift
                // and send notification
                if (scRequest.Type == ExternalShiftRequestType.TakeExternalShift)
                {
                    foreach (var shift in scRequest.ExternalShiftBroadcast.Shifts)
                        shift.Employee = Externalemp; //Assign the open shift to the employee

                    foreach (var ExternalShiftRequest in db.ExternalShiftRequests.Where(scr => scr.ExternalShiftBroadcast.Id == scRequest.ExternalShiftBroadcast.Id && scr.Id != scRequest.Id))
                    {
                        ExternalShiftRequest.Status = RequestStatus.Denied;
                        ExternalShiftRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                        ExternalShiftRequest.ActionedComment = acceptDTO.Reason;
                        ExternalShiftRequest.ActionedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == scRequest.ExternalShiftBroadcast.Shifts.First().InternalLocation.BusinessLocation.Id);
                    }

                    MessagingService.OpenShiftRequestAccept(scRequest.CreatedBy.Email, scRequest.CreatedBy.FirstName, scRequest.ExternalShiftBroadcast.Description);
                }
                db.SaveChanges();
                
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage RejectExternalShiftRequest(string id, ExternalShiftActionDTO rejectDTO)
        {
            try
            {
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                var scRequest = db.ExternalShiftRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);

                //Ensure user has "Put" manager permissions for the business location which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", scRequest.ExternalShiftBroadcast.Shifts.First().Roster.BusinessLocation.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

                if (scRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending shift change request.");

                //Update Request object
                scRequest.Status = RequestStatus.Denied;
                scRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                scRequest.ActionedComment = rejectDTO.Reason;
                //Attach the Employee record which matches the logged in user profile and business id.
                scRequest.ActionedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == scRequest.ExternalShiftBroadcast.Shifts.First().Roster.BusinessLocation.Id);

                db.SaveChanges();

                MessagingService.ShiftChangeRequestRejected(scRequest.CreatedBy.Email, scRequest.CreatedBy.FirstName, "", scRequest.ActionedBy.FirstName + ' ' + scRequest.ActionedBy.LastName, rejectDTO.Reason);


                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        public HttpResponseMessage ApproveRecurringShiftChangeRequest(string id, ShiftChangeActionDTO acceptDTO)
        {
            try
            {
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                var scRequest = db.RecurringShiftChangeRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);

                if (scRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending shift change request.");

                var weekStarting = WebUI.Common.Common.GetStartOfWeek(scRequest.OccurenceDate, DayOfWeek.Monday);

                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", scRequest.ShiftTemplate.BusinessLocation.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

                //Ensure week starting date is in the future AND is not already rostered
                //if (weekStarting < WebUI.Common.Common.DateTimeNowLocal())
                //    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "This shift occurs in a week starting in the past");

                //Ensure occurence date is after current time
                if (new DateTime(scRequest.OccurenceDate.Year, scRequest.OccurenceDate.Month, scRequest.OccurenceDate.Day, scRequest.ShiftTemplate.StartTime.Hours, scRequest.OccurenceDate.Minute, 0) < WebUI.Common.Common.DateTimeNowLocal())
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "This shift occurs in the past");

                var roster = db.Rosters.FirstOrDefault(r => r.WeekStartDate == weekStarting && r.BusinessLocation.Id == scRequest.ShiftTemplate.BusinessLocation.Id);
                if (roster != null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Roster already created for week starting " + weekStarting.ToShortDateString());

                //Update Request object
                scRequest.Status = RequestStatus.Approved;
                scRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                scRequest.ActionedComment = acceptDTO.Reason;
                //Attach the Employee record which matches the logged in user profile and business id.
                scRequest.ActionedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == scRequest.ShiftTemplate.BusinessLocation.Id);

                if (scRequest.Type == ShiftRequestType.Cancel)
                {
                    var occurenceDayOfWeek = scRequest.OccurenceDate.DayOfWeek;

                    //Create one off OPEN shift template to fill this shift
                    ShiftTemplate sst = new ShiftTemplate
                    {
                        Id = Guid.NewGuid(),
                        WeekStarting = weekStarting,
                        StartTime = scRequest.ShiftTemplate.StartTime,
                        FinishTime = scRequest.ShiftTemplate.FinishTime,
                        Monday = (occurenceDayOfWeek == DayOfWeek.Monday ? true : false),
                        Tuesday = (occurenceDayOfWeek == DayOfWeek.Tuesday ? true : false),
                        Wednesday = (occurenceDayOfWeek == DayOfWeek.Wednesday ? true : false),
                        Thursday = (occurenceDayOfWeek == DayOfWeek.Thursday ? true : false),
                        Friday = (occurenceDayOfWeek == DayOfWeek.Friday ? true : false),
                        Saturday = (occurenceDayOfWeek == DayOfWeek.Saturday ? true : false),
                        Sunday = (occurenceDayOfWeek == DayOfWeek.Sunday ? true : false),
                        Frequency = ShiftFrequency.OneOff,
                        BusinessLocation = scRequest.ShiftTemplate.BusinessLocation,
                        Employee = null, //set to an OPEN SHIFT 
                        InternalLocation = scRequest.ShiftTemplate.InternalLocation,
                        Role = scRequest.ShiftTemplate.Role,
                        Enabled = true
                    };

                    db.ShiftTemplates.Add(sst);

                    //TODO create an exception to recurring shift template to show that this have been cancelled???
                }

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
