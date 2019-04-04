using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Http;
using WebUI.Models;

namespace WebUI.Controllers
{
    [AuthorizeUser]
    public class EmployeeController : Controller
    {
        //
        // GET: /Employee/
        public ActionResult Index(Guid businesslocationid)
        {
            BusinessController bc = new BusinessController();
            var busLoc = bc.GetBusinessLocation(businesslocationid, this.Session);
            ViewBag.BusinessLocationId = businesslocationid;
            ViewBag.BusinessId = busLoc.BusinessId;
            return PartialView(GetEmployeesList(businesslocationid));
        }

        

        //
        // GET: /Employee/Details/5
        public ActionResult Details(Guid id )
        {
            EmployeeDTO employeeDTO = GetEmployee(id);
            if (employeeDTO == null)
                return HttpNotFound();
            return PartialView(employeeDTO);
        }

#if DEBUG
        public JsonResult PunchClock(Guid employeeid, Guid businessLocationId, string timePunch)
        {
            var dtTimePunchTicks = DateTime.Parse(timePunch).Ticks;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
               
                var responseMessage = httpClient.PostAsJsonAsync("/api/TimeCardAPI/ClockInTest/" + employeeid.ToString() + "/BusLocation/" + businessLocationId.ToString() + "/DateTimePunch/"+ dtTimePunchTicks.ToString(), employeeid).Result;
                //var responseMessage = httpClient.PostAsJsonAsync("/api/TimeCardAPI/ClockInTest/" + employeeid.ToString()+ "/DateTimePunch/" + dtTimePunchTicks.ToString(), employeeid).Result;

                //var responseMessage = httpClient.PostAsJsonAsync("/api/TimeCardAPI/ClockIn/" + employeeid.ToString() + "/BusLocation/" + businessLocationId.ToString() , employeeid).Result;

                if (responseMessage.IsSuccessStatusCode)
                {
                    var clockInOutReponseDto = JsonConvert.DeserializeObject<ClockInOutReponseDTO>(responseMessage.Content.ReadAsStringAsync().Result);
                    var msg = clockInOutReponseDto.Message;
                    return Json(new { success = "true", message = msg });
                }
                else
                { //If and error occurred add details to model error.
                    var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                    ModelState.AddModelError(String.Empty, error.Message);
                    return Json(new { success = "false", message = error.Message }); // do a concatenation of the model state errors
                }
            }
        }
#endif
        //
        // GET: /Employee/Create
        public ActionResult Create(Guid? businessLocationId,Guid businessId, bool addedAnother = false )
        {
            
            ViewBag.AddedAnother = addedAnother;

            //Get roles for business
            using (BusinessController bc = new BusinessController())
            {
               bc.GetBusinessRoles(businessId, this.Session);
               BusinessDTO businessDTO = bc.GetBusiness(businessId, this.Session);
                ViewBag.BusinessRoles = businessDTO.EnabledRoles;
                ViewBag.SelectedBusinessName = businessDTO.Name;

                if (!businessLocationId.HasValue)
                {
                    var enabledBusinessLocations = businessDTO.BusinessLocations.Where(e => e.Enabled);
                    //Ensure the business only has one location then set to this, otherwise throw an exception
                    if (enabledBusinessLocations.Count() == 1)
                        ViewBag.SelectedBusinessLocationId = enabledBusinessLocations.First().Id;
                    else
                        throw new Exception("Business location must be specified.");
                }
                else
                    ViewBag.SelectedBusinessLocationId = businessLocationId.Value;

                ViewBag.BusinessLocations = new SelectList(businessDTO.BusinessLocations, "Id", "Name", ViewBag.SelectedBusinessLocationId);
               // ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
            }

            return PartialView(new EmployeeDTO { BusinessId = businessId, BusinessLocationId = (Guid)ViewBag.SelectedBusinessLocationId, IsActive = true });  
        }

