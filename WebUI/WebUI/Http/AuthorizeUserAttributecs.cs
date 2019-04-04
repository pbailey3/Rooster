using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using WebUI.Common;

namespace WebUI.Http
{
    public class AuthorizeUserAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            IPrincipal user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }

            //Compare the list of roles required against those in the users token. Is no intersection then return false (unauthorized)
            if (base.Roles != string.Empty)
            {
                var _rolesSplit = base.Roles.Split(',').ToList();
                var userRoles = ClaimsHelper.GetUserRoles(WebUI.Common.Common.GetToken(System.Web.HttpContext.Current));
                if (_rolesSplit.Count > 0 && _rolesSplit.Intersect(userRoles).Count() == 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
