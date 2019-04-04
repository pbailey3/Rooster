using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Web;
using WebUI.DTOs;

namespace WebUI.Common
{
    public class ClaimsHelper
    {
        public static string GetClaimValue(string sessionToken, string claimKey)
        {
            var token = new JwtSecurityToken(sessionToken);

            var claimValue = from c in token.Claims
                             where c.Type == claimKey
                            select c.Value;
            return claimValue.FirstOrDefault();
        }

        
        public static IEnumerable<Claim> GetClaims(string sessionToken)
        {
            var token = new JwtSecurityToken(sessionToken);
          
            return token.Claims;
        }
        
        //Check any locations in the business to see if the users is a manager
        public static bool IsBusinesssManager(HttpContext httpContext, List<BusinessLocationSummaryDTO> businessLocList)
        {
            var allowed = false;
            foreach (var busLoc in businessLocList)
            {
                if (httpContext.User.Identity.IsAuthenticated)
                {
                    allowed = GetUserBusinessLocationManager(Common.GetToken(httpContext)).Contains(busLoc.Id.ToString());
                    if(allowed)
                        break;
                }
            }
          
            return allowed;
        }

        public static bool IsLocationManager(HttpContext httpContext, Guid businessLocationId)
        {
            var allowed = false;

            if (httpContext.User.Identity.IsAuthenticated)
                allowed = GetUserBusinessLocationManager(Common.GetToken(httpContext)).Contains(businessLocationId.ToString());

            return allowed;
        }


        public static bool IsInRole(HttpContext httpContext, string roleName)
        {
            var allowed = false;

            if (httpContext.User.Identity.IsAuthenticated)
                allowed = IsInRole(Common.GetToken(httpContext), roleName);

            return allowed;
        }

        public static bool IsInRole(string sessionToken, string roleName)
        {
            return GetUserRoles(sessionToken).Contains(roleName);
        }

        public static List<string> GetUserRoles(string sessionToken)
        {
            var token = new JwtSecurityToken(sessionToken);
            var roleClaim = from c in token.Claims
                            where c.Type == "role"
                            select c.Value;
            return roleClaim.ToList();
        }

        private static List<string> GetUserBusinessLocationManager(string sessionToken)
        {
            var token = new JwtSecurityToken(sessionToken);
            var roleClaim = from c in token.Claims
                            where c.Type == "BusinessLocationManagerId"
                            select c.Value;
            return roleClaim.ToList();
        }

    }
}