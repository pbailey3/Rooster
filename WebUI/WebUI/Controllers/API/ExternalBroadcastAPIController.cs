using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using WebUI.DTOs;
using WebUI.Models;
using System.Device.Location;
using WebUI.Common;
using WebUI.Http;
using System.Web.Script.Serialization;

namespace WebUI.Controllers.API
{
    [Authorize]
    public class ExternalBroadcastAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();
        // GET api/ExternalBroadcastAPI
        [Route("api/ExternalBroadcastAPI/ExternalJobPosting")]
        [HttpGet]
        public ExternalBroadcastDTO ExternalJobPosting()
        {
            ExternalBroadcastDTO getexternalBroadcast = new ExternalBroadcastDTO();
            getexternalBroadcast.Roles = db.Roles.Select(x => new DropDownDTO
            {
                ValueGuid = x.Id,
                Text = x.Name
            }).ToList();
            getexternalBroadcast.Employees = db.Employees.Select(x => new DropDownDTO
            {
                ValueGuid = x.Id,
                Text = x.FirstName + " " + x.LastName
            }).ToList();
            return getexternalBroadcast;
        }

        // POST api/ExternalBroadcastAPI
        [Route("api/ExternalBroadcastAPI/ExternalJobPosting")]
        [HttpPost]
        public ExternalBroadcastDTO ExternalJobPosting([FromBody]ExternalBroadcastDTO model)
        {
            var status = 0;


            ExternalShiftBroadcast externalBrodcaseShift = new ExternalShiftBroadcast();
            externalBrodcaseShift.Id = Guid.NewGuid();
            externalBrodcaseShift.Description = model.Description;
            externalBrodcaseShift.Wage = model.Wage;
            externalBrodcaseShift.Status = ExternalShiftStatus.Pending;

            db.ExternalShiftBroadcasts.Add(externalBrodcaseShift);
            status = db.SaveChanges();

            //check if any Qualificatoin is selected.
            if (model.Qualification != null)
            {
                foreach (var item in model.Qualification)
                {
                    ExternalShiftQualification shiftQualification = new ExternalShiftQualification();
                    shiftQualification.Id = Guid.NewGuid();
                    shiftQualification.QualificationslookupQualificationId = item.ValueGuid;
                    shiftQualification.ExternalShiftBroadcastId = externalBrodcaseShift.Id;
                    db.ExternalShiftQualifications.Add(shiftQualification);
                    db.SaveChanges();
                }
            }


            foreach (var item in model.BroadcastedOpenShiftList)
            {
                var shiftResult = db.Shifts.Where(a => a.Id == item.BroadcastedOpenShiftId).FirstOrDefault();
                shiftResult.ExternalShiftBroadcastId = externalBrodcaseShift.Id;
                db.SaveChanges();
            }

            //Check if any Skill is selected.
            if (model.Skills != null)
            {
                foreach (var item in model.Skills)
                {
                    ExternalShiftSkills skills = new ExternalShiftSkills();
                    skills.Id = Guid.NewGuid();
                    skills.ExternalShiftBroadcastId = externalBrodcaseShift.Id;
                    skills.IndustrySkillsId = item.ValueGuid;
                    db.ExternalShiftSkills.Add(skills);
                    db.SaveChanges();
                }
            }

            ExternalBroadcastDTO externalBroadCastDTO = MapperFacade.MapperConfiguration.Map<ExternalShiftBroadcast, ExternalBroadcastDTO>(externalBrodcaseShift);
            return externalBroadCastDTO;
        }

