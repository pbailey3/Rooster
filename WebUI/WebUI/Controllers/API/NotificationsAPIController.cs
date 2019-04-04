using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Threading.Tasks;
using System.Web;
using WebUI.Models;
using System.Configuration;
using System.Diagnostics;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class NotificationsAPIController : ApiController
    {
        //Sends a message to a specified user id only
        public async Task<HttpResponseMessage> Post(string message, string userId)
        {
           
                var userTag = "username:" + userId;

                var notification = new Dictionary<string, string> { { "message", message } };
                await Notifications.Instance.Hub.SendTemplateNotificationAsync(notification, userTag)
                 .ContinueWith(t => Trace.TraceError("ERROR:"+t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}



