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

namespace WebUI.Controllers.API
{
    public class RequestAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();


        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public IEnumerable<EmployerRequestDTO> GetRequestList()
        {
            try
            {
                var email = HttpContext.Current.User.Identity.Name;
                
                //Get list of all pending requests for any businesses that the user is a manager of
                
                //First get list of businesses which user is a manager of
                var mgrBusLocIdList = from emp in db.Employees
                                   where emp.UserProfile.Email == email && emp.IsAdmin == true
                                   select emp.BusinessLocation.Id;

                //Get list of Employer Requests linked to busineses which user is a manager of
                var requestList = from er in db.EmployerRequests
                                  where er.Status == RequestStatus.Pending
                                      && mgrBusLocIdList.Contains(er.BusinessLocation.Id)
                                  select new EmployerRequestDTO { 
                                      Id = er.Id, 
                                      Business_Id = er.BusinessLocation.Business.Id,
                                      BusinessLocation_Id = er.BusinessLocation.Id, 
                                      CreatedDate = er.CreatedDate, 
                                      Business_Name = er.BusinessLocation.Business.Name,
                                      Location_Name = er.BusinessLocation.Name,
                                      Requester_Name = er.UserProfile.FirstName + " " + er.UserProfile.LastName
                                  };

                return requestList.AsEnumerable();
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPut]
        [ActionName("CreateEmployerRequest")]
        [Authorize]
        public HttpResponseMessage CreateEmployerRequest(string id)
        {
            try
            {
                var busLocIdGd = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                //Check that employee email address is not already registered to the business
                if (db.Employees.Any(emp => emp.Email == email && emp.BusinessLocation.Id == busLocIdGd))
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Email address already registered with business location");

                //Check that employee is not already linked to the business
                if (db.Employees.Any(emp => emp.UserProfile.Email == email && emp.BusinessLocation.Id == busLocIdGd))
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "User is already registered with business location");

                //Check that a request hasn't already been lodged with the business previously
                if (db.EmployerRequests.Any(er => er.UserProfile.Email == email && er.BusinessLocation.Id == busLocIdGd))
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Request has already been registered with business location");

                var empRequest = new EmployerRequest();

                empRequest.Id = Guid.NewGuid();
                empRequest.CreatedDate = WebUI.Common.Common.DateTimeNowLocal();
                empRequest.Status = RequestStatus.Pending;
                empRequest.BusinessLocation = db.BusinessLocations.SingleOrDefault(b => b.Id == busLocIdGd);
                empRequest.UserProfile = db.UserProfiles.SingleOrDefault(usr => usr.Email == email);

                db.EmployerRequests.Add(empRequest);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        [ActionName("ApproveEmployerRequest")]
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public HttpResponseMessage ApproveEmployerRequest(string id)
        {
            try
            {
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                //Create Employee under business
                var empRequest = db.EmployerRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);
                
                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", empRequest.BusinessLocation.Business.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");
                   
                if(empRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending request.");

                //Create new Employee object
                Employee employee = new Employee();
                employee.Id = Guid.NewGuid(); //Assign new ID on save.
                
                //Copy details from UserProfile
                employee.Email = empRequest.UserProfile.Email;
                employee.FirstName = empRequest.UserProfile.FirstName;
                employee.LastName = empRequest.UserProfile.LastName;
                employee.MobilePhone = empRequest.UserProfile.MobilePhone;
                employee.Type = EmployeeType.Casual; //Default to Casual employee type

                employee.UserProfile = empRequest.UserProfile;
                employee.BusinessLocation = empRequest.BusinessLocation;

                db.Employees.Add(employee);
                
                //Update Request object
                empRequest.Employee = employee;
                empRequest.Status = RequestStatus.Approved;
                empRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                //Attach the EMployee record which matches the logged in user profile and business id.
                empRequest.ActionedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == empRequest.BusinessLocation.Id);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        [ActionName("RejectEmployerRequest")]
        [Authorize]
        public HttpResponseMessage RejectEmployerRequest(string id)
        {
            try
            {
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                //Create Employee under business
                var empRequest = db.EmployerRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);
                
                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", empRequest.BusinessLocation.Business.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");
                   
                if (empRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending request.");

                //Update Request object
                empRequest.Status = RequestStatus.Denied;
                empRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                //Attach the Employee record which matches the logged in user profile and business id.
                empRequest.ActionedBy = db.UserProfiles.FirstOrDefault(usr => usr.Email == email).Employees.FirstOrDefault(emp => emp.BusinessLocation.Id == empRequest.BusinessLocation.Id);

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }


        [HttpPost]
        [ActionName("ApproveEmployeeRequest")]
        [Authorize]
        public HttpResponseMessage ApproveEmployeeRequest(string id)
        {
            try
            {
                //TODO check logged in user approving request is a manaer
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                var empRequest = db.EmployeeRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);
                var loggedInUserProfile = db.UserProfiles.FirstOrDefault(up => up.Email == email);

                if (loggedInUserProfile == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find user profile.");

                if (empRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending request.");

                //Ensure request is linked to Employee record with same email address as the logged in user
                if (!string.IsNullOrEmpty(empRequest.Employee.Email) && empRequest.Employee.Email != email )
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Incorrect email address linked to request, unable to approve.");

                //Ensure request is linked to Employee record with same mobile phone number as the logged in user but only validate if request does NOT have a email address
                if (string.IsNullOrEmpty(empRequest.Employee.Email) 
                    && !string.IsNullOrEmpty(empRequest.Employee.MobilePhone) && empRequest.Employee.MobilePhone != loggedInUserProfile.MobilePhone)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Incorrect mobile number linked to request, unable to approve.");


                //Link the existing Employee object to the logged in user profile with same email address
                empRequest.Employee.UserProfile = loggedInUserProfile;
                
                //Update Request object
                empRequest.ActionedBy = loggedInUserProfile;
                empRequest.Status = RequestStatus.Approved;
                empRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
               
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        [ActionName("RejectEmployeeRequest")]
        [Authorize]
        public HttpResponseMessage RejectEmployeeRequest(string id)
        {
            try
            {
                //TODO check logged in user approving request is a manaer
                var requestId = Guid.Parse(id);
                var email = HttpContext.Current.User.Identity.Name;

                var empRequest = db.EmployeeRequests.FirstOrDefault(er => er.Id == requestId && er.Status == RequestStatus.Pending);
                var loggedInUserProfile = db.UserProfiles.FirstOrDefault(up => up.Email == email);

                if (loggedInUserProfile == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find user profile.");

                if (empRequest == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending request.");

                //Ensure request is linked to Employee record with same email address as the logged in user
                if (empRequest.Employee.Email != email)
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Incorrect email address linked to request, unable to approve.");

                //Update Request object
                empRequest.Status = RequestStatus.Denied;
                empRequest.ActionedDate = WebUI.Common.Common.DateTimeNowLocal();
                //Attach the Employee record which matches the logged in user profile and business id.
                empRequest.ActionedBy = loggedInUserProfile;

                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

    }
}