        // GET api/ExternalBroadcastAPI
        [Route("api/ExternalBroadcastAPI/ExternalShiftSetUp")]
        [HttpGet]
        public ExternalBroadcastDTO ExternalShiftSetUp(Guid ID)
        {
            ExternalBroadcastDTO getexternalBroadcast = new ExternalBroadcastDTO();
            getexternalBroadcast.Skills = db.IndustrySkills.Where(x => x.IndustryTypesID == db.Shifts.FirstOrDefault(shft => shft.Id == ID).Roster.BusinessLocation.Business.Type.IndustryType.ID).Select(m => new DropDownDTO { ValueGuid = m.Id, Text = m.Name }).ToList();

            getexternalBroadcast.BroadcastedOpenShiftList = db.Shifts.Where(a => a.Id == ID).ToList().Select(m => new BroadcastedOpenShiftsDTO { Finish = m.FinishTime, Start = m.StartTime, BroadcastedOpenShiftId = ID }).ToList();

            getexternalBroadcast.Qualification = db.Qualificationslookups.Where(x => x.IndustryTypesID == db.Shifts.FirstOrDefault(shft => shft.Id == ID).Roster.BusinessLocation.Business.Type.IndustryType.ID).Select(x => new DropDownDTO
            {
                ValueGuid = x.QualificationId,
                Text = x.QualificationName,

            }).ToList();


            return getexternalBroadcast;
        }


        [Route("api/ExternalBroadcastAPI/GetShifts")]
        [HttpGet]
        public List<BroadcastedOpenShiftsDTO> GetShifts([FromUri] Guid ID, Guid RosterID, bool isNewRow)
        {
            List<BroadcastedOpenShiftsDTO> shifts = new List<BroadcastedOpenShiftsDTO>();
            if (isNewRow == false)
            {
                shifts = db.Shifts.Where(a => a.Id != ID && a.Roster.Id == RosterID && a.IsPublished == true && a.Employee.Id == null && (a.ExternalShiftBroadcastId == null || a.ExternalShiftBroadcastId == Guid.Empty)).OrderByDescending(a => a.StartTime).Select(m => new BroadcastedOpenShiftsDTO { Finish = m.FinishTime, Start = m.StartTime, BroadcastedOpenShiftId = m.Id }).ToList();
            }
            else
            {
                shifts = db.Shifts.Where(a => a.Id == ID && a.Roster.Id == RosterID && a.IsPublished == true && a.Employee.Id == null && (a.ExternalShiftBroadcastId == null || a.ExternalShiftBroadcastId == Guid.Empty)).OrderByDescending(a => a.StartTime).Select(m => new BroadcastedOpenShiftsDTO { Finish = m.FinishTime, Start = m.StartTime, BroadcastedOpenShiftId = m.Id }).ToList();
            }
            return shifts;
        }

        [HttpGet]
        [Route("api/ExternalBroadcastAPI/GetEmployeeExternalShift")]
        public ExternalShiftEmployeeIndexDTO GetEmployeeExternalShift()
        {
            var email = HttpContext.Current.User.Identity.Name;
            ExternalShiftEmployeeIndexDTO result = new ExternalShiftEmployeeIndexDTO();
            var dtNow = WebUI.Common.Common.DateTimeNowLocal();

            result.ExternalShits = GetExternalShifts(email);

            var empExternalShiftRequests = from sr in db.ExternalShiftRequests
                                           where sr.CreatedBy.Email == email
                                           && sr.Type == ExternalShiftRequestType.TakeExternalShift
                                           select sr.ExternalShiftBroadcast.Id;

            result.ExternalShiftRequests = empExternalShiftRequests.AsEnumerable();
            return result;
        }


