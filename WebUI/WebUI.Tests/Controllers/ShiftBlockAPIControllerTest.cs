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
    public class ShiftBlockAPIControllerTest
    {
        //Test creation of a new business
        ShiftBlockAPIController shiftBlockAPIController;

        [TestInitialize]
        public void Setup()
        {
            if (shiftBlockAPIController == null)
            {
                shiftBlockAPIController = new ShiftBlockAPIController();
                HelperMethods.SetupPostControllerForTest(shiftBlockAPIController, "RosterAPI");
            }

            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        [TestMethod]
        public void TestShiftBlockAPI()
        {
            DataModelDbContext db = new DataModelDbContext();
            Business business = db.Businesses.FirstOrDefault(b => b.Name == "Bar Rumba");
            Role role = db.Roles.FirstOrDefault(r => r.Business.Id == business.Id);

            Random rnd = new Random();
            int rHourStart = rnd.Next(4, 14); //Rand start hour between 4 and 14
            int rMinStart = rnd.Next(0, 60); //Rand start hour between 0 and 60

            ShiftBlockDTO sbDTO = new ShiftBlockDTO
            {
                StartTime = new TimeSpan(rHourStart, rMinStart, 0),
                FinishTime = new TimeSpan(20, 0, 0), //set finish time of 8pm
                FinishNextDay = false,
                BusinessId = business.Id,
                RoleId = role.Id
            };

            var shiftBlocksbeforeAdd = shiftBlockAPIController.GetShiftBlocks(business.Id);

            shiftBlockAPIController.Post(sbDTO);

            var shiftBlocksAfterAdd = shiftBlockAPIController.GetShiftBlocks(business.Id);

            Assert.IsTrue(shiftBlocksAfterAdd.Count() == (shiftBlocksbeforeAdd.Count() + 1), "Shift block has not been saved correctly");

            var obj1 = shiftBlocksAfterAdd.Where(sb => sb.StartTime == new TimeSpan(rHourStart, rMinStart, 0)
                                                && sb.FinishTime == new TimeSpan(20, 0, 0))
                                                .FirstOrDefault();
            Assert.IsNotNull(obj1);

            //Test Get single DTO
            var obj2 = shiftBlockAPIController.Get(obj1.Id);

            Assert.IsNotNull(obj2);
            Assert.IsTrue(obj2.StartTime == sbDTO.StartTime
                 && obj2.FinishTime == sbDTO.FinishTime
                 && obj2.FinishNextDay == sbDTO.FinishNextDay
                 && obj2.BusinessId == sbDTO.BusinessId
                 && obj2.RoleId == sbDTO.RoleId, "Objects do not match");

            //Test Update
            obj2.StartTime = new TimeSpan(rHourStart + 1, rMinStart, 0);
            obj2.FinishTime = new TimeSpan(2, 0, 0);
            obj2.FinishNextDay = true;

            shiftBlockAPIController.Put(obj2.Id, obj2);

            //Test Get again to verify changes DTO
            var obj3 = shiftBlockAPIController.Get(obj1.Id);

            Assert.IsTrue(obj3.StartTime == new TimeSpan(rHourStart + 1, rMinStart, 0)
                 && obj2.FinishTime == new TimeSpan(2, 0, 0)
                 && obj2.FinishNextDay == true, "Update did not work");


            shiftBlockAPIController.Delete(obj1.Id);

            var shiftBlocksAfterDelete = shiftBlockAPIController.GetShiftBlocks(business.Id);

            Assert.IsTrue(shiftBlocksAfterDelete.Count() == shiftBlocksbeforeAdd.Count(), "Shift block has not been deleted correctly");

            //Test Get single DTO after delete
            bool exceptionThrown = false;
            try
            {

                var obj4 = shiftBlockAPIController.Get(obj1.Id);
            }
            catch (Exception ex)
            {
                exceptionThrown = true;

                var mg = ex.Message;
            }

            Assert.IsTrue(exceptionThrown, "Exception not thrown when getting deleted item");

        }

    }
}
