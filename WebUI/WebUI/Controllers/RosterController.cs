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
using System.Web.Script.Serialization;

namespace WebUI.Controllers
{
    [AuthorizeUser]
    public class RosterController : Controller
    {

        //
        // GET: /Roster/
        //Get list of Rosters with Week start date between the dates specified
        public ActionResult Index(Guid? businessid, int? month, int? year, Guid? businesslocationid)
        {
            var startDate = new DateTime();
            var endDate = new DateTime();
            var firstOfMonth = new DateTime();
            EmployerSummaryDTO employers = null;

            Guid selectedBusinessId = Guid.Empty;

            if (!businessid.HasValue)
            {
                //Get first business
                using (EmployerController ec = new EmployerController())
                {
                    employers = ec.GetEmployerSummary(this.Session);
                    if (employers.Employers.Count > 0)
                    {
                        foreach (var employer in employers.Employers.OrderBy(e => e.BusinessName).ThenBy(e => e.Name))
                        {
                            if (WebUI.Common.ClaimsHelper.IsLocationManager(this.HttpContext.ApplicationInstance.Context, employer.Id))
                            {
                                selectedBusinessId = employer.BusinessId;
                                break;
                            }
                        }
                    }
                }
            }
            else
                selectedBusinessId = businessid.Value;

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
            ViewBag.BusinessId = selectedBusinessId;

            using (var bc = new BusinessController())
            {

                BusinessDTO businessDTO = bc.GetBusiness(selectedBusinessId, this.Session);
                ViewBag.SelectedBusinessName = businessDTO.Name;

                if (!businesslocationid.HasValue)
                    ViewBag.SelectedBusinessLocationId = businessDTO.BusinessLocations.OrderBy(e => e.Name).First().Id;
                else
                    ViewBag.SelectedBusinessLocationId = businesslocationid.Value;

                ViewBag.BusinessLocations = new SelectList(businessDTO.BusinessLocations, "Id", "Name", ViewBag.SelectedBusinessLocationId);
                ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
            }

            using (var ec = new EmployerController())
            {
                if (employers == null)
                    employers = ec.GetEmployerSummary(this.Session);

                var employersManage = employers.Employers.Where(e => WebUI.Common.ClaimsHelper.IsLocationManager(System.Web.HttpContext.Current, e.Id));

                ViewBag.Businesses = new SelectList(employersManage.Select(b => new { BusinessId = b.BusinessId, BusinessName = b.BusinessName }).Distinct(), "BusinessId", "BusinessName", ViewBag.BusinessId);
            }


            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/RosterAPI?businesslocationid=" + ViewBag.SelectedBusinessLocationId.ToString() + "&startDate=" + HttpUtility.UrlEncode(startDate.ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(endDate.ToShortDateString()));
                var val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<RosterDTO>>(response.Result)).Result;