        private IEnumerable<ExternalBroadcastDTO> GetExternalShifts(string email)
        {
            List<ExternalBroadcastDTO> model = new List<ExternalBroadcastDTO>();
            var dtNow = WebUI.Common.Common.DateTimeNowLocal();

            //Getting user info from userProfile.
            var userinfo = db.UserProfiles.Where(a => a.Email == email && a.IsRegisteredExternal == true).FirstOrDefault();
            if (userinfo != null)
            {
                //Get user Qualifications
                var userQualifications = db.UserQualifications.Where(a => a.UserProfileId == userinfo.Id).ToList();

                List<Guid> userQuailificaions = new List<Guid>();
                List<Guid> userSkill = new List<Guid>();

                foreach (var item in userQualifications)
                    userQuailificaions.Add(item.QualificationslookupQualificationId);

                //Get user Skill
                var userSkills = db.UserSkills.Where(a => a.UserProfileId == userinfo.Id).ToList();
                foreach (var skill in userSkills)
                    userSkill.Add(skill.IndustrySkillsId);

                //Need to make status same as ShiftChangeRequestStatus
                var externalShifts = db.ExternalShiftBroadcasts.Where(a => a.Status == ExternalShiftStatus.Pending).ToList();

                foreach (var externalShift in externalShifts)
                {
                    //Get ExternalShiftBroadCast
                    List<Guid> externalShiftQuilificaion = new List<Guid>();
                    List<Guid> externalShiftSkills = new List<Guid>();
                    //Get ExternalShiftQualifications  
                    foreach (var item in externalShift.ShiftQualifications)
                    {
                        var ExternalShiftQualification = db.ExternalShiftQualifications.Where(a => a.Id == item.Id).FirstOrDefault();
                        externalShiftQuilificaion.Add(ExternalShiftQualification.QualificationslookupQualificationId);
                    }

                    //Get ExternalShiftSkills
                    foreach (var item in externalShift.ShiftSkills)
                    {
                        var externalShiftSkill = db.ExternalShiftSkills.Where(a => a.Id == item.Id).FirstOrDefault();
                        externalShiftSkills.Add(externalShiftSkill.IndustrySkillsId);
                    }

                    //Get Shifts which are externaly BroadCasted.
                    var shift = db.Shifts.Where(a => a.ExternalShiftBroadcastId == externalShift.Id && a.IsPublished == true && a.Employee == null && a.StartTime > dtNow).FirstOrDefault();

                    if (shift != null)
                    {


                        ShiftDTO objShift = MapperFacade.MapperConfiguration.Map<Shift, ShiftDTO>(shift);
                        //Get Business Location details 
                        var businessLocation = db.BusinessLocations.Where(a => a.Id == objShift.BusinessLocationId).FirstOrDefault();

                        //User Lat, Long 
                        double lat1 = Convert.ToDouble(userinfo.CurrentAddress.PlaceLatitude);
                        double long1 = Convert.ToDouble(userinfo.CurrentAddress.PlaceLongitude);

                        //Business Lat,Long
                        double lat2 = Convert.ToDouble(businessLocation.Address.PlaceLatitude);
                        double long2 = Convert.ToDouble(businessLocation.Address.PlaceLongitude);

                        //Get Distance between User and Businesslocation.
                        var oUserLocaction = new GeoCoordinate(lat1, long1);
                        var oBusinessLocation = new GeoCoordinate(lat2, long1);
                        double distance = oUserLocaction.GetDistanceTo(oBusinessLocation);
                        double DistanceInKM = distance / 1000;

                        if (DistanceInKM <= userinfo.Distance)
                        {
                            if (Check(userQuailificaions, externalShiftQuilificaion) && Check(userSkill, externalShiftSkills) == true)
                            {
                                var Externalshift = MapperFacade.MapperConfiguration.Map<ExternalShiftBroadcast, ExternalBroadcastDTO>(externalShift);
                                model.Add(Externalshift);
                            }
                        }
                    }
                }
            }
            return model;
        }

        private bool Check(List<Guid> Userlist, List<Guid> ExternalShfitlist)
        {
            //If nothing specified for external shift then return true.
            if (ExternalShfitlist.Count == 0)
                return true;
            else
            {
                bool ischecked = false;
                foreach (var item in ExternalShfitlist)
                {
                    if (Userlist.Contains(item))
                    {
                        ischecked = true;
                    }
                    else
                    {
                        return ischecked = false;
                    }
                }
                return ischecked;
            }
        }

        // GET api/ExternalBroadcastAPI
        [Route("api/ExternalBroadcastAPI/SearchExternal")]
        [HttpGet]
        //[Authorize(Roles = Constants.RoleBusinessLocationManager)]
        public ExternalUserProfileListDTO SearchExternal()
        {
            ExternalUserProfileListDTO externalUserProfile = new ExternalUserProfileListDTO();

            var email = HttpContext.Current.User.Identity.Name;
            externalUserProfile.ExternalUserProfile = db.UserProfiles.Where(usr => usr.IsRegisteredExternal == true && usr.Email != email).Select(a => new UserProfilesDTO
            {
                ExternalUserID = a.Id,
                AboutMe = a.AboutMe,
                Address = a.Address,
                ImageData = a.UserPreferences.ImageData,
                FirstName = a.FirstName,
                LastName = a.LastName,
                Email = a.Email
            }).ToList();

            return externalUserProfile;
        }

