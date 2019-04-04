using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class TimesheetController : Controller
    {
        //
        // GET: /Timesheet/
        //Get list of timesheets with Week start date between the dates specified
        public ActionResult Index(Guid? businessid, int? month, int? year, Guid? businesslocationid)
        {
            var startDate = new DateTime();
            var endDate = new DateTime();
            var firstOfMonth = new DateTime();
           
            //If no start or end dates provided then get current minus 1 week and plus 1 month
            if (!month.HasValue || !year.HasValue)
            {
                var dtNow = WebUI.Common.Common.DateTimeNowLocal();
                //Get current month
                firstOfMonth = new DateTime(dtNow.Year, dtNow.Month, 1);
            }
            else
                firstOfMonth = new DateTime(year.Value, month.Value, 1);

            startDate = WebUI.Common.Common.GetStartOfWeek(firstOfMonth, DayOfWeek.Monday);
            var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
            //Now find the next Sunday following last day of month.
            endDate = WebUI.Common.Common.GetCurrentOrNextWeekday(lastOfMonth, DayOfWeek.Sunday);

            ViewBag.SelectedMonth = firstOfMonth;

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.BusinessId = businessid.Value;
            ViewBag.SelectedBusinessLocationId = businesslocationid.Value;

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/TimesheetAPI?businesslocationid=" + ViewBag.SelectedBusinessLocationId.ToString() + "&startDate=" + HttpUtility.UrlEncode(startDate.ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(endDate.ToShortDateString()));
                var val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<TimesheetDTO>>(response.Result)).Result;

                return PartialView(val);
            }
        }

        // GET: /Timesheet/
        //Get all timesheetentires for each day of week for  given timesheet ID
        public ActionResult WeekView(Guid timeSheetId)
        {
            var timesheetWeekDTO = GetTimesheetData(timeSheetId);
         
            return PartialView(timesheetWeekDTO);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult EditTimesheetEntry(Guid id)
        {
            TimesheetEntryDTO val = null;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/TimesheetAPI/TimesheetEntry/" + id.ToString());
                val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<TimesheetEntryDTO>(response.Result)).Result;
            }

                return PartialView("_TimesheetEntryEditPartial", val);
        }

        [HttpPost]
        public JsonResult ApproveUpdateTimesheetEntry(TimesheetEntryDTO model)
        {
            //If updating then Finish day should be set if it is not alread
            if (!model.FinishDay.HasValue)
                model.FinishDay = model.StartDay;

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.PostAsJsonAsync("/api/TimesheetAPI/ApproveUpdateTimesheetEntry", model).Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                   // model.TimeCard.
                    //TODO if getting next unnapproved add call to new API service to find next unnapproved
                    var timesheetWeekDTO = GetTimesheetData(model.TimesheetId);
                    var nextTimesheetEntry = timesheetWeekDTO.TimesheetEntries.Where(s => s.StartDateTime >= model.StartDateTime && !s.Approved).OrderBy(s => s.StartDateTime).ToList().FirstOrDefault();
                    
                    //TODO NEED to be able to reload the Timsheet with ID
                    return Json(new { success = "true", message = "Thanks", nextId = (nextTimesheetEntry != null)?nextTimesheetEntry.Id.ToString(): "" });
                }
                else
                { //If and error occurred add details to model error.
                    var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                    return Json(new { success = "false", message = error.Message }); // do a concatenation of the model state errors
                }

            }

          
        }


        private TimesheetWeekDTO GetTimesheetData(Guid timeSheetId)
        {
            TimesheetWeekDTO val = null;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/TimesheetAPI/" + timeSheetId.ToString());
                val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<TimesheetWeekDTO>(response.Result)).Result;

                using (var bc = new BusinessController())
                {

                    var businessDTO = bc.GetBusiness(val.BusinessId, this.Session);
                    ViewBag.BusinessId = val.BusinessId;
                    ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
                    ViewBag.SelectedBusinessName = businessDTO.Name;
                    ViewBag.SelectedBusinessLocationId = val.BusinessLocationId;

                    if (businessDTO.HasMultiInternalLocations)
                        ViewBag.BusinessLocations = new SelectList(businessDTO.BusinessLocations, "Id", "Name", ViewBag.SelectedBusinessLocationId);
                }

                //using (var ec = new EmployerController())
                //{
                //    var employers = ec.GetEmployerSummary(this.Session);
                //    ViewBag.Businesses = new SelectList(employers.Employers, "BusinessId", "BusinessName", ViewBag.BusinessId);
                //}
            }
            return val;
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetWeekTimesheetSummary(Guid id)
        {
            TimesheetSummaryDTO val = null;

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/TimesheetAPI/GetTimesheetSummary/" + id.ToString());
                val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<TimesheetSummaryDTO>(response.Result)).Result;
            }

            return PartialView("_TimesheetSummaryPartial", val);
        }

        [HttpPost]
        public JsonResult ApproveTimesheet(TimesheetSummaryDTO model)
        {

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.PostAsJsonAsync("/api/TimesheetAPI/ApproveTimesheet", model.Id).Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                    return Json(new { success = "true", message = "Thanks"});
                }
                else
                { //If and error occurred add details to model error.
                    var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                    return Json(new { success = "false", message = error.Message }); // do a concatenation of the model state errors
                }

            }
        }

    }
}
