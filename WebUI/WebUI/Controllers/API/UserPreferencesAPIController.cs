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
    public class UserPreferencesAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        //Gets a list of email addresses for all users linked to a given business location with a preference
        internal List<UserPreferencesDTO> GetUserPrefsForBusLocation(Guid businessLocationId)
        {
            var busLocEmployeesPrefs =   db.Employees.Where(emp => emp.BusinessLocation.Id == businessLocationId
                && emp.UserProfile != null).Select(a => a.UserProfile.UserPreferences).ToList();

            return MapperFacade.MapperConfiguration.Map<List<UserPreferences>, List<UserPreferencesDTO>>(busLocEmployeesPrefs);
        }

        // GET api/UserPreferencesAPI
        [Authorize]
        public UserPreferencesDTO GetPreferencesForCurrentUser()
        {
            var email = HttpContext.Current.User.Identity.Name;

            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return MapperFacade.MapperConfiguration.Map<UserPreferences, UserPreferencesDTO>(userProfile.UserPreferences);
        }

        // GET api/UserPreferencesAPI/
        [Authorize]
        [Route("api/UserPreferencesAPI/{email}")]
        internal UserPreferencesDTO GetPreferencesForUser(string email)
        {
            UserProfile userProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email);
            if (userProfile == null)
            {
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }

            return MapperFacade.MapperConfiguration.Map<UserPreferences, UserPreferencesDTO>(userProfile.UserPreferences);
        }
    
            // Put api/BusinessPreferencesAPI
            // Update method
        [Authorize]
        public HttpResponseMessage Put(Guid id, UserPreferencesDTO usrPrefDTO)
        {
            var email = HttpContext.Current.User.Identity.Name;
            UserProfile usrProfile = db.UserProfiles.FirstOrDefault(usr => usr.Email == email && usr.Id == id);

            if (usrProfile != null)
            {
                if (!ModelState.IsValid)
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                 try
                {
                     var userPreferences = db.UserPreferences.Find(usrPrefDTO.Id);

                     //If NULL, then retain any existing image data as no overrite is occurring (we don't pass back the whole image each time)
                     //This is a bit of a hack, best solution is to seperate out any uploaded documents as per this page (http://www.codeproject.com/Articles/658522/Storing-Images-in-SQL-Server-using-EF-and-ASP-NET)
                     if(usrPrefDTO.ImageData == null)
                     {
                         usrPrefDTO.ImageData = userPreferences.ImageData;
                         usrPrefDTO.ImageType = userPreferences.ImageType;
                     }

                     userPreferences = MapperFacade.MapperConfiguration.Map<UserPreferencesDTO, UserPreferences>(usrPrefDTO, userPreferences);

                    db.Entry(userPreferences).State = EntityState.Modified;

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