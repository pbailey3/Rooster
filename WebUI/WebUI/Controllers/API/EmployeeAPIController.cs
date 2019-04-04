using AutoMapper;
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
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;
using ZXing.QrCode;

namespace WebUI.Controllers.API
{
    public class EmployeeAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/EmployeeAPI
        [Authorize]
        public IEnumerable<EmployeeDTO> GetEmployees(Guid businessLocationid)
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocationid.ToString()))
            {
                //Get all active employees for a given businessid
                return MapperFacade.MapperConfiguration.Map<IEnumerable<Employee>, IEnumerable<EmployeeDTO>>(db.Employees.Where(e => e.BusinessLocation.Id == businessLocationid && e.IsActive).AsEnumerable());
            }
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have appropriate permission"));
        }

        [Authorize]
        // GET api/EmployeeAPI/5
        public EmployeeDTO GetEmployee(Guid id)
        {
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", employee.BusinessLocation.Id.ToString()))
            {

                return MapperFacade.MapperConfiguration.Map<Employee, EmployeeDTO>(employee);
            }
            else
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have appropriate permission"));
        }

        [Authorize]
        // PUT api/EmployeeAPI/5
        public HttpResponseMessage PutEmployee(Guid id, EmployeeDTO employeeDTO)
        {
            if (!ModelState.IsValid)
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

            if (id != employeeDTO.Id)
                return Request.CreateResponse(HttpStatusCode.BadRequest);

            if (employeeDTO.Roles != null && employeeDTO.Roles.Count > 5)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Maximimum of 5 roles only can be added"));
           
            try
            {
                bool sendEmailNotification = false;

                var employee = db.Employees.Find(id);

                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", employee.BusinessLocation.Id.ToString()))
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have appropriate permission"));

                //Do not allow modification of email addresses which are linked to userprofiles already
                if (employee.UserProfile != null && employeeDTO.Email != employee.Email)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "You can not modify email address for an employee which is linked to a registered user"));
                
                //If the email address is being updated then send notification to new address
                if (employee.Email != employeeDTO.Email && !String.IsNullOrEmpty(employeeDTO.Email))
                    sendEmailNotification = true;

                 //Map the updates across to original object
                employee = MapperFacade.MapperConfiguration.Map<EmployeeDTO, Employee>(employeeDTO, employee);

                //Check to see if there is already an employee with this same email address registered for the business location.
                var employeeExistingEmail = db.Employees.FirstOrDefault(emp => !String.IsNullOrEmpty(employeeDTO.Email) && emp.Email == employee.Email && emp.Id != employee.Id && emp.BusinessLocation.Id == employee.BusinessLocation.Id);
                if (employeeExistingEmail != null)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "An employee with this email address already exists for this business"));
                
              
                //Lookup BusinessLocation and attach, so that no updates or inserts are performed on BusinessType lookup table
                var busLoc = db.BusinessLocations.Single(b => b.Id == employeeDTO.BusinessLocationId);
                employee.BusinessLocation = busLoc;

                if(employee.IsAdmin)
                    employee.ManagerBusinessLocations.Add(busLoc);
                else
                {
                    //if not admin but currently linked 
                    if (employee.ManagerBusinessLocations.Contains(busLoc))
                        employee.ManagerBusinessLocations.Remove(busLoc);
                }
                //Hook up any roles selected
                employee.Roles.Clear(); //Clear first so we can hook up correct DB items
                if (employeeDTO.Roles != null)
                {
                    foreach (RoleDTO roleDTO in employeeDTO.Roles)
                    {
                        Role role = db.Roles.FirstOrDefault(r => r.Id == roleDTO.Id);
                        if (role == null)
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        employee.Roles.Add(role);
                    }
                }

                //If user email has been updated then send notification and request
                if (sendEmailNotification)
                {
                    //Remove any old employee requests
                    if (employee.EmployeeRequest !=null)
                        db.Entry(employee.EmployeeRequest).State = EntityState.Deleted;

                    //Create an entry in the EMployeeRequest table
                    EmployeeRequest empRequest = new EmployeeRequest()
                    {
                        Id = Guid.NewGuid(),
                        CreatedDate = WebUI.Common.Common.DateTimeNowLocal(),
                        Status = RequestStatus.Pending,
                        BusinessLocation = employee.BusinessLocation
                    };
                    employee.EmployeeRequest = empRequest;

                    //If an existing user profile exists with matching email address
                    if (db.UserProfiles.Any(usr => usr.Email == employee.Email))
                    {
                        //Send user email notifying them that a business has registered their email and they need to approve to link
                        MessagingService.BusinessRegisteredEmployee( employee.FirstName, employee.BusinessLocation.Business.Name, true, employee.Email, employee.MobilePhone);
                    }
                    else //Send email notifying them that they have been registered with a business and invite to register an account
                        MessagingService.BusinessRegisteredEmployee( employee.FirstName, employee.BusinessLocation.Business.Name, false, employee.Email, employee.MobilePhone);
                }
               
                db.Entry(employee).State = EntityState.Modified;

                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }
        internal EmployeeDTO CreateNewEmployee(EmployeeDTO employeeDTO, bool insertNewRoles = false)
        {
            if (!ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", employeeDTO.BusinessLocationId.ToString()))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have appropriate permission"));

            if (employeeDTO.Roles != null && employeeDTO.Roles.Count > 5)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Maximimum of 5 roles only can be added"));

            //Check to see if there is already an employee with this same email address registered for the business location.
            var employeeExisting = db.Employees.FirstOrDefault(emp => !String.IsNullOrEmpty(employeeDTO.Email) && emp.Email == employeeDTO.Email && emp.BusinessLocation.Id == employeeDTO.BusinessLocationId);
            if (employeeExisting != null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "An employee with this email address already exists for this business"));

            //Check to see if there is already an employee with this same mobile phone registered for the business location.
            var employeeMobileExisting = db.Employees.FirstOrDefault(emp => !String.IsNullOrEmpty(employeeDTO.MobilePhone) && emp.MobilePhone == employeeDTO.MobilePhone && emp.BusinessLocation.Id == employeeDTO.BusinessLocationId);
            if (employeeMobileExisting != null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "An employee with this Mobile Phone already exists for this business"));

            var employee = MapperFacade.MapperConfiguration.Map<EmployeeDTO, Employee>(employeeDTO);
            employee.Id = Guid.NewGuid(); //Assign new ID on save.

            //Create QR code for the employee
            System.Drawing.Bitmap qrCodeImage = WebUI.Common.Common.GenerateQR(employee.Id.ToString());

            using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
            {
                qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                employee.QRCode = memory.ToArray();
            }

            //Lookup Business and attach, so that no updates or inserts are performed on BusinessType lookup table
            var busLoc = db.BusinessLocations.Single(b => b.Id == employeeDTO.BusinessLocationId);
            employee.BusinessLocation = busLoc;

            if (employee.IsAdmin)
                employee.ManagerBusinessLocations.Add(busLoc);


            //Hook up any roles selected
            employee.Roles.Clear(); //Clear first so we can hook up correct DB items
            if (employeeDTO.Roles != null)
            {
                foreach (RoleDTO roleDTO in employeeDTO.Roles)
                {
                    //Find the role by specific ID or else by matching businessId AND name
                    Role role = db.Roles.FirstOrDefault(r => r.Id == roleDTO.Id || (r.Business.Id == employeeDTO.BusinessId && r.Name == roleDTO.Name));
                    if (role == null)
                    {
                        if(!insertNewRoles)
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Role does not exist"));
                        else
                        {
                            role = new Role
                            {
                                Id = Guid.NewGuid(), //Assign new ID on save.
                                Name = roleDTO.Name,
                                Enabled = true
                            };

                            //Lookup Business and attach, so that no updates or inserts are performed on BusinessType lookup table
                            role.Business = db.Businesses.Find(employeeDTO.BusinessId);
                            db.Roles.Add(role);
                          //  employee.Roles.Add(role);
                        }
                    }
                    employee.Roles.Add(role);
                }
            }

            //If user email or mobile phone is entered then set up an employee request
            if (!String.IsNullOrEmpty(employee.Email) || !String.IsNullOrEmpty(employee.MobilePhone))
            {
                //Create an entry in the EMployeeRequest table
                EmployeeRequest empRequest = new EmployeeRequest()
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = WebUI.Common.Common.DateTimeNowLocal(),
                    Status = RequestStatus.Pending,
                    BusinessLocation = employee.BusinessLocation
                };
                employee.EmployeeRequest = empRequest;

                var existingUserProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == employee.Email || usr.MobilePhone == employee.MobilePhone);

                //If an existing user profile exists with matching email address or a matching mobile phone number
                if (existingUserProfile != null)
                {
                    //Send user email notifying them that a business has registered their email and they need to approve to link
                    MessagingService.BusinessRegisteredEmployee( employee.FirstName, employee.BusinessLocation.Business.Name, true, existingUserProfile.Email, employee.MobilePhone);
                }
                else //Send email notifying them that they have been registered with a business and invite to register an account
                    MessagingService.BusinessRegisteredEmployee( employee.FirstName, employee.BusinessLocation.Business.Name, false, employee.Email, employee.MobilePhone);
            }

            db.Employees.Add(employee);
            db.SaveChanges();

            employeeDTO = MapperFacade.MapperConfiguration.Map<Employee, EmployeeDTO>(employee);

            return employeeDTO;
        }

        // POST api/EmployeeAPI
        public HttpResponseMessage PostEmployee(EmployeeDTO employeeDTO)
        {
            if (ModelState.IsValid)
            {
                employeeDTO = this.CreateNewEmployee(employeeDTO);

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, employeeDTO);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = employeeDTO.Id }));
                return response;
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        // DELETE api/EmployeeAPI/5
        public HttpResponseMessage DeleteEmployee(Guid id)
        {
            Employee employee = db.Employees.Find(id);
            if (employee == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", employee.BusinessLocation.Id.ToString()))
            {

                employee.IsActive = false;

                //If there is a user profile login attached, then remove this so they can no longer access the business information
                if (employee.UserProfile != null)
                    employee.UserProfile.Employees.Remove(employee);

                //When removing and employee set any of their currently assigned shifts to be Open shifts for shifts after NOW
                DateTime dtNow = WebUI.Common.Common.DateTimeNowLocal();
                var shifts = db.Shifts.Where(s => s.StartTime > dtNow && s.Employee.Id == employee.Id);
                foreach(var shift in shifts)
                {
                    shift.Employee = null;
                    db.Entry(shift).State = EntityState.Modified;
                }

                var shiftTemplates = db.ShiftTemplates.Where(st => st.Employee.Id == employee.Id);
                foreach (var shiftTemplate in shiftTemplates)
                {
                    shiftTemplate.Enabled = false;
                    db.Entry(shiftTemplate).State = EntityState.Modified;
                }
              
                db.Entry(employee).State = EntityState.Modified;

                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }
                catch (Exception ex)
                {
                    var sds = ex.Message;

                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));

        }

        [HttpGet]
        [Route("api/EmployeeAPI/GetCurrentEmployee/businesslocationid/{businesslocationid}")]
        [Authorize]
        public EmployeeDTO GetCurrentEmployee(Guid businesslocationid)
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            Employee employee = userProfile.Employees.FirstOrDefault(e => e.BusinessLocation.Id == businesslocationid);
            if (employee == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return MapperFacade.MapperConfiguration.Map<Employee, EmployeeDTO>(employee);
        }

        #region Role Methods

        // POST api/EmployeeAPI/Employee/5/Role/4
        public HttpResponseMessage PostEmployeeRole(Guid employeeId, Guid roleId)
        {
            Employee employee = db.Employees.Find(employeeId);
            if (employee == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

           // employee.Roles.Add(new Role { Id = roleId });
            Role role = db.Roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            employee.Roles.Add(role);

            db.Entry(employee).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // DELETE api/EmployeeAPI/Employee/5/Role/4
        public HttpResponseMessage DeleteEmployeeRole(Guid employeeId, Guid roleId)
        {
            Employee employee = db.Employees.Find(employeeId);
            if (employee == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            Role role = employee.Roles.FirstOrDefault(r => r.Id == roleId);
            if (role == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            employee.Roles.Remove(role);

            db.Entry(employee).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}