using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using Thinktecture.IdentityModel.Extensions;

namespace WebUI.Tests.Controllers
{
    [TestClass]
    public class WebAPISecurityTest
    {
        readonly string baseUri = System.Configuration.ConfigurationManager.AppSettings.Get("ApiBaseURL");

        [TestMethod]
        public void TestAuthorization()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };

            client.SetBasicAuthentication("bob", "bob");

            var response = client.GetAsync("api/BusinessAPI").Result;
            response.EnsureSuccessStatusCode();

        }
    }
}
