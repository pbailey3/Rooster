using System;
using System.Web.Mvc;
using WebUI.DTOs;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebUI.Common;
using WebUI.Http;
using System.Collections.Generic;

namespace WebUI.Controllers
{
    //[Authorize]
    //[InitializeSimpleMembership]
    public class RequestController : Controller
    {

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetTotalRequestCount()
        {
           var numRequests = String.Empty;

            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/SummaryAPI/TotalRequestCount").Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                    numRequests = response.Content.ReadAsStringAsync().Result;
            }
            return numRequests;

        }


        //[HttpGet]
        //[ValidateJsonAntiForgeryToken]
        //[AuthorizeUser]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        //public string GetShiftRequestCount()
        //{
        //    var numRequests = String.Empty;

        //    //Current user has requested to be linked to a business
        //    using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
        //    {
        //        HttpResponseMessage response = httpClient.GetAsync("api/SummaryAPI/ShiftRequestCount").Result;
        //        Response.StatusCode = (int)response.StatusCode;
        //        if (!response.IsSuccessStatusCode)
        //            return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
        //        else
        //            numRequests = response.Content.ReadAsStringAsync().Result;
        //    }
        //    return numRequests;

        //}

        //[HttpGet]
        //[ValidateJsonAntiForgeryToken]
        //[AuthorizeUser]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        //public string GetExternalShiftRequestCount()
        //{
        //    var numRequests = String.Empty;

        //    //Current user has requested to be linked to a business
        //    using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
        //    {
        //        HttpResponseMessage response = httpClient.GetAsync("api/SummaryAPI/ExternalShiftRequestCount").Result;
        //        Response.StatusCode = (int)response.StatusCode;
        //        if (!response.IsSuccessStatusCode)
        //            return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
        //        else
        //            numRequests = response.Content.ReadAsStringAsync().Result;
        //    }
        //    return numRequests;
        //}

        //[HttpGet]
        //[ValidateJsonAntiForgeryToken]
        //[AuthorizeUser]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        //public string GetEmployerRequestCount()
        //{
        //    var numRequests = String.Empty;

        //    //Current user has requested to be linked to a business
        //    using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
        //    {
        //        HttpResponseMessage response = httpClient.GetAsync("api/SummaryAPI/EmployerRequestCount").Result;
        //        Response.StatusCode = (int)response.StatusCode;
        //        if (!response.IsSuccessStatusCode)
        //            return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
        //        else
        //            numRequests = response.Content.ReadAsStringAsync().Result;
        //    }
        //    return numRequests;

        //}

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public string GetEmployeeRequestCount()
        {
            var numRequests = String.Empty;

            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/EmployerAPI/GetEmployeeRequests").Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                    numRequests = JsonConvert.DeserializeObject<List<EmployeeRequestDTO>>(response.Content.ReadAsStringAsync().Result).Count.ToString();
            }
            return numRequests;

        }


    }
}
