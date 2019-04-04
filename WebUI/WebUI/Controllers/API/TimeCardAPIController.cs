using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Thinktecture.IdentityModel;
using WebUI.DTOs;
using WebUI.Models;
using WebUI.Common;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class TimeCardAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

#if DEBUG
        // POST api/EmployeeAPI
        [HttpPost]
       // [Route("api/TimeCardAPI/ClockInTest/{employeeId}/BusLocation/{businessLocationId}/DateTimePunch/{timePunch}")]
        [Route("api/TimeCardAPI/ClockInTest/{employeeId}/BusLocation/{businessLocationId}/DateTimePunch/{timePunchTicks}")]

        public ClockInOutReponseDTO ClockInOutTEST(Guid employeeId, Guid businessLocationId, long timePunchTicks)
        {
            var timePunchVal = new DateTime(timePunchTicks);
            return PunchTimeClock(employeeId, businessLocationId, timePunchVal);
        }
#endif
        // POST api/EmployeeAPI
        [HttpPost]
        [Route("api/TimeCardAPI/ClockIn/{employeeId}/BusLocation/{businessLocationId}")]
        public ClockInOutReponseDTO ClockInOut(Guid employeeId, Guid businessLocationId)
        {
            return PunchTimeClock(employeeId, businessLocationId);
        }

        private ClockInOutReponseDTO PunchTimeClock(Guid employeeId, Guid businessLocationId, DateTime? timePunchVal = null)
        {
            //NOTE: EMployee Id can be an EMployee QR code, or a UserProfile QR code. Need to check for both to get a match.
            var responseObj = new ClockInOutReponseDTO();

            //First check if this is an employeeif
            Employee emp = db.Employees.Find(employeeId);
            var authEmail = HttpContext.Current.User.Identity.Name;

            if (emp == null) //Try and see if it is a UserProfile QR code with a linked employee to the selected business location
            {
                var usrProfile = db.UserProfiles.Find(employeeId);
                if (usrProfile != null)
                {
                    //Find employee linked to userprofile for selected business location
                    emp = usrProfile.Employees.FirstOrDefault(e => e.BusinessLocation.Id == businessLocationId);
                    if (emp == null)
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No matching employee record to the business location could be found"));
                }
                else
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No matching employee or user profile records could be found "));

            }
            //throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            if (!emp.IsActive)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Employee is NOT active"));

            if (emp.BusinessLocation.Id != businessLocationId)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Employee does not belong to business location"));

            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocationId.ToString()))
            {
#if DEBUG
                DateTime timePunch = (timePunchVal.HasValue)? timePunchVal.Value : WebUI.Common.Common.DateTimeNowLocal();
#else
                DateTime timePunch = WebUI.Common.Common.DateTimeNowLocal();
#endif

                //See if there are any timecards not chceked out for the employee
                TimeCard tc = db.TimeCards.FirstOrDefault(t => t.Employee.Id == emp.Id && !t.ClockOut.HasValue);
                bool exists = false;
                //If there is no open check in record create a new time card entry
                if (tc == null)
                { //CHECK IN

                    //Get the roster object associated with this time punch
                    var weekStartDate = WebUI.Common.Common.GetStartOfWeek(timePunch, DayOfWeek.Monday);
                    var roster = db.Rosters.FirstOrDefault(r => r.BusinessLocation.Id == businessLocationId && r.WeekStartDate == weekStartDate);
                    if (roster == null)
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to find matching Roster obejct for week"));

                    //See if we can locate a shift at the business location which corresponds to this check in event.
                    //Search for a start time which is within 30 mins of timepunch
                    var dtMinTime = timePunch.AddMinutes(-WebUI.Common.Constants.TimeCardShiftVariance);
                    var dtMaxTime = timePunch.AddMinutes(WebUI.Common.Constants.TimeCardShiftVariance);
                    var shift = db.Shifts.FirstOrDefault(s => s.Roster.BusinessLocation.Id == businessLocationId && s.IsPublished && s.Employee.Id == emp.Id
                                                        && s.StartTime >= dtMinTime && s.StartTime <= dtMaxTime);

                    tc = new TimeCard();
                    tc.Roster = roster;
                    tc.Id = Guid.NewGuid();
                    tc.ClockIn = timePunch;
                    tc.Employee = emp;
                    tc.Shift = shift;
                    responseObj.CheckInOut = CheckInOutEnum.CheckIn;
                    responseObj.Message = "Check in confirmed. ";
                    if (shift != null)
                        responseObj.ShiftDetails = shift.StartTime.ToShortTimeString() + " - " + shift.FinishTime.ToShortTimeString();

                    tc.TimesheetEntry = new TimesheetEntry();
                    tc.TimesheetEntry.Id = Guid.NewGuid();
                    //If a matched shift has been found then default the timesheet entry to the matched shift
                    //Otherwise set it to the timeclocked entry
                    if (shift != null)
                    {
                        tc.TimesheetEntry.StartDateTime = shift.StartTime;
                        tc.TimesheetEntry.FinishDateTime = shift.FinishTime;
                    }
                    else
                        tc.TimesheetEntry.StartDateTime = new DateTime(timePunch.Year, timePunch.Month, timePunch.Day, timePunch.Hour, timePunch.Minute, 0);
                }
                else //Check out the previous record
                {
                    //CHECK OUT
                    exists = true;
                    tc.ClockOut = timePunch;
                    responseObj.CheckInOut = CheckInOutEnum.CheckOut;
                    responseObj.Message = "Check out confirmed. ";
                    if (tc.Shift != null)
                        responseObj.ShiftDetails = tc.Shift.StartTime.ToShortTimeString() + " - " + tc.Shift.FinishTime.ToShortTimeString();
                    else //Timesheet entry default finished time to the timepunch if it has not been defaulted to any linked shift
                        tc.TimesheetEntry.FinishDateTime = new DateTime(timePunch.Year, timePunch.Month, timePunch.Day, timePunch.Hour, timePunch.Minute, 0);
                }

                responseObj.Message += (emp.FirstName + " " + emp.LastName + " " + timePunch.ToString(WebUI.Common.Common.GetLocaleDateTimeDisplayFormat(HttpContext.Current.Request.UserLanguages.FirstOrDefault())) + " @ " + emp.BusinessLocation.Name);
                tc.BusinessLocation = emp.BusinessLocation;
                tc.LastUpdatedDate = WebUI.Common.Common.DateTimeNowLocal();
                tc.LastUpdatedBy = db.UserProfiles.First(Usr => Usr.Email == authEmail).Employees.First(u => u.BusinessLocation.Id == businessLocationId);

                if (!exists)
                    db.TimeCards.Add(tc);

                db.SaveChanges();

                return responseObj;
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }
    }
}
