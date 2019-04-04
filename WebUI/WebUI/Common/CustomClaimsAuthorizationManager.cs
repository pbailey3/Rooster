using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using WebUI.Common;

namespace WebUI.Controllers.Common
{
    public class CustomClaimsAuthorizationManager : ClaimsAuthorizationManager
    {
        public override bool CheckAccess(AuthorizationContext context)
        {
            var resource = context.Resource.First().Value;
            var resourceId = (context.Resource.Count > 1 ? context.Resource[1].Value : null); 
            var action = context.Action.First().Value;

            //If user in Role of Sys Admin then always return true
            if (context.Principal.IsInRole(Constants.RoleSysAdmin))
                return true;

            // hardcoded rules could be replaced by injection or load from xml
           
            //Test Business membership for retrieveing details
            if ( action == "Get" && resource == "BusinessId" )
                return context.Principal.Claims.Any(c => c.Type == Constants.ClaimBusinessEmployee && c.Value == resourceId);

            //Test Business membership for retrieveing details
            if (action == "Put" && resource == "BusinessId")
                return context.Principal.Claims.Any(c => c.Type == Constants.ClaimBusinessAdmin && c.Value == resourceId);

            //Test Business location membership for updating business location details
            if (action == "Put" && resource == "BusinessLocationId")
                return context.Principal.Claims.Any(c => c.Type == Constants.ClaimBusinessLocationManager && c.Value == resourceId);
           
            //If no specific tests are applied then return false
            return false;
        }
    }
}