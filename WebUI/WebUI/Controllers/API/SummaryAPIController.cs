using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebUI.Models;
using AutoMapper;
using WebUI.DTOs;
using System.Threading;
using WebUI.Common;
using System.Data.Entity.Core.Objects;
using Thinktecture.IdentityModel;

namespace WebUI.Controllers.API
{
    public class SummaryAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        private List<EmployerRequest> EmployerRequestList(List<Guid> mgrBusIdList)
        {
            return (from er in db.EmployerRequests
                    where er.Status == RequestStatus.Pending
                        && mgrBusIdList.Contains(er.BusinessLocation.Id)
                    select er).ToList();
        }

        private List<Guid> GetManagerList(string email)
        {
            return (from emp in db.Employees
                    where emp.UserProfile.Email == email && emp.IsAdmin == true
                    select emp.BusinessLocation.Id).ToList();
        }
        internal List<RecurringShiftChangeRequest> RecurringShiftChangeRequestList(List<Guid> mgrBusIdList)
        {
            DateTime datetimeNow = WebUI.Common.Common.DateTimeNowLocal();

            return (from sr in db.RecurringShiftChangeRequests
                    where sr.Status == RequestStatus.Pending
                        && mgrBusIdList.Contains(sr.ShiftTemplate.InternalLocation.BusinessLocation.Id)
                        && DbFunctions.CreateDateTime(sr.OccurenceDate.Year, sr.OccurenceDate.Month, sr.OccurenceDate.Day, sr.OccurenceDate.Hour, sr.OccurenceDate.Minute, sr.OccurenceDate.Second) >= datetimeNow  //Only get change requests which are in the future.
                    select sr).ToList();
        }
        internal List<ShiftChangeRequest> ShiftChangeRequestList(List<Guid> mgrBusIdList)
        {
            DateTime datetimeNow = WebUI.Common.Common.DateTimeNowLocal();

            return (from sr in db.ShiftChangeRequests
                    where sr.Status == RequestStatus.Pending
                        && mgrBusIdList.Contains(sr.Shift.InternalLocation.BusinessLocation.Id)
                        && DbFunctions.CreateDateTime(sr.Shift.StartTime.Year, sr.Shift.StartTime.Month, sr.Shift.StartTime.Day, sr.Shift.StartTime.Hour, sr.Shift.StartTime.Minute, sr.Shift.StartTime.Second) >= datetimeNow  //Only get change requests which are in the future.
                    select sr).ToList();
        }

        internal List<ExternalShiftRequest> ExternalShiftRequestList()
        {
            DateTime datetimeNow = WebUI.Common.Common.DateTimeNowLocal();

            return (from sr in db.ExternalShiftRequests
                    where sr.Status == RequestStatus.Pending
                    select sr).ToList();
        }

        [Authorize]
        [Route("api/SummaryAPI/TotalRequestCount")]
        public int GetTotalRequestCount()
        {
            var employerRequests = GetEmployerRequestCount();
            var shiftRequests = GetShiftRequestCount();
            var externalShiftRequests = GetExternalShiftRequestCount();

            return (employerRequests + shiftRequests + externalShiftRequests);
        }

        [Authorize]
        [Route("api/SummaryAPI/EmployerRequestCount")]
        public int GetEmployerRequestCount()
        {
            var email = HttpContext.Current.User.Identity.Name;

            //Get list of businesses which user is a manager of
            var mgrBusIdList = GetManagerList(email);

            return EmployerRequestList(mgrBusIdList).Count;
        }

        [Authorize]
        [Route("api/SummaryAPI/ShiftRequestCount")]
        public int GetShiftRequestCount()
        {
            var email = HttpContext.Current.User.Identity.Name;

            //Get list of businesses which user is a manager of
            var mgrBusIdList = GetManagerList(email);
            var scRequestList = ShiftChangeRequestList(mgrBusIdList);
            var rscRequestList = RecurringShiftChangeRequestList(mgrBusIdList);

            return scRequestList.Count + rscRequestList.Count;
        }

        [Authorize]
        [Route("api/SummaryAPI/ExternalShiftRequestCount")]
        public int GetExternalShiftRequestCount()
        {
            var email = HttpContext.Current.User.Identity.Name;

            //Get list of businesses which user is a manager of
            var mgrBusIdList = GetManagerList(email);
            var scRequestList = ExternalShiftRequestList();
            // var rscRequestList = RecurringShiftChangeRequestList(mgrBusIdList);

            return scRequestList.Count;
        }

