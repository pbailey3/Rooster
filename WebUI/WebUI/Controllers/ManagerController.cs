using Newtonsoft.Json;
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
using System.Device.Location;
using WebUI.Models;

namespace WebUI.Controllers
{
    public class ManagerController : Controller
    {
        public ActionResult AllRequestIndex()
        {
            var allRequests = new RequestsDTO();

            allRequests.EmployerRequests = GetEmployerRequestList();
            allRequests.ShiftChangeRequests = GetShiftChangeRequestList();
            allRequests.ExternalShiftRequests = GetExternalshiftRequestList();

            return PartialView(allRequests);
        }

        //
        // GET: /Manager/RequestIndex
        // [Authorize(Roles = Constants.RoleBusinessLocationManager)]
        public ActionResult RequestIndex()
        {
            IEnumerable<EmployerRequestDTO> er = GetEmployerRequestList();

            return PartialView(er);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public JsonResult GetAllRequests()
        {
            IEnumerable<EmployerRequestDTO> er = GetEmployerRequestList();
            ShiftChangeRequestListsDTO scr = GetShiftChangeRequestList();
            ExternalShiftRequestListsDTO esr = GetExternalshiftRequestList();
            IEnumerable<EmployeeRequestDTO> employeeRequests = GetEmployeeRequestList();
            //IEnumerable<ExternalBroadcastDTO> EmployeeExternalRerquest = GetEmployeeExternalshifts();

            List<RequestDTO> requests = new List<RequestDTO>();

            requests.AddRange(er.Select(req => new RequestDTO
            {
                Id = req.Id,
                RequestType = RequestTypeDTO.Employer,
                RequesterName = req.Requester_Name,
                BusinessName = req.Business_Name,
                BusinessLocationName = req.Location_Name,
                CreatedDate = req.CreatedDate
            }));

            if (scr.ShiftChangeRequests != null)
            {
                //Shift Cancel requests.
                requests.AddRange(scr.ShiftChangeRequests.Where(req => req.Type == ShiftRequestTypeDTO.Cancel).Select(req => new RequestDTO
                {
                    Id = req.Id,
                    ShiftId = req.ShiftId,
                    RequestType = RequestTypeDTO.ShiftCancel,
                    RequesterName = req.EmployeeName,
                    BusinessName = req.BusinessName,
                    BusinessLocationName = req.BusinessLocationName,
                    BusinessLocationId = req.BusinessLocationId,
                    StartDateTime = req.StartDateTime,
                    FinishDateTime = req.FinishDateTime,
                    CreatedDate = req.CreatedDate,
                    Reason = req.Reason
                }));

                //Take open shift requests.
                requests.AddRange(scr.ShiftChangeRequests.Where(req => req.Type == ShiftRequestTypeDTO.TakeOpenShift).Select(req => new RequestDTO
                {
                    Id = req.Id,
                    ShiftId = req.ShiftId,
                    RequestType = RequestTypeDTO.TakeOpenShift,
                    RequesterName = req.EmployeeName,
                    BusinessName = req.BusinessName,
                    BusinessLocationName = req.BusinessLocationName,
                    StartDateTime = req.StartDateTime,
                    FinishDateTime = req.FinishDateTime,
                    CreatedDate = req.CreatedDate
                }));
            }


            if (esr.ExternalShiftRequests != null)
            {
                //ExternalShift Cancel requests.
                requests.AddRange(esr.ExternalShiftRequests.Where(req => req.Type == ExternalShiftRequestTypeDTO.Cancel).Select(req => new RequestDTO
                {
                    Id = req.Id,
                    ExternalShiftBroadCastID = req.ExternalShiftBroadCastID,
                    RequestType = RequestTypeDTO.TakeExternalShiftBroadCast,
                    RequesterName = req.UserName,
                    //BusinessName = req.BusinessName,
                    //BusinessLocationName = req.BusinessLocationName,
                    //BusinessLocationId = req.BusinessLocationId,
                    //StartDateTime = req.StartDateTime,
                    //FinishDateTime = req.FinishDateTime,
                    CreatedDate = req.CreatedDate
                }));

                //Take Externalshift requests.
                requests.AddRange(esr.ExternalShiftRequests.Where(req => req.Type == ExternalShiftRequestTypeDTO.TakeExternalShift).Select(req => new RequestDTO
                {
                    Id = req.Id,
                    ExternalShiftBroadCastID = req.ExternalShiftBroadCastID,
                    RequestType = RequestTypeDTO.TakeExternalShiftBroadCast,
                    RequesterName = req.UserName,
                    //BusinessName = req.BusinessName,
                    //BusinessLocationName = req.BusinessLocationName,
                    //BusinessLocationId = req.BusinessLocationId,
                    //StartDateTime = req.StartDateTime,
                    //FinishDateTime = req.FinishDateTime,
                    CreatedDate = req.CreatedDate
                }));
            }

            if (scr.RecurringShiftChangeRequests != null)
            {
                //Recurring shift cancel requests.
                requests.AddRange(scr.RecurringShiftChangeRequests.Where(req => req.Type == ShiftRequestTypeDTO.Cancel).Select(req => new RequestDTO
                {
                    Id = req.Id,
                    RequestType = RequestTypeDTO.RecurringShiftCancel,
                    RequesterName = req.EmployeeName,
                    BusinessName = req.BusinessName,
                    BusinessLocationName = req.BusinessLocationName,
                    StartDateTime = DateTime.Parse(req.OccurenceDate.ToShortDateString() + ' ' + req.StartTime.ToString()),
                    FinishDateTime = DateTime.Parse(req.OccurenceDate.ToShortDateString() + ' ' + req.FinishTime.ToString()),
                    CreatedDate = req.CreatedDate
                }));
            }
            requests.AddRange(employeeRequests.Select(req => new RequestDTO
            {
                Id = req.Id,
                RequestType = RequestTypeDTO.Employee,
                RequesterName = null,
                BusinessName = req.BusinessLocation_Business_Name,
                BusinessLocationName = req.BusinessLocation_Name,
                StartDateTime = DateTime.MinValue,
                FinishDateTime = DateTime.MinValue,
                CreatedDate = req.CreatedDate
            }));


            return Json(requests);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public JsonResult GetEmployerRequests()
        {
            IEnumerable<EmployerRequestDTO> er = GetEmployerRequestList();
            return Json(er);
        }


        private IEnumerable<EmployerRequestDTO> GetEmployerRequestList()
        {
            IEnumerable<EmployerRequestDTO> er = new List<EmployerRequestDTO>();
            if (ClaimsHelper.IsInRole(System.Web.HttpContext.Current, Constants.RoleBusinessLocationManager))
            {

                //Get a summary of the employes currently linked to the Employee
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/RequestAPI");
                    er = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<EmployerRequestDTO>>(response.Result)).Result;
                }
            }
            return er;
        }

