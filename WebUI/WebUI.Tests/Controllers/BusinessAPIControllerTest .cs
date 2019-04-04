using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using WebUI.Controllers.API;
using WebUI.DTOs;
using Moq;
using System.Web;
using System.Web.Mvc;
using System.Web.Http.Controllers;
using System.Security.Principal;

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
               // var controllerContext = new Mock<ControllerContext>();
               // var principal = new Moq.Mock<IPrincipal>();
               //// principal.Setup(p => p.IsInRole("Administrator")).Returns(true);
               // principal.SetupGet(x => x.Identity.Name).Returns("manager@test.com");
               // controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);

                var request = new Mock<HttpRequestBase>();
               // request.SetupGet(r => r.Form).Returns(formData);
                var context = new Mock<HttpContextBase>();
                context.SetupGet(c => c.Request).Returns(request.Object);

               
                businessAPIController = new BusinessAPIController();
                //HttpControllerContext context = new HttpControllerContext(new RequestContext(mockContext, new RouteData()), businessAPIController);
                //businessAPIController.ControllerContext = controllerContext.Object; ;
                businessAPIController.ControllerContext = new ControllerContext(context.Object, new RouteData(), businessAPIController);

                HelperMethods.SetupPostControllerForTest(businessAPIController, "BusinessAPI");
            }

            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        [TestMethod]
        public void TestRegisterBusiness()
        {
       
          
            BusinessRegisterDTO busRegDTO = new BusinessRegisterDTO();

            busRegDTO.Name = "Test business" + DateTime.Now.Ticks.ToString() ;
            busRegDTO.HasMultiBusLocations = false;
            busRegDTO.HasMultiInternalLocations = false;
            busRegDTO.TypeId = 2;
            busRegDTO.BusinessLocation = new BusinessLocationDTO
            {
                Phone = "555555",
                AddressLine1 = "test addr1",
                AddressLine2 = "test addr2",
                AddressSuburb = "test Suburb",
                AddressPostcode = "2000",
                AddressState = "NSW"
            };

            var httpResponseMsg = businessAPIController.PostBusiness(busRegDTO);
            
            Assert.IsNotNull(httpResponseMsg.IsSuccessStatusCode);

        }
    }
}
