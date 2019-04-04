using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Http;

namespace WebUI.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User != null && User.Identity.IsAuthenticated)
                return RedirectToAction("AuthIndex");
            else
                return RedirectToAction("Login", "Account", null);
        }

        [AuthorizeUser]
        public ActionResult AuthIndex()
        {
            ViewBag.Message = "You are currently logged into Rooster";
            //Get summary data
            HomeSummaryDTO summary = null;

            //Get a summary of the employes currently linked to the Employee
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/SummaryAPI");
                summary = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<HomeSummaryDTO>(response.Result)).Result;

                var viewedWizardResponsed = httpClient.GetAsync("api/UserProfileAPI/GetHasViewedTutorial").Result;
                var viewedWizard = JsonConvert.DeserializeObject<bool>(viewedWizardResponsed.Content.ReadAsStringAsync().Result);

                summary.HasViewedWizard = viewedWizard;
            }

            return View(summary);
        }

        public ActionResult About()
        {
            return PartialView();
        }

        public ActionResult FAQ()
        {
            return PartialView();
        }
        public ActionResult TermsAndConditions()
        {
            return PartialView();
        }

        public ActionResult PrivacyPolicy()
        {
            return PartialView();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return PartialView();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult ViewedWizard()
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var response = httpClient.PostAsJsonAsync("api/UserProfileAPI/ViewedWizard", true).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }
            return Content("Success");
        }

        /// <summary>
        /// To Update the Current location of User
        /// </summary>
        /// <param name="userLocation"></param>
        /// <returns></returns>
        public ActionResult UpdateLocation(UserProfilesDTO userLocation)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var Data = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Line1", userLocation.Line1),
                    new KeyValuePair<string, string>("Line2", userLocation.Line2),
                    new KeyValuePair<string, string>("Suburb", userLocation.Suburb),
                    new KeyValuePair<string, string>("State", userLocation.State),
                    new KeyValuePair<string, string>("Postcode", userLocation.Postcode),
                    new KeyValuePair<string, string>("PlaceLongitude", Convert.ToString(userLocation.PlaceLongitude)),
                    new KeyValuePair<string, string>("PlaceLatitude", Convert.ToString(userLocation.PlaceLatitude)),
                    new KeyValuePair<string, string>("PlaceId", userLocation.PlaceId)
                });
                var response = httpClient.PostAsync("api/UserProfileAPI/UpdateLocation", Data).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
            }

            return Content("Success");
        }

        /// <summary>
        /// To Update the Current location of User
        /// </summary>
        /// <param name="userLocation"></param>
        /// <returns></returns>
        public ActionResult UpdateRange(UserProfilesDTO userLocation)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var Data = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Distance", Convert.ToString(userLocation.Distance)),
                    new KeyValuePair<string, string>("IndustryTypeId", Convert.ToString(userLocation.IndustryTypeId))
                });
                var response = httpClient.PostAsync("api/UserProfileAPI/UpdateDistance", Data).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
            }
            return Content("Success");
        }

        public ActionResult GetSiteSearch(string Prefix)
        {
            HomeSummaryDTO model = new HomeSummaryDTO();
            using (HttpClientWrapper httpclient = new HttpClientWrapper(Session))
            {
                var response = httpclient.GetAsync("api/SummaryAPI/GetSites").Result;
                model = JsonConvert.DeserializeObject<HomeSummaryDTO>(response.Content.ReadAsStringAsync().Result);
            }

            var SiteSearchResult = (from m in model.SiteSearch
                                    where m.Text.Contains(Prefix.ToLower())
                                    select m).ToList();

            return Json(SiteSearchResult, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSearchType(Guid id)
        {
            HomeSummaryDTO model = new HomeSummaryDTO();
            var IsBusiness = string.Empty;
            using (HttpClientWrapper httpclient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpclient.GetAsync("api/SummaryAPI/IsBusiness?id=" + id).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                else
                    IsBusiness = response.Content.ReadAsStringAsync().Result;
            }
            return Json(IsBusiness, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSearchResult(Guid id, string Name, string searchType)
        {
            if (searchType == "BusinessSearch")
            {
                EmployerSearchDTO searchData = new EmployerSearchDTO()
                {
                    id = id,
                    Name = Name,
                    SearchType = EmployerSearchTypeDTO.Business_Location_Name
                };
                return RedirectToAction("BusinessProfileDetails", "Roster", searchData);
            }
            else
            {
                return RedirectToAction("ExternalUserProfile", "Roster", new { externalUserID = id });
            }

        }

        public ActionResult SearchError()
        {
            return PartialView();
        }
    }
}
