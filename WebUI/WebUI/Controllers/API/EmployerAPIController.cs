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
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.WebApi;
using WebUI.Common;
using System.Collections;
using System.Device.Location;

namespace WebUI.Controllers.API
{
    public class EmployerAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        [Route("api/EmployerAPI/GetEmployeeRequests")]
        public IEnumerable<EmployeeRequestDTO> GetEmployeeRequestsList()
        {
            try
            {
                var email = HttpContext.Current.User.Identity.Name;

                return GetEmployeeRequests(email);

            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        private IEnumerable<EmployeeRequestDTO> GetEmployeeRequests(string email)
        {
            var userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);

            //Get list of any outstanding employee requests from businesses for logged in user email address
            var employeeRequestList = from empRequest in db.EmployeeRequests
                                      join busLoc in db.BusinessLocations on empRequest.BusinessLocation.Id equals busLoc.Id
                                      join emp in db.Employees on empRequest.Employee.Id equals emp.Id
                                      where ( //To find requests, need to match either the email address or the phone number with the registered user that is logged in.
                                                (!string.IsNullOrEmpty(emp.Email) && emp.Email == email)
                                                || 
                                                (!string.IsNullOrEmpty(emp.MobilePhone) && emp.MobilePhone == userProfile.MobilePhone)
                                             ) 
                                      && emp.UserProfile == null
                                      && empRequest.Status == RequestStatus.Pending  //Get any pending employee requets which match the logged in user email address
                                      select empRequest;

            return MapperFacade.MapperConfiguration.Map<IEnumerable<EmployeeRequest>, IEnumerable<EmployeeRequestDTO>>(employeeRequestList).ToList();
        }

        // GET api/EmployerAPI
        [Authorize]
        public EmployerSummaryDTO GetEmployerSummary()
        {
            EmployerSummaryDTO empSummary = new EmployerSummaryDTO();
            var email = HttpContext.Current.User.Identity.Name;

            var busSummaryList = from busLoc in db.BusinessLocations
                                 join emp in db.Employees on busLoc.Id equals emp.BusinessLocation.Id
                                 join usr in db.UserProfiles on emp.UserProfile.Id equals usr.Id
                                 where usr.Email == email
                                 select new BusinessLocationSummaryDTO { BusinessId = busLoc.Business.Id, BusinessName = busLoc.Business.Name, Id = busLoc.Id, Name = busLoc.Name, HasInternalLocations = busLoc.Business.HasMultiInternalLocations };

            empSummary.Employers = busSummaryList.Distinct().ToList();

            empSummary.EmployeeRequests = GetEmployeeRequests(email).ToList();

            return empSummary;
        }

        [HttpPost]
        [Authorize]
        [ActionName("SearchBusinesses")]
        public EmployerSearchDTO SearchBusinesses(EmployerSearchDTO searchData)
        {
            searchData.SearchResults = null;
            var email = HttpContext.Current.User.Identity.Name;

            if (searchData.SearchType == EmployerSearchTypeDTO.Business_Location_Name)
            {
                //Get any matching business locations
                var busLocs = (from busLoc in db.BusinessLocations
                               where busLoc.Name.ToLower().Contains(searchData.Name.ToLower())
                               select new BusinessLocationSummaryDTO { BusinessId = busLoc.Business.Id, BusinessName = busLoc.Business.Name, Id = busLoc.Id, Name = busLoc.Name, HasInternalLocations = busLoc.Business.HasMultiInternalLocations }).ToList();
                //Also match any business names   
                var businesses = (from bus in db.Businesses
                                  join busLoc in db.BusinessLocations on bus.Id equals busLoc.Business.Id
                                  where bus.Name.ToLower().Contains(searchData.Name.ToLower())
                                  select new BusinessLocationSummaryDTO { BusinessId = busLoc.Business.Id, BusinessName = busLoc.Business.Name, Id = busLoc.Id, Name = busLoc.Name, HasInternalLocations = busLoc.Business.HasMultiInternalLocations }).ToList();

                businesses.AddRange(busLocs);

                searchData.SearchResults = businesses.Distinct().ToList();
            }
            else if (searchData.SearchType == EmployerSearchTypeDTO.Manager_Name)
            {
                searchData.SearchResults = (from busLoc in db.BusinessLocations
                                            join mgr in db.Employees on busLoc.Id equals mgr.BusinessLocation.Id
                                            where mgr.ManagerBusinessLocations.Contains(busLoc)
                                                && (
                                                        (mgr.FirstName.ToLower().Contains(searchData.Name.ToLower()) || mgr.LastName.ToLower().Contains(searchData.Name.ToLower())) //TODO - doe we need to join into User Profile table to search names? what is source of truth
                                                        || (searchData.Name.ToLower().Equals(mgr.FirstName.ToLower() + " " + mgr.LastName.ToLower()))
                                                    )
                                            select new BusinessLocationSummaryDTO { BusinessId = busLoc.Business.Id, BusinessName = busLoc.Business.Name, Id = busLoc.Id, Name = busLoc.Name, HasInternalLocations = busLoc.Business.HasMultiInternalLocations }).ToList();
            }

            //Now get summary of all Employer requests from the current user
            searchData.SearchRequests = (from er in db.EmployerRequests
                                         where er.UserProfile.Email == email
                                         select new EmployerRequestDTO { Id = er.Id, Status = (RequestStatusDTO)er.Status, Business_Id = er.BusinessLocation.Business.Id, BusinessLocation_Id = er.BusinessLocation.Id, CreatedDate = er.CreatedDate }).ToList();

            searchData.CurrentBusinesses = (from busLoc in db.BusinessLocations
                                            join emp in db.Employees on busLoc.Id equals emp.BusinessLocation.Id
                                            join usr in db.UserProfiles on emp.UserProfile.Id equals usr.Id
                                            where usr.Email == email
                                            select busLoc.Id).ToList();
            return searchData;

        }


    }
}