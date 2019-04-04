using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class EmployeeShiftActionAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        [ActionName("GetBusinessLocationManagerDetails")]
        public IEnumerable<EmployeeSummaryDTO> GetBusinessLocationManagerDetails(Guid Id)
        {

            var busLoc = db.BusinessLocations.Find(Id);

            if (busLoc == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", busLoc.Business.Id.ToString()))
            {
                List<EmployeeSummaryDTO> managerList = new List<EmployeeSummaryDTO>();

                //Get all other shifts which overlap and are at the same internal location
                var mgrList = db.Employees.Where(e => e.BusinessLocation.Id == Id
                                                && e.ManagerBusinessLocations.Any(bl => bl.Id == Id));

                //TODO: if a userprofile exists for mgr then the details in the profile should overwrite the employee table
                return MapperFacade.MapperConfiguration.Map<IEnumerable<Employee>, IEnumerable<EmployeeSummaryDTO>>(mgrList.AsEnumerable());
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));

        }

        [ActionName("GetWorkingWithDetails")]
        public IEnumerable<string> GetWorkingWithDetails(Guid Id)
        {
            var shift = db.Shifts.Find(Id);

            if (shift == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", shift.Roster.BusinessLocation.Business.Id.ToString()))
            {
                List<string> workingWithList = new List<string>();

                //Get all other shifts which overlap and are at the same internal location
                var nameList = db.Shifts.Where(s => s.Roster.BusinessLocation.Id == shift.Roster.BusinessLocation.Id
                                                && s.InternalLocation.Id == shift.InternalLocation.Id
                                                && s.Id != Id
                                               && ((s.StartTime >= shift.StartTime && s.StartTime < shift.FinishTime) //Start time overlaps
                                                    || (s.FinishTime > shift.StartTime && s.FinishTime <= shift.FinishTime) //end time overlaps
                                                    || (s.FinishTime >= shift.FinishTime && s.StartTime <= shift.StartTime))) //Start time and end time bother greater (ie shift is bigger than compareing shift)
                                                    .Select(s => (!String.IsNullOrEmpty(s.Employee.FirstName) && !String.IsNullOrEmpty(s.Employee.LastName)) ? (s.Employee.FirstName + " " + s.Employee.LastName) : "Open Shift").ToList();

                var nn = nameList.ToList<string>();

                return nameList.ToList();
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));


        }

        [HttpPost]
        [ActionName("CancelShift")]
        public HttpResponseMessage CancelShift(Guid Id, ShiftChangeActionDTO shiftCancelDTO)
        {
            var shift = db.Shifts.Find(Id);
            if (shift == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Shift not found");

            //Only the staff member assigned to the shift can request to cancel the shift
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", shift.Roster.BusinessLocation.Business.Id.ToString())
                || shift.Employee.UserProfile.Email != User.Identity.Name)
            {
                if (shiftCancelDTO.Reason == String.Empty)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Reason cannot be blank");

                //Check to see if there is already a pending cancellation request
                var shiftCancelRequest = db.ShiftChangeRequests.Where(sc => sc.Type == ShiftRequestType.Cancel
                                                                        && sc.Shift.Id == Id
                                                                        && sc.Status == RequestStatus.Pending);
                if (shiftCancelRequest.Count() > 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Existing cancellation request already for shift id:" + Id.ToString());

                ShiftChangeRequest shiftChangeRequest = new ShiftChangeRequest
                {
                    Id = Guid.NewGuid(),
                    Reason = shiftCancelDTO.Reason,
                    Shift = shift,
                    Type = ShiftRequestType.Cancel,
                    Status = RequestStatus.Pending,
                    CreatedDate = WebUI.Common.Common.DateTimeNowLocal(),
                    CreatedBy = shift.Employee

                };

                //Get all managers for this busiess location
                var managers = db.Employees.Where(m => m.IsAdmin == true && m.BusinessLocation.Id == shift.Roster.BusinessLocation.Id);
                foreach (var mgr in managers)
                {
                    if (mgr.UserProfile != null)
                    {
                        //Send notifications to managers of the affected business location information that the employee has requested to cancel a shift
                        MessagingService.ShiftCancelRequest(mgr.UserProfile.Email, mgr.UserProfile.FirstName, shift.Employee.UserProfile.FirstName + ' ' + shift.Employee.UserProfile.LastName, shift.InternalLocation.BusinessLocation.Name, shift.StartTime, shift.FinishTime);
                    }
                }


                db.ShiftChangeRequests.Add(shiftChangeRequest);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, shiftChangeRequest.Id);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));


        }

        [HttpPost]
        [ActionName("CancelRecurringShiftInstance")]
        public HttpResponseMessage CancelRecurringShiftInstance(Guid Id, RecurringShiftChangeActionDTO shiftCancelDTO)
        {
            var shift = db.ShiftTemplates.Find(Id);
            if (shift == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Recurring shift not found");

            //Only the staff member assigned to the shift can request to cancel the shift
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", shift.BusinessLocation.Business.Id.ToString())
                || shift.Employee.UserProfile.Email != User.Identity.Name)
            {
                if (shiftCancelDTO.Reason == String.Empty)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Reason cannot be blank");

                //Check to see if there is already a pending cancellation request
                var shiftCancelRequest = db.RecurringShiftChangeRequests.Where(sc => sc.Type == ShiftRequestType.Cancel
                                                                        && sc.ShiftTemplate.Id == Id
                                                                        && sc.Status == RequestStatus.Pending
                                                                        && sc.OccurenceDate == shiftCancelDTO.OccurenceDate);
                if (shiftCancelRequest.Count() > 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Existing cancellation request already pending for recurring shift");

                RecurringShiftChangeRequest shiftChangeRequest = new RecurringShiftChangeRequest
                {
                    Id = Guid.NewGuid(),
                    Reason = shiftCancelDTO.Reason,
                    ShiftTemplate = shift,
                    Type = ShiftRequestType.Cancel,
                    OccurenceDate = shiftCancelDTO.OccurenceDate,
                    Status = RequestStatus.Pending,
                    CreatedDate = WebUI.Common.Common.DateTimeNowLocal()
                };

                db.RecurringShiftChangeRequests.Add(shiftChangeRequest);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, shiftChangeRequest.Id);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));


        }


        [HttpGet]
        [Route("api/EmployeeShiftActionAPI/GetRecurringShiftChangeRequests")]
        public IEnumerable<RecurringShiftChangeRequestDTO> GetRecurringShiftChangeRequests(string startDate, string endDate)
        {
            var userName = User.Identity.Name;
            DateTime sDate = DateTime.Parse(startDate);
            DateTime eDate = DateTime.Parse(endDate);

            if (String.IsNullOrEmpty(userName))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Username is empty"));

            var recShiftChangeRequestList = db.RecurringShiftChangeRequests.Where(rs => rs.ShiftTemplate.Employee.UserProfile.Email == userName
                                                                            && rs.OccurenceDate <= eDate
                                                                            && rs.OccurenceDate >= sDate
                                                                            && rs.Status != RequestStatus.Denied);

            return MapperFacade.MapperConfiguration.Map<IEnumerable<RecurringShiftChangeRequest>, IEnumerable<RecurringShiftChangeRequestDTO>>(recShiftChangeRequestList.AsEnumerable());

        }


        [HttpGet]
        [Route("api/EmployeeShiftActionAPI/GetShiftChangeRequests")]
        public IEnumerable<ShiftChangeRequestDTO> GetShiftChangeRequests(string startDate, string endDate)
        {
            var userName = User.Identity.Name;
            DateTime sDate = DateTime.Parse(startDate);
            DateTime eDate = DateTime.Parse(endDate);

            if (String.IsNullOrEmpty(userName))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Username is empty"));

            var shiftChangeRequestList = db.ShiftChangeRequests.Where(sc => sc.CreatedBy.Email == userName
                                                                            && sc.Shift.StartTime <= eDate
                                                                            && sc.Shift.StartTime >= sDate
                                                                            && sc.Status != RequestStatus.Denied);
            return MapperFacade.MapperConfiguration.Map<IEnumerable<ShiftChangeRequest>, IEnumerable<ShiftChangeRequestDTO>>(shiftChangeRequestList.AsEnumerable());

        }

        [HttpPost]
        [ActionName("RequestOpenShift")]
        public HttpResponseMessage RequestOpenShift(Guid Id, ShiftChangeActionDTO shiftRequestDTO)
        {
            var shift = db.Shifts.Find(Id);
            if (shift == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Shift not found");

            //Only the staff member assigned to the shift can request to cancel the shift
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", shift.Roster.BusinessLocation.Business.Id.ToString()))
            {
                var email = HttpContext.Current.User.Identity.Name;

                var employee = db.Employees.First(emp => emp.UserProfile.Email == email && emp.BusinessLocation.Id == shift.Roster.BusinessLocation.Id);

                //Check to see if there is already a pending shift request
                var shiftRequest = db.ShiftChangeRequests.Where(sc => sc.Type == ShiftRequestType.TakeOpenShift
                                                                        && sc.Shift.Id == Id
                                                                        && sc.Status == RequestStatus.Pending
                                                                        && sc.CreatedBy.Id == employee.Id);
                if (shiftRequest.Count() > 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Existing request already pending for shift id:" + Id.ToString());

                ShiftChangeRequest shiftChangeRequest = new ShiftChangeRequest
                {
                    Id = Guid.NewGuid(),
                    Shift = shift,
                    Type = ShiftRequestType.TakeOpenShift,
                    Status = RequestStatus.Pending,
                    Reason = shiftRequestDTO.Reason,
                    CreatedDate = WebUI.Common.Common.DateTimeNowLocal(),
                    CreatedBy = employee
                };

                db.ShiftChangeRequests.Add(shiftChangeRequest);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, shiftChangeRequest.Id);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));


        }



        [HttpPost]
        [ActionName("RequestExternalShift")]
        public HttpResponseMessage RequestExternalShift(Guid Id, ExternalShiftActionDTO ExternalshiftRequestDTO)
        {
            try
            {
                var ExternalshiftBroadcast = db.ExternalShiftBroadcasts.Find(Id);
                if (ExternalshiftBroadcast == null)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "ExternalShiftBroadCast not found");

                //Only the staff member assigned to the shift can request to cancel the shift

                var email = HttpContext.Current.User.Identity.Name;

                var user = db.UserProfiles.FirstOrDefault(a => a.Email == email && a.IsRegisteredExternal == true);

                var ExternalempLoyee = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == ExternalshiftBroadcast.Shifts.First().InternalLocation.BusinessLocation.Id);

                //Check to see if there is already a pending shift request
                var shiftRequest = db.ExternalShiftRequests.Where(sc => sc.Type == ExternalShiftRequestType.TakeExternalShift
                                                                        && sc.ExternalShiftBroadcast.Id == Id
                                                                        && sc.Status == RequestStatus.Pending
                                                                        && sc.CreatedBy.Id == user.Id);
                if (shiftRequest.Count() > 0)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Existing request already pending for shift id:" + Id.ToString());

                ExternalShiftRequest shiftChangeRequest = new ExternalShiftRequest
                {
                    Id = Guid.NewGuid(),
                    ExternalShiftBroadcast = ExternalshiftBroadcast,
                    Type = ExternalShiftRequestType.TakeExternalShift,
                    Status = RequestStatus.Pending,
                    Reason = ExternalshiftRequestDTO.Reason,
                    CreatedDate = WebUI.Common.Common.DateTimeNowLocal(),
                    CreatedBy = user
                };

                db.ExternalShiftRequests.Add(shiftChangeRequest);
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.Created, shiftChangeRequest.Id);
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Trace.TraceInformation(
                              "Class: {0}, Property: {1}, Error: {2}",
                              validationErrors.Entry.Entity.GetType().FullName,
                              validationError.PropertyName,
                              validationError.ErrorMessage);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.ExpectationFailed);
            }
        }


        [HttpGet]
        [Route("api/EmployeeShiftActionAPI/EmployeeExternalShifts")]
        public ExternalBroadcastDTO EmployeeExternalShifts(Guid Id)
        {
            ExternalBroadcastDTO model = new ExternalBroadcastDTO();

            var result = db.ExternalShiftBroadcasts.Where(a => a.Id == Id).FirstOrDefault();
            model = MapperFacade.MapperConfiguration.Map<ExternalShiftBroadcast, ExternalBroadcastDTO>(result);

            model.Skills = db.ExternalShiftSkills.Where(a => a.ExternalShiftBroadcastId == Id).Select(x => new DropDownDTO
            {
                ValueGuid = x.IndustrySkill.Id,
                Text = x.IndustrySkill.Name
            }).ToList();


            model.Qualification = db.ExternalShiftQualifications.Where(a => a.ExternalShiftBroadcastId == Id).Select(x => new DropDownDTO
            {
                Text = x.Qualificationslookup.QualificationName,
                ValueGuid = x.Qualificationslookup.QualificationId
            }).ToList();
            return model;

        }

        [HttpPost]
        public HttpResponseMessage MessageforExternalShift(Guid Id, ExternalBroadcastDTO model)
        {
            var ExternalshiftBroadcast = db.ExternalShiftBroadcasts.Find(Id);
            var email = HttpContext.Current.User.Identity.Name;
            //var Externalemployee = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == ExternalshiftBroadcast.Shifts.First().InternalLocation.BusinessLocation.Id);
            var ExternalshiftRequest = db.ExternalShiftRequests.Where(a => a.ExternalShiftBroadcast.Id == Id && a.CreatedBy.Id == db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Id).FirstOrDefault();
            if (ExternalshiftRequest == null)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "ExternalShiftRequest not found");

            ExternalshiftRequest.ExternalShiftMessage = model.EmployeeMessage;
            db.SaveChanges();

            return Request.CreateResponse(HttpStatusCode.Created, ExternalshiftRequest.Id);
        }

    }
}
