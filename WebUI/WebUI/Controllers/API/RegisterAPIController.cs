using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using System.Threading.Tasks;
using System.Web;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class RegisterAPIController : ApiController
    {
        private NotificationHubClient hub;

        public RegisterAPIController()
        {
            hub = Notifications.Instance.Hub;
        }

        public class DeviceRegistration
        {
            public string Platform { get; set; }
            public string Handle { get; set; }
            public string[] Tags { get; set; }
        }

        // POST api/register
        // This creates a registration id
        public async Task<string> Post([FromBody] string handle )
        {
            // make sure there are no existing registrations for this push handle (used for iOS and Android)
            string newRegistrationId = null;
            
            if (handle != null)
            {
                var registrations = await hub.GetRegistrationsByChannelAsync(handle, 100);

                foreach (RegistrationDescription registration in registrations)
                {
                    if (newRegistrationId == null)
                    {
                        newRegistrationId = registration.RegistrationId;
                    }
                    else
                    {
                        await hub.DeleteRegistrationAsync(registration);
                    }
                }
            }

            if (newRegistrationId == null) newRegistrationId = await hub.CreateRegistrationIdAsync();

            return newRegistrationId;
        }

        // PUT api/register/5
        // This creates or updates a registration (with provided PNS handle) at the specified id
        [HttpPut]
        public async Task<HttpResponseMessage> Put(string id, DeviceRegistration deviceUpdate)
        {

            // IMPORTANT: add logic to make sure that caller is allowed to register for the provided tags
            RegistrationDescription registration = null;

            switch (deviceUpdate.Platform)
            {
                //case "mpns":
                //    var toastTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                //        "<wp:Notification xmlns:wp=\"WPNotification\">" +
                //           "<wp:Toast>" +
                //                "<wp:Text1>$(message)</wp:Text1>" +
                //           "</wp:Toast> " +
                //        "</wp:Notification>";
                //    registration = new MpnsTemplateRegistrationDescription(deviceUpdate.Handle, toastTemplate);
                //    break;
                //case "wns":
                //    toastTemplate = @"<toast><visual><binding template=""ToastText01""><text id=""1"">$(message)</text></binding></visual></toast>";
                //    registration = new WindowsTemplateRegistrationDescription(deviceUpdate.Handle, toastTemplate);
                //    break;
                case "apns":
                    var alertTemplate = "{\"aps\":{\"alert\":\"$(message)\"}}";
                    registration = new AppleTemplateRegistrationDescription(deviceUpdate.Handle, alertTemplate);
                    break;
                case "gcm":
                    var messageTemplate = "{\"data\":{\"msg\":\"$(message)\"}}";
                    registration = new FcmTemplateRegistrationDescription(deviceUpdate.Handle, messageTemplate);
                    break;
                default:
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            registration.RegistrationId = id;

            var username = HttpContext.Current.User.Identity.Name;

            // add check if user is allowed to add these tags
            registration.Tags = new HashSet<string>(deviceUpdate.Tags);
            if(!registration.Tags.Contains("all"))
                registration.Tags.Add("all");
            registration.Tags.Add("username:" + username);

            try
            {
                await hub.CreateOrUpdateRegistrationAsync(registration);
            }
            catch (MessagingException e)
            {
                ReturnGoneIfHubResponseIsGone(e);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // DELETE api/register/5
        public async void Delete(string id)
        {
            await hub.DeleteRegistrationAsync(id);
        }

        private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
        {
            var webex = e.InnerException as WebException;
            if (webex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = (HttpWebResponse)webex.Response;
                if (response.StatusCode == HttpStatusCode.Gone)
                    throw new HttpRequestException(HttpStatusCode.Gone.ToString());
            }
        }
    }
}



