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
using System.Collections.Generic;


namespace WebUI.Tests.Controllers
{
    [TestClass]
    public class BusinessAPIControllerTest
    {
         //Test creation of a new business
        BusinessAPIController businessAPIController;
           
        [TestInitialize]
        public void Setup()
        {
            if (businessAPIController == null)
            {
                businessAPIController = new BusinessAPIController();
                HelperMethods.SetupPostControllerForTest(businessAPIController,"BusinessAPI");
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
        public void TestGetBusinesses()
        {
           
            var businessDTOList = businessAPIController.GetBusinesses();
            
            Assert.IsNotNull(businessDTOList);
            
        }

        [TestMethod]
        public void TestPostBusiness()
        {
            this.PostBusiness();
        }

        #region Helper Methods

        public string PostBusiness()
        {
            var businessDTO = new BusinessDTO
            {
                Name = "TestPost Business" + DateTime.Now.GetHashCode().ToString(),
                Phone = "2343034",
                AddressLine1 = "Test Address l1",
                AddressSuburb = "TestSuburb",
                AddressState = "NSW",
                AddressPostcode = "2000",
                TypeId = 2,
                TypeDetail = "Bar",
                TypeIndustry = "Hospitality"
            };
            
            HttpResponseMessage response = businessAPIController.PostBusiness(businessDTO);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
            return response.Headers.Location.Segments[3];
        }

        public string GetRole(string businessID)
        {

            var roleDTO = new RoleDTO
            {
                Id = Guid.NewGuid(),
                Name = "UnitTestRole" + DateTime.Now.GetHashCode().ToString(),
                BusinessId = Guid.Parse(businessID)
            };

            HttpResponseMessage response = businessAPIController.PostBusinessRole(roleDTO.BusinessId, roleDTO.Id, roleDTO);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
            return response.Headers.Location.Segments[3];
        }



        #endregion

        [TestMethod]
        public void TestDeleteBusiness()
        {
            var businessDTO = new BusinessDTO
            {
                Name = "TestDelete Business" + DateTime.Now.GetHashCode().ToString(),
                Phone = "765433 2",
                AddressLine1 = "Test AddressD l1",
                AddressSuburb = "TestSuburbD",
                AddressState = "NSW",
                AddressPostcode = "2001",
                TypeId = 3,
                TypeDetail = "Restaurant",
                TypeIndustry = "Hospitality"
            };

            //First create a new business
            HttpResponseMessage response = businessAPIController.PostBusiness(businessDTO);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);

            var queryString = new List<string>(response.Headers.Location.Segments);
            var newID = Guid.Parse(queryString[queryString.Count - 1]);
           
           businessAPIController.DeleteBusiness(newID);

           try
           {
               //now try to load the deleted business
               var savedBusinessDTO = businessAPIController.GetBusiness(newID);
               Assert.Fail(); //Should not get here as exception is thrown.
           }
           catch (HttpResponseException ex)
           {
               Assert.IsTrue(ex.Response.StatusCode == HttpStatusCode.NotFound);
           }


        }

        [TestMethod]
        public void TestPutBusiness()
        {
            var businessDTO = new BusinessDTO
            {
                Name =  "TestPost2 Business" + DateTime.Now.GetHashCode().ToString(),
                Phone = "765433 2",
                AddressLine1 = "Test Address2 l1",
                AddressSuburb = "TestSuburb2",
                AddressState = "NSW",
                AddressPostcode = "2001",
                TypeId = 3,
                TypeDetail = "Restaurant",
                TypeIndustry = "Hospitality"
            };

            //First create a new business
            HttpResponseMessage response = businessAPIController.PostBusiness(businessDTO);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);

            var queryString = new List<string>(response.Headers.Location.Segments);
            var newID = Guid.Parse(queryString[queryString.Count - 1]);
            businessDTO.Id = newID;

            //now load the newly saved business back up
            var savedBusinessDTO = businessAPIController.GetBusiness(newID);
            Assert.AreEqual(businessDTO, savedBusinessDTO);

            //Test UPDATE
            savedBusinessDTO.Name = savedBusinessDTO.Name.Replace("TestPost2", "TestPut2");
            //Test changing busienss type
            savedBusinessDTO.TypeId = 4;
            savedBusinessDTO.TypeDetail = "Hotel";

            //Call update method
            response = businessAPIController.PutBusiness(newID, savedBusinessDTO);

            //now load the newly updated business back up and compare
            var updatedBusinessDTO = businessAPIController.GetBusiness(newID);
            Assert.AreEqual(savedBusinessDTO, updatedBusinessDTO);

        }

        [TestMethod]
        public void TestPostDeleteBusinessRole()
        {
            BusinessAPIControllerTest businessAPITest = new BusinessAPIControllerTest();
            businessAPITest.Setup();

            string businessID = businessAPITest.PostBusiness();

            var roleDTO = new RoleDTO
            {
                Id = Guid.NewGuid(),
                Name = "UnitTestRole" + DateTime.Now.GetHashCode().ToString(),
                BusinessId = Guid.Parse(businessID)
            };

            HttpResponseMessage response = businessAPIController.PostBusinessRole(roleDTO.BusinessId, roleDTO.Id, roleDTO);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);
            Guid roleId = Guid.Parse(response.Headers.Location.Segments[3]);

            HttpResponseMessage deleteResponse = businessAPIController.DeleteBusinessRole(roleDTO.BusinessId, roleId);
            Assert.AreEqual(deleteResponse.StatusCode, HttpStatusCode.OK);
        }

    }
}
