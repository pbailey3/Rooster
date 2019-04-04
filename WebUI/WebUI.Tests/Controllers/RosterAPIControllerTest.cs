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
    public class RosterAPIControllerTest
    {
        //Test creation of a new business
        RosterAPIController rosterAPIController;

        [TestInitialize]
        public void Setup()
        {
            if (rosterAPIController == null)
            {
                rosterAPIController = new RosterAPIController();
                HelperMethods.SetupPostControllerForTest(rosterAPIController, "RosterAPI");
            }

            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        //[TestMethod]
        //public void TestGetRosterList()
        //{
        //    DataModelDbContext db = new DataModelDbContext();
        //    Business business = db.Businesses.FirstOrDefault(b => b.Name == "Bar Rumba");
            
        //    //db.Businesses.
        //    string sDate = DateTime.MinValue.ToShortDateString();
        //    string eDate = DateTime.MaxValue.ToShortDateString();

        //    var rosterDTOList = rosterAPIController.GetRostersForBusiness(business.Id, sDate, eDate);
        //    Assert.IsTrue(rosterDTOList.Count() == 4 ,"Roster count should be 4");
        //    var obj1 = rosterDTOList.Where(r => r.WeekStartDate == DateTime.Parse("2013-09-23")).FirstOrDefault();
        //    Assert.IsNotNull(obj1);
        //    Assert.IsTrue(obj1.Shifts.Count == 2);
          
        //    var objShift1 = obj1.Shifts.FirstOrDefault(s=> s.StartTime == DateTime.Parse("2013-09-23 08:00:00") 
        //                                                    && s.FinishTime == DateTime.Parse("2013-09-23 12:00:00")
        //                                                    && s.EmployeeId == Constants.RobertEmployeeID
        //                                                    && s.InternalLocationId == Constants.MainBarLocationID
        //                                                    && s.InternalLocationName == "Main Bar"
        //                                                    && s.RoleId == Constants.BarTenderRoleID
        //                                                    && s.RoleName == "Bar tender");

        //    Assert.IsNotNull(objShift1);
        //    var obj2 = rosterDTOList.Where(r => r.WeekStartDate == DateTime.Parse("2013-09-30")).FirstOrDefault();
        //    Assert.IsNotNull(obj2);
        //    Assert.IsNotNull(rosterDTOList);

        //}

    }
}
