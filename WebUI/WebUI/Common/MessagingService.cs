using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebUI.Controllers.API;
using WebUI.DTOs;

namespace WebUI.Common
{
    /// <summary>
    /// This service provides the single entry point for any messaging 
    /// </summary>
    public static class MessagingService
    {
        public static UserPreferencesDTO GetUserPreferences(string userEmail)
        {
            UserPreferencesDTO userPrefDTO = CacheManager.Instance.Get<UserPreferencesDTO>(CacheManager.CACHE_KEY_USER_PREFS + userEmail);

            if (userPrefDTO == null) //Not found in cache
            {
                using (UserPreferencesAPIController userPrefsAPI = new UserPreferencesAPIController())
                {
                    userPrefDTO = userPrefsAPI.GetPreferencesForUser(userEmail);
                    CacheManager.Instance.Add(CacheManager.CACHE_KEY_USER_PREFS + userEmail, userPrefDTO);
                }
            }

            return userPrefDTO;
            
        }

        public static bool IsNotificationsEnabled
        {
            get
            {
                //Only send email and push notifications if config is enabled.
                return bool.Parse(ConfigurationManager.AppSettings.Get("EnableNotifications"));
             }
        }
        public static void Referral(string toAddress, string toName, string referrerName)
        {
            if (MessagingService.IsNotificationsEnabled)
                MailHelper.SendReferralMail(toAddress, toName, referrerName);
        }

