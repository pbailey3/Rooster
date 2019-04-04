using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;
using FeatureToggle;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class TimesheetAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();


        // GET api/timesheetapi?businessId=1231232&startDate=12312&endDate=1232132
        public IEnumerable<TimesheetDTO> GetTimesheetsForBusiness(Guid businessLocationId, string startDate, string endDate)
        {
            if (Is<TimesheetFeature>.Enabled)
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

                    var timeSheets = MapperFacade.MapperConfiguration.Map<IEnumerable<Timesheet>, IEnumerable<TimesheetDTO>>(rosterList.Select(r => r.Timesheet).AsEnumerable());


                    //Calculate total hours of all timesheet entries for this period.
                    foreach (var timesheet in timeSheets)
                    {
                        TimeSpan totalShiftTime = new TimeSpan();
                        Decimal totalCost = new decimal();

                        var timeSheetEntries = db.TimesheetEntries.Where(ts => ts.TimeCard.Roster.Id == timesheet.RosterId);
                        foreach (var timesheetEntry in timeSheetEntries)
                        {

                            var timeSpan = timesheetEntry.FinishDateTime - timesheetEntry.StartDateTime;
                            if (timeSpan.HasValue)
                            {
                                totalShiftTime = totalShiftTime.Add(timeSpan.Value);
                                var shiftCost = (decimal)timeSpan.Value.TotalHours * timesheetEntry.TimeCard.Employee.PayRate.GetValueOrDefault();
                                totalCost += shiftCost;
                            }
                        }

                        timesheet.TotalTimeSheetEntries = timeSheetEntries.Count();
                        if ((decimal)totalShiftTime.TotalHours > 0)
                            timesheet.AverageWage = totalCost / (decimal)totalShiftTime.TotalHours;
                        timesheet.TotalHours = totalShiftTime.TotalHours;

                        timesheet.TotalCost = totalCost;

                    }

                    return timeSheets;
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }

        private bool CheckTimeCardContainsErrors(TimeCard timecard)
        {
            bool containsErrors = false;

            //Check if entry contains errors
            //Flag if there is not matched rostered shift 
            if (timecard.Shift == null)
                containsErrors = true;
            if (!timecard.ClockOut.HasValue) //Flag if the user has not clocked out.
                containsErrors = true;
            if (!containsErrors
                  && timecard.Shift != null)
            {

                var dtStartMinTime = timecard.Shift.StartTime.AddMinutes(-WebUI.Common.Constants.TimeCardShiftVariance);
                var dtStartMaxTime = timecard.Shift.StartTime.AddMinutes(WebUI.Common.Constants.TimeCardShiftVariance);

                var dtFinishMinTime = timecard.Shift.FinishTime.AddMinutes(-WebUI.Common.Constants.TimeCardShiftVariance);
                var dtFinishMaxTime = timecard.Shift.FinishTime.AddMinutes(WebUI.Common.Constants.TimeCardShiftVariance);

                if ((timecard.ClockIn < dtStartMinTime || dtStartMaxTime < timecard.ClockIn)//If the clockin is earlier than set period before shit or later than period after rostered start then error
                    || (timecard.ClockOut < dtFinishMinTime || timecard.ClockOut > dtFinishMaxTime)) //Or the clock out is more than set time early or later 
                    containsErrors = true;
            }

            return containsErrors;
        }

        // GET api/TimesheetAPI/5
        public TimesheetWeekDTO GetWeekTimesheet(Guid id)
        {
            if (Is<TimesheetFeature>.Enabled)
            {
                Timesheet timesheet = db.Timesheets.Find(id);
                if (timesheet == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

                if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", timesheet.Roster.BusinessLocation.Business.Id.ToString()))
                {
                    var weekTimesheetDTO = MapperFacade.MapperConfiguration.Map<Timesheet, TimesheetWeekDTO>(timesheet);
                    weekTimesheetDTO.TimesheetEntries = new List<TimesheetEntryDTO>();

                    DateTime weekStart = timesheet.Roster.WeekStartDate;
                    DateTime weekEnd = weekStart.AddDays(7);

                    //Get all timecard entries which have a clock in date time within the Timesheets roster week
                    var timecards = db.TimeCards.Where(tc => tc.BusinessLocation.Id == timesheet.Roster.BusinessLocation.Id && weekStart <= tc.ClockIn && tc.ClockIn < weekEnd);
                    foreach (var timecard in timecards)
                    {
                        var timesheetEntryDTO = MapperFacade.MapperConfiguration.Map<TimesheetEntry, TimesheetEntryDTO>(timecard.TimesheetEntry);

                        timesheetEntryDTO.ContainsErrors = this.CheckTimeCardContainsErrors(timecard);

                        weekTimesheetDTO.TimesheetEntries.Add(timesheetEntryDTO);
                    }

                    return weekTimesheetDTO;
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }

        [HttpGet]
        [Route("api/TimesheetAPI/TimesheetEntry/{id}")]
        public TimesheetEntryDTO GetTimesheetEntry(Guid id)
        {
            if (Is<TimesheetFeature>.Enabled)
            {
                TimesheetEntry timesheetEntry = db.TimesheetEntries.Find(id);
                if (timesheetEntry == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

                if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", timesheetEntry.TimeCard.BusinessLocation.Business.Id.ToString()))
                {
                    var timesheetEntryDTO = MapperFacade.MapperConfiguration.Map<TimesheetEntry, TimesheetEntryDTO>(timesheetEntry);
                    timesheetEntryDTO.PayRate = timesheetEntry.TimeCard.Employee.PayRate.GetValueOrDefault();

                    var timeSpan = timesheetEntry.FinishDateTime - timesheetEntry.StartDateTime;
                    if (timeSpan.HasValue)
                    {
                        timesheetEntryDTO.Pay = (decimal)timeSpan.Value.TotalHours * timesheetEntryDTO.PayRate;
                    }

                    return timesheetEntryDTO;
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }

        [HttpPost]
        [Route("api/TimesheetAPI/ApproveUpdateTimesheetEntry")]
        public HttpResponseMessage ApproveTimesheetEntry([FromBody] TimesheetEntryDTO timesheetEntryDTO)
        {
            if (Is<TimesheetFeature>.Enabled)
            {
                TimesheetEntry timesheetEntry = db.TimesheetEntries.Find(timesheetEntryDTO.Id);
                if (timesheetEntry == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

                var email = HttpContext.Current.User.Identity.Name;

                UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
                if (userProfile == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

                //Check business rules
                if (!timesheetEntryDTO.FinishDateTime.HasValue)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Finish time for shift must be specified");

                if (timesheetEntryDTO.FinishDateTime.Value < timesheetEntryDTO.StartDateTime)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Finish time for shift must be AFTER start time");

                if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", timesheetEntry.TimeCard.BusinessLocation.Business.Id.ToString()))
                {
                    timesheetEntry.Approved = true;
                    timesheetEntry.ApprovedDateTime = WebUI.Common.Common.DateTimeNowLocal();
                    timesheetEntry.ApprovedBy = userProfile;
                    timesheetEntry.StartDateTime = timesheetEntryDTO.StartDateTime;
                    timesheetEntry.FinishDateTime = timesheetEntryDTO.FinishDateTime;

                    try
                    {
                        db.Entry(timesheetEntry).State = EntityState.Modified;
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
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));


        }

        [HttpGet]
        [Route("api/TimesheetAPI/GetTimesheetSummary/{id}")]
        public TimesheetSummaryDTO GetTimesheetSummary(Guid id)
        {
            if (Is<TimesheetFeature>.Enabled)
            {
                TimesheetSummaryDTO timesheetSummaryDto = new TimesheetSummaryDTO();

                Timesheet timesheet = db.Timesheets.Find(id);
                if (timesheet == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

                if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", timesheet.Roster.BusinessLocation.Business.Id.ToString()))
                {
                    DateTime weekStart = timesheet.Roster.WeekStartDate;
                    DateTime weekEnd = weekStart.AddDays(7);

                    timesheetSummaryDto.WeekStartDate = weekStart;

                    Decimal totalCost = new decimal();
                    TimeSpan totalShiftTime = new TimeSpan();

                    //Get all timecard entries which have a clock in date time within the Timesheets roster week
                    var timecards = db.TimeCards.Where(tc => tc.BusinessLocation.Id == timesheet.Roster.BusinessLocation.Id && weekStart <= tc.ClockIn && tc.ClockIn < weekEnd);
                    timesheetSummaryDto.AllTimeCardsApproved = true;
                    foreach (var timecard in timecards)
                    {
                        timesheetSummaryDto.TotalShifts++;

                        if (!timecard.TimesheetEntry.Approved)
                            timesheetSummaryDto.AllTimeCardsApproved = false;
                        else
                        {

                            var timeSpan = timecard.TimesheetEntry.FinishDateTime - timecard.TimesheetEntry.StartDateTime;
                            if (timeSpan.HasValue)
                            {
                                totalShiftTime = totalShiftTime.Add(timeSpan.Value);
                                var shiftCost = (decimal)timeSpan.Value.TotalHours * timecard.Employee.PayRate.GetValueOrDefault();
                                totalCost += shiftCost;
                            }
                        }
                    }

                    timesheetSummaryDto.TotalHours = totalShiftTime.TotalHours;
                    timesheetSummaryDto.TotalCost = totalCost;
                    timesheetSummaryDto.AvgHourlyRate = totalCost / (decimal)totalShiftTime.TotalHours;

                    return timesheetSummaryDto;
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }


        [HttpPost]
        [Route("api/TimesheetAPI/ApproveTimesheet")]
        public HttpResponseMessage ApproveTimesheet([FromBody] Guid id)
        {
            if (Is<TimesheetFeature>.Enabled)
            {
                Timesheet timesheet = db.Timesheets.Find(id);
                if (timesheet == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

                var email = HttpContext.Current.User.Identity.Name;

                UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
                if (userProfile == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));

                //Check business rules
                //All timecard entries must be approved.
                DateTime weekStart = timesheet.Roster.WeekStartDate;
                DateTime weekEnd = weekStart.AddDays(7);
                DateTime timeNow = WebUI.Common.Common.DateTimeNowLocal();

                //Can not appove timesheet until week has passed
                if (timeNow < weekEnd)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Cannot approve timesheet until week has ended");

                //Get all timecard entries which have a clock in date time within the Timesheets roster week
                var unnapprovedTimeCards = db.TimeCards.Where(tc => tc.BusinessLocation.Id == timesheet.Roster.BusinessLocation.Id && weekStart <= tc.ClockIn && tc.ClockIn < weekEnd && tc.TimesheetEntry.Approved == false);
                if (unnapprovedTimeCards.Count() > 0)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "All timesheet entries must be approved before the timesheet can be approved");
                //End check business rules

                if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", timesheet.Roster.BusinessLocation.Business.Id.ToString()))
                {
                    timesheet.Approved = true;
                    timesheet.ApprovedDateTime = WebUI.Common.Common.DateTimeNowLocal();
                    timesheet.ApprovedBy = userProfile;

                    try
                    {
                        db.Entry(timesheet).State = EntityState.Modified;
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
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));

        }
    }
}
