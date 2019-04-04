using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;
using WebUI.Common;
using WebUI.Controllers.API.Security;

namespace WebUI
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Error(object sender, EventArgs e)
        {
            //Catch any unhandled applciation exceptions. THis is where any from async calls would be caught.
            // Get the exception object.
            Exception exc = Server.GetLastError();
            Trace.TraceError(exc.Message);
            var ai = new TelemetryClient();
            ai.TrackException(exc);

        }
        protected void Application_Start()
        {
           AreaRegistration.RegisterAllAreas();

            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AuthConfig.RegisterAuth();
            AutoMapperConfig.Configure();

#if DEBUG
            GlobalConfiguration.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
#endif

#if LOCAL
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.DisableTelemetry = true;
#else
            Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration.Active.InstrumentationKey = System.Web.Configuration.WebConfigurationManager.AppSettings["iKey"];
#endif


        }

        protected void Application_BeginRequest()
        {
            //Do not set user local dynamically set it by webconfig to avoid browser issues with US dates in AUS
            //SetUserLocale();
        }

        /// <summary>
        /// Sets the culture and UI culture to a specific culture. Allows overriding of currency
        /// and optionally disallows setting the UI culture.
        /// 
        /// You can also limit the locales that are allowed in order to minimize
        /// resource access for locales that aren't implemented at all.
        /// </summary>
        /// <param name="culture">
        /// 2 or 5 letter ietf string code for the Culture to set. 
        /// Examples: en-US or en</param>
        /// <param name="uiCulture">ietf string code for UiCulture to set</param>
        /// <param name="currencySymbol">Override the currency symbol on the culture</param>
        /// <param name="setUiCulture">
        /// if uiCulture is not set but setUiCulture is true 
        /// it's set to the same as main culture
        /// </param>
        /// <param name="allowedLocales">
        /// Names of 2 or 5 letter ietf locale codes you want to allow
        /// separated by commas. If two letter codes are used any
        /// specific version (ie. en-US, en-GB for en) are accepted.
        /// Any other locales revert to the machine's default locale.
        /// Useful reducing overhead in looking up resource sets that
        /// don't exist and using unsupported culture settings .
        /// Example: de,fr,it,en-US
        /// </param>
        public static void SetUserLocale(string culture = null,
            string uiCulture = null,
            string currencySymbol = null,
            bool setUiCulture = true,
            string allowedLocales = null)
        {
            // Use browser detection in ASP.NET
            if (string.IsNullOrEmpty(culture) && HttpContext.Current != null)
            {
                HttpRequest Request = HttpContext.Current.Request;

                // if no user lang leave existing but make writable
                if (Request.UserLanguages == null)
                {
                    Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentCulture.Clone() as CultureInfo;
                    if (setUiCulture)
                        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture.Clone() as CultureInfo;

                    return;
                }

                culture = Request.UserLanguages[0];
            }
            else
                culture = culture.ToLower();

            if (!string.IsNullOrEmpty(uiCulture))
                setUiCulture = true;

            if (!string.IsNullOrEmpty(culture) && !string.IsNullOrEmpty(allowedLocales))
            {
                allowedLocales = "," + allowedLocales.ToLower() + ",";
                if (!allowedLocales.Contains("," + culture + ","))
                {
                    int i = culture.IndexOf('-');
                    if (i > 0)
                    {
                        culture = culture.Substring(0, i);
                        if (!allowedLocales.Contains("," + culture + ","))
                        {
                            // Always create writable CultureInfo
                            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentCulture.Clone() as CultureInfo;
                            if (setUiCulture)
                                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture.Clone() as CultureInfo;

                            return;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(culture))
                culture = CultureInfo.InstalledUICulture.IetfLanguageTag;

            if (string.IsNullOrEmpty(uiCulture))
                uiCulture = culture;

            try
            {
                CultureInfo Culture = new CultureInfo(culture);

                if (currencySymbol != null && currencySymbol != "")
                    Culture.NumberFormat.CurrencySymbol = currencySymbol;

                Thread.CurrentThread.CurrentCulture = Culture;

                if (setUiCulture)
                {
                    var UICulture = new CultureInfo(uiCulture);
                    Thread.CurrentThread.CurrentUICulture = UICulture;
                }
            }
            catch {

                System.Diagnostics.Trace.TraceWarning("Global.asax.cs->SetUserLocale() : Threw and exception");
            }
        }

        // claims transformation
        protected void Application_PostAuthenticateRequest()
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            HttpCookie tokenCookie = Request.Cookies[Constants.CookieTokenKey];

            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                CustomPrincipalSerializeModel serializeModel = serializer.Deserialize<CustomPrincipalSerializeModel>(authTicket.UserData);

                CustomPrincipal newUser = new CustomPrincipal(authTicket.Name)
                {
                    Email = serializeModel.Email,
                    FirstName = serializeModel.FirstName,
                    Token = tokenCookie.Value
                };
                HttpContext.Current.User = newUser;
            }
        }
    }
}