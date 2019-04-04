using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class RosterAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/RosterAPI/5
        public RosterDTO GetRoster(Guid id)
        {
            Roster roster = db.Rosters.Find(id);
            if (roster == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", roster.BusinessLocation.Business.Id.ToString()))
            {
                return MapperFacade.MapperConfiguration.Map<Roster, RosterDTO>(roster);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        // GET api/rosterapi?businessId=1231232&startDate=12312&endDate=1232132
        public IEnumerable<RosterDTO> GetRostersForBusiness(Guid businessLocationId, string startDate, string endDate)
        {
            DateTime sDate = DateTime.Parse(startDate);
            DateTime eDate = DateTime.Parse(endDate);
            BusinessLocation businessLocation = db.BusinessLocations.Find(businessLocationId);
            if (businessLocation == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", businessLocation.Business.Id.ToString()))
            {
                var rosterList = db.Rosters.Where(r => r.BusinessLocation.Id == businessLocationId && r.WeekStartDate >= sDate && r.WeekStartDate <= eDate);

                if (rosterList == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }

                return MapperFacade.MapperConfiguration.Map<IEnumerable<Roster>, IEnumerable<RosterDTO>>(rosterList.AsEnumerable());
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        // POST api/shiftapi
        //Create a new shift
        public HttpResponseMessage PostRoster([FromBody]RosterCreateDTO rosterCreateDTO)
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", rosterCreateDTO.BusinessLocationId.ToString()))
            {
                if (ModelState.IsValid)
                {
                    Roster existingRoster = db.Rosters.Where(r => r.BusinessLocation.Id == rosterCreateDTO.BusinessLocationId
                                                                && r.WeekStartDate == rosterCreateDTO.WeekStartDate.Date).FirstOrDefault();
                    //Business rules
                    if (existingRoster != null)
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "A roster already exists for the week starting " + rosterCreateDTO.WeekStartDate.ToShortDateString());
                    //Cannot create a roster for a weeks starting in the past EXCEPT if it is the current week.
                    if (rosterCreateDTO.WeekStartDate.Date < WebUI.Common.Common.GetNextWeekday(WebUI.Common.Common.DateTimeNowLocal(), DayOfWeek.Monday).Date.AddDays(-7))
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "A roster cannot be created for a week starting in the past");

                    var rosterList = new List<Roster>();
                    var businessLoc = db.BusinessLocations.Find(rosterCreateDTO.BusinessLocationId);
                    var rosterWeek1 = CreateRosterForWeekStarting(rosterCreateDTO, businessLoc);
                    db.Rosters.Add(rosterWeek1);

                    //If rosters are being created for a month or fortnight.
                    if (rosterCreateDTO.RostersToCreate == RosterToCreateEnum.Fortnight
                        || rosterCreateDTO.RostersToCreate == RosterToCreateEnum.Month)
                    {
                        var rosterWeek2 = CreateRosterForWeekStarting(rosterCreateDTO, businessLoc, 1);
                        db.Rosters.Add(rosterWeek2);
                        //For month, add remaining two weeks
                        if (rosterCreateDTO.RostersToCreate == RosterToCreateEnum.Month)
                        {
                            var rosterWeek3 = CreateRosterForWeekStarting(rosterCreateDTO, businessLoc, 2);
                            db.Rosters.Add(rosterWeek3);
                            var rosterWeek4 = CreateRosterForWeekStarting(rosterCreateDTO, businessLoc, 3);
                            db.Rosters.Add(rosterWeek4);
                        }
                    }

                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }
            }
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have appropriate permission"));
        }

        #region Private helper methods
        private Roster CreateRosterForWeekStarting(RosterCreateDTO rosterCreateDTO, BusinessLocation businessLocation, int weeksToAdd = 0)
        {
            var roster = MapperFacade.MapperConfiguration.Map<RosterCreateDTO, Roster>(rosterCreateDTO);
            roster.Id = Guid.NewGuid(); //Assign new ID on save.
            roster.BusinessLocation = businessLocation;
            roster.WeekStartDate = roster.WeekStartDate.AddDays(7 * weeksToAdd);

            roster.Timesheet = new Timesheet { Id = Guid.NewGuid() };

            //Add template shifts to roster
            if (rosterCreateDTO.UseShiftTemplates)
            {
                //First set up all recurring weekly shifs
                var weeklyShiftTemplates = new List<ShiftTemplate>();
                weeklyShiftTemplates.AddRange(businessLocation.ShiftTemplates.Where(st => st.Frequency == ShiftFrequency.Weekly && st.Enabled));
                CreateShiftsFromTemplates(roster, weeklyShiftTemplates);

                //Then set up any once off shifts for this week
                var oneOffShiftTemplates = new List<ShiftTemplate>();
                oneOffShiftTemplates.AddRange(businessLocation.ShiftTemplates.Where(st => st.Frequency == ShiftFrequency.OneOff && st.WeekStarting == roster.WeekStartDate && st.Enabled));
                CreateShiftsFromTemplates(roster, oneOffShiftTemplates);


                var weekOfYear = WebUI.Common.Common.GetWeekOfYear(roster.WeekStartDate);

                //If this is an odd week in the year then apply fortnight1 (this is due to 1st week of year being off, so start year on fortnight1)
                if (weekOfYear % 2 == 1)
                {
                    var firstWeekShiftTemplates = businessLocation.ShiftTemplates.Where(st => st.Frequency == ShiftFrequency.Fortnight1 && st.Enabled).ToList();

                    CreateShiftsFromTemplates(roster, firstWeekShiftTemplates);
                }
                else //Every other week apply Fortnight2
                {
                    var secondWeekShiftTemplates = businessLocation.ShiftTemplates.Where(st => st.Frequency == ShiftFrequency.Fortnight2 && st.Enabled);

                    CreateShiftsFromTemplates(roster, secondWeekShiftTemplates);
                }

                if (weeksToAdd == 3) //If we have added at least 4 weeks (ie one month) then insert monthly template
                {
                    var monthShiftTemplates = businessLocation.ShiftTemplates.Where(st => st.Frequency == ShiftFrequency.Monthly && st.Enabled);

                    CreateShiftsFromTemplates(roster, monthShiftTemplates);
                }
            }
            return roster;
        }

        private void CreateShiftsFromTemplates(Roster roster, IEnumerable<ShiftTemplate> shiftTemplates)
        {
            foreach (ShiftTemplate shiftTemplate in shiftTemplates)
            {
                if (IsShiftRequired(shiftTemplate, DayOfWeek.Monday))
                    AddShiftsFromTemplate(roster, shiftTemplate, DayOfWeek.Monday);
                if (IsShiftRequired(shiftTemplate, DayOfWeek.Tuesday))
                    AddShiftsFromTemplate(roster, shiftTemplate, DayOfWeek.Tuesday);
                if (IsShiftRequired(shiftTemplate, DayOfWeek.Wednesday))
                    AddShiftsFromTemplate(roster, shiftTemplate, DayOfWeek.Wednesday);
                if (IsShiftRequired(shiftTemplate, DayOfWeek.Thursday))
                    AddShiftsFromTemplate(roster, shiftTemplate, DayOfWeek.Thursday);
                if (IsShiftRequired(shiftTemplate, DayOfWeek.Friday))
                    AddShiftsFromTemplate(roster, shiftTemplate, DayOfWeek.Friday);
                if (IsShiftRequired(shiftTemplate, DayOfWeek.Saturday))
                    AddShiftsFromTemplate(roster, shiftTemplate, DayOfWeek.Saturday);
                if (IsShiftRequired(shiftTemplate, DayOfWeek.Sunday))
                    AddShiftsFromTemplate(roster, shiftTemplate, DayOfWeek.Sunday);
            }
        }

        private void AddShiftsFromTemplate(Roster roster, ShiftTemplate shiftTemplate, DayOfWeek dayWeek)
        {
            var shift = CreateShiftFromTemplate(roster, shiftTemplate, dayWeek);
            if (shift != null && shift.StartTime > WebUI.Common.Common.DateTimeNowLocal()) //ensure shift does not occur in the paste
                roster.Shifts.Add(shift);

        }

        private Shift CreateShiftFromTemplate(Roster roster, ShiftTemplate shiftTemplate, DayOfWeek dayOfWeek)
        {
            int dayWeekOffset = 6; //Set default offset for Sunday (6 days from monday which is our week start date)
            //Adjust offset so that Monday is zero, and sunday is 6
            if (dayOfWeek != DayOfWeek.Sunday)
                dayWeekOffset = (int)dayOfWeek - 1;

            DateTime dtOccurrenceDate = roster.WeekStartDate.AddDays(dayWeekOffset);

            //Ensure that this shift template does not have a corresponding recurring shift cancellation which is approved
            var approvedShiftCancelRequests = db.RecurringShiftChangeRequests.Where(sc => sc.ShiftTemplate.Id == shiftTemplate.Id
                                                                                 && sc.OccurenceDate == dtOccurrenceDate
                                                                                  && sc.Type == ShiftRequestType.Cancel
                                                                                  && sc.Status == RequestStatus.Approved);
            // DbFunctions.CreateDateTime
            //If the shift template does not have a corresponding cancellation for this occurrence, then proceed and create the shift
            if (approvedShiftCancelRequests.Count() == 0)
            {
                Shift shift = new Shift();
                shift.Id = Guid.NewGuid();

                shift.InternalLocation = shiftTemplate.InternalLocation;
                shift.Role = shiftTemplate.Role;
                shift.Employee = shiftTemplate.Employee;
                //shift.ExternalShiftBroadcast = new ExternalShiftBroadcast { Id = Guid.NewGuid(), Description = "", Wage = "" };
                shift.StartTime = DateTime.Parse(roster.WeekStartDate.AddDays(dayWeekOffset).ToShortDateString() + " " + shiftTemplate.StartTime.ToString());
                if (shiftTemplate.FinishNextDay)
                    shift.FinishTime = DateTime.Parse(roster.WeekStartDate.AddDays(dayWeekOffset + 1).ToShortDateString() + " " + shiftTemplate.FinishTime.ToString());
                else
                    shift.FinishTime = DateTime.Parse(roster.WeekStartDate.AddDays(dayWeekOffset).ToShortDateString() + " " + shiftTemplate.FinishTime.ToString());
                shift.ShiftTemplate = shiftTemplate;
                return shift;
            }
            else
                return null;
        }

        private static bool IsShiftRequired(ShiftTemplate shiftTemplate, DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return shiftTemplate.Monday;
                case DayOfWeek.Tuesday:
                    return shiftTemplate.Tuesday;
                case DayOfWeek.Wednesday:
                    return shiftTemplate.Wednesday;
                case DayOfWeek.Thursday:
                    return shiftTemplate.Thursday;
                case DayOfWeek.Friday:
                    return shiftTemplate.Friday;
                case DayOfWeek.Saturday:
                    return shiftTemplate.Saturday;
                case DayOfWeek.Sunday:
                    return shiftTemplate.Sunday;
            }
            return false;
        }
        #endregion Private Helper methods
    }
}