        //
        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EmployeeDTO employeedto, List<string> selectedRoles, bool addedAnother = false)
        {
            ViewBag.AddedAnother = addedAnother;

            if (selectedRoles != null)
            {
                if (selectedRoles.Count() > 5)
                    ModelState.AddModelError("", "Maximimum of 5 roles can be added");
                else
                {

                    employeedto.Roles = new List<RoleDTO>();
                    foreach (var selectedRole in selectedRoles)
                        employeedto.Roles.Add(new RoleDTO { Id = Guid.Parse(selectedRole), Name = "PLACEHOLDER" });
                }
            }

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("/api/EmployeeAPI", employeedto).Result;
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        if (employeedto.AddAnother)
                            return RedirectToAction("Create", new { businessLocationId = employeedto.BusinessLocationId, businessId = employeedto.BusinessId, addedAnother = true });
                        else
                            return RedirectToAction("Index", new { businesslocationid = employeedto.BusinessLocationId });

                    }
                    else
                    { //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }
                }
            }

            //Get roles for business
            BusinessController bc = new BusinessController();
            ViewBag.BusinessRoles = bc.GetBusinessRoles(employeedto.BusinessId, this.Session);

            return PartialView(employeedto);

        }

        //
        // GET: /Employee/Edit/5
        public ActionResult Edit(Guid id)
        {
            EmployeeDTO employeeDTO = GetEmployee(id);
            if (employeeDTO == null)
                return HttpNotFound();
            else
            {
                //Get roles for business
                BusinessController bc = new BusinessController();
                ViewBag.BusinessRoles = bc.GetBusinessRoles(employeeDTO.BusinessId, this.Session);

                //ViewBag.BusinessDetails = GetBusinessSummaryList();
                return PartialView(employeeDTO);
            }
        }

        //
        // POST: /Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EmployeeDTO employeedto, List<string> selectedRoles)
        {
            if (selectedRoles != null)
            {
                if (selectedRoles.Count() > 5)
                    ModelState.AddModelError("", "Maximimum of 5 roles can be added");
                else
                {
                    employeedto.Roles = new List<RoleDTO>();
                    foreach (var selectedRole in selectedRoles)
                        employeedto.Roles.Add(new RoleDTO { Id = Guid.Parse(selectedRole), Name = "PLACEHOLDER" });
                }
            }

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/EmployeeAPI/" + employeedto.Id, employeedto).Result;
                    if (responseMessage.IsSuccessStatusCode)
                        return RedirectToAction("Index", new { businesslocationid = employeedto.BusinessLocationId });
                    else
                    { //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }
                }
            }

            //Get roles for business
            BusinessController bc = new BusinessController();
            ViewBag.BusinessRoles = bc.GetBusinessRoles(employeedto.BusinessId, this.Session);

            return PartialView(employeedto);
        }

        //
        // GET: /Employee/Delete/5
        public ActionResult Delete(Guid id)
        {
            EmployeeDTO employeedto = this.GetEmployee(id);
            if (employeedto == null)
                return HttpNotFound();
            ViewBag.BusinessId = employeedto.BusinessId;

            return PartialView(employeedto);
        }

        //
        // POST: /Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id, Guid businessLocationId)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.DeleteAsync("api/EmployeeAPI/" + id.ToString()).Result;
                responseMessage.EnsureSuccessStatusCode();
            }
            return RedirectToAction("Index", new { businesslocationid = businessLocationId });
        }
        #region Role actions
        //
        // GET: /Employee/RoleIndex/5
        public ActionResult RoleIndex(Guid employeeid)
        {
            EmployeeDTO employeeDTO = GetEmployee(employeeid);
            ViewBag.EmployeeId = employeeDTO.Id;
            ViewBag.BusinessId = employeeDTO.BusinessId;
            ViewBag.BusinessLocationId = employeeDTO.BusinessLocationId;
            ViewBag.EmployeeName = employeeDTO.FullName;
            return PartialView(employeeDTO.Roles);
        }

        //
        // GET: /Employee/RoleDelete/5
        public ActionResult RoleDelete(Guid employeeid, Guid id)
        {
            EmployeeDTO employeeDTO = GetEmployee(employeeid);
            if (employeeDTO == null)
                return HttpNotFound();
            else
            {
                RoleDTO roleDTO = employeeDTO.Roles.Find(r => r.Id == id);
                ViewBag.EmployeeId = employeeid;
                return PartialView(roleDTO);
            }
        }

        //
        // POST: /Employee/RoleDelete/5
        [HttpPost, ActionName("RoleDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult RoleDeleteConfirmed(Guid employeeid, Guid id)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.DeleteAsync("api/EmployeeAPI/Employee/" + employeeid.ToString() + "/Role/" + id.ToString()).Result;
                responseMessage.EnsureSuccessStatusCode();
            }
            return RedirectToAction("RoleIndex", new { employeeid = employeeid });
        }

        //
        // GET: /Employee/RoleCreate
        public ActionResult RoleCreate(Guid employeeid)
        {
             EmployeeDTO employeeDTO = GetEmployee(employeeid);
             if (employeeDTO == null)
                 return HttpNotFound();
             else
             {
                 ViewBag.EmployeeId = employeeid;
                 //Get roles for current employee business which have not already been assigned to employee
                 BusinessController controller = new BusinessController();
                 List<RoleDTO> roles = controller.GetBusinessRoles(employeeDTO.BusinessId, Session);
                 ViewBag.Roles = (from RoleDTO role in roles
                                      where !employeeDTO.Roles.Contains(role)
                                      select role).ToList();
                 ViewBag.BusinessId = employeeDTO.BusinessId;
                     
                 return PartialView();
             }
        }

        //
        // POST: /Employee/RoleCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleCreate(RoleDTO roleDTO, Guid employeeid)
        {
            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    // var responseMessage = httpClient.PutAsJsonAsync("api/EmployeeAPI/" + employeedto.Id, employeedto).Result;

                    var responseMessage = httpClient.PostAsJsonAsync("api/EmployeeAPI/Employee/" + employeeid.ToString() + "/Role/" + roleDTO.Id.ToString(), roleDTO).Result;
                    responseMessage.EnsureSuccessStatusCode();
                    return RedirectToAction("RoleIndex", new { employeeid = employeeid });
                }
           }
            else
                return PartialView();
        }

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetEmployeeRoles(string Id)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/EmployeeActionAPI/GetEmployeeRoles/" + Id).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                {
                    var roleList = JsonConvert.DeserializeObject<List<RoleDTO>>(response.Content.ReadAsStringAsync().Result);
                    return Content(response.Content.ReadAsStringAsync().Result);
                }
            }
        }

        [HttpGet]
        [ValidateJsonAntiForgeryToken]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult GetEmployeesInRole(Guid Id)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/EmployeeActionAPI/GetEmployeesInRole/" + Id).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
                else
                {
                    var employeesList = JsonConvert.DeserializeObject<List<EmployeeSummaryDTO>>(response.Content.ReadAsStringAsync().Result);
                    return Content(response.Content.ReadAsStringAsync().Result);
                }
            }
        }
        #endregion

        #region Helper Methods

        internal IEnumerable<EmployeeDTO> GetEmployeesList(Guid businesslocationid, HttpSessionStateBase session = null)
        {
            session = session ?? this.Session;

            using (HttpClientWrapper httpClient = new HttpClientWrapper(session))
            {
                Task<String> response = httpClient.GetStringAsync("api/EmployeeAPI?businesslocationid=" + businesslocationid.ToString());
                return Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<EmployeeDTO>>(response.Result)).Result;
            }
        }

        private EmployeeDTO GetEmployee(Guid id)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/EmployeeAPI/" + id.ToString());
                return Task.Factory.StartNew(() => JsonConvert.DeserializeObject<EmployeeDTO>(response.Result)).Result;
            }
        }

        //private IEnumerable<object> GetBusinessSummaryList()
        //{
        //    BusinessController businessController = new BusinessController();
        //    var businessList = businessController.GetBuinesses();
        //    return (from BusinessDTO business in businessList
        //            select new { Id = business.Id, Name = business.Name }).ToList();
        //}
        #endregion
    }
}