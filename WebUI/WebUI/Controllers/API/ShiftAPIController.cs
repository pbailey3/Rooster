using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;


namespace WebUI.Controllers.API
{
    [Authorize]
    public class ShiftAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        

        //Get a roster with Shifts for one day of the week
        public IEnumerable<ShiftDTO> GetShiftsForDay(Guid rosterId, DayOfWeek dayOfWeek)
        {
            var roster = db.Rosters.Find(rosterId);
            
            if (roster == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", roster.BusinessLocation.Business.Id.ToString()))
            {
               var shiftList = db.Shifts.Where(s => s.Roster.Id == rosterId);
               var shiftDTOList =  MapperFacade.MapperConfiguration.Map<IEnumerable<Shift>, IEnumerable<ShiftDTO>>(shiftList);
               //LINQ to entities does not support query using dayOfWeek, therefore filter after converting to DTO collection (overhead of retrieveing all records)
               return shiftDTOList.Where(s => s.StartDateTime.DayOfWeek == dayOfWeek);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        //Get a roster with Shifts for one day of the week
        public IEnumerable<ShiftDTO> GetShiftsForDay(Guid businessLocationId, DateTime date)
        {
            DateTime eDate = date.AddDays(1);
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocationId.ToString()))
            {
                var shiftList = db.Shifts.Where(s => s.Roster.BusinessLocation.Id == businessLocationId 
                                                && s.StartTime >= date && s.StartTime < eDate);
                var shiftDTOList = MapperFacade.MapperConfiguration.Map<IEnumerable<Shift>, IEnumerable<ShiftDTO>>(shiftList);
                return shiftDTOList;
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        // POST api/shiftapi
        //Create a new shift
        public HttpResponseMessage PostShift([FromBody]ShiftDTO shiftDTO, [FromUri]Guid businessLocationId)
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocationId.ToString()))
            {
                if (ModelState.IsValid)
                {
                    var dateMinusWeek = shiftDTO.StartDateTime.AddDays(-7);
                    Roster roster = db.Rosters.Find(shiftDTO.RosterId);
                  
                    //Business rules
                    if (roster == null)
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find corresponding roster");
                    if (shiftDTO.StartDateTime > shiftDTO.FinishDateTime)
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Shift start time must be before end time");
                    if (shiftDTO.StartDateTime < roster.WeekStartDate
                        || shiftDTO.StartDateTime >= roster.WeekStartDate.AddDays(7))
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Shift start time must be within the roster week starting '" + roster.WeekStartDate.ToShortDateString() + "'");
                    if ((shiftDTO.FinishDateTime - shiftDTO.StartDateTime).Days > 1)
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Shift can not span more than one day");
                    if (shiftDTO.StartDateTime < WebUI.Common.Common.DateTimeNowLocal())
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Shift start time must not be in the past");

                    using (ScheduleActionAPIController scheduleAPIController = new ScheduleActionAPIController())
                    {
                        var unavailEmployees = scheduleAPIController.GetUnavailableEmployees(shiftDTO.BusinessLocationId.GetValueOrDefault(), shiftDTO.StartDateTime.Ticks.ToString(), shiftDTO.FinishDateTime.Ticks.ToString(), Guid.Empty);
                       if (unavailEmployees.FirstOrDefault(e => e.Id == shiftDTO.EmployeeId) != null)
                       {
                           return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Employee is not available on this day");
                       }
                    }

                    var shift = MapperFacade.MapperConfiguration.Map<ShiftDTO, Shift>(shiftDTO);
                    shift.Id = Guid.NewGuid(); //Assign new ID on save.
                    shift.Roster = roster;
                    shift.Employee = db.Employees.Find(shiftDTO.EmployeeId);
                    shift.InternalLocation = db.InternalLocations.Find(shiftDTO.InternalLocationId);
                    shift.Role = db.Roles.Find(shiftDTO.RoleId);

                    db.Shifts.Add(shift);
                    db.SaveChanges();

                    //If save as shift block also selected, then create new shift block
                    if (shiftDTO.SaveAsShiftBlock)
                    {
                        ShiftBlockDTO shiftBlockDTO = new ShiftBlockDTO
                        {
                            StartTime = shiftDTO.StartTime,
                            FinishTime = shiftDTO.FinishTime,
                            BusinessLocationId = businessLocationId,
                            RoleId = shiftDTO.RoleId,
                            FinishNextDay = (shiftDTO.FinishDay > shiftDTO.StartDay)
                        };

                        using (ShiftBlockAPIController sb = new ShiftBlockAPIController())
                        {
                            sb.Post(shiftBlockDTO);
                        }

                    }

                    //HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                    //response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = role.Id }));
                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }
        // PUT api/ShiftAPI/5
        //Update an existing record
        [Authorize]
        public HttpResponseMessage PutShift(Guid id, ShiftDTO shiftDTO)
        {
            var businessLocId = db.Shifts.Find(id).Roster.BusinessLocation.Id;

            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocId.ToString()))
            {
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                if (id != shiftDTO.Id)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Id provided does not match object.");
                try
                {
                  
                    var shift = MapperFacade.MapperConfiguration.Map<ShiftDTO, Shift>(shiftDTO, db.Shifts.Single(s => s.Id == id));

                    //If an already published shift is being edited need to send notifications for any changes to employee as a cancel notification
                    if (shift.IsPublished)
                    {
                        //Notify employee that the shift is being taken from as a cancellation
                        if( shift.Employee != null && shift.Employee.UserProfile != null && shift.Employee.Id != shiftDTO.EmployeeId)
                            MessagingService.ShiftCancelled(shift.Employee.UserProfile.Email, shift.Employee.UserProfile.FirstName, shift.InternalLocation.BusinessLocation.Name, shift.StartTime, shift.FinishTime);
                    }

                    //If the employee assigned to the shift has changed, need to notify them that they have a shift broadcast
                    var employeeAdded = (shiftDTO.EmployeeId != null && (shift.Employee == null || shift.Employee.Id != shiftDTO.EmployeeId));
                   
                    shift.Employee = db.Employees.Find(shiftDTO.EmployeeId);

                    //Notify the employee they have been added to a published shift
                    if (shift.IsPublished && employeeAdded && shift.Employee.UserProfile != null)
                        MessagingService.ShiftBroadcast(shift.Employee.UserProfile.Email, shift.Employee.UserProfile.FirstName);
                   
                    shift.InternalLocation = db.InternalLocations.Find(shiftDTO.InternalLocationId);
                    shift.Role = db.Roles.Find(shiftDTO.RoleId);

                    db.Entry(shift).State = EntityState.Modified;

                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }
      

        // GET api/shiftapi/5
        [HttpGet]
        public ShiftDTO Get(Guid id)
        {
            var shift = db.Shifts.Find(id);
            if (shift == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", shift.Roster.BusinessLocation.Business.Id.ToString()))
            {
                return MapperFacade.MapperConfiguration.Map<Shift, ShiftDTO>(shift);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        //// DELETE api/shiftapi/5
        public HttpResponseMessage Delete(Guid id)
        {
            var shift = db.Shifts.Find(id);
            if (shift == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            //If published, send notification to staff member to tell them shift is cancelled
            if (shift.IsPublished && shift.Employee != null && shift.Employee.UserProfile != null)
                MessagingService.ShiftCancelled(shift.Employee.UserProfile.Email, shift.Employee.UserProfile.FirstName, shift.InternalLocation.BusinessLocation.Name, shift.StartTime, shift.FinishTime);
           
            db.ShiftChangeRequests.RemoveRange(shift.ShiftChangeRequests);
            
            db.Shifts.Remove(shift);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);

        }

        // GET api/<controller>
        [HttpGet]
        [Route("api/ShiftAPI/OpenShiftsForCurrent")]
        public OpenShiftsEmployeeIndexDTO GetOpenShiftsForCurrentUser()
        {
            var email = HttpContext.Current.User.Identity.Name;
            var dtNow = WebUI.Common.Common.DateTimeNowLocal();

            //Get list of roles which the user has been assigned
            var empRoles = from emp in db.Employees
                           where emp.UserProfile.Email == email
                           select emp.Roles;

            //Get list of businesses which user is an employee of
            var empBusIdList = from emp in db.Employees
                               where emp.UserProfile.Email == email
                               select emp.BusinessLocation.Id;

            //Get list of open shifts Requests linked to busineses which user is an employee of
            var openShiftList = from s in db.Shifts
                                where s.IsPublished == true
                                && s.Employee == null
                                && s.StartTime > dtNow
                                && empBusIdList.Contains(s.Roster.BusinessLocation.Id)
                                && (empRoles.SelectMany(x => x).Any(y => y.Id == s.Role.Id) || s.Role == null)
                                select s;

            //Get list of open shift requests from current user
            var empOpenShiftRequests = from sr in db.ShiftChangeRequests
                               where sr.CreatedBy.UserProfile.Email == email
                               && sr.Type == ShiftRequestType.TakeOpenShift
                               && sr.Shift.StartTime > dtNow
                               select sr.Shift.Id;

            OpenShiftsEmployeeIndexDTO retVal = new OpenShiftsEmployeeIndexDTO();
            retVal.OpenShifts = MapperFacade.MapperConfiguration.Map<IEnumerable<Shift>, IEnumerable<ShiftDTO>>(openShiftList.AsEnumerable());
            retVal.OpenShiftRequests = empOpenShiftRequests.AsEnumerable();
            return retVal;
        }
    }
}