                return PartialView(val);
            }
        }

        /// <summary>
        /// Goes to Cost Approval Page
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult CostApproval(Guid ID, Guid BusinessLocationID, Guid RosterID)
        {
            ShiftBlockDTO shiftBlock = new ShiftBlockDTO();
            shiftBlock.Id = ID;
            shiftBlock.RosterId = RosterID;
            shiftBlock.BusinessLocationId = BusinessLocationID;
            return PartialView(shiftBlock);
        }

        /// <summary>
        /// Shift Setup Page
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public ActionResult ShiftSetUp(Guid ID, Guid RosterID)
        {
            ExternalBroadcastDTO result = new ExternalBroadcastDTO();
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var response = httpClient.GetAsync("api/ExternalBroadcastAPI/ExternalShiftSetUp?ID=" + ID).Result;
                result = JsonConvert.DeserializeObject<ExternalBroadcastDTO>(response.Content.ReadAsStringAsync().Result);
                result.Id = ID;
                result.RosterId = RosterID;
            }
            return PartialView(result);
        }

        [HttpPost]
        public ActionResult ShiftSetUp()
        {
            return RedirectToAction("BroadCastConfirmation");
        }

        public ActionResult GetShifts(Guid ID, Guid RosterID, bool newRow)
        {
            List<BroadcastedOpenShiftsDTO> result = new List<BroadcastedOpenShiftsDTO>();
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var response = httpClient.GetAsync("api/ExternalBroadcastAPI/GetShifts?ID=" + ID + "&RosterID=" + RosterID + "&isNewRow=" + newRow).Result;
                result = JsonConvert.DeserializeObject<List<BroadcastedOpenShiftsDTO>>(response.Content.ReadAsStringAsync().Result);
                return Json(new { Success = true, shifts = result }, JsonRequestBehavior.AllowGet);
                // result.ExternalBroadcastShiftId = ID;
            }
        }



        public ActionResult ExternalShiftSetUpSave(ExternalBroadcastDTO model)
        {
            ExternalBroadcastDTO result = new ExternalBroadcastDTO();
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {

                var response = httpClient.PostAsJsonAsync<ExternalBroadcastDTO>("api/ExternalBroadcastAPI/ExternalJobPosting", model).Result;
                if (response.IsSuccessStatusCode)
                {
                    var resultWorkHistory = JsonConvert.DeserializeObject<ExternalBroadcastDTO>(response.Content.ReadAsStringAsync().Result);
                    return Json(new { Success = true, workHistory = resultWorkHistory }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = false });
                }
            }
        }

        public ActionResult BroadCastConfirmation()
        {
            return PartialView();
        }

        //
        // GET: /Roster/
        //Get all shifts for day of week 
        public ActionResult DayView(Guid businessId, Guid businessLocationId, DateTime date, string returnView, Guid rosterId)
        {
            ViewBag.Date = date;
            ViewBag.BusinessLocationId = businessLocationId;
            ViewBag.BusinessId = businessId;
            ViewBag.ReturnView = returnView;
            ViewBag.RosterId = rosterId;

            BusinessController bc = new BusinessController();
            var businessDTO = bc.GetBusiness(businessId, this.Session);
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ShiftAPI?businessLocationId=" + businessLocationId.ToString() + "&date=" + date.ToString("yyyy-MM-dd"));//HttpUtility.UrlEncode(date.ToShortDateString()));
                var val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ShiftDTO>>(response.Result)).Result;

                return PartialView(val);
            }
        }

        //
        // GET: /Roster/
        //Get all shifts for day of week form given roster week ID
        public ActionResult WeekViewPrint(Guid rosterId)
        {
            return View(GetRosterData(rosterId));
        }

        //
        // GET: /Roster/
        //Get all shifts for day of week form given roster week ID
        public ActionResult WeekView(Guid rosterId, Guid? location)
        {
            var rosterDTO = GetRosterData(rosterId);

            BusinessController bc = new BusinessController() ;
            var businessLocationDTO = bc.GetBusinessLocation(rosterDTO.BusinessLocationId, this.Session);
            ViewBag.BusinessInternalLocations = businessLocationDTO.GetEnabledInternalLocations();
            ViewBag.SelectedInternalLocation = location.GetValueOrDefault();

            //Get roster id's for prev and next weeks (if exist)
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/RosterAPI?businesslocationid=" + rosterDTO.BusinessLocationId.ToString() + "&startDate=" + HttpUtility.UrlEncode(rosterDTO.WeekStartDate.AddDays(-7).ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(rosterDTO.WeekStartDate.AddDays(7).ToShortDateString()));
                var rosters = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<List<RosterDTO>>(response.Result)).Result;
                if (rosters.Count > 1)
                {
                    foreach (var roster in rosters)
                    {
                        if (roster.WeekStartDate == rosterDTO.WeekStartDate.AddDays(-7)) //If this is previous week
                            ViewBag.PrevWeekRosterId = roster.Id;
                        else if (roster.WeekStartDate == rosterDTO.WeekStartDate.AddDays(7)) //If this is following week
                            ViewBag.NextWeekRosterId = roster.Id;
                    }
                }
            }

            if (location.GetValueOrDefault() != Guid.Empty)  //Filter out elements dependant on the selected location
                rosterDTO.Shifts.RemoveAll(s => s.InternalLocationId != location.GetValueOrDefault());
            //rosterDTO.Shifts.RemoveAll(s => s.IsPublished == true);

            return PartialView(rosterDTO);
        }


        private RosterDTO GetRosterData(Guid rosterId)
        {
            RosterDTO val = null;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/RosterAPI/" + rosterId.ToString());
                val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RosterDTO>(response.Result)).Result;

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

                using (var ec = new EmployerController())
                {
                    var employers = ec.GetEmployerSummary(this.Session);
                    ViewBag.Businesses = new SelectList(employers.Employers, "BusinessId", "BusinessName", ViewBag.BusinessId);
                }
            }
            return val;
        }


        //
        // GET: /Roster/ShiftCreate
        public ActionResult ShiftCreate(Guid businesslocationId, DateTime startDate, string returnView, Guid rosterId, DayOfWeek dayOfWeek, Guid? internalLocationId)
        {
            //Set the mindate to the specified day of week in the current roster week
            DateTime date = WebUI.Common.Common.GetCurrentOrNextWeekday(startDate, dayOfWeek);

            ViewBag.ReturnView = returnView;
            ShiftDTO shiftDTO = new ShiftDTO();
            shiftDTO.StartDay = date;
            shiftDTO.FinishDay = date;
            shiftDTO.RosterId = rosterId;

            BusinessController bc = new BusinessController();
            var businessLocationDTO = bc.GetBusinessLocation(businesslocationId, this.Session);
            var businessDTO = bc.GetBusiness(businessLocationDTO.BusinessId, this.Session);

            shiftDTO.BusinessLocationId = businesslocationId;
            shiftDTO.BusinessId = businessDTO.Id;
            shiftDTO.InternalLocationId = internalLocationId.GetValueOrDefault();

            ViewBag.Roles = businessDTO.EnabledRoles;
            ViewBag.ShiftBlocks = businessLocationDTO.ShiftBlocks;
            ViewBag.Employees = this.GetEmployeesAndRoles(businesslocationId);
            ViewBag.BusinessLocationId = businesslocationId;
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
            ViewBag.BusinessInternalLocations = businessLocationDTO.GetEnabledInternalLocations();
            ViewBag.InternalLocationId = internalLocationId.GetValueOrDefault();


            ViewBag.MinDate = date;

            //  if (returnView == "DayView")
            ViewBag.MaxDate = date.AddDays(1);
            // else//Max date should be the last day of the roster period (+1) for finishing on next day
            //     ViewBag.MaxDate = startDate.AddDays(7);

            return PartialView(shiftDTO);
        }

        //
        // POST: /Roster/ShiftCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShiftCreate(ShiftDTO shiftDTO, DateTime minDate, string returnView)
        {
            if (shiftDTO.StartDateTime > shiftDTO.FinishDateTime
                || shiftDTO.StartDateTime == shiftDTO.FinishDateTime)
                ModelState.AddModelError(String.Empty, "Start time must be before the finish time");

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("api/ShiftAPI?businessLocationId=" + shiftDTO.BusinessLocationId.ToString(), shiftDTO).Result;

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        if (shiftDTO.SaveAsShiftBlock)
                        {
                            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + shiftDTO.BusinessLocationId.ToString()); //Remove the stale business item from the cache
                            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + shiftDTO.BusinessId.ToString()); //Remove the stale business item from the cache
                        }
                        if (returnView == "WeekView")
                            return RedirectToAction(returnView, new { rosterid = shiftDTO.RosterId });
                        else if (returnView == "DayView")
                            return RedirectToAction(returnView, new { businessId = shiftDTO.BusinessId, businessLocationId = shiftDTO.BusinessLocationId, date = shiftDTO.StartDateTime, returnView = "", rosterid = shiftDTO.RosterId });
                        else
                            throw new Exception("Unknown return view of '" + returnView + "' specified in ShiftCreate()");
                    }
                    else
                    { //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }
                }
            }

            //If reached here, and error has occurred
            BusinessController bc = new BusinessController();
            var businessLocationDTO = bc.GetBusinessLocation(shiftDTO.BusinessLocationId.Value, this.Session);
            var businessDTO = bc.GetBusiness(businessLocationDTO.BusinessId, this.Session);
            ViewBag.Roles = businessDTO.EnabledRoles;
            ViewBag.ShiftBlocks = businessLocationDTO.ShiftBlocks;
            ViewBag.Employees = this.GetEmployeesAndRoles(shiftDTO.BusinessLocationId.Value);
            ViewBag.BusinessLocationId = shiftDTO.BusinessLocationId.Value;
            ViewBag.BusinessInternalLocations = businessLocationDTO.GetEnabledInternalLocations();
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
            ViewBag.MinDate = minDate;
            //Max date should be the last day of the roster period (+1) for finishing on next day
            ViewBag.MaxDate = minDate.AddDays(7);
            ViewBag.ReturnView = returnView;

            return PartialView(shiftDTO);
        }

        //
        // GET: /Roster/ShiftCreate
        public ActionResult ShiftEdit(Guid id, Guid businessLocationId, string returnView)
        {
            ViewBag.ReturnView = returnView;

            using (HttpClientWrapper httpClient = new HttpClientWrapper(this.Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ShiftAPI/" + id.ToString());
                var shiftDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShiftDTO>(response.Result)).Result;

                ViewBag.MinDate = shiftDTO.StartDateTime;
                ViewBag.MaxDate = shiftDTO.StartDateTime.AddDays(1);

                BusinessController bc = new BusinessController();
                var businessLocationDTO = bc.GetBusinessLocation(businessLocationId, this.Session);
                var businessDTO = bc.GetBusiness(businessLocationDTO.BusinessId, this.Session);
                ViewBag.Roles = businessDTO.EnabledRoles;
                ViewBag.Employees = this.GetEmployeesAndRoles(businessLocationId);
                ViewBag.BusinessId = businessLocationDTO.BusinessId;
                ViewBag.BusinessLocationId = businessLocationId;
                ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
                ViewBag.BusinessInternalLocations = businessLocationDTO.GetEnabledInternalLocations();

                return PartialView(shiftDTO);
            }

        }


        // POST: /Roster/ShiftEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ShiftEdit(ShiftDTO shiftDTO, Guid businessLocationId, string returnView)
        {
            if (shiftDTO.StartDateTime > shiftDTO.FinishDateTime
                || shiftDTO.StartDateTime == shiftDTO.FinishDateTime)
                ModelState.AddModelError(String.Empty, "Start time must be before the finish time");

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/ShiftAPI/" + shiftDTO.Id.ToString(), shiftDTO).Result;

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        if (returnView == "WeekView")
                            return RedirectToAction(returnView, new { rosterid = shiftDTO.RosterId });
                        else if (returnView == "DayView")
                            return RedirectToAction(returnView, new { businessId = shiftDTO.BusinessId, businessLocationId = businessLocationId, date = shiftDTO.StartDateTime, returnView = "", rosterid = shiftDTO.RosterId });
                        else
                            throw new Exception("Unknown return view of '" + returnView + "' specified in ShiftEdit()");
                    }
                    //return RedirectToAction("DayView", new { rosterId = shiftDTO.RosterId, businessId = businessId, date = minDate });
                    else
                    { //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }
                }
            }

            BusinessController bc = new BusinessController();
            var businessLocationDTO = bc.GetBusinessLocation(businessLocationId, this.Session);
            var businessDTO = bc.GetBusiness(businessLocationDTO.BusinessId, this.Session);
            ViewBag.Roles = businessDTO.EnabledRoles;
            ViewBag.Employees = this.GetEmployeesAndRoles(shiftDTO.BusinessLocationId.Value);
            ViewBag.BusinessId = businessLocationDTO.BusinessId;
            ViewBag.BusinessLocationId = businessLocationId;
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
            ViewBag.BusinessInternalLocations = businessLocationDTO.GetEnabledInternalLocations();
            ViewBag.ReturnView = returnView;

            ViewBag.MinDate = shiftDTO.StartDateTime;
            ViewBag.MaxDate = shiftDTO.StartDateTime.AddDays(1);

            return PartialView(shiftDTO);
        }

        //
        // GET: /Business/Delete/5
        public ActionResult ShiftDelete(Guid id, string returnView)
        {
            ViewBag.ReturnView = returnView;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(this.Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ShiftAPI/" + id.ToString());
                return PartialView(Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShiftDTO>(response.Result)).Result);
            }
        }

        //
        // POST: /Business/Delete/5
        [HttpPost, ActionName("ShiftDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult ShiftDeleteConfirmed(Guid id, string returnView, string rosterId)
        {
            ShiftDTO shiftDTO = null;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                //Get the shift first so we can use the details for redirecting afterwards
                Task<String> response = httpClient.GetStringAsync("api/ShiftAPI/" + id.ToString());
                shiftDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShiftDTO>(response.Result)).Result;

                //Now delete the shift
                var responseMessage = httpClient.DeleteAsync("api/ShiftAPI/" + id.ToString()).Result;
                responseMessage.EnsureSuccessStatusCode();
            }

            if (returnView == "WeekView")
                return RedirectToAction(returnView, new { rosterid = rosterId });
            else if (returnView == "DayView")
                return RedirectToAction(returnView, new { businessId = shiftDTO.BusinessId, businessLocationId = shiftDTO.BusinessLocationId, date = shiftDTO.StartDateTime, returnView = "", rosterid = rosterId });
            else
                throw new Exception("Unknown return view of '" + returnView + "' specified in ShiftCreate()");

        }

        //JSON Javascript callback methods
        //public ActionResult GetEmployeesForRole(string roleId, string businessId)
        //{
        //    var employees = GetEmployeesInRole(Guid.Parse(roleId), Guid.Parse(businessId));

        //    return this.Json(employees, JsonRequestBehavior.AllowGet);
        //}

        //  
        // GET: /Roster/ShiftCreate
        public ActionResult RosterCreate(Guid businessId, Guid businessLocationId)
        {
            RosterCreateDTO model = new RosterCreateDTO
            {
                BusinessId = businessId,
                BusinessLocationId = businessLocationId,
                UseShiftTemplates = true //Always use Shift templates
            };

            ViewBag.BusinessId = businessId;
            ViewBag.BusinessLocationId = businessLocationId;
            //Set the next available roster week for creation to be either the monday following the last roster date, OR is none exist, then set to the next monday AFTER today.
            ViewBag.FirstRosterDate = GetNextRosterDate(businessLocationId);

            using (var bc = new BusinessController())
            {
                var busLocs = bc.GetBusinessLocationsForManager(businessId, this.Session);
                ViewBag.BusinessLocations = new SelectList(busLocs, "Id", "Name");
            }
            return PartialView(model);
        }



        //
        // POST: /Roster/RosterCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RosterCreate(RosterCreateDTO rosterCreateDTO)
        {
            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("api/RosterAPI", rosterCreateDTO).Result;

                    if (responseMessage.IsSuccessStatusCode)
                        return RedirectToAction("Index", new { businessId = rosterCreateDTO.BusinessId, month = rosterCreateDTO.WeekStartDate.Month, year = rosterCreateDTO.WeekStartDate.Year, businessLocationId = rosterCreateDTO.BusinessLocationId });
                    else
                    { //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }
                }
            }

            //Set the next available roster week for creation to be either the monday following the last roster date, OR is none exist, then set to the next monday AFTER today.

            ViewBag.BusinessId = rosterCreateDTO.BusinessId;

            using (var bc = new BusinessController())
            {
                var busLocs = bc.GetBusinessLocationsForManager(rosterCreateDTO.BusinessId, this.Session);
                //Set the next available roster week for creation to be either the monday following the last roster date, OR is none exist, then set to the next monday AFTER today.
                ViewBag.FirstRosterDate = GetNextRosterDate(rosterCreateDTO.BusinessLocationId);

                ViewBag.BusinessLocations = new SelectList(busLocs, "Id", "Name");
            }

            return PartialView(rosterCreateDTO);
        }

        //
        // GET: /Roster/RosterBroadcast
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult RosterBroadcast(Guid businessId, Guid businessLocationId, int? monthDate, int? yearDate, Guid? location)
        {
            if (!monthDate.HasValue)
                monthDate = WebUI.Common.Common.DateTimeNowLocal().Month;
            if (!yearDate.HasValue)
                yearDate = WebUI.Common.Common.DateTimeNowLocal().Year;

            //Get rosters by default for the current month
            DateTime startDate = new DateTime(yearDate.Value, monthDate.Value, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1); //Get last day of current month

            ViewBag.DisplayMonth = startDate;
            ViewBag.IntLocation = location.GetValueOrDefault();

            //Set start date to be the monday of the week which first day of month is within
            if (startDate.DayOfWeek != DayOfWeek.Monday)
            {
                if (startDate.DayOfWeek == DayOfWeek.Sunday)
                    startDate = startDate.AddDays(-6);
                else
                    startDate = startDate.AddDays(-(((int)startDate.DayOfWeek) - 1));
            }

            ViewBag.BusinessId = businessId;
            ViewBag.BusinessLocationId = businessLocationId;
            using (var bc = new BusinessController())
            {
                var bus = bc.GetBusiness(businessId, this.Session);
                ViewBag.BusinessLocations = new SelectList(bus.BusinessLocations, "Id", "Name", businessLocationId);
                var busLoc = bc.GetBusinessLocation(businessLocationId, this.Session);
                ViewBag.BusinessInternalLocations = busLoc.GetEnabledInternalLocations();
                ViewBag.BusinessLocationName = busLoc.Name;
            }


            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/RosterAPI?businessLocationId=" + businessLocationId.ToString() + "&startDate=" + HttpUtility.UrlEncode(startDate.ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(endDate.ToShortDateString()));
                var val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<RosterDTO>>(response.Result)).Result;

                foreach (RosterDTO roster in val) //Remove any shifts which are not for the location ID OR have already been published.
                {
                    if (location.GetValueOrDefault() != Guid.Empty)  //Filter out elements dependant on the selected location
                        roster.Shifts.RemoveAll(s => s.InternalLocationId != location.GetValueOrDefault());
                    roster.Shifts.RemoveAll(s => s.IsPublished == true);
                }
                return PartialView(val);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RosterBroadcast(Guid businessId, Guid businessLocationId, DateTime displayDate)
        {
            RosterBroadcastCheckDTO rosterBroadcastCheckDTO = new RosterBroadcastCheckDTO();
            rosterBroadcastCheckDTO.RosterBroadCasts = new List<RosterBroadcastDTO>();
            rosterBroadcastCheckDTO.ConflictingShifts = new List<ShiftDTO>();

            foreach (string key in Request.Form.AllKeys.Where(s => s.StartsWith("CHKB|")))
            {
                var checkbox = Request.Form[key];
                if (checkbox != "false")// if not false then true,false is returned
                {
                    //Roster day is selected to publish.
                    var keySubstr = key.Split('|');
                    var rosterId = Guid.Parse(keySubstr[1]);
                    var locationId = Guid.Parse(keySubstr[2]);
                    var dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), keySubstr[3]);

                    rosterBroadcastCheckDTO.RosterBroadCasts.Add(new RosterBroadcastDTO { Id = rosterId, BusinessId = businessId, BusinessLocationId = businessLocationId, LocationId = locationId, Day = dayOfWeek });
                }
            }

            //Add to the session to be used later
            this.Session[Constants.SessionRosterBroadcastKey + businessId.ToString()] = rosterBroadcastCheckDTO;

            if (rosterBroadcastCheckDTO.RosterBroadCasts.Count > 0)
            {
                bool shiftsUnassigned = false;
                bool shiftsConflicting = false;
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    foreach (RosterBroadcastDTO rosterBroadcastDTO in rosterBroadcastCheckDTO.RosterBroadCasts)
                    {
                        //First step: Check for any unnassigned shifts in the list of shifts to be broadcast
                        //var response = httpClient.GetAsync("api/RosterBroadcastAPI/  + businessId.ToString(), rostersToBroadcast).Result;
                        Task<String> response = httpClient.GetStringAsync("api/RosterBroadcastAPI/GetUnassignedShifts?rosterId=" + rosterBroadcastDTO.Id.ToString() + "&locationId=" + rosterBroadcastDTO.LocationId + "&day=" + rosterBroadcastDTO.Day);
                        var val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ShiftDTO>>(response.Result)).Result;

                        //If unnassigned shifts were returned
                        if (val.Count() > 0)
                        {
                            rosterBroadcastDTO.UnassignedShifts = val;
                            shiftsUnassigned = true;
                        }
                    }

                    //Get unique list of roster and days being broadcast
                    foreach (dynamic confShiftCheck in rosterBroadcastCheckDTO.RosterBroadCasts.Select(r => new { Id = r.Id, Day = r.Day }).Distinct())
                    {
                        //Second step: Check for any conflicting shifts in the list of shifts to be broadcast
                        Task<String> responseMessage = httpClient.GetStringAsync("api/RosterBroadcastAPI/GetConflictingShifts?rosterId=" + confShiftCheck.Id.ToString() + "&day=" + confShiftCheck.Day);
                        var valConflicts = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ShiftDTO>>(responseMessage.Result)).Result;

                        //If conflicting shifts were returned
                        if (valConflicts.Count() > 0)
                        {
                            rosterBroadcastCheckDTO.ConflictingShifts.AddRange(valConflicts);
                            shiftsConflicting = true;
                        }
                    }
                }

                //If there are unnassigned shifts then direct user to the unnassigned shifts page. Otherwise continue to broadcast
                if (shiftsUnassigned || shiftsConflicting)
                {
                    return RedirectToAction("RosterBroadcastShiftCheck", new { businessId = businessId, businessLocationId = businessLocationId, displayDate = displayDate });
                }
                else
                {
                    return RedirectToAction("RosterBroadcastNext", new { businessId = businessId, businessLocationId = businessLocationId, displayDate = displayDate });
                }
            }
            else
            {
                return RedirectToAction("RosterBroadcast", new { businessId = businessId, monthDate = displayDate.Month, yearDate = displayDate.Year, businessLocationId = businessLocationId });
            }
        }

        //
        // GET: /Roster/RosterBroadcastShiftCheck
        [HttpGet]
        public ActionResult RosterBroadcastShiftCheck(Guid businessId, Guid businessLocationId, DateTime displayDate)
        {
            ViewBag.EmployeeRoles = GetEmployeesAndRoles(businessLocationId);
            ViewBag.BusinessId = businessId;
            ViewBag.BusinessLocationId = businessLocationId;
            ViewBag.DisplayDate = displayDate;

            RosterBroadcastCheckDTO rostersToBroadcastCheck = (RosterBroadcastCheckDTO)this.Session[Constants.SessionRosterBroadcastKey + businessId.ToString()];

            return PartialView(rostersToBroadcastCheck);
        }

        ////
        //// GET: /Roster/RosterBroadcastNext
        public ActionResult RosterBroadcastNext(Guid businessId, Guid businessLocationId, DateTime displayDate)
        {
            //Any conflicts or unnassigned shifts have been fixed or ignored
            //Get broadcast values out of session
            RosterBroadcastCheckDTO rostersBroadcastCheck = (RosterBroadcastCheckDTO)this.Session[Constants.SessionRosterBroadcastKey + businessId.ToString()];
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.PutAsJsonAsync("api/RosterBroadcastAPI", rostersBroadcastCheck.RosterBroadCasts).Result;
                responseMessage.EnsureSuccessStatusCode();
            }

            return RedirectToAction("Index", new { businessid = businessId, month = displayDate.Month, year = displayDate.Year, businesslocationid = businessLocationId });
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult AssignShift(string shiftId, string empId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                //First get the shift to update
                Task<String> response = httpClient.GetStringAsync("api/ShiftAPI/" + shiftId);
                var shiftDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShiftDTO>(response.Result)).Result;

                //Second, post the changes for the shift.
                //update the employee id
                shiftDTO.EmployeeId = Guid.Parse(empId);
                var responseMessage = httpClient.PutAsJsonAsync("api/ShiftAPI/" + shiftId, shiftDTO).Result;
                Response.StatusCode = (int)responseMessage.StatusCode;
                if (!responseMessage.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(responseMessage.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }
            return Content("Success");

        }

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetUnavailableEmployees(Guid businessLocationId, string startTime, string finishTime, Guid? shiftId)
        {
            DateTime dtStart = DateTime.Parse(startTime);
            DateTime dtFinish = DateTime.Parse(finishTime);

            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/ScheduleActionAPI/GetUnavailableEmployees/BusinessLocation/" + businessLocationId.ToString() + "/Start/" + dtStart.Ticks.ToString() + "/Finish/" + dtFinish.Ticks.ToString() + "/Shift/" + ((shiftId != null) ? shiftId.ToString() : Guid.Empty.ToString())).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                {
                    var employeesList = JsonConvert.DeserializeObject<List<EmployeeSummaryDTO>>(response.Content.ReadAsStringAsync().Result);

                    return Content(response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        public ActionResult GetNextRosterDate(string Id)
        {
            var nextDate = this.GetNextRosterDate(Guid.Parse(Id));
            return this.Content(JsonConvert.SerializeObject(nextDate.ToShortDateString()));
        }

        // GET: /OpenShiftIndex/
        public ActionResult OpenShiftIndex()
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ShiftAPI/OpenShiftsForCurrent");
                var openShifts = (Task.Factory.StartNew(() => JsonConvert.DeserializeObject<OpenShiftsEmployeeIndexDTO>(response.Result)).Result);
                return View(openShifts);
            }
        }



        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult SendFeedback(string feedback)
        {
            var user = (CustomPrincipal)this.Request.RequestContext.HttpContext.User;
           
            MailHelper.SendFeedbackMail(user.Email, user.FirstName, feedback);
           
            return Content("Success");
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult RequestShift(Guid id)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var shiftRequestDTO = new ShiftChangeActionDTO
                {
                    Id = id,
                    Reason = "Open shift"
                };

                var responseMessage = httpClient.PostAsJsonAsync("api/EmployeeShiftActionAPI/RequestOpenShift/" + id.ToString(), shiftRequestDTO).Result;
                if (!responseMessage.IsSuccessStatusCode)
                {
                    var errorMessage = JsonConvert.DeserializeObject<dynamic>(responseMessage.Content.ReadAsStringAsync().Result).Message.ToString();
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return this.Content(errorMessage);
                }

                return Content("Success");
            }


        }

        // GET: /EmployeeExternalShift/
        [HttpGet]
        public ActionResult EmployeeExternal(Guid externalShiftID)
        {
            ExternalBroadcastDTO model = new ExternalBroadcastDTO();
            model.Id = externalShiftID;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responce = httpClient.GetAsync("api/EmployeeShiftActionAPI/EmployeeExternalShifts?Id=" + externalShiftID).Result;
                model = JsonConvert.DeserializeObject<ExternalBroadcastDTO>(responce.Content.ReadAsStringAsync().Result);
            }
            return PartialView(model);
        }

        //[HttpPost]
        //public ActionResult EmployeeExternal(ExternalBroadcastDTO model)
        //{
        //    Guid ID = model.Id;
        //    return RedirectToAction("EmployeeMessageforExternalshift", new { id = ID });
        //}

        [HttpPost]
        public ActionResult RequestExternalShift(Guid id)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                ExternalBroadcastDTO model = new ExternalBroadcastDTO();
                var ExternalshiftRequestDTO = new ExternalShiftActionDTO
                {
                    Id = id,
                    Reason = "External shift Broadcast"
                };

                var responseMessage = httpClient.PostAsJsonAsync("api/EmployeeShiftActionAPI/RequestExternalShift/" + id.ToString(), ExternalshiftRequestDTO).Result;
                if (!responseMessage.IsSuccessStatusCode)
                {
                    var errorMessage = JsonConvert.DeserializeObject<dynamic>(responseMessage.Content.ReadAsStringAsync().Result).Message.ToString();
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return this.Content(errorMessage);
                }
                model.Id = id;

                return PartialView("~/Views/Roster/EmployeeMessageforExternalshift.cshtml", model);
                //return Content("Success");
            }
        }

        //GET: /EmployeeMessageforExternalshift/
        //[HttpGet]
        //public ActionResult EmployeeMessageforExternalshift(Guid id)
        //{
        //    ExternalBroadcastDTO model = new ExternalBroadcastDTO();
        //    model.Id = id;
        //    return PartialView(model);
        //}

        [HttpPost]
        public ActionResult EmployeeMessageforExternalshift(string json)
        {
            ExternalBroadcastDTO model = JsonConvert.DeserializeObject<ExternalBroadcastDTO>(json);

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.PostAsJsonAsync("api/EmployeeShiftActionAPI/MessageforExternalShift/" + model.Id.ToString(), model).Result;
                if (!responseMessage.IsSuccessStatusCode)
                {
                    var errorMessage = JsonConvert.DeserializeObject<dynamic>(responseMessage.Content.ReadAsStringAsync().Result).Message.ToString();
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return this.Content(errorMessage);
                }

                return PartialView("~/Views/Roster/EmployeeExternalShiftRequestConfirmation.cshtml");
            }
        }

        //GET: /EmployeeExternalShiftRequestConfirmation/
        //[HttpGet]
        //public ActionResult EmployeeExternalShiftRequestConfirmation()
        //{
        //    return PartialView();
        //}


        #region Private helper methods

        private DateTime GetNextRosterDate(Guid? businessLocationId)
        {
            RosterDTO maxRosterDate = null;

            if (businessLocationId != Guid.Empty)
            {
                //First, get the list of rosters already created from today
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/RosterAPI?businesslocationid=" + businessLocationId.ToString() + "&startDate=" + HttpUtility.UrlEncode(WebUI.Common.Common.DateTimeNowLocal().AddDays(-7).ToShortDateString()) + "&endDate=" + HttpUtility.UrlEncode(DateTime.MaxValue.ToShortDateString()));
                    var rosters = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<RosterDTO>>(response.Result)).Result;

                    maxRosterDate = rosters.OrderByDescending(r => r.WeekStartDate).FirstOrDefault();
                }
            }

            DateTime retVal = DateTime.MinValue;

            if (maxRosterDate != null)
            {
                Trace.TraceInformation("GetNextRosterDate(" + businessLocationId.ToString() + "): maxRosterDate.WeekStartDate = '" + maxRosterDate.WeekStartDate.ToString() + "'");
                retVal = maxRosterDate.WeekStartDate.AddDays(7);
            }
            else
            {
                Trace.TraceInformation("GetNextRosterDate(" + businessLocationId.ToString() + "maxRosterDate == null");
                //Subtract seven days from next start date so they can create roster for CURRENT week if they need to
                retVal = WebUI.Common.Common.GetNextWeekday(WebUI.Common.Common.DateTimeNowLocal(), DayOfWeek.Monday).AddDays(-7);
            }

            return retVal;
        }

        //private List<EmployeeRoleSummaryDTO> GetEmployeesInRole(Guid roleId, Guid businessId)
        //{
        //    using (EmployeeController empController = new EmployeeController())
        //    {
        //        return (from employee in empController.GetEmployeesList(businessId, this.Session)
        //                     from role in employee.Roles
        //                     where role.Id == roleId
        //                select new EmployeeRoleSummaryDTO { Id = employee.Id, FullName = employee.FullName }).ToList();

        //    }
        //}
        private List<EmployeeRoleSummaryDTO> GetEmployeesAndRoles(Guid businessLocationId)
        {
            using (EmployeeController empController = new EmployeeController())
            {
                return (from employee in empController.GetEmployeesList(businessLocationId, this.Session)
                        select new EmployeeRoleSummaryDTO { Id = employee.Id, FullName = employee.FullName, Roles = employee.Roles.Select(r => r.Id).ToList() }).ToList();
            }
        }
        #endregion Private helper methods

        /// <summary>
        /// Manager will search for external user profile and will get the list of User(s)
        /// </summary>
        /// <returns></returns>
        public ActionResult SearchExternal()
        {
            ExternalUserProfileListDTO model = new ExternalUserProfileListDTO();

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responce = httpClient.GetAsync("api/ExternalBroadcastAPI/SearchExternal").Result;
                model = JsonConvert.DeserializeObject<ExternalUserProfileListDTO>(responce.Content.ReadAsStringAsync().Result);
            }
            return PartialView(model);
        }

        /// <summary>
        /// Manager can see the profile of a singe External UserProfile by his profie ID.
        /// </summary>
        /// <param name="externalUserID"></param>
        /// <returns></returns>
        public ActionResult ExternalUserProfile(Guid externalUserID)
        {
            UserProfilesDTO up = new UserProfilesDTO();
            //if (ClaimsHelper.IsInRole(System.Web.HttpContext.Current, Constants.RoleBusinessLocationManager))
            //{
            //Get a summary of the employes currently linked to the Employee
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var response = httpClient.GetAsync("api/UserProfileAPI/ExternalUserProfile?ID=" + externalUserID).Result;
                up = JsonConvert.DeserializeObject<UserProfilesDTO>(response.Content.ReadAsStringAsync().Result);
                // er = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ExternalShiftRequestListsDTO>(response.Result)).Result;
            }
            //}

            return PartialView(up);
        }

        public ActionResult BusinessProfileDetails(EmployerSearchDTO searchData)
        {
            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    HttpResponseMessage response = httpClient.PostAsJsonAsync("api/EmployerAPI/SearchBusinesses", searchData).Result;
                    searchData = JsonConvert.DeserializeObject<EmployerSearchDTO>(response.Content.ReadAsStringAsync().Result);
                }
            }

            return PartialView(searchData);
        }

        public ActionResult AddAsEmployee(Guid userId, Guid businessid)
        {
            if (ClaimsHelper.IsInRole(System.Web.HttpContext.Current, Constants.RoleBusinessLocationManager))
            {
                using (HttpClientWrapper httpclient = new HttpClientWrapper(Session))
                {
                    EmployeeDTO employeeDTO = new EmployeeDTO { BusinessId = businessid };
                    HttpResponseMessage response = httpclient.PostAsJsonAsync("api/ExternalBroadcastAPI/AddAsEmployee?ID=" + userId, employeeDTO).Result;
                    Response.StatusCode = (int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                        return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }
            return Content("Success");
        }
        public ActionResult UserRecommendations(Guid RecommendedUserID)
        {
            using (HttpClientWrapper httpclient = new HttpClientWrapper(Session))
            {
                var userRecommendationsDTO = new UserRecommendationsDTO();
                HttpResponseMessage response = httpclient.PostAsJsonAsync("api/UserProfileAPI/UserRecommendations?ID=" + RecommendedUserID, userRecommendationsDTO).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
            }
            return Content("Success");
        }

        public ActionResult ContactExternalUser(Guid userId, string txtAcceptReason)
        {
            using (HttpClientWrapper httpclient = new HttpClientWrapper(Session))
            {
                var MessageDTO = new MessagesDTO { Message = txtAcceptReason, DateSent = DateTime.Now };
                HttpResponseMessage response = httpclient.PostAsJsonAsync("api/ExternalBroadcastAPI/ContactExternalUser?ID=" + userId, MessageDTO).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
            }
            return Content("Success");
        }

        public ActionResult UserEndorseASkill(Guid userId, Guid SkillID)
        {
            using (HttpClientWrapper httpclient = new HttpClientWrapper(Session))
            {

                UserSkillsDTO usrskill = new UserSkillsDTO { UserSkillId = SkillID };
                HttpResponseMessage response = httpclient.PostAsJsonAsync("api/UserProfileAPI/UserEndorseASkill?ID=" + userId, usrskill).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
            }
            return Content("Success");
        }

        public ActionResult OpenShiftPage()
        {
            OpenShiftDTO ops = new OpenShiftDTO();

            //Get a summary of the employes currently linked to the Employee
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var response = httpClient.GetAsync("api/ExternalBroadcastAPI/GetOpenShifts").Result;
                ops = JsonConvert.DeserializeObject<OpenShiftDTO>(response.Content.ReadAsStringAsync().Result);
            }
            return PartialView(ops);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="JsonResult"></param>
        /// <param name="Distance"></param>
        /// <param name="FilterChoice"></param>
        /// <returns></returns>
        public ActionResult GetFilteredOpenShifts(string JsonResult, string Distance, int FilterChoice)
        {
            OpenShiftDTO ops = new OpenShiftDTO();

            //Get a summary of the employes currently linked to the Employee
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var response = httpClient.GetAsync("api/ExternalBroadcastAPI/GetFilteredOpenShifts?JsonResult=" + JsonResult + "&Distancce=" + Distance).Result;
                ops = JsonConvert.DeserializeObject<OpenShiftDTO>(response.Content.ReadAsStringAsync().Result);

                if (FilterChoice == 1)
                {
                    ops.OpenShiftList = ops.OpenShiftList.OrderByDescending(a => a.Distance).ToList();
                }
                else if (FilterChoice == 2)
                {
                    ops.OpenShiftList = ops.OpenShiftList.OrderByDescending(a => a.totalPrice).ToList();
                }
                else if (FilterChoice == 3)
                {
                    ops.OpenShiftList = ops.OpenShiftList.OrderByDescending(a => a.totalhourse).ToList();
                }
            }
            return PartialView("_GetFilteredOpenShift", ops);
        }
    }
}
