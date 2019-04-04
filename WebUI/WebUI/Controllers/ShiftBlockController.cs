using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
    public class ShiftBlockController : Controller
    {
       
        // GET: /ShiftBlock/
        public ActionResult Index(Guid businesslocationid)
        {
            ViewBag.BusinessLocationId = businesslocationid;
            using(var bc = new BusinessController())
            {
                var bus = bc.GetBusinessLocation(businesslocationid,this.Session);
                ViewBag.BusinessId = bus.BusinessId;
            }

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/ShiftBlockAPI/businesslocation/" + businesslocationid.ToString() + "/shiftblocks");
                var shiftBlocks = (Task.Factory.StartNew(() => JsonConvert.DeserializeObject<IEnumerable<ShiftBlockDTO>>(response.Result)).Result);
                return PartialView(shiftBlocks);
            }
        }

        // GET: /ShiftBlock/Details/5
        public ActionResult Details(Guid? id)
        {
            //if (id == null)
            //{
            //    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            //}
            //ShiftBlockDTO shiftblockdto = db.ShiftBlockDTOes.Find(id);
            //if (shiftblockdto == null)
            //{
            //    return HttpNotFound();
            //}
            return View();
            //return View(shiftblockdto);
        }

        // GET: /ShiftBlock/Create
        public ActionResult Create(Guid businesslocationid)
        {
            //Get roles for business
            using (BusinessController bc = new BusinessController())
            {
                var busLoc = bc.GetBusinessLocation(businesslocationid, this.Session);
                ViewBag.BusinessRoles = bc.GetBusinessRoles(busLoc.BusinessId, this.Session);
                ViewBag.BusinessLocationId = businesslocationid;
            }
            return PartialView();
        }

        // POST: /ShiftBlock/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BusinessId,BusinessLocationId,RoleId,StartTime,FinishTime,FinishNextDay")] ShiftBlockDTO shiftblockdto)
        {
            if (shiftblockdto.StartTime > shiftblockdto.FinishTime
                && !shiftblockdto.FinishNextDay)
                ModelState.AddModelError(String.Empty, "Start time must be before the finish time or next day finish must be selected");

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("api/ShiftBlockAPI", shiftblockdto).Result;
                    responseMessage.EnsureSuccessStatusCode();

                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + shiftblockdto.BusinessLocationId.ToString()); //Remove the stale business item from the cache
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + shiftblockdto.BusinessId.ToString()); //Remove the stale business item from the cache

                    return RedirectToAction("Index", new { businesslocationid = shiftblockdto.BusinessLocationId});
                }
            }

            //Get roles for business
            using (BusinessController bc = new BusinessController())
            {
                var busLoc = bc.GetBusinessLocation(shiftblockdto.BusinessLocationId, this.Session);
                ViewBag.BusinessRoles = bc.GetBusinessRoles(busLoc.BusinessId, this.Session);
                ViewBag.BusinessId = shiftblockdto.BusinessId;
                ViewBag.BusinessLocationId = shiftblockdto.BusinessLocationId;
            }

            return PartialView();
        }

        // GET: /ShiftBlock/Edit/5
        public ActionResult Edit(Guid? id, Guid businesslocationId)
        {
            //Get roles for business
            BusinessController bc = new BusinessController();
            var busLoc = bc.GetBusinessLocation(businesslocationId, this.Session);
            ViewBag.BusinessRoles = bc.GetBusinessRoles(busLoc.BusinessId, this.Session);
            ViewBag.BusinessId = busLoc.BusinessId;
            ViewBag.BusinessLocationId = businesslocationId;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/ShiftBlockAPI/" + id.ToString());
                    var shiftBlockDTO = (Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShiftBlockDTO>(response.Result)).Result);
                    return PartialView(shiftBlockDTO);
                }
            }
        }

        // POST: /ShiftBlock/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,BusinessId,BusinessLocationId,RoleId,StartTime,FinishTime,FinishNextDay")] ShiftBlockDTO shiftblockdto)
        {
            if (shiftblockdto.StartTime > shiftblockdto.FinishTime
                  && !shiftblockdto.FinishNextDay)
                ModelState.AddModelError(String.Empty, "Start time must be before the finish time or next day finish must be selected");

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/ShiftBlockAPI/" + shiftblockdto.Id.ToString(), shiftblockdto).Result;
                    responseMessage.EnsureSuccessStatusCode();
                   
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + shiftblockdto.BusinessLocationId.ToString()); //Remove the stale business item from the cache
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + shiftblockdto.BusinessId.ToString()); //Remove the stale business item from the cache

                    return RedirectToAction("Index", new { businesslocationid = shiftblockdto.BusinessLocationId});
                }
            }

            //Get roles for business
            using (BusinessController bc = new BusinessController())
            {
                var busLoc = bc.GetBusinessLocation(shiftblockdto.BusinessLocationId, this.Session);
                ViewBag.BusinessRoles = bc.GetBusinessRoles(busLoc.BusinessId, this.Session);
                ViewBag.BusinessId = shiftblockdto.BusinessId;
                ViewBag.BusinessLocationId = shiftblockdto.BusinessLocationId;
            }

            return PartialView(shiftblockdto);
        }

        // GET: /ShiftBlock/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/ShiftBlockAPI/" + id.ToString());
                    var shiftBlockDTO = (Task.Factory.StartNew(() => JsonConvert.DeserializeObject<ShiftBlockDTO>(response.Result)).Result);
                    ViewBag.BusinessId = shiftBlockDTO.BusinessId;
                    ViewBag.BusinessLocationId = shiftBlockDTO.BusinessLocationId;
                    return PartialView(shiftBlockDTO);
                }
            }
        }

        // POST: /ShiftBlock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid businesslocationId, Guid id)
        {
            //Get roles for business
            BusinessController bc = new BusinessController();
            var busLoc = bc.GetBusinessLocation(businesslocationId, this.Session);
            

            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                var responseMessage = httpClient.DeleteAsync("api/ShiftBlockAPI/" + id.ToString()).Result;
                responseMessage.EnsureSuccessStatusCode();

                CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + busLoc.Id.ToString()); //Remove the stale business item from the cache
                CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + busLoc.BusinessId.ToString()); //Remove the stale business item from the cache

            }
            return RedirectToAction("Index", new { businesslocationid = businesslocationId });
           
        }
    }
}
