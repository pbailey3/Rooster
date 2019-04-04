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
using FeatureToggle;

namespace WebUI.Controllers.API
{
    [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
    public class ShiftBlockAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/<controller>
        [HttpGet]
        [Route("api/ShiftBlockAPI/businesslocation/{businesslocationid}/shiftblocks")]
        public IEnumerable<ShiftBlockDTO> GetShiftBlocks(Guid businesslocationid)
        {

            if (Is<ShiftBlockFeature>.Enabled)
            {
                var busLocation = db.BusinessLocations.Find(businesslocationid);
                if (busLocation != null)
                {
                    if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", busLocation.Business.Id.ToString()))
                    {
                        var shiftBlockList = db.ShiftBlocks.Where(sb => sb.BusinessLocation.Id == businesslocationid);

                        return MapperFacade.MapperConfiguration.Map<IEnumerable<ShiftBlock>, IEnumerable<ShiftBlockDTO>>(shiftBlockList.AsEnumerable());
                    }
                    else
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }

        // GET api/<controller>/5
        public ShiftBlockDTO Get(Guid id)
        {
            if (Is<ShiftBlockFeature>.Enabled)
            {
                var shiftBlock = db.ShiftBlocks.Find(id);
                if (shiftBlock == null)
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
                if (ClaimsAuthorization.CheckAccess("Get", "BusinessId", shiftBlock.BusinessLocation.Business.Id.ToString()))
                    return MapperFacade.MapperConfiguration.Map<ShiftBlock, ShiftBlockDTO>(shiftBlock);
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }

        // POST api/<controller>
        public void Post([FromBody]ShiftBlockDTO shiftBlockDTO)
        {
            if (Is<ShiftBlockFeature>.Enabled)
            {
                if (ModelState.IsValid)
                {
                    if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", shiftBlockDTO.BusinessLocationId.ToString()))
                    {
                        //Business rules
                        if (shiftBlockDTO.StartTime > shiftBlockDTO.FinishTime && !shiftBlockDTO.FinishNextDay)
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "ShiftBlock start time must be before end time"));

                        var shiftBlock = MapperFacade.MapperConfiguration.Map<ShiftBlockDTO, ShiftBlock>(shiftBlockDTO);
                        shiftBlock.Id = Guid.NewGuid(); //Assign new ID on save.

                        shiftBlock.BusinessLocation = db.BusinessLocations.Find(shiftBlockDTO.BusinessLocationId);
                        shiftBlock.Role = db.Roles.Find(shiftBlockDTO.RoleId);

                        db.ShiftBlocks.Add(shiftBlock);
                        db.SaveChanges();
                    }
                    else
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                else
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }

        // PUT api/<controller>/5
        public void Put(Guid id, [FromBody]ShiftBlockDTO shiftBlockDTO)
        {
            if (Is<ShiftBlockFeature>.Enabled)
            {
                if (ModelState.IsValid)
                {
                    if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", shiftBlockDTO.BusinessLocationId.ToString()))
                    {
                        var shiftBlock = MapperFacade.MapperConfiguration.Map<ShiftBlockDTO, ShiftBlock>(shiftBlockDTO, db.ShiftBlocks.Single(st => st.Id == id));

                        //Business rules
                        if (shiftBlockDTO.StartTime > shiftBlockDTO.FinishTime && !shiftBlockDTO.FinishNextDay)
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "ShiftBlock start time must be before end time"));

                        shiftBlock.Role = db.Roles.Find(shiftBlockDTO.RoleId);

                        db.Entry(shiftBlock).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                else
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }

        // DELETE api/<controller>/5
        public void Delete(Guid id)
        {
            if (Is<ShiftBlockFeature>.Enabled)
            {
                ShiftBlock shiftBlock = db.ShiftBlocks.Find(id);
                if (shiftBlock != null)
                {
                    if (ClaimsAuthorization.CheckAccess("Put", "BusinessId", shiftBlock.BusinessLocation.Business.Id.ToString()))
                    {
                        db.ShiftBlocks.Remove(shiftBlock);

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
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }
    }
}