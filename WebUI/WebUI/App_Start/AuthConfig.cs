using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using WebUI.Models;
using WebMatrix.WebData;
using System.Configuration;
using WebUI.Common;

namespace WebUI
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {// WebSecurity.InitializeDatabaseConnection("DefaultConnection", "UserProfile", "UserId", "UserName", autoCreateTables: true);

            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");
            OAuthWebSecurity.RegisterClient(new FacebookClient2017(ConfigurationManager.AppSettings.Get("OAuthFacebookAppId"),
                ConfigurationManager.AppSettings.Get("OAuthFacebookAppSecret")));
            //OAuthWebSecurity.RegisterFacebookClient(
            //    appId: ConfigurationManager.AppSettings.Get("OAuthFacebookAppId"),
            //    appSecret: ConfigurationManager.AppSettings.Get("OAuthFacebookAppSecret"),
            //    displayName: "Facebook");

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
