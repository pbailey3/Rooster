using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
//using Thinktecture.IdentityModel.Authorization;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class EmployeeRosterAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/employeerosterapi?startDate=12312&endDate=1232132
        public IEnumerable<ShiftDTO> GetShiftsForEmployee(string startDate, string endDate)
        {
            DateTime sDate = DateTime.Parse(startDate);
            DateTime eDate = DateTime.Parse(endDate);
            var userName = User.Identity.Name;
            
            if(String.IsNullOrEmpty(userName))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Username is empty"));
            
            var shiftList = db.Shifts.Where(s => s.StartTime >= sDate
                                            && s.StartTime <= eDate
                                            && s.IsPublished == true
                                            && s.Employee.UserProfile.Email == userName);
            
            return MapperFacade.MapperConfiguration.Map<IEnumerable<Shift>, IEnumerable<ShiftDTO>>(shiftList.AsEnumerable());

        }
    }
}
