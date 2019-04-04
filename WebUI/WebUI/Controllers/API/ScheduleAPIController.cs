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
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
   [Authorize]
    public class ScheduleAPIController : ApiController
    {
       
       private DataModelDbContext db = new DataModelDbContext();

       // GET api/calendarapi
       public IEnumerable<ScheduleDTO> GetScheduleList()
       {
           var loggedInEmail = User.Identity.Name;
           if (loggedInEmail == null)
               throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "User identity name is empty"));

           //Check to see if there is already a pending cancellation request
           var schedules = db.Schedules.Where(ce => ce.UserProfile.Email == loggedInEmail);

           return MapperFacade.MapperConfiguration.Map<IEnumerable<Schedule>, IEnumerable<ScheduleDTO>>(schedules.AsEnumerable());

       }

       // GET api/scheduleapi/5
       public ScheduleDTO GetSchedule(int id)
       {
           var schedule = db.Schedules.Find(id);
           if (schedule == null)
               throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Schedule with given ID does not exist"));

           var loggedInEmail = User.Identity.Name;
           if (schedule.UserProfile.Email != loggedInEmail)
               throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "User unauthorised to access the requested schedule resource"));

           return MapperFacade.MapperConfiguration.Map<Schedule, ScheduleDTO>(schedule);
       }

       // POST api/calendarapi
       public HttpResponseMessage Post([FromBody]ScheduleDTO scheduleDTO)
       {
           if (ModelState.IsValid)
           {
               var recurringCalendarEvent = MapperFacade.MapperConfiguration.Map<ScheduleDTO, Schedule>(scheduleDTO);
               //recurringCalendarEvent.Id = Guid.NewGuid(); //Assign new ID on save.

              //Ensure unsupported options are not created
               if (scheduleDTO.Frequency != 0 &&  scheduleDTO.Frequency != 1  && scheduleDTO.Frequency != 2)
                   throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Frequency not supported. Must be once off (0), Daily (1) or Weekly(2)"));

               //Lookup Business and attach, so that no updates or inserts are performed on BusinessType lookup table
               recurringCalendarEvent.UserProfile = db.UserProfiles.Single(u => u.Email == User.Identity.Name);
               if (recurringCalendarEvent.UserProfile == null)
                   throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Userprofile is null"));
               try
               {
                   db.Schedules.Add(recurringCalendarEvent);
                   db.SaveChanges();

               }
               catch (Exception ex)
               {
                   var mes = ex.Message;
               }
               return Request.CreateResponse(HttpStatusCode.Created);
           }
           else
               return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
       }

       // PUT api/calendarapi/5
       public HttpResponseMessage Put(int id, [FromBody]ScheduleDTO scheduleDTO)
       {
           if (ModelState.IsValid)
                 //&& scheduleDTO.EndDate > calendarEventDTO.StartTime)  //Ensure entry is a valid start date and finish date
           {
               var schedule = db.Schedules.Find(scheduleDTO.Id);

               if (schedule == null)
                   return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Unable to find schedule with given id");

               //Ensure only supported types created.
               if (scheduleDTO.Frequency != 0 && scheduleDTO.Frequency != 1 && scheduleDTO.Frequency != 2)
                   throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Frequency not supported. Must be once off (0), Daily (1) or Weekly(2)"));


               if (schedule.UserProfile.Email != User.Identity.Name)
                   throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are not authorised  to access this schedule"));

               try
               {
                   var scheduleMOD = MapperFacade.MapperConfiguration.Map<ScheduleDTO, Schedule>(scheduleDTO, schedule);

                  

                   db.Entry(scheduleMOD).State = EntityState.Modified;

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

       // DELETE api/scheduleapi/5
       public HttpResponseMessage Delete(int id)
       {
           var schedule = db.Schedules.Find(id);
           if (schedule == null)
               return Request.CreateResponse(HttpStatusCode.NotFound);

           if (schedule.UserProfile.Email != User.Identity.Name)
               throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "You are not authorised  to access this schedule event"));

           db.Schedules.Remove(schedule);

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