        // GET api/SummaryAPI
        [Authorize]
        public HomeSummaryDTO GetSummary()
        {
            HomeSummaryDTO empSummary = new HomeSummaryDTO();
            var email = HttpContext.Current.User.Identity.Name;
            DateTime now = WebUI.Common.Common.DateTimeNowLocal();

            //Get list of businesses which user is a manager of
            var mgrBusIdList = GetManagerList(email);

            //Get list of businesses which user is an employee of
            var empBusIdList = from emp in db.Employees
                               where emp.UserProfile.Email == email
                               select emp.BusinessLocation.Id;

            var upcomingShiftList = from s in db.Shifts
                                    where s.IsPublished == true
                                    && s.Employee.UserProfile.Email == email
                                    && s.StartTime > now
                                    && DbFunctions.DiffHours(now, s.StartTime) < 168 //168 hours is 7 days
                                    select new
                                    {
                                        Id = s.Id,
                                        BusinessLocationId = s.Roster.BusinessLocation.Id,
                                        StartDate = s.StartTime,
                                        FinishDate = s.FinishTime,
                                        BusinessLocationName = s.Roster.BusinessLocation.Name,
                                        Role = s.Role.Name
                                    };

            var busSummaryList = from busLoc in db.BusinessLocations
                                 join emp in db.Employees on busLoc.Id equals emp.BusinessLocation.Id
                                 join usr in db.UserProfiles on emp.UserProfile.Id equals usr.Id
                                 where usr.Email == email
                                 select new BusinessLocationSummaryDTO { BusinessId = busLoc.Business.Id, BusinessName = busLoc.Business.Name, Id = busLoc.Id, Name = busLoc.Name, HasInternalLocations = busLoc.Business.HasMultiInternalLocations, UserIsAdmin = mgrBusIdList.Contains(busLoc.Id) };

            var busProfileAddress = from User in db.UserProfiles
                                    where User.Email == email
                                    select new UserProfilesDTO
                                    {
                                        Line1 = User.CurrentAddress.Line1,
                                        Suburb = User.CurrentAddress.Suburb,
                                        Line2 = User.CurrentAddress.Line2,
                                        State = User.CurrentAddress.State,
                                        Postcode = User.CurrentAddress.Postcode,
                                        PlaceLongitude = User.CurrentAddress.PlaceLongitude,
                                        PlaceLatitude = User.CurrentAddress.PlaceLatitude,
                                        PlaceId = User.CurrentAddress.PlaceId,
                                        Distance = User.Distance,
                                        IndustryTypeId = User.IndustryTypesID
                                    };
            using (var ExternalBroadcastAPI = new ExternalBroadcastAPIController())
            {
                empSummary.ExternalShiftBroadCastEmpoyee = ExternalBroadcastAPI.GetEmployeeExternalShift();
            }

            empSummary.Employers = busSummaryList.Distinct().ToList();

            empSummary.UpcomingShifts = upcomingShiftList.OrderBy(s => s.StartDate);

            empSummary.IndustryType = db.IndustryTypes.Select(x => new DropDownDTO
            {
                Value = x.ID,
                Text = x.Name
            }).ToList();

            //empSummary.IndustryType = db.IndustryTypes.GroupBy(g => g.Industry).Select(x => new BusinessTypeDTO { Id = x.Select(s => s.Id).FirstOrDefault(), Industry = x.Key }).Distinct().ToList();

            empSummary.Address = busProfileAddress.FirstOrDefault();

            // Temporary List of data for Distance Dropdown.
            List<DropDownDTO> DistanceListStatic = new List<DropDownDTO>();
            DistanceListStatic.Add(new DropDownDTO { Value = 1, Text = "1 Kms" });
            DistanceListStatic.Add(new DropDownDTO { Value = 5, Text = "5 Kms" });
            DistanceListStatic.Add(new DropDownDTO { Value = 10, Text = "10 Kms" });
            DistanceListStatic.Add(new DropDownDTO { Value = 20, Text = "20 Kms" });

            empSummary.LocationRange = DistanceListStatic;

            using (var shiftApiController = new ShiftAPIController())
            {
                empSummary.OpenShiftsEmployee = shiftApiController.GetOpenShiftsForCurrentUser();
            }

            return empSummary;
        }

        [Authorize]
        [Route("api/SummaryAPI/GetSites")]
        public HomeSummaryDTO GetSites()
        {
            HomeSummaryDTO model = new HomeSummaryDTO();
            var email = HttpContext.Current.User.Identity.Name;

            var employee = db.Employees.FirstOrDefault(x => x.Email == email);

            if (employee != null)
            {
                model.SiteSearch = new List<DropDownDTO>();
                //if user is a manager then he can find all the businesses, Employee of there own business and can get all the external users 
                if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", employee.BusinessLocation.Id.ToString()))
                {
                    //Get the exernal users which are registered for get the ExternalBroadCast Shift.
                    model.SiteSearch = db.UserProfiles.Where(x => x.Email != email && x.IsRegisteredExternal == true).Select(a => new DropDownDTO
                    {
                        Text = a.FirstName.ToLower() + " " + a.LastName.ToLower(),
                        ValueGuid = a.Id
                    }).ToList();
                }

                //if user is an employee then he can find his co-employee and all the business
                var BusinessLocationIds = (from e in db.Employees
                                           where e.Email == email
                                           select e.BusinessLocation.Id).ToList();
                foreach (var businesslocationid in BusinessLocationIds)
                {
                    var emps = db.Employees.Where(a => a.BusinessLocation.Id == businesslocationid && a.Email != email && a.UserProfile.Id != null).Select(y => new DropDownDTO { Text = y.FirstName.ToLower() + " " + y.LastName.ToLower(), ValueGuid = y.Id }).ToList();
                    foreach (var emp in emps)
                    {
                        model.SiteSearch.Add(emp);
                    }
                }

                //get all businesses 
                var business = db.Businesses.Select(a => new DropDownDTO { Text = a.Name.ToLower(), ValueGuid = a.Id }).ToList();
                foreach (var item in business)
                {
                    model.SiteSearch.Add(item);
                }
            }
            //if user is a Externaluser then he can only find the all Businesses
            else
            {
                model.SiteSearch = db.Businesses.Select(a => new DropDownDTO
                {
                    Text = a.Name.ToLower(),
                    ValueGuid = a.Id
                }).ToList();
            }
            return model;
        }

        [Authorize]
        [Route("api/SummaryAPI/IsBusiness")]
        [HttpGet]
        public bool IsBusiness(Guid id)
        {
            bool IsBusiness = db.Businesses.Any(a => a.Id == id);

            return IsBusiness;
        }
    }
}