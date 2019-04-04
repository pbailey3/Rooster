using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebUI.Controllers.API;
using System.Net.Http;
using WebUI.DTOs;
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
using AutoMapper;

namespace WebUI.Tests.Controllers
{
    [TestClass]
    public class AccountAPIControllerTest
    {
        AccountAPIController accountAPIController;
           
        [TestInitialize]
        public void Setup()
        {
            if (accountAPIController == null)
            {
                accountAPIController = new AccountAPIController();
                HelperMethods.SetupPostControllerForTest(accountAPIController, "AccountAPI");
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

        //[TestMethod]
        //public void TestPostAccount()
        //{
        //    var dateTimeHash = DateTime.Now.GetHashCode().ToString();

        //    var registerModelDTO = new RegisterModelDTO { FirstName = "TestFirst", 
        //        LastName = "TestLast", 
        //        Password = "password", 
        //        Email = "test@" + dateTimeHash + ".com", 
        //        Address.Line1 = "Address Line1",
        //        AddressLine2 = "Address Line1",
        //        AddressSuburb = "Suburb",
        //        AddressState = "NSW",
        //        AddressPostcode = "2000",
        //        MobilePhone = "123321456"
        //    };

        //    //Test a new creation account
        //    var result = accountAPIController.PostAccount(registerModelDTO);
        //    var confirmationToken =  Json.Decode( result.Content.ReadAsStringAsync().Result);
        //   Assert.IsTrue(result.IsSuccessStatusCode);

        //    //Now test a duplicate account creation
        //    result = accountAPIController.PostAccount(registerModelDTO);
        //    Assert.IsFalse(result.IsSuccessStatusCode);
        //}

        //[TestMethod]
        //public void TestGetAccount() //Test validate account
        //{
        //    var email = "test@" + DateTime.Now.GetHashCode().ToString() + ".com";

        //    var registerModelDTO = new RegisterModelDTO { 
        //        FirstName = "TestGetFirst", 
        //        LastName = "TestGetLast", 
        //        Password = "password2", 
        //        Email = email ,
        //        MobilePhone = "55511122",
        //        AddressLine1 = "Test Address Line1",
        //        AddressSuburb = "TestSuburb",
        //        AddressPostcode = "2000",
        //        AddressState = "NSW"
        //    };

        //    //Test a new creation account
        //    var result = accountAPIController.PostAccount(registerModelDTO);
        //    Assert.IsTrue(result.IsSuccessStatusCode);
            
           
        //    //Test invalid password login
        //    try
        //    {
        //        var result2 = UserCredentials.ValidateBasic(email, "password2");
        //        Assert.Fail("No Exception thrown");
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex is HttpResponseException)
        //        {
               
        //        }
        //    }

        //}


    }
}
