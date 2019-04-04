using AutoMapper;
using ScheduleWidget.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
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
    public class ScheduleActionAPIController : ApiController
    {

        private DataModelDbContext db = new DataModelDbContext();

        // GET api/scheduleapi/5
        [ActionName("GetUnavailableEmployees")]
        [Route("api/ScheduleActionAPI/GetUnavailableEmployees/BusinessLocation/{businessLocationId}/Start/{startTime}/Finish/{finishTime}/Shift/{shiftId}")]
        public IEnumerable<EmployeeSummaryDTO> GetUnavailableEmployees(Guid businessLocationId, string startTime, string finishTime, Guid shiftId)
       {
           DateTime dtStart = new DateTime(long.Parse(startTime));
           DateTime dtFinish = new DateTime(long.Parse(finishTime));
           var busLoc = db.BusinessLocations.Find(businessLocationId);
           if (busLoc == null)
               throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Business Location with given ID does not exist"));

           //ONCE-OFF: If unavail start time is within shift period then staff member is unavailable
           var unavailList1 = db.Schedules.Where(s => s.StartDate == dtStart.Date && s.Frequency == (short)FrequencyEnum.OnceOff &&
                                                      (dtStart.TimeOfDay <= s.StartTime && s.StartTime <= dtFinish.TimeOfDay)
                                                      ).SelectMany(s => s.UserProfile.Employees.Where(e => e.BusinessLocation.Id == businessLocationId)).Distinct().ToList();

           //If end time is within shift period then staff member is unavailable                                          
           var unavailList2 = db.Schedules.Where(s => s.StartDate == dtStart.Date && s.Frequency == (short)FrequencyEnum.OnceOff &&
                                                      (dtStart.TimeOfDay <= s.EndTime && s.EndTime <= dtFinish.TimeOfDay)
                                                      ).SelectMany(s => s.UserProfile.Employees.Where(e => e.BusinessLocation.Id == businessLocationId)).Distinct().ToList();

           //If shift is completely covered by unavailability is unavailable                                      
           var unavailList3 = db.Schedules.Where(s => s.StartDate == dtStart.Date && s.Frequency == (short)FrequencyEnum.OnceOff &&
                                                      (dtStart.TimeOfDay >= s.StartTime && dtFinish.TimeOfDay <= s.EndTime)).SelectMany(s => s.UserProfile.Employees.Where(e => e.BusinessLocation.Id == businessLocationId)).Distinct().ToList();


           //Recurring daily, if shift starts within the start and end dates of the recurrence AND (start Times overlaps with unavailable time OR Unavailable start time is within shift time)                                
           var unavailList4 = db.Schedules.Where(s => s.Frequency == (short)FrequencyEnum.Daily &&
                                                   (s.StartDate <= dtStart.Date && dtStart.Date <= s.EndDate) && //AND Shift start date is within recurring period
                                                      ((s.StartTime <= dtStart.TimeOfDay && dtStart.TimeOfDay <= s.EndTime)  || //AND shift start time is within the unavailable time on the day
                                                      (dtStart.TimeOfDay <= s.StartTime && s.StartTime <= dtFinish.TimeOfDay)) //OR Unavailable start time is within shift time
                                                      ).SelectMany(s => s.UserProfile.Employees.Where(e => e.BusinessLocation.Id == businessLocationId)).Distinct().ToList();

           //Recurring daily, if shift  ends within the start and end dates of the recurrence AND end time overlaps with unavailable time)                                 
           var unavailList5 = db.Schedules.Where(s => s.Frequency == (short)FrequencyEnum.Daily &&
                                                     (s.StartDate <= dtFinish.Date && dtFinish.Date <= s.EndDate) &&  // AND shift finish date is within recurring period
                                                     (s.StartTime <= dtFinish.TimeOfDay && dtFinish.TimeOfDay <= s.EndTime) //AND finish time is within unavailable time
                                                      ).SelectMany(s => s.UserProfile.Employees.Where(e => e.BusinessLocation.Id  == businessLocationId)).Distinct().ToList();
            
           DayOfWeekEnum startDay = GetDayOfWeek(dtStart.DayOfWeek);
           DayOfWeekEnum finishDay = GetDayOfWeek(dtFinish.DayOfWeek);

             //Recurring weekly AND day of week is a selected day of week AND Shift start date is within recurring period AND start time is within unavailable time                         
           var unavailList6 = db.Schedules.Where(s => s.Frequency == (short)FrequencyEnum.Weekly && //Recurring weekly
                                                   ( ((int)s.DaysOfWeek & (int)startDay) != 0)  &&  //AND day of week is a selected day of week
                                                    (s.StartDate <= dtStart.Date && dtStart.Date <= s.EndDate) && //AND Shift start date is within recurring period
                                                    (s.StartTime <= dtStart.TimeOfDay  && dtStart.TimeOfDay <= s.EndTime) //AND shift start time is within the unavailable time on the day
                                                    ).SelectMany(s => s.UserProfile.Employees.Where(e => e.BusinessLocation.Id == businessLocationId)).Distinct().ToList();

           //Recurring weekly AND shift finish day of week is a selected day of week AND Shift finish date is within recurring period AND finish time is within unavailable time                         
           var unavailList7 = db.Schedules.Where(s => s.Frequency == (short)FrequencyEnum.Weekly && //Recurring weekly
                                                   (((int)s.DaysOfWeek & (int)finishDay) != 0) &&  //AND day of week is a selected day of week
                                                    (s.StartDate <= dtFinish.Date && dtFinish.Date <= s.EndDate) && //AND Shift finish date is within recurring period 
                                                    (s.StartTime <= dtFinish.TimeOfDay && dtFinish.TimeOfDay <= s.EndTime) //AND finish time is within unavailable time   
                                                    ).SelectMany(s => s.UserProfile.Employees.Where(e => e.BusinessLocation.Id == businessLocationId)).Distinct().ToList();

           var overlappingShifts = db.Shifts.Where(s => s.InternalLocation.BusinessLocation.Id == businessLocationId
                                                                                          && s.Employee != null
                                                                                          && s.Id != shiftId //Filter out the given shift ID as this could be an edit shift, if create new this should be Guid.Empty
                                                                                          && dtStart <= s.FinishTime
                                                                                          && dtFinish >= s.StartTime)
                                                                                          .Select(s => s.Employee).ToList();
           unavailList1.AddRange(unavailList2);
           unavailList1.AddRange(unavailList3);
           unavailList1.AddRange(unavailList4);
           unavailList1.AddRange(unavailList5);
           unavailList1.AddRange(unavailList6);
           unavailList1.AddRange(unavailList7);
           unavailList1.AddRange(overlappingShifts);

           return MapperFacade.MapperConfiguration.Map<IEnumerable<Employee>, IEnumerable<EmployeeSummaryDTO>>(unavailList1.Distinct());
       }

        private DayOfWeekEnum GetDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return DayOfWeekEnum.Mon;
                case DayOfWeek.Tuesday:
                    return DayOfWeekEnum.Tue;
                case DayOfWeek.Wednesday:
                    return DayOfWeekEnum.Wed;
                case DayOfWeek.Thursday:
                   return DayOfWeekEnum.Thu;
                case DayOfWeek.Friday:
                    return DayOfWeekEnum.Fri;
                case DayOfWeek.Saturday:
                    return DayOfWeekEnum.Sat;
                case DayOfWeek.Sunday:
                    return DayOfWeekEnum.Sun;
                default:
                    throw new Exception("Invalid day of week");
            }   
        }
    }


}
