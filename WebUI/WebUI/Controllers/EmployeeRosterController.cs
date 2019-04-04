using Newtonsoft.Json;
using ScheduleWidget.ScheduledEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Http;

namespace WebUI.Controllers
{
    [AuthorizeUser]
    public class EmployeeRosterController : Controller
    {

        //
        // GET: /EmployeeRoster/
        //Get list of Rosters with Week start date between the dates specified
        public ActionResult FullCalendar()
        {
            return PartialView();
        }

        public JsonResult GetSchedules(string start, string end)
        {
            var calendarObjects = new List<object>();

            var range = new DateRange()
            {
                StartDateTime = DateTime.Parse(start),// ScheduleHelper.FromUnixTimestamp(start),
                EndDateTime = DateTime.Parse(end)//ScheduleHelper.FromUnixTimestamp(end)
            };

            //First populate with all Rostered shifts
            IEnumerable<ShiftDTO> shifts = null;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/EmployeeRosterAPI?startDate=" + HttpUtility.UrlEncode(range.StartDateTime.ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(range.EndDateTime.ToShortDateString()));
                shifts = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ShiftDTO>>(response.Result)).Result;
            }

            foreach (var shift in shifts)
            {
                calendarObjects.Add(new
                {
                    id = shift.Id,
                    title = shift.BusinessName + ":" + shift.InternalLocationName,
                    start = shift.StartDateTime.ToString("s"), //ScheduleHelper.ToUnixTimestamp(shift.StartTime),
                    end = shift.FinishDateTime.ToString("s"),//ScheduleHelper.ToUnixTimestamp(shift.FinishTime),
                    businessId = shift.BusinessId,
                    businessLocationId = shift.BusinessLocationId,
                    length = shift.ShiftLength.Hours,
                    location = shift.InternalLocationName,
                    hoursToShift = WebUI.Common.Common.HoursTillStart(shift),
                    color = "red",
                    allDay = false,
                    type = "work"
                });
            }

            //Also populate with recurring shifts which do not have matching published shifts
            // IEnumerable<ShiftDTO> recurringShifts = null;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                //Get List of approved/pending cancelled shift change requests and hide from calendar.
                Task<String> res = httpClient.GetStringAsync("api/EmployeeShiftActionAPI/GetRecurringShiftChangeRequests?startDate=" + HttpUtility.UrlEncode(range.StartDateTime.ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(range.EndDateTime.ToShortDateString()));
                var recurringShiftChangeRequests = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<RecurringShiftChangeRequestDTO>>(res.Result)).Result;

                //Get List of approved/pending cancelled shift change requests and hide from calendar.
                Task<String> scRes = httpClient.GetStringAsync("api/EmployeeShiftActionAPI/GetShiftChangeRequests?startDate=" + HttpUtility.UrlEncode(range.StartDateTime.ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(range.EndDateTime.ToShortDateString()));
                var shiftChangeRequests = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ShiftChangeRequestDTO>>(scRes.Result)).Result;

