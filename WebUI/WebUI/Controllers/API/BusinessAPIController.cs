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
using WebUI.Http;
using WebUI.Common;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class BusinessAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/BusinessAPI
        [Authorize(Roles = Constants.RoleSysAdmin)]
        public IEnumerable<BusinessDTO> GetBusinesses()
        {
           return MapperFacade.MapperConfiguration.Map<IEnumerable<Business>, IEnumerable<BusinessDTO>>(db.Businesses.AsEnumerable());
        }

        // GET api/BusinessAPI/5
        [Authorize]
        public BusinessDTO GetBusiness(Guid id)
        {
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", id.ToString()))
            {

                Business business = db.Businesses.Find(id);
                if (business == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }

                return MapperFacade.MapperConfiguration.Map<Business, BusinessDTO>(business);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        // PUT api/BusinessAPI/5
        //Update an existing record
        [Authorize]
        public HttpResponseMessage PutBusiness(Guid id, BusinessDetailsDTO businessDTO)
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", id.ToString()))
            {
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                if (id != businessDTO.Id)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);

                try
                {
                    var business = MapperFacade.MapperConfiguration.Map<BusinessDetailsDTO, Business>(businessDTO, db.Businesses.Single(b => b.Id == id));

                    //Lookup BusinessType and attach, so that no updates or inserts are performed on BusinessType lookup table
                    business.Type = db.BusinessTypes.Single(t => t.Id == businessDTO.TypeId);

                    ////Apply any role updates or inserts
                    //foreach (RoleDTO roleDTO in businessDTO.Roles)
                    //{
                    //    Role role = business.Roles.FirstOrDefault(r => r.Id == roleDTO.Id);
                    //    //If there is a role, then need to update
                    //    if (role != null)
                    //        role.Name = roleDTO.Name;
                    //    else //This is a new role being added
                    //    {
                    //        var newRole = MapperFacade.MapperConfiguration.Map<RoleDTO, Role>(roleDTO);
                    //        newRole.Id = Guid.NewGuid();
                    //        business.Roles.Add(newRole);
                    //    } //Note: deletion of roles is not supported here
                    //}

                    db.Entry(business).State = EntityState.Modified;

                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }
      
        // POST api/BusinessAPI
        // Create method
        [Authorize]
        public HttpResponseMessage PostBusiness(BusinessDetailsDTO businessDTO)
        {
            if (ModelState.IsValid)
            {

                //Check business with same name does not already exist:
                var existingBusiness = db.Businesses.Where(b=> b.Name == businessDTO.Name).FirstOrDefault();
                if (existingBusiness != null)
                   throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "A business already exists with the name '"+ businessDTO.Name+"'"));

                var business = MapperFacade.MapperConfiguration.Map<BusinessDetailsDTO, Business>(businessDTO);
              
                business.Id = Guid.NewGuid(); //Assign new ID on save.

                //Lookup BusinessType and attach, so that no updates or inserts are performed on BusinessType lookup table
                business.Type = db.BusinessTypes.Find(businessDTO.TypeId);

                //When creating new business if no business locations, must have one location by default
                if (business.BusinessLocations.Count == 0)
                {
                    var busLoc = new BusinessLocation
                    {
                        Id = Guid.NewGuid(),
                        Name = businessDTO.BusinessLocation.Name,
                        Business = business,
                        Address = new Address
                        {
                            Line1 = businessDTO.BusinessLocation.Address.Line1,
                            Line2 = businessDTO.BusinessLocation.Address.Line2,
                            Suburb = businessDTO.BusinessLocation.Address.Suburb,
                            Postcode = businessDTO.BusinessLocation.Address.Postcode,
                            State = businessDTO.BusinessLocation.Address.State,
                            PlaceLatitude = businessDTO.BusinessLocation.Address.Lat,
                            PlaceLongitude = businessDTO.BusinessLocation.Address.Long,
                            PlaceId = businessDTO.BusinessLocation.Address.PlaceId
                        },
                        Phone = businessDTO.BusinessLocation.Phone
                    };

                    foreach (var intLoc in businessDTO.BusinessLocation.InternalLocations)
                    {
                        if (!String.IsNullOrEmpty(intLoc.Name))
                        {
                            busLoc.InternalLocations.Add(new InternalLocation
                            {
                                Id = Guid.NewGuid(),
                                Name = intLoc.Name,
                                BusinessLocation = busLoc
                            });
                        }
                    }
                    business.BusinessLocations.Add(busLoc);


                    //Need to create a default preferences object
                    BusinessPreferences busPref = new BusinessPreferences { Id = Guid.NewGuid(), CancelShiftAllowed = true, CancelShiftTimeframe = 0, BusinessLocation = busLoc };
                    db.BusinessPreferences.Add(busPref);

                    //New business will also have default employee who created as a manager
                    var emp = new Employee();
                    var email = HttpContext.Current.User.Identity.Name;

                    emp.Id = Guid.NewGuid(); //Assign new ID on save.
                    emp.BusinessLocation = busLoc;
                    //Lookup user profile for authenicated user who is creating business
                    emp.UserProfile = db.UserProfiles.Single(usr => usr.Email == email);
                    emp.Email = emp.UserProfile.Email;
                    emp.FirstName = emp.UserProfile.FirstName;
                    emp.LastName = emp.UserProfile.LastName;
                    emp.MobilePhone = emp.UserProfile.MobilePhone;
                    emp.Type = EmployeeType.FullTime; //Default to Full time employee type
                    emp.IsAdmin = true;
                    emp.ManagerBusinessLocations.Add(busLoc);

                    db.Employees.Add(emp);
                }
                
                db.Businesses.Add(business);
                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, business.Id);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = business.Id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/BusinessAPI/5
        [Authorize(Roles = Constants.RoleSysAdmin)]
        public HttpResponseMessage DeleteBusiness(Guid id)
        {
            Business business = db.Businesses.Find(id);
            if (business == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            db.Businesses.Remove(business);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, MapperFacade.MapperConfiguration.Map<Business, BusinessDTO>(business));
        }

        #region Internal Location Methods
       
        // POST 
        [HttpPost]
        [Route("api/BusinessAPI/BusinessLocation/{businesslocationId}/InternalLocation/{internalLocationId}")]
        public HttpResponseMessage PostinternalLocation(Guid businesslocationId, Guid internalLocationId, InternalLocationDTO internalLocationDTO)
        {
            BusinessLocation businessLocation = db.BusinessLocations.Find(businesslocationId);
            if (businessLocation == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocation.Id.ToString()))
            {
                if (ModelState.IsValid)
                {
                    var location = MapperFacade.MapperConfiguration.Map<InternalLocationDTO, InternalLocation>(internalLocationDTO);
                    location.Id = Guid.NewGuid(); //Assign new ID on save.
                    location.Enabled = true;

                    //Lookup Business and attach, so that no updates or inserts are performed on BusinessType lookup table
                    location.BusinessLocation= db.BusinessLocations.SingleOrDefault(b => b.Id == businesslocationId);

                    db.InternalLocations.Add(location);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created);
                }
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        [HttpPut]
        [Route("api/BusinessAPI/BusinessLocation/{businesslocationId}/InternalLocation/{internalLocationId}")]
        public HttpResponseMessage PutinternalLocation(Guid businessLocationId, Guid internalLocationId, InternalLocationDTO internalLocationDTO)
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocationId.ToString()))
            {
            if (ModelState.IsValid)
            {
                var internalLocation = MapperFacade.MapperConfiguration.Map<InternalLocationDTO, InternalLocation>(internalLocationDTO, db.InternalLocations.Single(l => l.Id == internalLocationId));

                db.Entry(internalLocation).State = EntityState.Modified;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
             else
                 throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        #endregion Internal Location Methods
        
        #region Role Methods

        // PUT api/BusinessAPI/Business/{businessId}/Role/{roleId}
        public HttpResponseMessage PutBusinessRole(Guid businessId, Guid roleId, RoleDTO roleDTO)
        {
            //Ensure user has "Put" manager permissions for the business which the request corresponds to
            if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", businessId.ToString()))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

            if (ModelState.IsValid)
            {
                var role = MapperFacade.MapperConfiguration.Map<RoleDTO, Role>(roleDTO, db.Roles.Single(r => r.Id == roleId));

                db.Entry(role).State = EntityState.Modified;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // POST api/BusinessAPI/Business/{businessId}/Role/{roleId}
        public HttpResponseMessage PostBusinessRole(Guid businessId, Guid roleId, RoleDTO roleDTO)
        {
            //Ensure user has "Put" manager permissions for the business which the request corresponds to
            if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", businessId.ToString()))
                return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

            if (String.IsNullOrEmpty(roleDTO.Name))
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Role name cannot be empty");

            if (db.Roles.Where(r => r.Name == roleDTO.Name && r.Business.Id == businessId).Count() > 0)
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Role already exists");
            
            if (ModelState.IsValid)
            {
                var role = MapperFacade.MapperConfiguration.Map<RoleDTO, Role>(roleDTO);
                role.Id = Guid.NewGuid(); //Assign new ID on save.

                //Lookup Business and attach, so that no updates or inserts are performed on BusinessType lookup table
                role.Business = db.Businesses.Find(businessId);

                db.Roles.Add(role);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created);
                response.Headers.Location = new Uri(Url.Link("BusinessRole", new { businessId = businessId, roleId = role.Id }));
                return response;
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // DELETE api/BusinessAPI/BusinessId/5/RoleId/1
        //REmoved as roles can only be disabled as they could be referenced by past shifts
        //public HttpResponseMessage DeleteBusinessRole(Guid businessId, Guid roleId)
        //{
        //    //Ensure user has "Put" manager permissions for the business which the request corresponds to
        //    if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", businessId.ToString()))
        //        return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

        //    //TODO: Need to ensure role is not linked to any employees before deleting? Or remove link as part of delete.
        //    //Role role = db.Roles.Include( r=> r.Employee).SingleOrDefault(r => r.Id == roleId);
        //     Role role = db.Roles.Find(roleId);
        //    if (role == null)
        //        return Request.CreateResponse(HttpStatusCode.NotFound);

        //    db.Roles.Remove(role);
            
        //    ////REmove role from any employees who have this role
        //    //foreach (var employeeWithRole in role.Employee)//.Where(emp => emp.RolContains(at.ArtistTypeID)).ToList())
        //    //    employeeWithRole.Roles.Remove(role);
           
        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException ex)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK);
        //}

        [HttpPost]
        [ActionName("EnableRole")]
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public HttpResponseMessage EnableRole(Guid Id)
        {
            try
            {
                //Find the Role
                var role = db.Roles.Find(Id);

                if (role == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", role.Business.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

                role.Enabled = true;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        [ActionName("DisableRole")]
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public HttpResponseMessage DisableRole(Guid Id)
        {
            try
            {
                //Find the Role
                var role = db.Roles.Find(Id);

                if (role == null)
                    return Request.CreateResponse(HttpStatusCode.NotFound);

                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", role.Business.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

                role.Enabled = false;

                //Remove role from any staff
                foreach (var emp in role.Employee)
                    emp.Roles.Remove(role);

                // delete any Shift blocks for this role type
                db.ShiftBlocks.RemoveRange(role.ShiftBlock);

                //Disable any Shift Templates for this role type
                foreach (ShiftTemplate st in role.ShiftTemplates)
                    st.Enabled = false;
             
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        #endregion
        [HttpPost]
        [ActionName("EnableInternalLocation")]
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public HttpResponseMessage EnableInternalLocation(Guid Id)
        {
            try
            {
                //Find the internal location
                var internalLoc = db.InternalLocations.Find(Id);

                if (internalLoc == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending request.");

                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", internalLoc.BusinessLocation.Business.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

                internalLoc.Enabled = true;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [HttpPost]
        [ActionName("DisableInternalLocation")]
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public HttpResponseMessage DisableInternalLocation(Guid Id)
        {
            try
            {
                //Find the internal location
                var internalLoc = db.InternalLocations.Find(Id);

                if (internalLoc == null)
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Unable to find pending request.");

                //Ensure user has "Put" manager permissions for the business which the request corresponds to
                if (!ClaimsAuthorization.CheckAccess("Put", "BusinessId", internalLoc.BusinessLocation.Business.Id.ToString()))
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You do not have permissions.");

                internalLoc.Enabled = false;
                db.SaveChanges();

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        #region Businesss Location methods
        // GET api/<controller>
        [HttpGet]
        [Route("api/BusinessAPI/business/{businessid}/locations")]
        public IEnumerable<BusinessLocationDTO> GetBusinessLocations(Guid businessid)
        {
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", businessid.ToString()))
            {
                var businessLocations = db.BusinessLocations.Where(sb => sb.Business.Id == businessid);

                return MapperFacade.MapperConfiguration.Map<IEnumerable<BusinessLocation>, IEnumerable<BusinessLocationDTO>>(businessLocations.AsEnumerable());
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        // GET api/<controller>
        [HttpGet]
        [Route("api/BusinessAPI/business/{businessid}/managerbusinesslocations")]
        public IEnumerable<BusinessLocationDTO> GetManagerBusinessLocations(Guid businessid)
        {
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", businessid.ToString()))
            {
                var userProfile = db.UserProfiles.FirstOrDefault(Usr => Usr.Email == HttpContext.Current.User.Identity.Name);

                List<BusinessLocation> buslocs = new List<BusinessLocation>();

                //Add all business locations which the user is a location manager for
                foreach (Employee emp in userProfile.Employees)
                    foreach (BusinessLocation bloc in emp.ManagerBusinessLocations.Where(bl => bl.Business.Id == businessid))
                        buslocs.Add(bloc);

                return MapperFacade.MapperConfiguration.Map<IEnumerable<BusinessLocation>, IEnumerable<BusinessLocationDTO>>(buslocs.AsEnumerable());
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }


        // GET api/<controller>
        [HttpGet]
        [Route("api/BusinessAPI/businesslocation/{businesslocationid}")]
        public BusinessLocationDTO GetBusinessLocation(Guid businesslocationid)
        {
           
                BusinessLocation businessLocation= db.BusinessLocations.Find(businesslocationid);
                if (businessLocation == null)
                {
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                }

                if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", businessLocation.Business.Id.ToString()))
                {
                    return MapperFacade.MapperConfiguration.Map<BusinessLocation, BusinessLocationDTO>(businessLocation);
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

         [Route("api/BusinessAPI/businesslocation")]
        public HttpResponseMessage PostBusinessLocation(BusinessLocationDTO businessLocationDTO)
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", businessLocationDTO.BusinessId.ToString()))
            {
                if (ModelState.IsValid)
                {
                    //Ensure this is not a duplicate location name
                    var existingBusLoc = db.BusinessLocations.FirstOrDefault(bl => bl.Business.Id == businessLocationDTO.BusinessId && bl.Name == businessLocationDTO.Name);
                    if(existingBusLoc != null)
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "A business location with this name already exists."));

                    var buslocation = MapperFacade.MapperConfiguration.Map<BusinessLocationDTO, BusinessLocation>(businessLocationDTO);
                    buslocation.Id = Guid.NewGuid(); //Assign new ID on save.
                    buslocation.Business = db.Businesses.Find(businessLocationDTO.BusinessId);
                    buslocation.Enabled = true;

                    foreach (var intLoc in businessLocationDTO.InternalLocations)
                    {
                        if (!String.IsNullOrEmpty(intLoc.Name))
                        {
                            buslocation.InternalLocations.Add(new InternalLocation
                            {
                                Id = Guid.NewGuid(),
                                Name = intLoc.Name,
                                BusinessLocation = buslocation
                            });
                        }
                    }
                 
                    //Need to create a default preferences object
                    BusinessPreferences busPref = new BusinessPreferences { Id = Guid.NewGuid(), CancelShiftAllowed = true, CancelShiftTimeframe = 0, BusinessLocation = buslocation };
                    db.BusinessPreferences.Add(busPref);

                    //New business will also have default employee who created as a manager
                    var emp = new Employee();
                    var email = HttpContext.Current.User.Identity.Name;

                    emp.Id = Guid.NewGuid(); //Assign new ID on save.
                    emp.BusinessLocation = buslocation;
                    //Lookup user profile for authenicated user who is creating business
                    emp.UserProfile = db.UserProfiles.Single(usr => usr.Email == email);
                    emp.Email = emp.UserProfile.Email;
                    emp.FirstName = emp.UserProfile.FirstName;
                    emp.LastName = emp.UserProfile.LastName;
                    emp.MobilePhone = emp.UserProfile.MobilePhone;
                    emp.Type = EmployeeType.FullTime; //Default to Full time employee type
                    emp.IsAdmin = true;

                    emp.ManagerBusinessLocations.Add(buslocation);

                    db.Employees.Add(emp);

                    db.BusinessLocations.Add(buslocation);
                    db.SaveChanges();

                    return Request.CreateResponse(HttpStatusCode.Created, buslocation.Id);
                }
                else
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

         [HttpPut]
         [Route("api/BusinessAPI/businesslocation/{businessLocationId}")]
         public HttpResponseMessage PutBusinessLocation(Guid businessLocationId, BusinessLocationDTO businessLocationDTO)
         {
             if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocationDTO.Id.ToString()))
             {

                 if (ModelState.IsValid)
                 {
                     var busLocation = MapperFacade.MapperConfiguration.Map<BusinessLocationDTO, BusinessLocation>(businessLocationDTO, db.BusinessLocations.Single(l => l.Id == businessLocationId));

                    foreach (var intLocDTO in businessLocationDTO.InternalLocations)
                    {
                        if (!String.IsNullOrEmpty(intLocDTO.Name))
                        {
                            if (intLocDTO.Id != Guid.Empty)
                            {
                                var intLocation = MapperFacade.MapperConfiguration.Map<InternalLocationDTO, InternalLocation>(intLocDTO, db.InternalLocations.Find(intLocDTO.Id));
                                if(intLocation.Name != intLocDTO.Name)
                                    db.Entry(intLocation).State = EntityState.Modified;
                            }
                            else
                            {
                                busLocation.InternalLocations.Add(new InternalLocation
                                {
                                    Id = Guid.NewGuid(),
                                    Name = intLocDTO.Name,
                                    BusinessLocation = busLocation
                                });

                            }
                        }
                    }

                    db.Entry(busLocation).State = EntityState.Modified;
                     db.SaveChanges();

                     return Request.CreateResponse(HttpStatusCode.OK);
                 }
                 else
                     return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
             }
             else
                 throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
         }
        #endregion
    }
}