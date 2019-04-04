using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using WebUI.Controllers.API;
using WebUI.DTOs;

namespace WebUI.Tests.Controllers
{
    [TestClass]
    public class ShiftTemplateAPIControllerTest
    {
        //Test creation of a new business
        ShiftTemplateAPIController shiftTemplateAPIController;

        [TestInitialize]
        public void Setup()
        {
            if (shiftTemplateAPIController == null)
            {
                shiftTemplateAPIController = new ShiftTemplateAPIController();
                HelperMethods.SetupPostControllerForTest(shiftTemplateAPIController, "ShiftTemplateAPI");
            }

            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        [TestMethod]
        public void TestGetShiftTemplateList()
        {
            Guid busId = Guid.Parse("E8B880E3-02F9-4323-B1F5-B0EA63D83E60"); //ID of Bar Rumba

            var shiftTemplateDTOList = shiftTemplateAPIController.GetRecurringShifts(busId);

            Assert.IsNotNull(shiftTemplateDTOList);

        }

        [TestMethod]
        public void TestPostShiftTemplate()
        {
            Guid busId = Guid.Parse("E8B880E3-02F9-4323-B1F5-B0EA63D83E60"); //ID of Bar Rumba

            ShiftTemplateDTO template = new ShiftTemplateDTO { Id = Guid.Empty, BusinessId = busId, StartTime = TimeSpan.Parse("08:00:00"), FinishTime = TimeSpan.Parse("14:00:00"), Monday = true };

           // var shiftTemplateDTOList = shiftTemplateAPIController.GetRecurringShifts(busId);

            //Assert.IsNotNull(shiftTemplateDTOList);

        }

    }
}
