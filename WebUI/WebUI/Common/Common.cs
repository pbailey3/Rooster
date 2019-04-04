using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using WebUI.DTOs;

namespace WebUI.Common
{
    public static class Common
    {
        public static string GetToken(HttpContext httpContext)
        {
            if (HttpContext.Current.User.Identity.IsAuthenticated
                && HttpContext.Current.User is CustomPrincipal)
                return ((CustomPrincipal)HttpContext.Current.User).Token;
            else
                return null;
        }

        public static string GetDomainFromRequest(HttpRequestBase request)
        {
            return request.Url.Scheme + System.Uri.SchemeDelimiter + request.Url.Host + (request.Url.IsDefaultPort ? "" : ":" + request.Url.Port);
        }
        public static DateTime DateTimeNowLocal()
        {
            var timeZone = WebConfigurationManager.AppSettings["Timezone"];

            DateTime myUtcDateTime = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTime(myUtcDateTime, TimeZoneInfo.FindSystemTimeZoneById(timeZone));
        }

        public static int GetWeekOfYear(DateTime dateTime)
        {
            var cal = DateTimeFormatInfo.CurrentInfo.Calendar;
            return cal.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
        }

        public static DateTime GetStartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }

        public static DateTime GetCurrentOrNextWeekday(DateTime start, DayOfWeek day)
        {
            //If start date is the same as target day, then return date
            if (start.DayOfWeek == day)
                return start;
            else
                return GetNextWeekday(start, day);
        }

        public static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
        {
            //If start date is the same as target day, then add 1 to ensure next day is returned
            if (start.DayOfWeek == day)
                start = start.AddDays(1);

            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
            return start.AddDays(daysToAdd);
        }

        public static int HoursTillStart(ShiftDTO shift)
        {
            var timeTilStart = shift.StartDateTime - DateTimeNowLocal();
            return ((timeTilStart.Days * 24) + timeTilStart.Hours);
        }
        public class State
        {
            public string StateValue { get; set; }
            public string StateName { get; set; }
        }
        public static List<State> GetStates()
        {
            var countryConfig = WebConfigurationManager.AppSettings["CountryConfig"];
            var states = new List<State>();

            if (countryConfig.ToUpper() == "US")
            {
                states = new List<State>()
                {
                     new State{StateValue="AL", StateName="Alabama" },
                     new State{StateValue="AK", StateName="Alaska"},
                     new State{StateValue="AZ", StateName="Arizona"},
                     new State{StateValue="AR", StateName="Arkansas"},
                     new State{StateValue="CA", StateName="California"},
                     new State{StateValue="CO", StateName="Colorado"},
                     new State{StateValue="CT", StateName="Connecticut"},
                     new State{StateValue="DC", StateName="District of Columbia"},
                     new State{StateValue="DE", StateName="Delaware"},
                     new State{StateValue="FL", StateName="Florida"},
                     new State{StateValue="GA", StateName="Georgia"},
                     new State{StateValue="HI", StateName="Hawaii"},
                     new State{StateValue="ID", StateName="Idaho"},
                     new State{StateValue="IL", StateName="Illinois"},
                     new State{StateValue="IN", StateName="Indiana"},
                     new State{StateValue="IA", StateName="Iowa"},
                     new State{StateValue="KS", StateName="Kansas"},
                     new State{StateValue="KY", StateName="Kentucky"},
                     new State{StateValue="LA", StateName="Louisiana"},
                     new State{StateValue="ME", StateName="Maine"},
                     new State{StateValue="MD", StateName="Maryland"},
                     new State{StateValue="MA", StateName="Massachusetts"},
                     new State{StateValue="MI", StateName="Michigan"},
                     new State{StateValue="MN", StateName="Minnesota"},
                     new State{StateValue="MS", StateName="Mississippi"},
                     new State{StateValue="MO", StateName="Missouri"},
                     new State{StateValue="MT", StateName="Montana"},
                     new State{StateValue="NE", StateName="Nebraska"},
                     new State{StateValue="NV", StateName="Nevada"},
                     new State{StateValue="NH", StateName="New Hampshire"},
                     new State{StateValue="NJ", StateName="New Jersey"},
                     new State{StateValue="NM", StateName="New Mexico"},
                     new State{StateValue="NY", StateName="New York"},
                     new State{StateValue="NC", StateName="North Carolina"},
                     new State{StateValue="ND", StateName="North Dakota"},
                     new State{StateValue="OH", StateName="Ohio"},
                     new State{StateValue="OK", StateName="Oklahoma"},
                     new State{StateValue="OR", StateName="Oregon"},
                     new State{StateValue="PA", StateName="Pennsylvania"},
                     new State{StateValue="RI", StateName="Rhode Island"},
                     new State{StateValue="SC", StateName="South Carolina"},
                     new State{StateValue="SD", StateName="South Dakota"},
                     new State{StateValue="TN", StateName="Tennessee"},
                     new State{StateValue="TX", StateName="Texas"},
                     new State{StateValue="UT", StateName="Utah"},
                     new State{StateValue="VT", StateName="Vermont"},
                     new State{StateValue="VA", StateName="Virginia"},
                     new State{StateValue="WA", StateName="Washington"},
                     new State{StateValue="WV", StateName="West Virginia"},
                     new State{StateValue="WI", StateName="Wisconsin"},
                     new State{StateValue="WY", StateName="Wyoming"}
                };
            }
            else if (countryConfig.ToUpper() == "AU")
            {
                states = new List<State>()
                {
                 new State{StateValue="NSW", StateName="NSW"},
                  new State{StateValue="VIC", StateName="VIC"},
                 new State{StateValue="ACT", StateName="ACT"},
                 new State{StateValue="QLD", StateName="QLD"},
                 new State{StateValue="NT", StateName="NT"},
                 new State{StateValue="SA", StateName="SA"},
                 new State{StateValue="TAS", StateName="TAS"},
                 new State{StateValue="WA", StateName="WA"}
                };

            }

            return states;
        }

