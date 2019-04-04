using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class CalendarController : Controller
    {
      
        #region Schedule Methods

        //
        // GET: /Calendar
        //Get list of Rosters with Week start date between the dates specified
        public ActionResult IndexSchedule()
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ScheduleAPI");
                var val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ScheduleDTO>>(response.Result)).Result;
                return PartialView(val);
            }
        }


        //
        // GET: /Calendar/CreateSchedule
        public ActionResult CreateSchedule()
        {
            var scheduleDTO = new ScheduleDTO()
            {
                StartDate = WebUI.Common.Common.DateTimeNowLocal(),
                StartTime = new TimeSpan(WebUI.Common.Common.DateTimeNowLocal().Hour, 0, 0),
                EndTime = new TimeSpan(WebUI.Common.Common.DateTimeNowLocal().Hour + 1, 0, 0),
                NumberOfOccurrences = 0
            };

            LoadViewBag();
            return PartialView(scheduleDTO);
        }

        //
        // POST: /Calendar/CreateSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSchedule(ScheduleDTO scheduleDTO)
        {
          
            //TODO:
            // 1) Check any conflict with broadcast shifts
            //First populate with all Rostered shifts
            //IEnumerable<ShiftDTO> shifts = null;
            //using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            //{
            //    Task<String> response = httpClient.GetStringAsync("api/EmployeeRosterAPI?startDate=" + HttpUtility.UrlEncode(scheduleDTO.S.StartDateTime.AddDays(1).ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(range.EndDateTime.AddDays(1).ToShortDateString()));
            //    shifts = JsonConvert.DeserializeObjectAsync<IEnumerable<ShiftDTO>>(response.Result).Result;

            //}
            // 2) check any conflict with recurring shifts

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                    httpClient.PostAsJsonAsync("/api/ScheduleAPI",  scheduleDTO).Result.EnsureSuccessStatusCode();

                return RedirectToAction("IndexSchedule");//, new { id = employeedto.BusinessId });
            }
            LoadViewBag();

            return PartialView(scheduleDTO);
        }

        //
        // GET: /Calendar/Delete
        public ActionResult DeleteSchedule(int id)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(this.Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ScheduleAPI/" + id.ToString());
                var obj = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ScheduleDTO>(response.Result)).Result;

                return PartialView(obj);
            }

        }

        //
        // POST: /Business/DeleteSchedule/5
        [HttpPost, ActionName("DeleteSchedule")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteScheduleConfirmed(int id)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.DeleteAsync("api/ScheduleAPI/" + id.ToString()).Result;
                responseMessage.EnsureSuccessStatusCode();
            }
            return RedirectToAction("IndexSchedule");
        }

        //
        // GET: /Calendar/Edit
        public ActionResult EditSchedule(int id)
        {

            using (HttpClientWrapper httpClient = new HttpClientWrapper(this.Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ScheduleAPI/" + id.ToString());
                var scheduleDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ScheduleDTO>(response.Result)).Result;

                LoadViewBag();

                return PartialView(scheduleDTO);
            }

        }


        // POST: /Calendar/EditSchedule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditSchedule(ScheduleDTO scheduleDTO)
        {
            //if (scheduleDTO.StartTime < DateTime.Now.AddHours(-1)  //Ensure entry is a valid start date and finish date
            //    || scheduleDTO.FinishTime < scheduleDTO.StartTime)
            //    ModelState.AddModelError(String.Empty, "Start time must be after current time and before the finish time");

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/ScheduleAPI/" + scheduleDTO.Id.ToString(), scheduleDTO).Result;

                    if (responseMessage.IsSuccessStatusCode)
                        return RedirectToAction("IndexSchedule");
                    else
                    { //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }
                }
            }
            LoadViewBag();
            return PartialView(scheduleDTO);
        }

        private void LoadViewBag()
        {
            LoadFrequencyChoices();
            LoadDaysOfWeekChoices();
        }

        private void LoadFrequencyChoices()
        {
            var list = new List<object>()
            {
                new { ID = 1, Name = "Daily" },
                new { ID = 2, Name = "Weekly" }//,
                //new { ID = 3, Name = "Biweekly" },
                //new { ID = 4, Name = "Monthly" }
            };

            ViewBag.FrequencyChoices = new SelectList(list, "ID", "Name");
        }

        private void LoadDaysOfWeekChoices()
        {
            var daysOfWeek = new List<object>()
            {
                new { Name = "Sat" },
                new { Name = "Sun" },
                new { Name = "Mon" },
                new { Name = "Tue" },
                new { Name = "Wed" },
                new { Name = "Thu" },
                new { Name = "Fri" }
            };

            ViewBag.DaysOfWeekChoices = new SelectList(daysOfWeek, "Name", "Name");
        }

        #endregion

    }
}
