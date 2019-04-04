using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Thinktecture.IdentityModel;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Http;
using WebUI.Models;

namespace WebUI.Controllers
{
    public class BusinessController : Controller
    {
        //
        // GET: /Business/
        [AuthorizeUser(Roles = Constants.RoleSysAdmin)]
        public ActionResult Index()
        {
           return View(this.GetBuinesses());
        }

        internal IEnumerable<BusinessDTO> GetBuinesses()
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/BusinessAPI");
                var val = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<BusinessDTO>>(response.Result)).Result;
          
                return val;
            }
        }

        // GET: /Business/Create
        [AuthorizeUser]
        public ActionResult CreateAddBusinessLocation(Guid businessId, bool? isNew)
        {
            BusinessDTO businessDTO = GetBusiness(businessId);

            var model = new BusinessLocationDTO
            {
                BusinessId = businessId
            };

          
            model.InternalLocations = new List<InternalLocationDTO>();

            model.InternalLocations.Add(new InternalLocationDTO { Name = "Default" });

            ViewBag.BusinessName = businessDTO.Name;
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
            ViewBag.IsNew = isNew.GetValueOrDefault(false);
            ViewBag.StatesList = WebUI.Common.Common.GetStates();

           return PartialView(model);
        }

        // POST: /Business/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAddBusinessLocation(BusinessLocationDTO businessLocationDTO, bool addNext)
        {
            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {

                    var response = httpClient.PostAsJsonAsync("/api/BusinessAPI/businesslocation", businessLocationDTO).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessLocationDTO.BusinessId.ToString()); //Remove the stale business item from the cache

                        var busLocId = Guid.Parse(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).ToString());

                        //If user selected to add next business location
                        if (addNext)
                            return RedirectToAction("RefreshToken", "Account", new { returnAction = "CreateAddBusinessLocation", returnController = "Business", urlParams = "businessId=" + businessLocationDTO.BusinessId.ToString() });
                        else
                            return RedirectToAction("RefreshToken", "Account", new { returnAction = "RoleCreate", returnController = "Business", urlParams = "businessId=" + businessLocationDTO.BusinessId.ToString() });
                    }
                    else
                    { //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(response.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }
                }
            }

            ViewBag.StatesList = WebUI.Common.Common.GetStates();
            BusinessDTO businessDTO = GetBusiness(businessLocationDTO.BusinessId);

            ViewBag.BusinessName = businessDTO.Name;
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;

            return PartialView(businessLocationDTO);
        
        }


        //
        // GET: /Business/Create
       [AuthorizeUser]
        public ActionResult Create()
        {
            BusinessDetailsDTO model = new BusinessDetailsDTO();
            model.BusinessLocation = new BusinessLocationDTO();
           
            model.BusinessLocation.InternalLocations = new List<InternalLocationDTO>();

            model.BusinessLocation.InternalLocations.Add(new InternalLocationDTO { Name = "Default" });

            ViewBag.StatesList = WebUI.Common.Common.GetStates();

            ViewBag.Industries = (from BusinessTypeDTO type in this.GetBusinessTypes()
                                  select type.Industry).Distinct().ToList();
            ViewBag.BusinesTypeDetails = new List<BusinessTypeDTO>();

            return PartialView(model);
        }



        //
        // POST: /Business/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BusinessDetailsDTO businessDTO, bool addNext, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {

                    var response = httpClient.PostAsJsonAsync("/api/BusinessAPI", businessDTO).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var busId = Guid.Parse(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).ToString());


                        //If user selected to add next business location
                        if (addNext)
                            return RedirectToAction("RefreshToken", "Account", new { returnAction = "CreateAddBusinessLocation", returnController = "Business", urlParams = "businessId=" + busId.ToString() });
                        else
                            return RedirectToAction("RefreshToken", "Account", new { returnAction = "RoleCreate", returnController = "Business", urlParams = "businessId=" + busId.ToString() });
                    }
                    else
                    {

                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(response.Content.ReadAsStringAsync().Result);

                        ModelState.AddModelError(response.ReasonPhrase, error.Message);
                    }

                }
            }

            //Model state is not valid or error occurred in postack
            if (String.IsNullOrEmpty(businessDTO.BusinessLocation.Address.Line1)
                || String.IsNullOrEmpty(businessDTO.BusinessLocation.Address.Suburb)
                 || String.IsNullOrEmpty(businessDTO.BusinessLocation.Address.State)
                  || String.IsNullOrEmpty(businessDTO.BusinessLocation.Address.Postcode))
                ModelState.AddModelError(String.Empty, "Address details must be entered");

            ViewBag.StatesList = WebUI.Common.Common.GetStates();

            ViewBag.SelIndustry = businessDTO.TypeIndustry;
            ViewBag.Industries = (from BusinessTypeDTO type in this.GetBusinessTypes()
                                  select type.Industry).Distinct().ToList();
            ViewBag.SelBusTypeId = businessDTO.TypeId;

            if (!String.IsNullOrEmpty(businessDTO.TypeIndustry))
                ViewBag.BusinesTypeDetails = this.GetBusinessTypes().Where(t => t.Industry == businessDTO.TypeIndustry);
            else
                ViewBag.BusinesTypeDetails = new List<BusinessTypeDTO>();

            return PartialView(businessDTO);

        }

       //[HttpPost]
       //public PartialViewResult AddInternalLocation(InternalLocation intLocation, int index)
       //{
       //    List<InternalLocationDTO> internalLocations = new List<InternalLocationDTO>();
       //    for (int i = 0; i < index; i++)
       //    {
       //        internalLocations.Add(null);
       //    }
       //    internalLocations.Add(new InternalLocationDTO());
       //    ViewBag.Index = index;
       //    var model = new BusinessRegisterDTO
       //    {
       //        InternalLocations = internalLocations
       //    };

       //    return PartialView("InternalLocationDTO", model);
       //}


        //
        // GET: /Business/Details/5
        public ActionResult Details(Guid id)
        {
            BusinessDTO businessDTO = GetBusiness(id);
            if (businessDTO == null)
                return HttpNotFound();
            return PartialView(businessDTO);
        }
        
        //
        // GET: /Business/Edit/5
        public ActionResult Edit(Guid id)
        {
            BusinessDTO businessDTO = GetBusiness(id);
            if (businessDTO == null)
                return HttpNotFound();
            else
            {
                BusinessDetailsDTO busDetailsDTO = MapperFacade.MapperConfiguration.Map<BusinessDTO, BusinessDetailsDTO>(businessDTO);

                ViewBag.SelIndustry = businessDTO.TypeIndustry;
                ViewBag.Industries = (from BusinessTypeDTO type in this.GetBusinessTypes()
                                      select type.Industry).Distinct().ToList();
                ViewBag.SelBusTypeId = businessDTO.TypeId;

                if (!String.IsNullOrEmpty(businessDTO.TypeIndustry))
                    ViewBag.BusinesTypeDetails = this.GetBusinessTypes().Where(t => t.Industry == businessDTO.TypeIndustry);

                return PartialView(busDetailsDTO);
            }
            
        }

        //
        // POST: /Business/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(BusinessDetailsDTO businessdetailsdto)
        {
            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {  
                    var responseMessage = httpClient.PutAsJsonAsync("api/BusinessAPI/" + businessdetailsdto.Id, businessdetailsdto).Result;
                    responseMessage.EnsureSuccessStatusCode();
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessdetailsdto.Id.ToString()); //Remove the stale item from the cache
                    return RedirectToAction("Details", new { id = businessdetailsdto.Id});
                }
            }
            return PartialView(businessdetailsdto);
        }

        //
        // GET: /Business/Delete/5
        public ActionResult Delete(Guid id)
        {
            BusinessDTO businessDTO = GetBusiness(id);
            if (businessDTO == null)
                return HttpNotFound();
            return View(businessDTO);
        }

        //
        // POST: /Business/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.DeleteAsync("api/BusinessAPI/" + id.ToString()).Result;
                responseMessage.EnsureSuccessStatusCode();
            }
            return RedirectToAction("Index");
        }

        #region Internal Location actions
        //
        // GET: /Business/InternalLocationIndex/5
        public ActionResult InternalLocationIndex(Guid businesslocationid)
        {
            BusinessLocationDTO businessLocationDTO = GetBusinessLocation(businesslocationid);
            ViewBag.BusinessId = businessLocationDTO.BusinessId;
            ViewBag.BusinessLocationId = businessLocationDTO.Id;
            return PartialView(businessLocationDTO.InternalLocations);
        }

        //
        // GET: /Business/InternalLocationCreate
        public ActionResult InternalLocationCreate(Guid businesslocationid)
        {
            ViewBag.BusinessLocationId = businesslocationid;
            return PartialView();
        }

        //
        // POST: /Business/InternalLocationCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InternalLocationCreate(InternalLocationDTO internalLocationDTO)
        {
            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("api/BusinessAPI/BusinessLocation/" + internalLocationDTO.BusinessLocationId.ToString() + "/InternalLocation/" + Guid.Empty.ToString(), internalLocationDTO).Result;
                    responseMessage.EnsureSuccessStatusCode();

                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + internalLocationDTO.BusinessLocationId.ToString()); //Remove the stale business item from the cache
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + internalLocationDTO.BusinessId.ToString()); //Remove the stale business item from the cache

                    return RedirectToAction("InternalLocationIndex", new { businesslocationid = internalLocationDTO.BusinessLocationId });
                }

            }
            else
            {
                ViewBag.BusinessLocationId = internalLocationDTO.BusinessLocationId;
                return PartialView();
            }
        }

        // GET: /Business/InternalLocationEdit/5
        public ActionResult InternalLocationEdit(Guid businesslocationid, Guid id)
        {
            BusinessLocationDTO businessLocationDTO = GetBusinessLocation(businesslocationid);
            if (businessLocationDTO == null)
                return HttpNotFound();
            return PartialView(businessLocationDTO.InternalLocations.FirstOrDefault(r => r.Id == id));
        }

        //
        // POST: /Business/InternalLocationEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InternalLocationEdit(InternalLocationDTO internalLocationDTO)
        {
            if (ModelState.IsValid)
            {
                //Replace with updated location
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/BusinessAPI/BusinessLocation/" + internalLocationDTO.BusinessLocationId.ToString() + "/InternalLocation/" + internalLocationDTO.Id.ToString(), internalLocationDTO).Result;
                    responseMessage.EnsureSuccessStatusCode();
                   
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + internalLocationDTO.BusinessLocationId.ToString()); //Remove the stale business item from the cache
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + internalLocationDTO.BusinessId.ToString()); //Remove the stale business item from the cache

                    return RedirectToAction("InternalLocationIndex", new { businesslocationid = internalLocationDTO.BusinessLocationId });
                }
            }
            else
                return PartialView(internalLocationDTO);
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult DisableInternalLocation(string locId, string businessLocationId, string businessId)
        {

            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/BusinessAPI/DisableInternalLocation/" + locId, String.Empty).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }

            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + businessLocationId); //Remove the stale business location item from the cache
            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessId); //Remove the stale business item from the cache

            return Content("Success");
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult EnableInternalLocation(string locId, string businessLocationId, string businessId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/BusinessAPI/EnableInternalLocation/" + locId, String.Empty).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }

            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + businessLocationId); //Remove the stale business location item from the cache
            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessId); //Remove the stale business item from the cache

            return Content("Success");
        }

        #endregion
        
        #region Preference actions
        //
        // GET: /Business/RoleDetails/5
        public ActionResult PreferencesDetails(Guid businessLocationId)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/BusinessPreferencesAPI/" + businessLocationId.ToString());
                var busPrefsDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<BusinessPreferencesDTO>(response.Result)).Result;
            
                return PartialView(busPrefsDTO);
            }
        }

        //
        // GET: /Business/PreferencesEdit/5
        public ActionResult PreferencesEdit(Guid id, Guid businesslocationId)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/BusinessPreferencesAPI/" + businesslocationId.ToString());
                var busPrefsDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<BusinessPreferencesDTO>(response.Result)).Result;
            
                return PartialView(busPrefsDTO);
            }
        }

        //
        // POST: /Business/PreferencesEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PreferencesEdit(BusinessPreferencesDTO busPreferencesDTO)
        {
            if (ModelState.IsValid)
            {
                //Replace with updated role
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/BusinessPreferencesAPI/" + busPreferencesDTO.Id.ToString(), busPreferencesDTO).Result;
                    responseMessage.EnsureSuccessStatusCode();
                    return RedirectToAction("PreferencesDetails", new { businesslocationid = busPreferencesDTO.BusinessLocationId });
                }
            }
            return PartialView(busPreferencesDTO);
        }

        #endregion

        #region Role actions
        //
        // GET: /Business/RoleIndex/5
        public ActionResult RoleIndex(Guid businessid, Guid? businessLocationId)
        {
            ViewBag.BusinessId = businessid;

            if (businessLocationId.HasValue)
                ViewBag.BusinessLocationId = businessLocationId.Value;

            ViewBag.HasMultiLocations = this.GetBusiness(businessid).HasMultiBusLocations;

            return PartialView( this.GetBusinessRoles(businessid));
        }
        //
        // GET: /Business/RoleDetails/5
        public ActionResult RoleDetails(Guid businessid, Guid id, Guid? businessLocationId)
        {
            if (businessLocationId.HasValue)
                ViewBag.BusinessLocationId = businessLocationId.Value;

            BusinessDTO businessDTO = GetBusiness(businessid);
            if (businessDTO == null)
                return HttpNotFound();
            return View(businessDTO.Roles.FirstOrDefault(r => r.Id == id));
        }
         //
        // GET: /Business/RoleEdit/5
        public ActionResult RoleEdit(Guid businessid, Guid id, Guid? businessLocationId)
        {
            ViewBag.BusinessId = businessid;

            if (businessLocationId.HasValue)
                ViewBag.BusinessLocationId = businessLocationId.Value;

            BusinessDTO businessDTO = GetBusiness(businessid);
            if (businessDTO == null)
                return HttpNotFound();
            return PartialView(businessDTO.Roles.FirstOrDefault(r => r.Id == id));
        }

        //
        // POST: /Business/RoleEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleEdit(RoleDTO roleDTO, Guid? businessLocationId)
        {
            if (ModelState.IsValid)
            {
               //Replace with updated role
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/BusinessAPI/Business/" + roleDTO.BusinessId.ToString() + "/Role/" + roleDTO.Id.ToString(), roleDTO).Result;
                    responseMessage.EnsureSuccessStatusCode();
                    
                    //Invalidate dependant cache item
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + roleDTO.BusinessId.ToString());
                    if(businessLocationId.HasValue)
                        return RedirectToAction("RoleIndex", new { businessid = roleDTO.BusinessId, businessLocationId = businessLocationId.Value });
                    else
                        return RedirectToAction("RoleIndex", new { businessid = roleDTO.BusinessId });
                }
            }
            return PartialView(roleDTO);
        }

        //
        // GET: /Business/RoleCreate
        public ActionResult RoleCreate(Guid businessid, bool? isNew, Guid? businessLocationId)
        {
            ViewBag.BusinessId = businessid;

            if (businessLocationId.HasValue)
                ViewBag.BusinessLocationId = businessLocationId.Value;

            ViewBag.IsNew = isNew.GetValueOrDefault(false);
            return PartialView();
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        public ActionResult AddRole(string roleName, string businessId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var roleDTO = new RoleDTO
                {
                    BusinessId = Guid.Parse(businessId),
                    Name = roleName,
                    Enabled = true
                };
                var responseMessage = httpClient.PostAsJsonAsync("api/BusinessAPI/Business/" + businessId + "/Role/" + Guid.Empty.ToString(), roleDTO).Result;
                if (!responseMessage.IsSuccessStatusCode)
                {
                    var errorMessage = JsonConvert.DeserializeObject<dynamic>(responseMessage.Content.ReadAsStringAsync().Result).Message.ToString();
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(errorMessage);
                }
                var roleId = responseMessage.Headers.Location.Segments[6];

                CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessId); //Remove the stale business item from the cache

                return Content(roleId);
            }
           

        }

        //
        // POST: /Business/RoleCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RoleCreate(RoleDTO roleDTO, Guid? businessLocationId)
        {
            roleDTO.Enabled = true;

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("api/BusinessAPI/Business/" + roleDTO.BusinessId.ToString() + "/Role/" + Guid.Empty.ToString(), roleDTO).Result;
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        //Invalidate dependant cache item
                        CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + roleDTO.BusinessId.ToString());
                        if(businessLocationId.HasValue)
                         return RedirectToAction("RoleIndex", new { businessid = roleDTO.BusinessId, businessLocationId = businessLocationId.Value });
                        else
                            return RedirectToAction("RoleIndex", new { businessid = roleDTO.BusinessId });
                    }
                    else
                    {
                        //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }

                }

            }

          
            ViewBag.IsNew = false;
            
            ViewBag.BusinessId = roleDTO.BusinessId;

            return PartialView();
        }

        //
        // GET: /Business/RoleDelete/5
        public ActionResult RoleDelete(Guid businessid, Guid id)
        {
            BusinessDTO businessDTO = GetBusiness(businessid);
            if (businessDTO == null)
                return HttpNotFound();
            else
            {
                RoleDTO roleDTO = businessDTO.Roles.Find(r => r.Id == id);
                ViewBag.BusinessId = businessid;
                return View(roleDTO);
            }
        }

        //
        // POST: /Business/RoleDelete/5
        [HttpPost, ActionName("RoleDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult RoleDeleteConfirmed(Guid businessid, Guid id)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.DeleteAsync("api/BusinessAPI/Business/" + businessid.ToString() + "/Role/" + id.ToString()).Result;
                responseMessage.EnsureSuccessStatusCode();
            }

            //Invalidate dependant cache item
            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessid.ToString());
            
            return RedirectToAction("RoleIndex", new { businessid = businessid });
        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult DisableRole(string roleId, string businessId)
        {

            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/BusinessAPI/DisableRole/" + roleId, String.Empty).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                {
                    Response.SuppressFormsAuthenticationRedirect = true;
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
                }
            }
            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessId); //Remove the stale business item from the cache

            return Content("Success");

        }

        [HttpPost]
        [ValidateJsonAntiForgeryToken]
        [AuthorizeUser]
        public ActionResult EnableRole(string roleId, string businessId)
        {
            //Current user has requested to be linked to a business
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.PostAsJsonAsync("api/BusinessAPI/EnableRole/" + roleId, String.Empty).Result;
                Response.StatusCode = (int)response.StatusCode;
                if (!response.IsSuccessStatusCode)
                    return this.Content(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).Message.ToString());
            }
            CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessId); //Remove the stale business item from the cache

            return Content("Success");
        }

        #endregion


        #region Helper Methods
        
        internal BusinessDTO GetBusiness(Guid id, HttpSessionStateBase session = null)
        {
           
            var businessDTO = CacheManager.Instance.Get<BusinessDTO>(CacheManager.CACHE_KEY_BUSINESS + id.ToString());

            if (businessDTO == null) //Not found in cache
            {
                DateTime Start = DateTime.Now;
               
                session = session ?? this.Session;

                using (HttpClientWrapper httpClient = new HttpClientWrapper(session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/BusinessAPI/" + id.ToString());
                    businessDTO = (Task.Factory.StartNew(() => JsonConvert.DeserializeObject<BusinessDTO>(response.Result)).Result);
                }
                CacheManager.Instance.Add(CacheManager.CACHE_KEY_BUSINESS + id.ToString(), businessDTO);

                TimeSpan ts = DateTime.Now - Start;
                System.Diagnostics.Trace.TraceInformation("Time to populate cache GetBusiness: '" + ts.TotalMilliseconds.ToString() + "'");
            }

           
            
            return (BusinessDTO)businessDTO;
        }

        internal BusinessLocationDTO GetBusinessLocation(Guid id, HttpSessionStateBase session = null)
        {
             System.Diagnostics.Trace.TraceInformation("Called BusinessController() - > GetBusinessLocation('"+id.ToString()+"')");
             var businessLocationDTO = CacheManager.Instance.Get<BusinessLocationDTO>(CacheManager.CACHE_KEY_BUSINESS_LOCATION + id.ToString());

            if (businessLocationDTO == null) //Not found in cache
            {
                DateTime Start = DateTime.Now;

                session = session ?? this.Session;

                using (HttpClientWrapper httpClient = new HttpClientWrapper(session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/BusinessAPI/businesslocation/" + id.ToString());
                    businessLocationDTO = (Task.Factory.StartNew(() => JsonConvert.DeserializeObject<BusinessLocationDTO>(response.Result)).Result);
                }
                CacheManager.Instance.Add(CacheManager.CACHE_KEY_BUSINESS_LOCATION + id.ToString(), businessLocationDTO);

                TimeSpan ts = DateTime.Now - Start;
                System.Diagnostics.Trace.TraceInformation("Time to populate cache GetBusinessLocation: '" + ts.TotalMilliseconds.ToString() + "'");
            }
           

            return (BusinessLocationDTO)businessLocationDTO;
        }

        internal IEnumerable<BusinessLocationDTO> GetBusinessLocationsForManager(Guid businessid, HttpSessionStateBase session = null)
        {
            session = (session != null) ? session : this.Session;

            using (HttpClientWrapper httpClient = new HttpClientWrapper(session))
            {
                Task<String> response = httpClient.GetStringAsync("api/BusinessAPI/business/" +  businessid.ToString()+"/managerbusinesslocations");
                return (Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<BusinessLocationDTO>>(response.Result)).Result);
            }
        }

        private IEnumerable<BusinessTypeDTO> GetBusinessTypes()
        {
            System.Diagnostics.Trace.TraceInformation("Called BusinessController() - > GetBusinessTypes()");
            var busTypes = CacheManager.Instance.Get<IEnumerable<BusinessTypeDTO>>(CacheManager.CACHE_KEY_BUSINESS_TYPES);

            if (busTypes == null) //Not found in cache
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/BusinessTypeAPI");
                    busTypes = (Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<BusinessTypeDTO>>(response.Result)).Result);
                }

                CacheManager.Instance.Add(CacheManager.CACHE_KEY_BUSINESS_TYPES, busTypes);
            }
            return ((IEnumerable<BusinessTypeDTO>)busTypes);
        }

        public List<RoleDTO> GetBusinessRoles(Guid id, HttpSessionStateBase session = null)
        {
            BusinessDTO businessDTO = GetBusiness(id, session);
          
            //Only return enabled roles
            return businessDTO.Roles.Where(r => r.Enabled).ToList();
        }


        public ActionResult GetBusinessTypes(string industryName)
        {
            var typeLIst = this.GetBusinessTypes().Where(bt => bt.Industry == industryName).ToList();

            return this.Json(typeLIst, JsonRequestBehavior.AllowGet);
        }

        private ShiftTemplateDTO GetShiftTemplateDTO(Guid id)
        {
            ShiftTemplateDTO shiftTemplateDTO = null;
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ShiftTemplateAPI/" + id.ToString());
                shiftTemplateDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShiftTemplateDTO>(response.Result)).Result;
            }
            return shiftTemplateDTO;
        }
        #endregion



        #region Recurring Shift actions
        //
        // GET: /Business/InternalLocationIndex/5
        public ActionResult RecurringShiftIndex(Guid businesslocationid)
        {
         
            using (BusinessController bc = new BusinessController())
            {
                var busLocDTO = bc.GetBusinessLocation(businesslocationid, this.Session);
                BusinessDTO businessDTO = bc.GetBusiness(busLocDTO.BusinessId, this.Session);
                ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
                ViewBag.BusinessLocationId = businesslocationid;
                ViewBag.BusinessId = busLocDTO.BusinessId;
            }

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ShiftTemplateAPI?businesslocationid=" + businesslocationid.ToString());
                var shiftTemplates = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ShiftTemplateDTO>>(response.Result)).Result;

                return PartialView(shiftTemplates);
            }
           
        }

        //
        // GET: /Business/InternalLocationCreate
        public ActionResult RecurringShiftCreate(Guid businessLocationId, Guid businessId)
        {
            ViewBag.BusinessLocationId = businessLocationId;

            //Get roles for business
            using (BusinessController bc = new BusinessController())
            {
                bc.GetBusinessRoles(businessId, this.Session);
                BusinessDTO businessDTO = bc.GetBusiness(businessId, this.Session);
                ViewBag.BusinessRoles = businessDTO.EnabledRoles;
                ViewBag.BusinessId = businessId;
                ViewBag.BusinessLocations = new SelectList(businessDTO.BusinessLocations, "Id", "Name", ViewBag.BusinessLocationId);
           
                var busLocDTO = businessDTO.BusinessLocations.FirstOrDefault( b => b.Id == businessLocationId);
                ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
                ViewBag.BusinessInternalLocations = busLocDTO.GetEnabledInternalLocations();
           }

            using (EmployeeController empController = new EmployeeController())
                ViewBag.BusinessEmployees = empController.GetEmployeesList(businessLocationId, Session);

            return PartialView();
        }

        //
        // POST: /Business/InternalLocationCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecurringShiftCreate(ShiftTemplateDTO shiftTemplateDTO)
        {
            if ((shiftTemplateDTO.StartTime > shiftTemplateDTO.FinishTime && !shiftTemplateDTO.FinishNextDay)
               || shiftTemplateDTO.StartTime == shiftTemplateDTO.FinishTime)
                ModelState.AddModelError(String.Empty, "Start time must be before the finish time or next day finish must be selected");

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("api/ShiftTemplateAPI", shiftTemplateDTO).Result;
                    responseMessage.EnsureSuccessStatusCode();
                    return RedirectToAction("RecurringShiftIndex", new { businesslocationid = shiftTemplateDTO.BusinessLocationId});
                }
            }
            else
            {

                ViewBag.BusinessLocationId = shiftTemplateDTO.BusinessLocationId;

                using (EmployeeController empController = new EmployeeController())
                    ViewBag.BusinessEmployees = empController.GetEmployeesList(shiftTemplateDTO.BusinessLocationId, Session);
                var busLocDTO = GetBusinessLocation(shiftTemplateDTO.BusinessLocationId);
                var businessDTO = GetBusiness(busLocDTO.BusinessId);
                ViewBag.BusinessRoles = businessDTO.EnabledRoles;
                ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;
                ViewBag.BusinessInternalLocations = busLocDTO.GetEnabledInternalLocations();

                return PartialView();
            }
        }
        //
        // GET: /Business/RecurringShiftDelete/5
        public ActionResult RecurringShiftDelete(Guid id)
        {
            var shiftTemplateDTO = GetShiftTemplateDTO(id);
            var businessDTO = GetBusiness(shiftTemplateDTO.BusinessId);
               
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;

            return PartialView(shiftTemplateDTO);
        }

        //
        // POST: /Business/RecurringShiftDelete/5
        [HttpPost, ActionName("RecurringShiftDelete")]
        [ValidateAntiForgeryToken]
        public ActionResult RecurringShiftDeleteConfirmed(Guid BusinessLocationId, Guid id)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.DeleteAsync("api/ShiftTemplateAPI/" + id.ToString()).Result;
                responseMessage.EnsureSuccessStatusCode();
            }
            return RedirectToAction("RecurringShiftIndex", new { businesslocationid = BusinessLocationId });
        }

        // GET: /Business/RecurringShiftEdit/5
        public ActionResult RecurringShiftEdit(Guid id)
        {
            ShiftTemplateDTO shiftTemplateDTO = GetShiftTemplateDTO(id);
            if (shiftTemplateDTO == null)
                return HttpNotFound();

            using (EmployeeController empController = new EmployeeController())
                ViewBag.BusinessEmployees = empController.GetEmployeesList(shiftTemplateDTO.BusinessLocationId, Session);

            ViewBag.BusinessId = shiftTemplateDTO.BusinessId;
            ViewBag.BusinessLocationId = shiftTemplateDTO.BusinessLocationId;

            var businessLocationDTO = GetBusinessLocation(shiftTemplateDTO.BusinessLocationId);

            var businessDTO = GetBusiness(shiftTemplateDTO.BusinessId);

            ViewBag.BusinessRoles = businessDTO.EnabledRoles;

            ViewBag.BusinessInternalLocations = businessLocationDTO.GetEnabledInternalLocations();
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;

            return PartialView(shiftTemplateDTO);
        }

        // POST: /Business/RecurringShiftEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RecurringShiftEdit(ShiftTemplateDTO shiftTemplateDTO)
        {
            if ((shiftTemplateDTO.StartTime > shiftTemplateDTO.FinishTime && !shiftTemplateDTO.FinishNextDay)
               || shiftTemplateDTO.StartTime == shiftTemplateDTO.FinishTime)
                ModelState.AddModelError(String.Empty, "Start time must be before the finish time or next day finish must be selected");

            if (ModelState.IsValid)
            {
                //Replace with updated location
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/ShiftTemplateAPI/" + shiftTemplateDTO.Id.ToString(), shiftTemplateDTO).Result;
                    if (responseMessage.IsSuccessStatusCode)
                        return RedirectToAction("RecurringShiftIndex", new { businesslocationid = shiftTemplateDTO.BusinessLocationId });
                    else
                    { //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }

                }
            }

            using (EmployeeController empController = new EmployeeController())
                ViewBag.BusinessEmployees = empController.GetEmployeesList(shiftTemplateDTO.BusinessId, Session);

            ViewBag.BusinessId = shiftTemplateDTO.BusinessId;
            var businessDTO = GetBusiness(shiftTemplateDTO.BusinessId);
            var businessLocationDTO = GetBusinessLocation(shiftTemplateDTO.BusinessLocationId);

            ViewBag.BusinessRoles = businessDTO.EnabledRoles;
            ViewBag.BusinessInternalLocations = businessLocationDTO.GetEnabledInternalLocations();
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;

            return PartialView(shiftTemplateDTO);

        }

        #endregion

        #region Business Location actions
        //
        // GET: /Business/BusinessLocationIndex/5
        public ActionResult BusinessLocationIndex(Guid businessid)
        {
            ViewBag.BusinessId = businessid;

            BusinessDTO businessDTO = GetBusiness(businessid);
            ViewBag.BusinessName = businessDTO.Name;
            return View(businessDTO.BusinessLocations);
        }

        //
        // GET: /Business/BusinessLocationCreate
        public ActionResult BusinessLocationCreate(Guid businessid)
        {
            ViewBag.StatesList = WebUI.Common.Common.GetStates();
            var business = this.GetBusiness(businessid);
            ViewBag.HasInternalLocations = business.HasMultiInternalLocations;
            ViewBag.BusinessName = business.Name;

            BusinessLocationDTO model = new BusinessLocationDTO();
            model.BusinessId = businessid;
            model.InternalLocations = new List<InternalLocationDTO>();
            model.InternalLocations.Add(new InternalLocationDTO { Name = "Default" });

            return PartialView(model);
        }

        //
        // POST: /Business/BusinessLocationCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BusinessLocationCreate(BusinessLocationDTO businessLocationDTO)
        {
            if (ModelState.IsValid)
            {
                //Replace with updated location
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("api/BusinessAPI/businesslocation", businessLocationDTO).Result;
                    responseMessage.EnsureSuccessStatusCode();

                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businessLocationDTO.BusinessId.ToString()); //Remove the stale business item from the cache

                    return RedirectToAction("RefreshToken", "Account", new { returnAction = "Details", returnController = "Business", urlParams = "Id=" + businessLocationDTO.BusinessId.ToString() });
                }
            }
            else
            {
                ViewBag.BusinessId = businessLocationDTO.BusinessId;
                ViewBag.StatesList = WebUI.Common.Common.GetStates();
                return View(businessLocationDTO);
            }
        }

        // GET: /Business/BusinessLocation/Edit/5
        public ActionResult BusinessLocationEdit(Guid businesslocationid)
        {
            if (businesslocationid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BusinessLocationDTO businessLocationDTO = GetBusinessLocation(businesslocationid);
        
            if (businessLocationDTO == null)
            {
                return HttpNotFound();
            }

            ViewBag.StatesList = WebUI.Common.Common.GetStates();

            BusinessDTO businessDTO = GetBusiness(businessLocationDTO.BusinessId);

            ViewBag.BusinessName = businessDTO.Name;
            ViewBag.HasInternalLocations = businessDTO.HasMultiInternalLocations;

            return PartialView(businessLocationDTO);
        }

        // POST: /BusinessLocation/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BusinessLocationEdit(BusinessLocationDTO businesslocationdto)
        {
            if (ModelState.IsValid)
            {
                //Replace with updated location
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/BusinessAPI/businesslocation/" + businesslocationdto.Id.ToString(), businesslocationdto).Result;
                    responseMessage.EnsureSuccessStatusCode();

                    //Invalidate cache items
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + businesslocationdto.Id.ToString());
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + businesslocationdto.BusinessId.ToString());

                    return RedirectToAction("Details", new { id = businesslocationdto.BusinessId });
                }
            }
            else
            {
                ViewBag.StatesList = WebUI.Common.Common.GetStates();

                return View(businesslocationdto);
            }
        }

        // GET: /Business/BusinessLocation/5
        public ActionResult BusinessLocationDetails(Guid businesslocationid)
        {
            if (businesslocationid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            BusinessLocationDTO businesslocationdto = GetBusinessLocation(businesslocationid);

            ViewBag.StatesList = WebUI.Common.Common.GetStates();

            if (businesslocationdto == null)
            {
                return HttpNotFound();
            }
            return View(businesslocationdto);
        }
        #endregion

    }
}