        public HttpResponseMessage AddAsEmployee(Guid ID, EmployeeDTO employeeDTO)
        {
            HttpResponseMessage message = new HttpResponseMessage();
            Guid SelectedBusinessID = employeeDTO.BusinessId;
            employeeDTO = new EmployeeDTO();
            employeeDTO = db.UserProfiles.Where(a => a.Id == ID).Select(usr => new EmployeeDTO
            {
                BusinessId = SelectedBusinessID,
                FirstName = usr.FirstName,
                LastName = usr.LastName,
                Email = usr.Email,
                DateOfBirth = usr.DateofBirth,
                MobilePhone = usr.MobilePhone,
                Type = EmployeeTypeDTO.External,
                IsAdmin = false,
                BusinessLocationId = db.Employees.Where(a => a.UserProfile.Email == HttpContext.Current.User.Identity.Name && a.BusinessLocation.Business.Id == SelectedBusinessID).FirstOrDefault().BusinessLocation.Id,
                IsActive = true,
            }).FirstOrDefault();

            using (EmployeeAPIController employeeApi = new EmployeeAPIController())
            {
                var employee = employeeApi.CreateNewEmployee(employeeDTO);
                if (employee != null)
                {
                    message = Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            return message;
        }

        [Route("api/ExternalBroadcastAPI/ContactExternalUser")]
        [HttpPost]
        public HttpResponseMessage ContactExternalUser(Guid ID, MessagesDTO MessageDTO)
        {
            try
            {
                Messages message = new Messages();
                message.Id = Guid.NewGuid();
                message.DateSent = MessageDTO.DateSent;
                message.Message = MessageDTO.Message;
                message.UserProfileMessageTo = db.UserProfiles.FirstOrDefault(a => a.Id == ID);
                message.UserProfilesMessageFrom = db.UserProfiles.FirstOrDefault(a => a.Email == HttpContext.Current.User.Identity.Name);
                db.Messages.Add(message);
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
            }
        }

        [Route("api/ExternalBroadcastAPI/GetOpenShifts")]
        [HttpGet]
        public OpenShiftDTO GetOpenShifts()
        {
            OpenShiftDTO model = new OpenShiftDTO();
            var email = HttpContext.Current.User.Identity.Name;


            //Get the user info.
            var users = db.UserProfiles.FirstOrDefault(k => k.Email == email && k.IsRegisteredExternal == true);
            if (users != null)
            {
                // check if the user is an employee.
                model.IsExternalandEmp = users.Employees.Any(a => a.Email == email && a.UserProfile.Id != null);
            }
            else
            {
                model.IsEmployee = db.Employees.Any(a => a.Email == email && a.UserProfile.Id != null);
            }

            // Temporary List of data for Distance Dropdown.
            List<DropDownDTO> DistanceListStatic = new List<DropDownDTO>();
            DistanceListStatic.Add(new DropDownDTO { Value = 1, Text = "1 Kms" });
            DistanceListStatic.Add(new DropDownDTO { Value = 5, Text = "5 Kms" });
            DistanceListStatic.Add(new DropDownDTO { Value = 10, Text = "10 Kms" });
            DistanceListStatic.Add(new DropDownDTO { Value = 20, Text = "20 Kms" });

            model.Distance = DistanceListStatic;

            //Filter by Choice dropdown..
            List<DropDownDTO> FilterByChoice = new List<DropDownDTO>();
            FilterByChoice.Add(new DropDownDTO { Value = 1, Text = "Distance" });
            FilterByChoice.Add(new DropDownDTO { Value = 2, Text = "Total Pay" });
            FilterByChoice.Add(new DropDownDTO { Value = 3, Text = "Hourly Rate" });

            model.Filter = FilterByChoice;

            return model;



            //return model;
        }


        [Route("api/ExternalBroadcastAPI/GetFilteredOpenShifts")]
        [HttpGet]
        public OpenShiftDTO GetFilteredOpenShifts(string JsonResult, string Distancce)
        {
            OpenShiftDTO model = new OpenShiftDTO();

            //deserialize json Object of Address
            OpenShiftDTO location = new OpenShiftDTO();
            location = new JavaScriptSerializer().Deserialize<OpenShiftDTO>(JsonResult);

            var email = HttpContext.Current.User.Identity.Name;
            var dtNow = WebUI.Common.Common.DateTimeNowLocal();
            //Get the user info.
            var user = db.UserProfiles.FirstOrDefault(a => a.Email == email);
            // check if the user is an employee.
            var emp = user.Employees.FirstOrDefault(a => a.UserProfile.Id != null);

            if (emp != null)
            {
                model.OpenShiftsEmployee = OpenShiftEmployee();

                model.OpenShiftList = (from shift in model.OpenShiftsEmployee.OpenShifts
                                       select new openShiftsList
                                       {
                                           Id = shift.Id,
                                           locationName = shift.BusinessLocationName,
                                           startTime = shift.StartTime,
                                           EndTime = shift.FinishTime,
                                           RoleName = shift.RoleName,
                                           date = shift.StartDay
                                       }).ToList();

                //Check if user is also an external user as well.
                var exteruser = emp.UserProfile.IsRegisteredExternal;
                if (exteruser == true)
                {
                    //Get Externalshifts
                    model.ExternalShiftBroadCastEmpoyee = ExternalShiftEpmloyee(location, Distancce);

                    foreach (var item in model.ExternalShiftBroadCastEmpoyee.ExternalShits)
                    {
                        var op = new openShiftsList();
                        op.Id = item.Id;
                        op.RoleName = item.Shifts.FirstOrDefault().RoleName;
                        op.locationName = item.BusinessLocationName;
                        op.startTime = item.Shifts.FirstOrDefault().StartTime;
                        op.EndTime = item.Shifts.FirstOrDefault().FinishTime;
                        //Getting total hours
                        TimeSpan time = op.EndTime.Subtract(op.startTime);
                        op.totalhourse = time.TotalHours;
                        //end
                        op.date = item.Shifts.FirstOrDefault().StartDay;
                        op.Description = item.Description;
                        op.rate = item.Wage;
                        //getting total pay
                        int totalhours = Convert.ToInt32(op.totalhourse);
                        op.totalPrice = op.rate * totalhours;
                        op.IsExternalShift = true;
                        op.Distance = Convert.ToInt32(item.Distance);
                        model.OpenShiftList.Add(op);
                    }
                }
            }
            else
            {
                //Get Externalshifts
                model.ExternalShiftBroadCastEmpoyee = ExternalShiftEpmloyee(location, Distancce);

                //add External shifts to list
                model.OpenShiftList = new List<openShiftsList>();

                foreach (var item in model.ExternalShiftBroadCastEmpoyee.ExternalShits)
                {
                    var op = new openShiftsList();
                    op.Id = item.Id;
                    op.RoleName = item.Shifts.FirstOrDefault().RoleName;
                    op.locationName = item.BusinessLocationName;
                    op.startTime = item.Shifts.FirstOrDefault().StartTime;
                    op.EndTime = item.Shifts.FirstOrDefault().FinishTime;
                    //Getting total hours
                    TimeSpan time = op.EndTime.Subtract(op.startTime);
                    op.totalhourse = time.TotalHours;
                    //end
                    op.date = item.Shifts.FirstOrDefault().StartDay;
                    op.Description = item.Description;
                    op.rate = item.Wage;
                    //getting total pay
                    int totalhours = Convert.ToInt32(op.totalhourse);
                    op.totalPrice = op.rate * totalhours;
                    op.IsExternalShift = true;
                    op.Distance = Convert.ToInt32(item.Distance);
                    model.OpenShiftList.Add(op);
                }
            }

            return model;
        }

        private OpenShiftsEmployeeIndexDTO OpenShiftEmployee()
        {
            var email = HttpContext.Current.User.Identity.Name;
            var dtNow = WebUI.Common.Common.DateTimeNowLocal();

            OpenShiftsEmployeeIndexDTO retVal = new OpenShiftsEmployeeIndexDTO();
            //Get list of businesses which user is an employee of
            var empBusIdList = from e in db.Employees
                               where e.UserProfile.Email == email
                               select e.BusinessLocation.Id;

            //Get list of open shifts Requests linked to busineses which user is an employee of

            var openShiftList = from s in db.Shifts
                                where s.IsPublished == true
                                && s.Employee == null
                                && s.StartTime > dtNow
                                && empBusIdList.Contains(s.Roster.BusinessLocation.Id)
                                select s;
            //Get list of open shift requests from current user
            var empOpenShiftRequests = from sr in db.ShiftChangeRequests
                                       where sr.CreatedBy.UserProfile.Email == email
                                       && sr.Type == ShiftRequestType.TakeOpenShift
                                       && sr.Shift.StartTime > dtNow
                                       select sr.Shift.Id;

            retVal.OpenShifts = MapperFacade.MapperConfiguration.Map<IEnumerable<Shift>, IEnumerable<ShiftDTO>>(openShiftList.AsEnumerable());
            retVal.OpenShiftRequests = empOpenShiftRequests.AsEnumerable();
            return retVal;
        }

        private ExternalShiftEmployeeIndexDTO ExternalShiftEpmloyee(OpenShiftDTO location, string distance)
        {
            var email = HttpContext.Current.User.Identity.Name;
            ExternalShiftEmployeeIndexDTO retVal = new ExternalShiftEmployeeIndexDTO();

            retVal.ExternalShits = GetExternalShiftsList(location, distance);

            var empExternalShiftRequests = from sr in db.ExternalShiftRequests
                                           where sr.CreatedBy.Email == email
                                           && sr.Type == ExternalShiftRequestType.TakeExternalShift
                                           select sr.ExternalShiftBroadcast.Id;

            retVal.ExternalShiftRequests = empExternalShiftRequests.AsEnumerable();
            return retVal;
        }

        private IEnumerable<ExternalBroadcastDTO> GetExternalShiftsList(OpenShiftDTO location, string distancekm)
        {
            var email = HttpContext.Current.User.Identity.Name;
            var dtNow = WebUI.Common.Common.DateTimeNowLocal();
            List<ExternalBroadcastDTO> ExternalBroadCastList = new List<ExternalBroadcastDTO>();
            //Getting user info from userProfile.
            var userinfo = db.UserProfiles.Where(a => a.Email == email && a.IsRegisteredExternal == true).FirstOrDefault();
            if (userinfo != null)
            {
                //Get user Qualifications
                var userQualifications = db.UserQualifications.Where(a => a.UserProfileId == userinfo.Id).ToList();

                List<Guid> userQuailificaions = new List<Guid>();
                List<Guid> userSkill = new List<Guid>();

                foreach (var item in userQualifications)
                    userQuailificaions.Add(item.QualificationslookupQualificationId);

                //Get user Skill
                var userSkills = db.UserSkills.Where(a => a.UserProfileId == userinfo.Id).ToList();
                foreach (var skill in userSkills)
                    userSkill.Add(skill.IndustrySkillsId);

                //Need to make status same as ShiftChangeRequestStatus
                var externalShifts = db.ExternalShiftBroadcasts.Where(a => a.Status == ExternalShiftStatus.Pending).ToList();

                foreach (var externalShift in externalShifts)
                {
                    //Get ExternalShiftBroadCast
                    List<Guid> externalShiftQuilificaion = new List<Guid>();
                    List<Guid> externalShiftSkills = new List<Guid>();
                    //Get ExternalShiftQualifications  
                    foreach (var item in externalShift.ShiftQualifications)
                    {
                        var ExternalShiftQualification = db.ExternalShiftQualifications.Where(a => a.Id == item.Id).FirstOrDefault();
                        externalShiftQuilificaion.Add(ExternalShiftQualification.QualificationslookupQualificationId);
                    }

                    //Get ExternalShiftSkills
                    foreach (var item in externalShift.ShiftSkills)
                    {
                        var externalShiftSkill = db.ExternalShiftSkills.Where(a => a.Id == item.Id).FirstOrDefault();
                        externalShiftSkills.Add(externalShiftSkill.IndustrySkillsId);
                    }

                    //Get Shifts which are externaly BroadCasted.
                    var shift = db.Shifts.Where(a => a.ExternalShiftBroadcastId == externalShift.Id && a.IsPublished == true && a.Employee == null && a.StartTime > dtNow).FirstOrDefault();

                    if (shift != null)
                    {
                        ShiftDTO objShift = MapperFacade.MapperConfiguration.Map<Shift, ShiftDTO>(shift);
                        //Get Business Location details 
                        var businessLocation = db.BusinessLocations.Where(a => a.Id == objShift.BusinessLocationId).FirstOrDefault();

                        if (location.Address.PlaceLatitude != null && location.Address.PlaceLongitude != null)
                        {
                            //var LocationString = location.Split(',');
                            //var userLocation = LocationString[0];
                            //var businessloc = db.BusinessLocations.Where(a => a.Address.Line1.Contains(userLocation)).ToList();

                            //New Location Lat, Long 
                            double lat1 = Convert.ToDouble(location.Address.PlaceLatitude);
                            double long1 = Convert.ToDouble(location.Address.PlaceLongitude);

                            //Business Lat,Long
                            double lat2 = Convert.ToDouble(businessLocation.Address.PlaceLatitude);
                            double long2 = Convert.ToDouble(businessLocation.Address.PlaceLongitude);

                            //Get Distance between User and Businesslocation.
                            var oUserLocaction = new GeoCoordinate(lat1, long1);
                            var oBusinessLocation = new GeoCoordinate(lat2, long1);
                            double distance = oUserLocaction.GetDistanceTo(oBusinessLocation);
                            double DistanceInKM = distance / 1000;
                            double selectedDistence = Convert.ToDouble(distancekm);
                            if (DistanceInKM <= selectedDistence)
                            {
                                if (Check(userQuailificaions, externalShiftQuilificaion) && Check(userSkill, externalShiftSkills) == true)
                                {
                                    var Externalshift = MapperFacade.MapperConfiguration.Map<ExternalShiftBroadcast, ExternalBroadcastDTO>(externalShift);
                                    Externalshift.Distance = DistanceInKM;
                                    ExternalBroadCastList.Add(Externalshift);
                                }
                            }
                        }
                        else
                        {
                            //User Lat, Long 
                            double lat1 = 0;
                            double long1 = 0;
                            if (userinfo.CurrentAddress.PlaceLatitude != null && userinfo.CurrentAddress.PlaceLongitude != null)
                            {
                                lat1 = Convert.ToDouble(userinfo.CurrentAddress.PlaceLatitude);
                                long1 = Convert.ToDouble(userinfo.CurrentAddress.PlaceLongitude);
                            }
                            else
                            {
                                lat1 = Convert.ToDouble(userinfo.Address.PlaceLatitude);
                                long1 = Convert.ToDouble(userinfo.Address.PlaceLongitude);
                            }
                            //Business Lat,Long
                            double lat2 = Convert.ToDouble(businessLocation.Address.PlaceLatitude);
                            double long2 = Convert.ToDouble(businessLocation.Address.PlaceLongitude);

                            //Get Distance between User and Businesslocation.
                            var oUserLocaction = new GeoCoordinate(lat1, long1);
                            var oBusinessLocation = new GeoCoordinate(lat2, long1);
                            double distance = oUserLocaction.GetDistanceTo(oBusinessLocation);
                            double DistanceInKM = distance / 1000;
                            double selectedDistence = Convert.ToDouble(distancekm);
                            if (DistanceInKM <= selectedDistence)
                            {
                                if (Check(userQuailificaions, externalShiftQuilificaion) && Check(userSkill, externalShiftSkills) == true)
                                {
                                    var Externalshift = MapperFacade.MapperConfiguration.Map<ExternalShiftBroadcast, ExternalBroadcastDTO>(externalShift);
                                    Externalshift.Distance = DistanceInKM;
                                    ExternalBroadCastList.Add(Externalshift);
                                }
                            }
                        }
                    }
                }
            }
            return ExternalBroadCastList;
        }
    }
}
