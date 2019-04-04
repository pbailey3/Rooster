using SendGrid;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace WebUI.Common
{
    public class MailHelper
    {
        public static void SendAdminUserRegisteredMail(string email, string firstName, string lastName)
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(ConfigurationManager.AppSettings.Get("mailAdminToEmailAddress")) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;

            var subject = String.Format(ConfigurationManager.AppSettings.Get("mailAdminUserRegisteredSubject"), ConfigurationManager.AppSettings.Get("EnvironmentName"));

            text = String.Format(ConfigurationManager.AppSettings.Get("mailAdminUserRegisteredBody"), email, firstName, lastName);
            html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }

        public static void SendOpenShiftRequestAcceptMail(string toAddress, string firstName, string shiftDetails)
        { 
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;

            var subject = ConfigurationManager.AppSettings.Get("mailOpenShiftRequestAcceptedSubject");

            text = String.Format(ConfigurationManager.AppSettings.Get("mailOpenShiftRequestAcceptedBody"), firstName, shiftDetails);
            html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }
        public static void SendOpenShiftBroadcastMail(string toAddress, string firstName)
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;

            var subject = ConfigurationManager.AppSettings.Get("mailOpenShiftBroadcastSubject");

            text = String.Format(ConfigurationManager.AppSettings.Get("mailOpenShiftBroadcastBody"), firstName);
            html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }

        public static void SendShiftBroadcastMailAsync(string toAddress, string firstName)
        { 
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;

            var subject = ConfigurationManager.AppSettings.Get("mailShiftBroadcastSubject");

            text = String.Format(ConfigurationManager.AppSettings.Get("mailShiftBroadcastBody"), firstName);
            html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }

        public static void SendShiftCancelRequestMail(string toAddress, string firstName, string employeeName, string busLocation, DateTime shiftStart, DateTime shiftFinish)
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;

            var subject = ConfigurationManager.AppSettings.Get("mailShiftCancelRequestSubject");

            text = String.Format(ConfigurationManager.AppSettings.Get("mailShiftCancelRequestBody"), firstName, employeeName, busLocation, shiftStart.ToString(), shiftFinish.ToString());
            html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }

        public static void SendShiftChangeRejectedMail(string toAddress, string firstName, string shiftDetails, string managerName, string reason)
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;

            var subject = ConfigurationManager.AppSettings.Get("mailShiftChangeRequestRejectedSubject");

            text = String.Format(ConfigurationManager.AppSettings.Get("mailShiftChangeRequestRejectedBody"), firstName, shiftDetails, managerName, reason);
            html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }

        public static void SendShiftCancelledMail(string toAddress, string firstName, string busLocation, DateTime shiftStart, DateTime shiftFinish )
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;

            var subject = ConfigurationManager.AppSettings.Get("mailShiftCancelledSubject");

            text = String.Format(ConfigurationManager.AppSettings.Get("mailShiftCancelledBody"), firstName, busLocation, shiftStart.ToString(), shiftFinish.ToString());
            html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }

        public static void SendBusRegEmplMail(string toAddress, string firstName, string busName, bool isExistingUser)
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;
            var textPlus = string.Empty;

            var subject = ConfigurationManager.AppSettings.Get("mailBusRegSubject");
            if (isExistingUser)
            {
                text = String.Format(ConfigurationManager.AppSettings.Get("mailBusRegExistingBody"), firstName, busName, ConfigurationManager.AppSettings.Get("ApiBaseURL"));
                html = MailTemplates.GetFormattedMailTemplate(subject, text);
            }
            else
            {
                text = String.Format(ConfigurationManager.AppSettings.Get("mailBusRegNewBody"), firstName, busName, ConfigurationManager.AppSettings.Get("ApiBaseURL"));
                text += "<br><br>" + String.Format(ConfigurationManager.AppSettings.Get("mailBusRegNewBodyVisit"), ConfigurationManager.AppSettings.Get("ApiBaseURL"));
                html = MailTemplates.GetFormattedMailTemplate(subject, text);
            }

            SendMail(from, to, subject, html, text);
        }

        public static void SendConfirmationMail(string toAddress, string firstName, string confirmationURL)
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();

            var subject = ConfigurationManager.AppSettings.Get("mailConfSubject");
            var text = String.Format(ConfigurationManager.AppSettings.Get("mailConfBody"), firstName, confirmationURL);

            var mailHTML = MailTemplates.GetFormattedMailTemplate(subject, text, confirmationURL, "Verify email");

            SendMail(from, to, subject, mailHTML, text);
        }

        public static void SendPasswordResetMail(string toAddress, string resetURL)
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();

            var subject = ConfigurationManager.AppSettings.Get("mailPwdResetSubject");
            var text = String.Format(ConfigurationManager.AppSettings.Get("mailPwdResetBody"), resetURL);
            var html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }

        public static void SendReferralMail(string toAddress, string toName, string referrerName)
        {
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(toAddress) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();

            var subject = ConfigurationManager.AppSettings.Get("mailReferSubject");
            var text = String.Format(ConfigurationManager.AppSettings.Get("mailReferBody"), toName, referrerName, ConfigurationManager.AppSettings.Get("ApiBaseURL"));
            var html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }

        public static void SendFeedbackMail(string email, string firstName, string feedback)
        { 
            // Setup the email properties.
            var from = new MailAddress(ConfigurationManager.AppSettings.Get("mailFromAddress"));
            var to = new MailAddress[] { new MailAddress(ConfigurationManager.AppSettings.Get("mailAdminToEmailAddress")) };
            var cc = new List<MailAddress>();
            var bcc = new List<MailAddress>();
            var html = string.Empty;
            var text = string.Empty;

            var subject = String.Format(ConfigurationManager.AppSettings.Get("mailAdminFeedbackSubject"), ConfigurationManager.AppSettings.Get("EnvironmentName"));

            text = String.Format(ConfigurationManager.AppSettings.Get("mailAdminFeedbackBody"), email, firstName, feedback);
            html = MailTemplates.GetFormattedMailTemplate(subject, text);

            SendMail(from, to, subject, html, text);
        }


        private static void SendMail(MailAddress from, MailAddress[] to, string subject, string html, string text)
        {
            if (bool.Parse(ConfigurationManager.AppSettings.Get("mailEnabled")))
            {
                // Create an email,
                var myMessage = new SendGridMessage(from, to, subject, html, text);

                // Create network credentials to access your SendGrid account.
                var username = ConfigurationManager.AppSettings.Get("mailSendGridUserName");
                var pswd = ConfigurationManager.AppSettings.Get("mailSendGridPassword");

                var credentials = new NetworkCredential(username, pswd);

                // Create an REST transport for sending email.
                var transportREST = new Web(credentials);

                // Send the email.
                transportREST.DeliverAsync(myMessage);

            }
        }
    }
}