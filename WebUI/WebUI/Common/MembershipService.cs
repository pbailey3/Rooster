using DotNetOpenAuth.AspNet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Http;
using WebUI.DTOs;
using WebUI.Http;

namespace WebUI.Common
{
    /// <summary>
    /// This service provides the single entry point for any membership service 
    /// </summary>
    public static class MembershipService
    {

        public static OAuthLoginModelResultDTO ValidateOAuthUser(AuthenticationResult result, HttpSessionStateBase session)
        {
            OAuthLoginModelResultDTO userRegistered = null; 

            using (HttpClientWrapper httpClient = new HttpClientWrapper(session))
            {
                //Call confirm account API method
                var responseMessage = httpClient.PostAsJsonAsync("/api/AccountAPI/ValidateOAuthUser", new OAuthLoginModelDTO { ProviderName = result.Provider, ProviderUserID = result.ProviderUserId }).Result;
                if (responseMessage.IsSuccessStatusCode)
                    userRegistered = JsonConvert.DeserializeObject<OAuthLoginModelResultDTO>(responseMessage.Content.ReadAsStringAsync().Result);
            }
            
            return userRegistered;
        }
    }
}