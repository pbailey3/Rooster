using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebUI.Controllers.API;
using System.Net.Http;
using WebUI.DTOs;
using AutoMapper;
using System.Web.Http.ModelBinding;
using System.Collections.Specialized;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Http.Routing;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using WebUI.Controllers.API.Security;
using System.Web.Helpers;


namespace WebUI.Tests.Controllers
{
    [TestClass]
    public class RequestAPIControllerTest
    {
        RequestAPIController requestAPIController;
           
        [TestInitialize]
        public void Setup()
        {
            if (requestAPIController == null)
            {
                requestAPIController = new RequestAPIController();
                HelperMethods.SetupPostControllerForTest(requestAPIController, "RequestAPI");
            }
            
            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);           
        }

        [TestMethod]
        public void TestAutoMapperConfig()
        {
            Mapper.AssertConfigurationIsValid();
        }

        [TestMethod]
        public void TestCreateRequest()
        {
            var dateTimeHash = DateTime.Now.GetHashCode().ToString();

            
            //Test a new creation account
            var result = requestAPIController.CreateRequest("D5E45AAA-B05E-4270-967F-CB0A0DAB6E93");
            var confirmationToken =  Json.Decode( result.Content.ReadAsStringAsync().Result);
           Assert.IsTrue(result.IsSuccessStatusCode);

           
        }
    }
}
