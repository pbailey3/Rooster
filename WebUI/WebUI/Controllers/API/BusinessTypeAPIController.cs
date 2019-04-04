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
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    public class BusinessTypeAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/BusinessTypeAPI
        public IEnumerable<BusinessTypeDTO> GetBusinessTypes()
        {
            return MapperFacade.MapperConfiguration.Map<IEnumerable<BusinessType>, IEnumerable<BusinessTypeDTO>>(db.BusinessTypes.AsEnumerable());
       
        }

        // GET api/BusinessTypeAPI/5
        public BusinessType GetBusinessType(int id)
        {
            BusinessType businesstype = db.BusinessTypes.Find(id);
            if (businesstype == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return businesstype;
        }

        // PUT api/BusinessTypeAPI/5
        public HttpResponseMessage PutBusinessType(int id, BusinessType businesstype)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != businesstype.Id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(businesstype).State = EntityState.Modified;

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

        // POST api/BusinessTypeAPI
        public HttpResponseMessage PostBusinessType(BusinessType businesstype)
        {
            if (ModelState.IsValid)
            {
                db.BusinessTypes.Add(businesstype);
                db.SaveChanges();

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, businesstype);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = businesstype.Id }));
                return response;
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        // DELETE api/BusinessTypeAPI/5
        public HttpResponseMessage DeleteBusinessType(int id)
        {
            BusinessType businesstype = db.BusinessTypes.Find(id);
            if (businesstype == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.BusinessTypes.Remove(businesstype);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, businesstype);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}