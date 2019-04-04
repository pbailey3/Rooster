using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using WebUI.Common;
using Thinktecture.IdentityModel.Http;

namespace WebUI.Http
{
    public class HttpClientWrapper : HttpClient
    {
        readonly string baseUri = System.Configuration.ConfigurationManager.AppSettings.Get("ApiBaseURL");
       
        public HttpClientWrapper(HttpSessionStateBase session)
            : base()
        {
            this.BaseAddress = new Uri(baseUri);
            string sessionToken = WebUI.Common.Common.GetToken(HttpContext.Current); ;
            this.DefaultRequestHeaders.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue(HttpContext.Current.Request.UserLanguages.FirstOrDefault()));
            this.SetToken("Session", (sessionToken == null ? "" : sessionToken.ToString()));
        }
    }
}