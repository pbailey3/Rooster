using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using WebUI.Controllers.API;
using WebUI.DTOs;
using System.Net.Http;
using System.Net;
using System.Web.Http;

namespace WebUI.Tests.Controllers
{
    [TestClass]
    public class EmployeeAPIControllerTest
    {
        //Test creation of a new business
        EmployeeAPIController employeeAPIController;

        [TestInitialize]
        public void Setup()
        {
            if (employeeAPIController == null)
            {
                employeeAPIController = new EmployeeAPIController();
                HelperMethods.SetupPostControllerForTest(employeeAPIController, "EmployeeAPI");
            }

            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        [TestMethod]
        public void TestGetEmployees()
        {

            var employeeDTOList = employeeAPIController.GetEmployees();

            Assert.IsNotNull(employeeDTOList);
        }

        [TestMethod]
        public void TestPostDeleteEmployee()
        {
            BusinessAPIControllerTest businessAPITest = new BusinessAPIControllerTest();
            businessAPITest.Setup();

            string businessID = businessAPITest.PostBusiness();

            var employeeDTO = new EmployeeDTO
            {
                FirstName = "Unit" + DateTime.Now.GetHashCode().ToString(),
                LastName = "TestPost",
                MobilePhone = "44122322",
                Email = "testEMAIL@htmail.com",
                Type = EmployeeTypeDTO.Casual,
                BusinessId = Guid.Parse(businessID)
            };
           
            //First create a new employee
            HttpResponseMessage response = employeeAPIController.PostEmployee(employeeDTO);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);

            var queryString = new List<string>(response.Headers.Location.Segments);
            var newID = Guid.Parse(queryString[queryString.Count - 1]);

            employeeAPIController.DeleteEmployee(newID);

            try
            {
                //now try to load the deleted business
                var savedEmployeeDTO = employeeAPIController.GetEmployee(newID);
                Assert.Fail(); //Should not get here as exception is thrown.
            }
            catch (HttpResponseException ex)
            {
                Assert.IsTrue(ex.Response.StatusCode == HttpStatusCode.NotFound);
            }

        }

        [TestMethod]
        public void TestPutGetEmployee()
        {
            BusinessAPIControllerTest businessAPITest = new BusinessAPIControllerTest();
            businessAPITest.Setup();

            string businessID = businessAPITest.PostBusiness();

            var employeeDTO = new EmployeeDTO
            {
                FirstName = "Unit" + DateTime.Now.GetHashCode().ToString(),
                LastName = "TestPostGet",
                MobilePhone = "876435",
                Email = "testEMAIL@htmail.com",
                Type = EmployeeTypeDTO.Casual,
                BusinessId = Guid.Parse(businessID)
            };

            //First create a new employee
            HttpResponseMessage response = employeeAPIController.PostEmployee(employeeDTO);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);

            var queryString = new List<string>(response.Headers.Location.Segments);
            var newID = Guid.Parse(queryString[queryString.Count - 1]);
            employeeDTO.Id = newID;

            //now load the newly saved business back up
            var savedEmployeeDTO = employeeAPIController.GetEmployee(newID);
            Assert.AreEqual(employeeDTO, savedEmployeeDTO);

            //Test UPDATE
            savedEmployeeDTO.LastName = savedEmployeeDTO.LastName.Replace("TestPostGet", "TestPutGet");
            //Test changing employee type
            savedEmployeeDTO.Type = EmployeeTypeDTO.Full_Time;
       
            //Call update method
            response = employeeAPIController.PutEmployee(newID, savedEmployeeDTO);

            //now load the newly updated business back up and compare
            var updatedEmployeeDTO = employeeAPIController.GetEmployee(newID);
            Assert.AreEqual(savedEmployeeDTO, updatedEmployeeDTO);

        }

        [TestMethod]
        public void TestPostDeleteEmployeeRole()
        {
            BusinessAPIControllerTest businessAPITest = new BusinessAPIControllerTest();
            businessAPITest.Setup();

            string businessID = businessAPITest.PostBusiness();
            string roleID = businessAPITest.GetRole(businessID);

            var employeeDTO = new EmployeeDTO
            {
                FirstName = "Unit" + DateTime.Now.GetHashCode().ToString(),
                LastName = "TestPostDeleteRole",
                MobilePhone = "876435",
                Email = "testEMAIL2@htmail.com",
                Type = EmployeeTypeDTO.Casual,
                BusinessId = Guid.Parse(businessID)
            };

            //First create a new employee
            HttpResponseMessage response = employeeAPIController.PostEmployee(employeeDTO);
            Assert.AreEqual(response.StatusCode, HttpStatusCode.Created);

            var queryString = new List<string>(response.Headers.Location.Segments);
            var newID = Guid.Parse(queryString[queryString.Count - 1]);

             HttpResponseMessage responseRole = employeeAPIController.PostEmployeeRole(newID, Guid.Parse(roleID));
             Assert.AreEqual(responseRole.StatusCode, HttpStatusCode.OK);

             //now load the newly saved employee back up and verify that role is added
             var savedEmployeeDTO = employeeAPIController.GetEmployee(newID);
             Assert.IsTrue(savedEmployeeDTO.Roles.Count > 0);
             Assert.IsTrue(savedEmployeeDTO.Roles[0].Id == Guid.Parse(roleID));

             HttpResponseMessage responseDeleteRole = employeeAPIController.DeleteEmployeeRole(newID, Guid.Parse(roleID));
             Assert.AreEqual(responseRole.StatusCode, HttpStatusCode.OK);
             
            //now load the employee back up and verify that role is removed
             savedEmployeeDTO = employeeAPIController.GetEmployee(newID);
             Assert.IsTrue(savedEmployeeDTO.Roles.Count == 0);
        }


    }
}
