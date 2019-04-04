using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class ShiftTemplateAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/shifttemplateapi/
        [HttpGet]
        [ActionName("ListForBusiness")]
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public IEnumerable<ShiftTemplateDTO> GetRecurringShifts(Guid businesslocationid)
        {
            var busLocation = db.BusinessLocations.Find(businesslocationid);
            if (busLocation != null)
            {
                if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", busLocation.Business.Id.ToString()))
                {
                    var shiftTemplateList = db.ShiftTemplates.Where(st => st.BusinessLocation.Id == businesslocationid && st.Enabled);

                    return MapperFacade.MapperConfiguration.Map<IEnumerable<ShiftTemplate>, IEnumerable<ShiftTemplateDTO>>(shiftTemplateList.AsEnumerable());
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
        }

        [HttpGet]
        [Route("api/ShiftTemplateAPI/ListForCurrentUser")]
        public IEnumerable<ShiftTemplateDTO> GetRecurringShiftsCurrentUser()
        {
            var userName = User.Identity.Name;

            if (String.IsNullOrEmpty(userName))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Username is empty"));

            var shiftTemplateList = db.ShiftTemplates.Where(st => st.Employee.UserProfile.Email == userName && st.Enabled);

            return MapperFacade.MapperConfiguration.Map<IEnumerable<ShiftTemplate>, IEnumerable<ShiftTemplateDTO>>(shiftTemplateList.AsEnumerable());
        }

        // GET api/shifttemplateapi/5
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public ShiftTemplateDTO Get(Guid id)
        {

            var shiftTemplate = db.ShiftTemplates.Find(id);
            if (shiftTemplate == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", shiftTemplate.BusinessLocation.Business.Id.ToString()))
                return MapperFacade.MapperConfiguration.Map<ShiftTemplate, ShiftTemplateDTO>(shiftTemplate);
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        // POST api/shifttemplateapi
        //Create method
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public HttpResponseMessage PostShiftTemplate(ShiftTemplateDTO shiftTemplateDTO)
        {
            if (ModelState.IsValid)
            {
                if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", shiftTemplateDTO.BusinessLocationId.ToString()))
                {
                    //Business rules
                    if (shiftTemplateDTO.StartTime > shiftTemplateDTO.FinishTime && !shiftTemplateDTO.FinishNextDay)
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Shift start time must be before end time");
                    //Role selected must be applicable to the Employee
                    if(shiftTemplateDTO.EmployeeId != null && shiftTemplateDTO.RoleId.HasValue && db.Employees.Find(shiftTemplateDTO.EmployeeId).Roles.FirstOrDefault(r => r.Id == shiftTemplateDTO.RoleId) == null)
                        return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Employee does not have the role specified");
                    
                    var shiftTemplate = MapperFacade.MapperConfiguration.Map<ShiftTemplateDTO, ShiftTemplate>(shiftTemplateDTO);
                     shiftTemplate.Id = Guid.NewGuid(); //Assign new ID on save.
                    
                    shiftTemplate.Enabled = true; //default to enabled

                    shiftTemplate.BusinessLocation = db.BusinessLocations.Find(shiftTemplateDTO.BusinessLocationId);
                    if(shiftTemplateDTO.InternalLocationId.HasValue)
                        shiftTemplate.InternalLocation = db.InternalLocations.Find(shiftTemplateDTO.InternalLocationId);
                    if (shiftTemplateDTO.RoleId.HasValue)
                        shiftTemplate.Role = db.Roles.Find(shiftTemplateDTO.RoleId);
                    if (shiftTemplateDTO.EmployeeId.HasValue)
                        shiftTemplate.Employee = db.Employees.Find(shiftTemplateDTO.EmployeeId);
                    
                    db.ShiftTemplates.Add(shiftTemplate);
                    db.SaveChanges();

                    shiftTemplateDTO = MapperFacade.MapperConfiguration.Map<ShiftTemplate, ShiftTemplateDTO>(shiftTemplate);
                    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, shiftTemplateDTO);
                    response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = shiftTemplateDTO.Id }));
                    return response;
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // PUT api/shifttemplateapi/5
        [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
        public void Put(Guid id, [FromBody]ShiftTemplateDTO shiftTemplateDTO)
        {
            if (ModelState.IsValid)
            {
                if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", shiftTemplateDTO.BusinessId.ToString()))
                {
                    var shiftTemplate = MapperFacade.MapperConfiguration.Map<ShiftTemplateDTO, ShiftTemplate>(shiftTemplateDTO, db.ShiftTemplates.Find(shiftTemplateDTO.Id));

                    //Business rules
                    if (shiftTemplateDTO.StartTime > shiftTemplateDTO.FinishTime && !shiftTemplateDTO.FinishNextDay)
                         throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Shift start time must be before end time"));
                    //Role selected must be applicable to the Employee
                    if (shiftTemplateDTO.EmployeeId != null && shiftTemplateDTO.RoleId.HasValue && db.Employees.Find(shiftTemplateDTO.EmployeeId).Roles.FirstOrDefault(r => r.Id == shiftTemplateDTO.RoleId) == null)
                        throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Employee does not have the role specified"));
                   

                    if (shiftTemplateDTO.InternalLocationId.HasValue)
                        shiftTemplate.InternalLocation = db.InternalLocations.Find(shiftTemplateDTO.InternalLocationId);
                    else
                        db.Entry(shiftTemplate).Reference(r => r.InternalLocation).CurrentValue = null;
                    if (shiftTemplateDTO.RoleId.HasValue)
                        shiftTemplate.Role = db.Roles.Find(shiftTemplateDTO.RoleId);
                    else
                        db.Entry(shiftTemplate).Reference(r => r.Role).CurrentValue = null;
                    if (shiftTemplateDTO.EmployeeId.HasValue)
                        shiftTemplate.Employee = db.Employees.Find(shiftTemplateDTO.EmployeeId);
                    else
                        db.Entry(shiftTemplate).Reference(r => r.Employee).CurrentValue = null;

                    db.Entry(shiftTemplate).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
        }

        // DELETE api/shifttemplateapi/5
        public void Delete(Guid id)
        {
            ShiftTemplate shiftTemplate = db.ShiftTemplates.Find(id);
            if (shiftTemplate != null)
            {
                if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", shiftTemplate.BusinessLocation.Id.ToString()))
                {
                    shiftTemplate.Enabled = false;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound, ex));
                    }
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
        }
    }
}
