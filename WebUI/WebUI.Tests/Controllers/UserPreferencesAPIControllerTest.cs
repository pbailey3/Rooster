using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using WebUI.Controllers.API;
using WebUI.DTOs;
using WebUI.Models;
using System.Data.Entity;
using WebUI.Tests.Common;


namespace WebUI.Tests.Controllers
{
    [TestClass]
    public class UserPreferencesAPIControllerTest
    {
        //Test creation of a new business
        UserPreferencesAPIController userPrefAPIController;

        [TestInitialize]
        public void Setup()
        {
            if (userPrefAPIController == null)
            {
                userPrefAPIController = new UserPreferencesAPIController();
                HelperMethods.SetupPostControllerForTest(userPrefAPIController, "RosterAPI");
            }

            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        [TestMethod]
        public void TestUserPrefAPI()
        {
            DataModelDbContext db = new DataModelDbContext();
            var userPrefs = userPrefAPIController.GetPreferencesForCurrentUser();
        }

    }
}
