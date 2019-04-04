
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Twilio;
//using Twilio.Exceptions;
//using Twilio.Rest.Api.V2010.Account;
//using Twilio.Types;

namespace WebUI.Common
{
    public class SmsHelper
    {
        private static void SendSms(string text, string numberTo)
        {
            if (bool.Parse(ConfigurationManager.AppSettings.Get("smsEnabled")))
            {
                // Use your account SID and authentication token instead
                // of the placeholders shown here.
                var fromNumber = ConfigurationManager.AppSettings.Get("smsTwilioFromNumber");
                var accountSID = ConfigurationManager.AppSettings.Get("smsTwilioSID");
                var authToken = ConfigurationManager.AppSettings.Get("smsTwilioSecret");

                // Initialize the TwilioClient.
                var client = new TwilioRestClient(accountSID, authToken);
                    
                try
                {
                    var message = client.SendMessage(fromNumber, numberTo, text);
                    if (message.RestException != null)
                        throw new Exception("Twilio RestException: "+message.RestException.Code + " : "+ message.RestException.Message);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message.ToString());
                }

            }
        }

        public static void SendBusRegEmplSms(string numberTo)
        {
            var text = String.Format(ConfigurationManager.AppSettings.Get("smsBusRegNewEmp"),  ConfigurationManager.AppSettings.Get("ApiBaseURL"));
            SendSms(text, numberTo);
        }

    }
}