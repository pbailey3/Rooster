using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using WebUI.Controllers.API;

namespace WebUI.Tests.Controllers
{
    [TestClass]
    public class BusinessTypeAPIControllerTest
    {
        //Test creation of a new business
        BusinessTypeAPIController businessTypeAPIController;

        [TestInitialize]
        public void Setup()
        {
            if (businessTypeAPIController == null)
            {
                businessTypeAPIController = new BusinessTypeAPIController();
                HelperMethods.SetupPostControllerForTest(businessTypeAPIController, "BusinessTypeAPI");
            }

            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        [TestMethod]
        public void TestGetBusinessTypes()
        {

            var businessTypeDTOList = businessTypeAPIController.GetBusinessTypes();

            Assert.IsNotNull(businessTypeDTOList);

        }
    }
}
