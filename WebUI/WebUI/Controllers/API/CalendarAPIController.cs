using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Thinktecture.IdentityModel.Authorization;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
   [Authorize]
    public class CalendarAPIController : ApiController
    {
       
       private DataModelDbContext db = new DataModelDbContext();

        // GET api/calendarapi
       public IEnumerable<CalendarEventDTO> GetEventsList()
       {
           var loggedInEmail = User.Identity.Name;
           if (loggedInEmail == null)
               throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User identity name is empty"));

           //Check to see if there is already a pending cancellation request
           var calendarEvents = db.CalendarEvents.Where(ce => ce.UserProfile.Email == loggedInEmail);

           return Mapper.Map<IEnumerable<CalendarEvent>, IEnumerable<CalendarEventDTO>>(calendarEvents.AsEnumerable());

       }

        // GET api/calendarapi/5
        public CalendarEventDTO Get(Guid id)
        {
            var calEvent = db.CalendarEvents.Find(id);
            if (calEvent == null)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Event with given ID does not exist"));

            var loggedInEmail = User.Identity.Name;
            if (calEvent.UserProfile.Email != loggedInEmail)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User unauthorised to access the requested calendar resource"));

            return Mapper.Map<CalendarEvent,CalendarEventDTO>(calEvent); 
        }

        // POST api/calendarapi
        public HttpResponseMessage Post([FromBody]CalendarEventDTO calendarEventDTO)
        {
            if (ModelState.IsValid
                && calendarEventDTO.StartTime >= DateTime.Now.AddHours(-1)  //Ensure entry is a valid start date and finish date
                && calendarEventDTO.FinishTime > calendarEventDTO.StartTime)
            {
                var calendarEvent = Mapper.Map<CalendarEventDTO, CalendarEvent>(calendarEventDTO);
                calendarEvent.Id = Guid.NewGuid(); //Assign new ID on save.

                //Lookup Business and attach, so that no updates or inserts are performed on BusinessType lookup table
                calendarEvent.UserProfile = db.UserProfiles.Single(u => u.Email == User.Identity.Name);
                if(calendarEvent.UserProfile == null)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Userprofile is null"));


                db.CalendarEvents.Add(calendarEvent);
                db.SaveChanges();

                calendarEventDTO = Mapper.Map<CalendarEvent, CalendarEventDTO>(calendarEvent);
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, calendarEventDTO);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = calendarEventDTO.Id }));
                return response;    
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // PUT api/calendarapi/5
        public HttpResponseMessage Put(Guid id, [FromBody]CalendarEventDTO calendarEventDTO)
        {
            if (ModelState.IsValid
                  && calendarEventDTO.FinishTime > calendarEventDTO.StartTime)  //Ensure entry is a valid start date and finish date
            {
                var calendarEvent = db.CalendarEvents.Find(calendarEventDTO.Id);

                if (calendarEvent == null)
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Unable to find calendar event with given id");

                if (calendarEvent.UserProfile.Email != User.Identity.Name)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are not authorised  to access this calendar event"));

                try
                {
                    var calendarEventMOD = Mapper.Map<CalendarEventDTO, CalendarEvent>(calendarEventDTO, calendarEvent);
                   
                    db.Entry(calendarEventMOD).State = EntityState.Modified;

                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        }

        // DELETE api/calendarapi/5
       public HttpResponseMessage Delete(Guid id)
        {
            var calendarEvent = db.CalendarEvents.Find(id);
            if (calendarEvent == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            if (calendarEvent.UserProfile.Email != User.Identity.Name)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are not authorised  to access this calendar event"));

            db.CalendarEvents.Remove(calendarEvent);

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
    }
}
