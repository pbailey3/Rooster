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
    public class ScheduleActionAPIControllerTest
    {
        //Test creation of a new business
        ScheduleActionAPIController scheduleActionAPIController;

        [TestInitialize]
        public void Setup()
        {
            if (scheduleActionAPIController == null)
            {
                scheduleActionAPIController = new ScheduleActionAPIController();
                HelperMethods.SetupPostControllerForTest(scheduleActionAPIController, "ScheduleActionAPI");
            }

            AutoMapperConfig.Configure();

            if (RouteTable.Routes.Count == 0)
                RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        [TestMethod]
        public void TestGetUnavailableEmployeesAPI()
        {
         /* TEST DATA 
            Id	Title	    Location	Description	StartDate	                StartTime	        EndDate	                EndTime	            Frequency	ScheduleRecurrence	NumberOfOccurrences	MonthlyInterval	DaysOfWeek	WeeklyInterval	UserProfile_Id
            3	ONCEOFFPARTIAL	NULL	NULL	    2015-07-28 00:00:00.000	    06:00:00.0000000	NULL	                13:00:00.0000000	0	        0	                0	                0	            0	        0	            AD082901-BB88-4E87-8EBD-71B22B2363AB
            4	ONCEOFFALLDAY	NULL	NULL	    2015-07-31 00:00:00.000	    00:00:00.0000000	NULL	                23:59:59.0000000	0	        0	                0	                0	            0	        0	            AD082901-BB88-4E87-8EBD-71B22B2363AB
            5	DAILYPARTIAL	NULL	NULL	    2015-07-19 00:00:00.000	    06:00:00.0000000	2015-08-02 00:00:00.000	07:00:00.0000000	1	        1	                0	                0	            127	        0	            AD082901-BB88-4E87-8EBD-71B22B2363AB
            6	DAILYALLDAY	    NULL	NULL	    2015-08-09 00:00:00.000	    00:00:00.0000000	2015-08-15 00:00:00.000	23:59:59.0000000	1	        1	                0	                0	            127	        0	            AD082901-BB88-4E87-8EBD-71B22B2363AB
            7	WEEKLYPARTIAL	NULL	NULL	    2015-07-19 00:00:00.000	    18:00:00.0000000	2015-08-06 00:00:00.000	22:00:00.0000000	2	        1	                0	                0	            10	        1	            AD082901-BB88-4E87-8EBD-71B22B2363AB
            8	WEEKLYALLDAY	NULL	NULL	    2015-08-17 00:00:00.000	    00:00:00.0000000	2015-10-22 00:00:00.000	23:59:59.0000000	2	        1	                0	                0	            1	        1	            AD082901-BB88-4E87-8EBD-71B22B2363AB
            */  
    //        Unavailability
    //1) ONCEOFFPARTIAL - Tues 28/7 6am - 1pm
    //2) ONCEOFFALLDAY - Fri 31/7 ALL DAY
    //3) DAILYPARTIAL - 19/7 - 2/08 6am-7am
    //4) DAILYALLDAY - 9/8 - 15/8 ALL DAY
    //5) WEEKLYPARTIAL - Mon/Wed 19/7 - 6/8 6pm-10pm
    //6) WEEKLYALLDAY - SUN 16/08 - 22/10 ALLDAY

            var busLocId = Guid.Parse("4883D368-92F9-4344-B7BF-078B0DD79052");
            Guid johnID = Guid.Parse("4A604EC3-5D8E-4D6F-9E1A-1A4C34D405BB");
            
            
            //1) ONCEOFFPARTIAL -  28/7 6am - 1pm
            //Test 1-1 - Unavailable
            var dtStart = DateTime.Parse("28/07/2015 06:00");
            var dtEnd = DateTime.Parse("28/07/2015 08:00");
            var unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);
            
            Assert.IsNotNull( unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 1-2 - Available
            dtStart = DateTime.Parse("28/07/2015 13:30");
            dtEnd = DateTime.Parse("28/07/2015 18:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 1-3 - Unavailable - Start time and end time greater than shift time
            dtStart = DateTime.Parse("28/07/2015 05:00");
            dtEnd = DateTime.Parse("28/07/2015 18:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));


            //2) ONCEOFFALLDAY - Fri 31/7 ALL DAY
            //Test 2-1 - Unavailable
            dtStart = DateTime.Parse("31/07/2015 13:30");
            dtEnd = DateTime.Parse("31/07/2015 18:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //3) DAILYPARTIAL - 19/7 - 2/08 6am-7am
            //Test 3-1 - Unavailable Start date
            dtStart = DateTime.Parse("19/07/2015 06:00");
            dtEnd = DateTime.Parse("19/07/2015 12:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 3-2 - Unavailable End date
            dtStart = DateTime.Parse("02/08/2015 06:00");
            dtEnd = DateTime.Parse("02/08/2015 12:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 3-3 - Available End date +1
            dtStart = DateTime.Parse("03/08/2015 06:00");
            dtEnd = DateTime.Parse("03/08/2015 12:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 3-4 - End date overlaps start of unavail, but start time overlaps period of previous day which is available
            dtStart = DateTime.Parse("18/07/2015 06:30");
            dtEnd = DateTime.Parse("19/07/2015 02:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 3-5 - Unavailable shift encompasses unavailable time completely
            dtStart = DateTime.Parse("19/07/2015 05:00");
            dtEnd = DateTime.Parse("19/07/2015 08:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));


            //4) DAILYALLDAY - 9/8 - 15/8 ALL DAY
            //Test 4-1 - Aavailable Start date - 1
            dtStart = DateTime.Parse("08/08/2015 06:00");
            dtEnd = DateTime.Parse("08/08/2015 12:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 4-2 - Unavailable Start date - 1 overlap midnight
            dtStart = DateTime.Parse("08/08/2015 23:00");
            dtEnd = DateTime.Parse("09/08/2015 04:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 4-3 - Unavailable Start date 
            dtStart = DateTime.Parse("09/08/2015 06:00");
            dtEnd = DateTime.Parse("09/08/2015 12:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 4-4 - Unavailable end date 
            dtStart = DateTime.Parse("15/08/2015 06:00");
            dtEnd = DateTime.Parse("15/08/2015 12:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 4-5 - Unavailable end date over midnight
            dtStart = DateTime.Parse("15/08/2015 23:00");
            dtEnd = DateTime.Parse("16/08/2015 04:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 4-6 - Available end date +1 over midnight
            dtStart = DateTime.Parse("16/08/2015 08:00");
            dtEnd = DateTime.Parse("16/08/2015 12:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

           
            //5) WEEKLYPARTIAL - Mon/Wed 19/7 - 6/8 6pm-10pm
            //Test 5-1 - Available Start date before block
            dtStart = DateTime.Parse("20/07/2015 16:00");
            dtEnd = DateTime.Parse("20/07/2015 17:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 5-2 - Unavailable block end time overlap block
            dtStart = DateTime.Parse("20/07/2015 16:00");
            dtEnd = DateTime.Parse("20/07/2015 18:30");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 5-3 - Unavailable start time in block end time over block
            dtStart = DateTime.Parse("20/07/2015 18:30");
            dtEnd = DateTime.Parse("20/07/2015 23:30");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 5-4 - Available same day, start block, after period
            dtStart = DateTime.Parse("20/07/2015 23:00");
            dtEnd = DateTime.Parse("20/07/2015 23:30");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 5-5 - Available next day after start of weekly
            dtStart = DateTime.Parse("21/07/2015 08:00");
            dtEnd = DateTime.Parse("21/07/2015 12:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 5-5 - Unavailable first wednesday in weekly recurring.
            dtStart = DateTime.Parse("22/07/2015 18:00");
            dtEnd = DateTime.Parse("22/07/2015 22:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //	6) WEEKLYALLDAY - SUNDAYS 17/08 - 22/10 ALLDAY
            //Test 6-1 - Available Start day before block first Sunday in block
            dtStart = DateTime.Parse("22/08/2015 16:00");
            dtEnd = DateTime.Parse("22/08/2015 19:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNull(unavailability.FirstOrDefault(u => u.Id == johnID));

            //Test 6-2 - Unavailable first Sunday in block
            dtStart = DateTime.Parse("23/08/2015 06:00");
            dtEnd = DateTime.Parse("23/08/2015 19:00");
            unavailability = scheduleActionAPIController.GetUnavailableEmployees(busLocId, dtStart.Ticks.ToString(), dtEnd.Ticks.ToString(), Guid.Empty);

            Assert.IsNotNull(unavailability.FirstOrDefault(u => u.Id == johnID));

        }

    }
}