                Task<String> response = httpClient.GetStringAsync("api/ShiftTemplateAPI/ListForCurrentUser");
                var shiftTemplates = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ShiftTemplateDTO>>(response.Result)).Result;


                foreach (ShiftTemplateDTO shiftTemplateDTO in shiftTemplates)
                {
                    var sch = shiftTemplateDTO.GetSchedule();
                    var occurrences = sch.Occurrences(range);
                    foreach (DateTime dt in occurrences)
                    {
                        //Filter out any recurring shifts which occur in the past.
                        if (dt < WebUI.Common.Common.DateTimeNowLocal())
                            continue;

                        //If there is a matching recurring shift cancel request for this template and it is approved, then do not show this item
                        if (recurringShiftChangeRequests.Where(sc => sc.ShiftTemplateId == shiftTemplateDTO.Id
                                                    && sc.OccurenceDate == dt
                                                    && sc.Type == ShiftRequestTypeDTO.Cancel
                                                    && sc.Status == RequestStatusDTO.Approved).Count() > 0)
                        {
                            continue;
                        }

                        //If there is a matching shift cancel request for this template and it is approved, then do not show this item
                        if (shiftChangeRequests.Where(sc => sc.StartDateTime == (dt + shiftTemplateDTO.StartTime)
                                                    && sc.ShiftTemplateId == shiftTemplateDTO.Id
                                                    && sc.Type == ShiftRequestTypeDTO.Cancel
                                                    && sc.Status == RequestStatusDTO.Approved).Count() > 0)
                        {
                            continue;
                        }


                        //Filter out fortnightly recurring shifts depending on odd even week in year
                        if (shiftTemplateDTO.Frequency == ShiftFrequencyDTO.Fortnight1
                            || shiftTemplateDTO.Frequency == ShiftFrequencyDTO.Fortnight2)
                        {
                            var weekOfYear = WebUI.Common.Common.GetWeekOfYear(dt);

                            //If is is an odd week and template is Fortnight 2, OR even week and fortnight 1 then this does not apply continue to next
                            if ((weekOfYear % 2 == 1 && shiftTemplateDTO.Frequency == ShiftFrequencyDTO.Fortnight2)
                                || (weekOfYear % 2 == 0 && shiftTemplateDTO.Frequency == ShiftFrequencyDTO.Fortnight1))
                                continue;
                        }

                        //Filter out any published shifts which match and will already be getting displayed.
                        if (shifts.Where(s => s.BusinessLocationId == shiftTemplateDTO.BusinessLocationId
                                    && s.EmployeeId == shiftTemplateDTO.EmployeeId
                                    && s.ShiftTemplateId == shiftTemplateDTO.Id
                                    && s.StartDateTime.DayOfYear == dt.DayOfYear).Count() == 0)
                        {
                            calendarObjects.Add(new
                            {
                                id = shiftTemplateDTO.Id,
                                title = "Recurring Shift",
                                location = shiftTemplateDTO.InternalLocationName,
                                role = shiftTemplateDTO.RoleName,
                                businessName = shiftTemplateDTO.BusinessName,
                                businessLocationName = shiftTemplateDTO.BusinessLocationName,
                                businessLocationId = shiftTemplateDTO.BusinessLocationId,
                                dateTicks = dt.Ticks,
                                start = (dt + shiftTemplateDTO.StartTime).ToString("s"),
                                end = (dt + shiftTemplateDTO.FinishTime).ToString("s"),
                                length = shiftTemplateDTO.ShiftLength.Hours,
                                allDay = false,
                                type = "recurring",
                                color = "grey"
                            });

                        }
                    }
                }
            }


            //Then populate with all unavailability details
            IEnumerable<ScheduleDTO> schedules = null;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ScheduleAPI");
                schedules = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ScheduleDTO>>(response.Result)).Result;
            }
            foreach (var schedule in schedules)
            {
                calendarObjects
                    .AddRange(schedule.NumberOfOccurrences > 0
                    ? GetSpecificNumberOfOccurrencesForDateRange(schedule, range)
                    : GetAllOccurrencesForDateRange(schedule, range));
            }

            return Json(calendarObjects.ToArray(), JsonRequestBehavior.AllowGet);
        }
        private IEnumerable<object> GetSpecificNumberOfOccurrencesForDateRange(ScheduleDTO scheduleDTO, DateRange range)
        {
            var calendarObjects = new List<object>();
            var occurrences = scheduleDTO.Schedule.Occurrences(range).ToArray();
            for (var i = 0; i < scheduleDTO.NumberOfOccurrences; i++)
            {
                var date = occurrences.ElementAtOrDefault(i);

                calendarObjects.Add(new
                {
                    id = scheduleDTO.Id,
                    title = scheduleDTO.Description,
                    start = (date + scheduleDTO.StartTime).ToString("s"),
                    end = (date + scheduleDTO.EndTime).ToString("s"),
                    //url = Url.Action("ScheduleOccurrence", "Home", new { id = scheduleDTO.ID, occurrenceDate = date }),
                    allDay = false,
                    type = "personal"
                });

            }
            return calendarObjects;
        }

        private IEnumerable<object> GetAllOccurrencesForDateRange(ScheduleDTO scheduleDTO, DateRange range)
        {
            var calendarObjects = new List<object>();
            foreach (var date in scheduleDTO.Schedule.Occurrences(range))
            {
                calendarObjects.Add(new
                {
                    id = scheduleDTO.Id,
                    title = scheduleDTO.Description,
                    start = (date + scheduleDTO.StartTime).ToString("s"),
                    end = (date + scheduleDTO.EndTime).ToString("s"),
                    //url = Url.Action("ScheduleOccurrence", "Home", new { id = scheduleDTO.ID, occurrenceDate = date }),
                    allDay = false
                });
            }

            return calendarObjects;
        }

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        public ActionResult GetBusinessLocationPreferences(string Id)
        {
            //var busLocPreferences = CacheManager.Instance.Get(CacheManager.CACHE_KEY_BUSINESS_LOCATION);

            //if (businessLocationDTO == null) //Not found in cache
            //{

            //TODO CACHE THIS
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/BusinessPreferencesAPI/GetPreferencesForBusinessLocation/" + Id).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                    return Content(response.Content.ReadAsStringAsync().Result);

            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult CancelShiftRequest(string shiftId, string reason)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var cancelDTO = new ShiftChangeActionDTO { Id = Guid.Parse(shiftId), Reason = reason };
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/EmployeeShiftActionAPI/CancelShift/" + shiftId, cancelDTO).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                    return Content("Success");
            }
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult CancelRecurringShiftRequest(string shiftId, string reason, string shiftDate)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var cancelDTO = new RecurringShiftChangeActionDTO { Id = Guid.Parse(shiftId), Reason = reason, OccurenceDate = new DateTime(long.Parse(shiftDate)) };
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/EmployeeShiftActionAPI/CancelRecurringShiftInstance/" + shiftId, cancelDTO).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                    return Content("Success");
            }
        }

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetShiftWorkingWithDetails(string Id)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/EmployeeShiftActionAPI/GetWorkingWithDetails/" + Id).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                {
                    var nameList = JsonConvert.DeserializeObject<List<string>>(response.Content.ReadAsStringAsync().Result);
                    return Content(response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        public ActionResult GetBusinessLocationManagerDetails(string Id)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/EmployeeShiftActionAPI/GetBusinessLocationManagerDetails/" + Id).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                else
                {
                    var nameList = JsonConvert.DeserializeObject<List<EmployeeSummaryDTO>>(response.Content.ReadAsStringAsync().Result);
                    return Content(response.Content.ReadAsStringAsync().Result);
                }
            }
        }


       
    }
}