        public static void OpenShiftsBroadcast(List<Guid> businessLocations)
        {
            if (MessagingService.IsNotificationsEnabled)
            {
                foreach (var busLocId in businessLocations)
                {
                    //Todo: get list of staff who work in each business locations and their preferences.
                    using (UserPreferencesAPIController userPrefsController = new UserPreferencesAPIController())
                    {
                        var userPrefs = userPrefsController.GetUserPrefsForBusLocation(busLocId);
                        foreach (var userPref in userPrefs)
                        {
                            if (userPref.InternalAvailableShifts)
                            {
                                if (userPref.NotifyByEmail)
                                    MailHelper.SendOpenShiftBroadcastMail(userPref.UserProfileEmail, userPref.UserProfileFirstName);
                                if (userPref.NotifyByApp)
                                {
                                    using (NotificationsAPIController notificationController = new NotificationsAPIController())
                                    {
                                        //Send push notification
                                        notificationController.Post(String.Format(ConfigurationManager.AppSettings.Get("mailShiftBroadcastBody"), userPref.UserProfileFirstName), userPref.UserProfileEmail)
                                             .ContinueWith(t => Trace.TraceError("ERROR:" + t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ShiftBroadcast(string toAddress, string firstName)
        {
            if (MessagingService.IsNotificationsEnabled)
            {
                var userPrefs = MessagingService.GetUserPreferences(toAddress);
                
                if (userPrefs.NotifyByEmail)
                    MailHelper.SendShiftBroadcastMailAsync(toAddress, firstName);
                if (userPrefs.NotifyByApp)
                {
                    using (NotificationsAPIController notificationController = new NotificationsAPIController())
                    {
                        //Send push notification
                        notificationController.Post(String.Format(ConfigurationManager.AppSettings.Get("mailShiftBroadcastBody"), firstName), toAddress)
                            .ContinueWith(t => Trace.TraceError("ERROR:" + t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
            }
        }


        public static void ShiftBroadcastNotRegistered(string toAddress, string firstName)
        {
            if (MessagingService.IsNotificationsEnabled)
            {
                  MailHelper.SendShiftBroadcastMailAsync(toAddress, firstName);
            }
        }

        public static void OpenShiftRequestAccept(string toAddress, string firstName, string shiftDetails)
        {
            if (MessagingService.IsNotificationsEnabled)
            {
                var userPrefs = MessagingService.GetUserPreferences(toAddress);

                if (userPrefs.NotifyByEmail)
                     MailHelper.SendOpenShiftRequestAcceptMail(toAddress, firstName, shiftDetails);
                if (userPrefs.NotifyByApp)
                {
                    using (NotificationsAPIController notificationController = new NotificationsAPIController())
                    {
                        //Send push notification
                        notificationController.Post(String.Format(ConfigurationManager.AppSettings.Get("mailOpenShiftRequestAcceptedBody"), firstName, shiftDetails), toAddress)
                           .ContinueWith(t => Trace.TraceError("ERROR:" + t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
              
            }
        }

        public static void BusinessRegisteredEmployee(string firstName, string busName, bool isExistingUser, string toAddress = null, string mobileNumber = null)
        {
            if (MessagingService.IsNotificationsEnabled)
            {
                if (isExistingUser)
                {
                    UserPreferencesDTO userPrefs = null;
                    if (!String.IsNullOrEmpty(toAddress))
                        userPrefs = MessagingService.GetUserPreferences(toAddress);

                    if (userPrefs.NotifyByEmail)
                        MailHelper.SendBusRegEmplMail(toAddress, firstName, busName, isExistingUser);
                    if (userPrefs.NotifyBySMS && !String.IsNullOrEmpty(mobileNumber))
                        SmsHelper.SendBusRegEmplSms(mobileNumber);
                    if (userPrefs.NotifyByApp)
                    {
                        using (NotificationsAPIController notificationController = new NotificationsAPIController())
                        {
                            //Send push notification
                            notificationController.Post(String.Format(ConfigurationManager.AppSettings.Get("mailBusRegExistingBody"), firstName, busName, ConfigurationManager.AppSettings.Get("ApiBaseURL")), toAddress)
                                 .ContinueWith(t => Trace.TraceError("ERROR:" + t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);
                        }
                    }
                }
                else //New user, does not already exist in Rooster
                {
                    if (!String.IsNullOrEmpty(toAddress))
                        MailHelper.SendBusRegEmplMail(toAddress, firstName, busName, isExistingUser);

                    if (!String.IsNullOrEmpty(mobileNumber))
                        SmsHelper.SendBusRegEmplSms(mobileNumber);
                }
            }
        }

        public static void PasswordReset(string toAddress, string resetURL)
        {
            if (MessagingService.IsNotificationsEnabled)
                MailHelper.SendPasswordResetMail(toAddress, resetURL);
        }

        public static void RegistrationConfirmation(string toAddress, string firstName, string confirmationURL)
        {
            if (MessagingService.IsNotificationsEnabled)
                MailHelper.SendConfirmationMail(toAddress, firstName, confirmationURL);
        }

        public static void RegistrationAdminConfirmation(string email, string firstName, string lastName)
        {
            if (MessagingService.IsNotificationsEnabled)
                MailHelper.SendAdminUserRegisteredMail(email, firstName, lastName);
        }

        public static void ShiftCancelled(string toAddress, string firstName, string busLocation, DateTime shiftStart, DateTime shiftFinish)
        {
            if (MessagingService.IsNotificationsEnabled)
            {
                var userPrefs = MessagingService.GetUserPreferences(toAddress);

                if (userPrefs.NotifyByEmail)
                     MailHelper.SendShiftCancelledMail(toAddress, firstName, busLocation, shiftStart, shiftFinish);
                if ( userPrefs.NotifyByApp)
                {
                    using (NotificationsAPIController notificationController = new NotificationsAPIController())
                    {
                        //Send push notification
                        notificationController.Post(String.Format(ConfigurationManager.AppSettings.Get("mailShiftCancelledBody"), firstName, busLocation, shiftStart.ToString(), shiftFinish.ToString()), toAddress)
                            .ContinueWith(t => Trace.TraceError("ERROR:" + t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
               
            }
        }

        public static void ShiftCancelRequest(string toAddress, string firstName, string employeeName, string busLocation, DateTime shiftStart, DateTime shiftFinish)
        {
            if (MessagingService.IsNotificationsEnabled)
            {
                var userPrefs = MessagingService.GetUserPreferences(toAddress);

                if (userPrefs.NotifyByEmail)
                    MailHelper.SendShiftCancelRequestMail(toAddress, firstName, employeeName, busLocation, shiftStart, shiftFinish);
            }
        }

        public static void ShiftChangeRequestRejected(string toAddress, string firstName, string shiftDetails, string managerName, string reason)
        {
            if (MessagingService.IsNotificationsEnabled)
            {
                var userPrefs = MessagingService.GetUserPreferences(toAddress);

                if (userPrefs.NotifyByEmail)
                    MailHelper.SendShiftChangeRejectedMail(toAddress, firstName, shiftDetails, managerName, reason);
                if (userPrefs.NotifyByApp)
                {
                    using (NotificationsAPIController notificationController = new NotificationsAPIController())
                    {
                        //Send push notification
                        notificationController.Post(String.Format(ConfigurationManager.AppSettings.Get("mailShiftChangeRequestRejectedBody"), firstName, shiftDetails, managerName, reason), toAddress)
                             .ContinueWith(t => Trace.TraceError("ERROR:" + t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);
                    }
                }

            }
        }

    }
}