        private IEnumerable<EmployeeRequestDTO> GetEmployeeRequestList()
        {
            IEnumerable<EmployeeRequestDTO> er = new List<EmployeeRequestDTO>();

            //Get a summary of the employes currently linked to the Employee
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/EmployerAPI/GetEmployeeRequests");
                er = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<EmployeeRequestDTO>>(response.Result)).Result;
            }
            return er;
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        //[Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public ActionResult ApproveRequest(string reqId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/RequestAPI/ApproveEmployerRequest/" + reqId, String.Empty).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }
            return Content("Success");

        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        // [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public ActionResult RejectRequest(string reqId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/RequestAPI/RejectEmployerRequest/" + reqId, String.Empty).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
            }
            return Content("Success");
        }

        //
        // GET: /Manager/RequestIndex
        // [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public ActionResult ShiftRequestIndex()
        {
            ShiftChangeRequestListsDTO er = null;
            er = GetShiftChangeRequestList();

            return PartialView(er);
        }

        private ShiftChangeRequestListsDTO GetShiftChangeRequestList()
        {
            ShiftChangeRequestListsDTO er = new ShiftChangeRequestListsDTO();
            if (ClaimsHelper.IsInRole(System.Web.HttpContext.Current, Constants.RoleBusinessLocationManager))
            {
                //Get a summary of the employes currently linked to the Employee
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/ManagerShiftActionAPI/GetShiftChangeRequests");
                    er = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShiftChangeRequestListsDTO>(response.Result)).Result;
                }
            }
            return er;
        }


        public ActionResult ExternalShiftRequestIndex()
        {
            ExternalShiftRequestListsDTO esr = null;
            esr = GetExternalshiftRequestList();
            return PartialView(esr);
        }

        public ActionResult ExternalUserProfile(Guid externalshiftRequestID, Guid ExternalShiftBroadCastID)
        {
            UserProfilesDTO up = new UserProfilesDTO();
            if (ClaimsHelper.IsInRole(System.Web.HttpContext.Current, Constants.RoleBusinessLocationManager))
            {
                //Get a summary of the employes currently linked to the Employee
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var response = httpClient.GetAsync("api/ManagerShiftActionAPI/ExternalUserProfile?ID=" + externalshiftRequestID).Result;
                    up = JsonConvert.DeserializeObject<UserProfilesDTO>(response.Content.ReadAsStringAsync().Result);
                    up.ExternalShiftBroadCastID = ExternalShiftBroadCastID;
                    up.ExternalshfitRequestID = externalshiftRequestID;
                    // er = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ExternalShiftRequestListsDTO>(response.Result)).Result;
                }
            }

            return PartialView(up);
        }


        private ExternalShiftRequestListsDTO GetExternalshiftRequestList()
        {
            ExternalShiftRequestListsDTO er = new ExternalShiftRequestListsDTO();
            if (ClaimsHelper.IsInRole(System.Web.HttpContext.Current, Constants.RoleBusinessLocationManager))
            {
                //Get a summary of the employes currently linked to the Employee
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var response = httpClient.GetAsync("api/ManagerShiftActionAPI/GetExternalShiftRequests").Result;
                    er = JsonConvert.DeserializeObject<ExternalShiftRequestListsDTO>(response.Content.ReadAsStringAsync().Result);
                    // er = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ExternalShiftRequestListsDTO>(response.Result)).Result;
                }
            }
            return er;
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public JsonResult GetShiftChangeRequests()
        {
            ShiftChangeRequestListsDTO srl = GetShiftChangeRequestList();

            return Json(srl);
        }


        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        //[Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public ActionResult RejectShiftChangeRequest(string reqId, string reason, bool isRecurring)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var rejectDTO = new ShiftChangeActionDTO { Id = Guid.Parse(reqId), Reason = reason };

                HttpResponseMessage response = null;

                if (!isRecurring)
                    response = httpClient.PostAsJsonAsync("api/ManagerShiftActionAPI/RejectShiftChangeRequest/" + reqId, rejectDTO).Result;
                else
                    response = httpClient.PostAsJsonAsync("api/ManagerShiftActionAPI/RejectRecurringShiftChangeRequest/" + reqId, rejectDTO).Result;

                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }

            }
            return Content("Success");

        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        //[Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public ActionResult ApproveShiftChangeRequest(string reqId, string reason, bool isRecurring)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var acceptDTO = new ShiftChangeActionDTO { Id = Guid.Parse(reqId), Reason = reason };

                if (!isRecurring)
                {
                    HttpResponseMessage response = httpClient.PostAsJsonAsync("api/ManagerShiftActionAPI/ApproveShiftChangeRequest/" + reqId, acceptDTO).Result;
                    Response.StatusCode = (int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                    {
                        Response.SuppressFormsAuthenticationRedirect = true;
                        return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                    }
                }
                else
                {
                    HttpResponseMessage response = httpClient.PostAsJsonAsync("api/ManagerShiftActionAPI/ApproveRecurringShiftChangeRequest/" + reqId, acceptDTO).Result;
                    Response.StatusCode = (int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                    {
                        Response.SuppressFormsAuthenticationRedirect = true;
                        return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                    }

                }
            }
            return Content("Success");

        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        //[Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public ActionResult ApproveExternalShiftRequest(string reqId, string reason, bool isRecurring)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var acceptDTO = new ExternalShiftActionDTO { Id = Guid.Parse(reqId), Reason = reason };

                if (!isRecurring)
                {
                    HttpResponseMessage response = httpClient.PostAsJsonAsync("api/ManagerShiftActionAPI/ApproveExternalShiftRequest/" + reqId, acceptDTO).Result;
                    Response.StatusCode = (int)response.StatusCode;
                    if (!response.IsSuccessStatusCode)
                    {
                        Response.SuppressFormsAuthenticationRedirect = true;
                        return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                    }
                }
            }
            return Content("Success");
        }

        [HttpPost]
        //[Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public ActionResult RejectExternalShiftRequest(string reqId, string reason, bool isRecurring)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var rejectDTO = new ExternalShiftActionDTO { Id = Guid.Parse(reqId), Reason = reason };

                HttpResponseMessage response = null;

                if (!isRecurring)
                    response = httpClient.PostAsJsonAsync("api/ManagerShiftActionAPI/RejectExternalShiftRequest/" + reqId, rejectDTO).Result;

                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());

            }
            return Content("Success");

        }

    }
}
