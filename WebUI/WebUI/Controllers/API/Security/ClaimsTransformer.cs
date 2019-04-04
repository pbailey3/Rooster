using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace WebUI.Controllers.API.Security
{
    public class ClaimsTransformer : ClaimsAuthenticationManager
    {
        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (!incomingPrincipal.Identity.IsAuthenticated)
                return base.Authenticate(resourceName, incomingPrincipal);
            

            using (var accountApiController = new AccountAPIController())
            {
                var claims = accountApiController.GetUserClaims(incomingPrincipal.Identity.Name);
                incomingPrincipal.Identities.First().AddClaims(claims);
            }
            
            return incomingPrincipal;
        }
    }
}