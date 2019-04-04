using AutoMapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
//using System.Data.Objects.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;


namespace WebUI.Controllers.API
{
    [Authorize]
    public class RosterBroadcastAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

       

        // POST api/RosterBroadcastapi
        //Create a new shift
        public HttpResponseMessage PutRosterBroadcast([FromBody]IEnumerable<RosterBroadcastDTO> rosterBroadcastListDTO)
        {
            //List to store all Employees who have had shifts broadcast
            List<EmployeeDTO> employees = new List<EmployeeDTO>();
            List<EmployeeDTO> employeesNoProfile = new List<EmployeeDTO>();
            List<ShiftDTO> openShifts = new List<ShiftDTO>();

            if(rosterBroadcastListDTO.Count() > 0)
            {
                Guid businessLocationId = db.Rosters.Find(rosterBroadcastListDTO.First().Id).BusinessLocation.Id;

                if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", businessLocationId.ToString()))
                {
                    //TODO: test allbusiness logic
                    foreach (RosterBroadcastDTO rosterBroadcastDTO in rosterBroadcastListDTO)
                    {
                        int dow = (int)rosterBroadcastDTO.Day + 1; // SQL Day of week

                        IQueryable<Shift> shiftsToBroadcast = null;
                        if (rosterBroadcastDTO.LocationId != Guid.Empty)
                        {
                            //Do the updates to broadcast all shifts on each day for each location specified 
                            shiftsToBroadcast = db.Shifts.Where(s => s.Roster.Id == rosterBroadcastDTO.Id
                                && s.InternalLocation.Id == rosterBroadcastDTO.LocationId
                                && SqlFunctions.DatePart("weekday", s.StartTime) == dow);
                        }
                        else
                        {
                            shiftsToBroadcast = db.Shifts.Where(s => s.Roster.Id == rosterBroadcastDTO.Id
                               && s.InternalLocation == null
                               && SqlFunctions.DatePart("weekday", s.StartTime) == dow);
                        }

                        foreach (Shift shift in shiftsToBroadcast)
                        {
                            shift.IsPublished = true;
                            //Add to list of employees who have shifts broadcast
                            if (shift.Employee != null) 
                            {
                                if (shift.Employee.UserProfile != null)
                                {
                                    //If email not already added to list to send
                                    if (employees.Where(em => em.Id == shift.Employee.Id).Count() == 0)
                                    {
                                        employees.Add(new EmployeeDTO
                                        {
                                            Id = shift.Employee.Id,
                                            FirstName = shift.Employee.UserProfile.FirstName,
                                            LastName = shift.Employee.UserProfile.LastName,
                                            Email = shift.Employee.UserProfile.Email
                                        });
                                    }
                                }
                                else
                                {
                                    //Employee does not have a linked userprofile, therefore try to use email is entered into employee by manager
                                    if(!String.IsNullOrEmpty(shift.Employee.Email)
                                        &&  (employeesNoProfile.Where(em => em.Id == shift.Employee.Id).Count() == 0))
                                    {
                                        employeesNoProfile.Add(new EmployeeDTO
                                        {
                                            Id = shift.Employee.Id,
                                            FirstName = shift.Employee.FirstName,
                                            LastName = shift.Employee.LastName,
                                            Email = shift.Employee.Email
                                        });
                                    }

                                }
                            }
                            

                            //If this is an OPEN shift
                            if (shift.Employee == null)
                                openShifts.Add(MapperFacade.MapperConfiguration.Map<Shift, ShiftDTO>(shift));
                        }
                    }
                    db.SaveChanges();

                    //Send notifications to any staff who have shifts being published.
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    foreach (var emp in employees)
                        MessagingService.ShiftBroadcast(emp.Email, emp.FirstName);
                    foreach (var emp in employeesNoProfile)
                        MessagingService.ShiftBroadcastNotRegistered(emp.Email, emp.FirstName);
                    sw.Stop();
                   
                    //Publish notification about open shifts
                    var busLocs = openShifts.Select(s => s.BusinessLocationId.Value).Distinct().ToList();
                    MessagingService.OpenShiftsBroadcast(busLocs);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        //Get a list of shifts that have not been assigned to any individual for the given Roster, location and day and are also not OPEN shifts (ie already published)
        [ActionName("GetUnassignedShifts")]
        //public IEnumerable<ShiftDTO> CheckUnassignedShifts([FromUri]Guid id, [FromBody]IEnumerable<RosterBroadcastDTO> shiftsToCheckForUnassigned)
        public IEnumerable<ShiftDTO> GetUnassignedShifts(Guid rosterId, Guid locationId, DayOfWeek day )
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", db.Rosters.Find(rosterId).BusinessLocation.Id.ToString()))
            {
                List<ShiftDTO> unassignedShifts = new List<ShiftDTO>();

                //if (shiftsToCheckForUnassigned.Count() > 0)
                //{
                    //foreach (RosterBroadcastDTO rosterBroadcastDTO in shiftsToCheckForUnassigned)
                    //{
                        //int dow = (int)rosterBroadcastDTO.Day + 1; // SQL Day of week
                        int dow = (int)day + 1; // SQL Day of week

                        //Find all shifts on given day for location where there is no assigned employee
                        var unassignedShiftsForDay = MapperFacade.MapperConfiguration.Map<IEnumerable<Shift>, IEnumerable<ShiftDTO>>(db.Shifts.Where(s => s.Roster.Id == rosterId
                                                                        && s.InternalLocation.Id == locationId
                                                                        && s.Employee == null
                                                                        && s.IsPublished == false
                                                                        && SqlFunctions.DatePart("weekday", s.StartTime) == dow).AsEnumerable());

                        return unassignedShiftsForDay;
                       // unassignedShifts.AddRange(unassignedShiftsForDay);

                    //}
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

        //Get a list of shifts that have been assigned to the same employee and which overlap and are not already published
        [ActionName("GetConflictingShifts")]
        public IEnumerable<ShiftDTO> GetConflictingShifts(Guid rosterId, DayOfWeek day)
        {
            if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", db.Rosters.Find(rosterId).BusinessLocation.Id.ToString()))
            {
                List<ShiftDTO> conflictingShifts = new List<ShiftDTO>();

                int dow = (int)day + 1; // SQL Day of week
                var conflicts = (from s1 in db.Shifts
                                 from s2 in db.Shifts
                                 where  s1.Id != s2.Id
                                 && s1.Employee == s2.Employee
                                 && s1.Roster.Id == rosterId
                                 && s2.Roster.Id == rosterId
                                 && SqlFunctions.DatePart("weekday", s1.StartTime) == dow
                                 && SqlFunctions.DatePart("weekday", s2.StartTime) == dow
                                 && s1.StartTime <= s2.FinishTime
                                 && s1.FinishTime >= s2.FinishTime
                                 && s1.IsPublished == false
                                 && s1.Employee != null
                                     select s1);
                var conflictingShiftsForDay = MapperFacade.MapperConfiguration.Map<IEnumerable<Shift>, IEnumerable<ShiftDTO>>(conflicts);

                return conflictingShiftsForDay;
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
        }

    }
}
