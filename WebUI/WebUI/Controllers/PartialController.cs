using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using WebUI.Models;
using WebUI.Controllers.API.Security;
using WebUI.DTOs;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Services;
using WebUI.Common;
using WebUI.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.Web.Helpers;
using System.Web.Routing;
using System.Threading.Tasks;
using System.IO;
using Thinktecture.IdentityModel.Http;
using System.Web.Script.Serialization;
using System.Net;


namespace WebUI.Controllers
{
    //[Authorize]
    public class PartialController : Controller
    {
        [AjaxChildActionOnly]
        public ActionResult NavMenu()
        {
             HomeSummaryDTO summary = null;

            //Get a summary of the employes currently linked to the Employee
             using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
             {
                 Task<String> response = httpClient.GetStringAsync("api/SummaryAPI");
                 summary = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<HomeSummaryDTO>(response.Result)).Result;
             }
             return PartialView(summary);
        }
    }

    public class AjaxChildActionOnlyAttribute : ActionMethodSelectorAttribute
    {
        public override bool IsValidForRequest(ControllerContext controllerContext, System.Reflection.MethodInfo methodInfo)
        {
            return controllerContext.RequestContext.HttpContext.Request.IsAjaxRequest() || controllerContext.IsChildAction;
        }
    }
}
