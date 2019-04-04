using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace WebUI.Common
{
    interface ICustomPrincipal : IPrincipal
    {
        string Email{ get; set; }
        string FirstName { get; set; }
        string Token { get; set; }
    }

    public class CustomPrincipal : ICustomPrincipal
    {
        public IIdentity Identity { get; private set; }
        public bool IsInRole(string role) { return false; }

        public CustomPrincipal(string email)
        {
            this.Identity = new GenericIdentity(email);
        }

        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Token { get; set; }
    }

    public class CustomPrincipalSerializeModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
    }
}