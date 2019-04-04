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
    public class EmployerController : Controller
    {
        //[Authorize(Roles=Constants.RoleBusinessLocationManager)]
        public ActionResult Index()
        {
            ViewBag.Message = "Employer Summary page.";
                     
            return PartialView(GetEmployerSummary());
        }

        internal EmployerSummaryDTO GetEmployerSummary(HttpSessionStateBase session = null)
        {
            session = (session != null) ? session : this.Session;

            using (HttpClientWrapper httpClient = new HttpClientWrapper(session))
            {
                Task<String> response = httpClient.GetStringAsync("api/EmployerAPI");
                return(Task.Factory.StartNew(() => JsonConvert.DeserializeObject<EmployerSummaryDTO>(response.Result)).Result);
           
            }
        }

        ///// <summary>
        ///// To get the Rooster Employment Partial.
        ///// </summary>
        ///// <returns></returns>
        //public ActionResult RoosterEmployment()
        //{
        //    ViewBag.Message = "Rooster Employment";

        //    return PartialView();
        //}

        public ActionResult Search()
        {
            ViewBag.Message = "Search for an employer";

            return PartialView();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Search(EmployerSearchDTO searchData)
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

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult RequestEmployer(string busId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PutAsJsonAsync("api/RequestAPI/CreateEmployerRequest/" + busId, String.Empty).Result;
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
        public void Refer(string refName, string refEmail)
        {
           var firstName = ClaimsHelper.GetClaimValue(WebUI.Common.Common.GetToken(System.Web.HttpContext.Current), Constants.ClaimFirstName);
           var lastName = ClaimsHelper.GetClaimValue(WebUI.Common.Common.GetToken(System.Web.HttpContext.Current), Constants.ClaimLastName);
           MessagingService.Referral(refEmail, refName, firstName + " " + lastName);
        }   

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult ApproveEmployeeRequest(string reqId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/RequestAPI/ApproveEmployeeRequest/" + reqId, String.Empty).Result;
                Response.StatusCode = (int)response.StatusCode;
                
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }
            //Need to refresh token so that claims are refreshed with new business as an employer
            return RedirectToAction("RefreshToken", "Account", new { returnAction = "ApproveEmployeeRequest", returnController = "Employer", string.Empty });
        }


      

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult RejectEmployeeRequest(string reqId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/RequestAPI/RejectEmployeeRequest/" + reqId, String.Empty).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }
            return Content("Success");

        }

    }
}
