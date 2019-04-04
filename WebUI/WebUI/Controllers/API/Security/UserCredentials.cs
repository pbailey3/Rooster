using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebUI.Models;

namespace WebUI.Controllers.API.Security
{
    public static class UserCredentials
    {
        public const  int MaxInvalidPasswordAttempts = 5;


        public static bool ValidateBasic(string username, string passwordHash)
        {

            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(passwordHash))
            {
                using (var accountApiController = new AccountAPIController())
                {
                    var loginModelDTO = accountApiController.GetAccount(username, passwordHash);

                    if (loginModelDTO != null)
                        return true;
                }
            }

            return false;
        }

        public static string[] GetUserRoles(string email)
        {
            using (var accountApiController = new AccountAPIController())
            {
                var roles = accountApiController.GetUserRoles(email);
                return roles;
            }
        }
    }
}