        public static Bitmap GenerateQR(string textToEncode)
        {
            var bw = new ZXing.BarcodeWriter();
            var encOptions = new ZXing.Common.EncodingOptions() { Width = 160, Height = 160, Margin = 0 };
            bw.Options = encOptions;
            bw.Format = ZXing.BarcodeFormat.QR_CODE;

            var result = new Bitmap(bw.Write(textToEncode));

            return result;
        }

        private static readonly StringDictionary dateFormats = new StringDictionary
        {
                { "en-US", "mm/dd/yyyy" },
                { "en-AU", "dd/mm/yyyy" },
                { "US", "mm/dd/yyyy" },
                { "AU", "dd/mm/yyyy" }
        };
        public static string GetLocalePlaceholderDateString(string languagePreference)
        {
            return (dateFormats.ContainsKey(languagePreference)) ? dateFormats[languagePreference] : dateFormats["en-AU"];
        }

        public static string GetLocaleDateDisplayFormat()
        {
            var countryConfig = System.Configuration.ConfigurationManager.AppSettings["CountryConfig"];

            if (countryConfig == "AU")
                return "dd/MM/yyyy";
            else if (countryConfig == "US")
                return "MM/dd/yyyy";
            else
                throw new Exception("Unsupported language preference, not date format defined: "+ countryConfig);
        }

        private static CultureInfo GetCultureInfo(string languagePreference)
        {
            var cultureVal = languagePreference;
            if (languagePreference == "AU")
                cultureVal = "en-AU";
            else if (languagePreference == "US")
                cultureVal = "en-US";
            var cultureInfo = CultureInfo.GetCultureInfo(cultureVal);
            return cultureInfo;
        }

        public static string GetLocaleDateTimeDisplayFormat(string languagePreference)
        {
            CultureInfo cultureInfo = GetCultureInfo(languagePreference);
            return cultureInfo.DateTimeFormat.ShortDatePattern + " HH:mm";
        }

        public static string GetLocaleDateTimeFirstDisplayFormat(string languagePreference)
        {
            CultureInfo cultureInfo = GetCultureInfo(languagePreference);
            return "HH:mm " + cultureInfo.DateTimeFormat.ShortDatePattern;
        }

    }
}