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
    public class BusinessPreferencesAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/BusinessPreferencesAPI/5
        [Authorize]
        public BusinessPreferencesDTO GetPreferencesForBusinessLocation(Guid Id)
        {
            var busLoc = db.BusinessLocations.Find(Id);
            if (busLoc == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
           
            if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", busLoc.Business.Id.ToString()))
            {
                return MapperFacade.MapperConfiguration.Map<BusinessPreferences, BusinessPreferencesDTO>(busLoc.BusinessPreferences);
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }
          // POST api/BusinessAPI
        // Create method
        [Authorize]
        public HttpResponseMessage Post(BusinessPreferencesDTO businessPrefDTO)
        {
            if (ModelState.IsValid)
            {
                var businessPref = MapperFacade.MapperConfiguration.Map<BusinessPreferencesDTO, BusinessPreferences>(businessPrefDTO);
                businessPref.Id = Guid.NewGuid(); //Assign new ID on save.

                db.BusinessPreferences.Add(businessPref);
                db.SaveChanges();

                businessPrefDTO = MapperFacade.MapperConfiguration.Map<BusinessPreferences, BusinessPreferencesDTO>(businessPref);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, businessPrefDTO);
                //response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = businessDTO.Id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // Put api/BusinessPreferencesAPI
        // Update method
        [Authorize]
        public HttpResponseMessage Put(Guid id, BusinessPreferencesDTO businessPrefDTO)
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessPrefDTO.BusinessLocationId.ToString()))
            {
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);

                if (id != businessPrefDTO.Id)
                    return Request.CreateResponse(HttpStatusCode.BadRequest);

                try
                {
                    var businessPreferences = MapperFacade.MapperConfiguration.Map<BusinessPreferencesDTO, BusinessPreferences>(businessPrefDTO, db.BusinessPreferences.Find(businessPrefDTO.Id));

                    db.Entry(businessPreferences).State = EntityState.Modified;

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
    }

}