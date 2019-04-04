using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI.Common
{
    public static class Constants
    {
        public const string CookieTokenKey = "TokenCookie";

        public const string SessionRosterBroadcastKey = "SessionRosterBroadcastKey";
        
        public const string SessionUsernameKey = "SessionUsernameKey";

        public const string SessionPasswordKey = "SessionPasswordKey";

        public const string FormatDate = @"{0:yyyy-MM-dd}"; //Standard format is needed for the HTML5 date control to understand

        public const string DisplayFormatTime = "hh:mm tt"; //Format to use when displaying on screen

        public const string FormatTimeSpan = @"{0:hh\:mm}";

        public const string FormatTotalHours = @"{0:N1}";

        public const string FormatCurrency = @"{0:C}";


        /******** Claims **********/
        public const string ClaimBusinessEmployee = "BusinessEmployeeId";

        public const string ClaimBusinessLocationManager = "BusinessLocationManagerId";
        
        public const string ClaimBusinessAdmin = "BusinessAdminId";
        
        public const string ClaimFirstName = "FirstName";

        public const string ClaimLastName = "LastName";

        /******** Roles **********/
        public const string RoleSysAdmin = "SysAdmin";

        public const string RoleBusinessAdmin = "Admin";

        public const string RoleBusinessLocationManager = "Manager";

        public const int TimeCardShiftVariance = 30;



    }
}