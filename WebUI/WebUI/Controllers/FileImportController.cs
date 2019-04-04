using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUI.Http;
using WebUI.DTOs;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;
using WebUI.Common;

namespace WebUI.Controllers
{
    [AuthorizeUser]
    public class FileImportController : Controller
    {
        // GET: FileImport
        public ActionResult FileUpload(Guid? businessLocationId, Guid businessId)
        {
            FileImportDTO model = new FileImportDTO();

            model.BusinessLocationId = (!businessLocationId.HasValue) ? model.BusinessLocationId : businessLocationId.Value;
            model.BusinessId = businessId;

            return PartialView(model);
        }

        public FileResult ExampleEmployeeCsv()
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(ms);
            writer.WriteLine("FirstName,LastName,Mobile,Email,Type,Admin,Roles");
            writer.WriteLine("John,,,,Full_Time,,\"Cleaner,Waiter,Glass monkey\"");
            writer.Flush();
            writer.Dispose();
            Response.AddHeader("Content-Disposition", "inline; filename=ExampleEmployee.csv");
            return new FileContentResult(ms.ToArray(), "text/csv");
        }


        //
        // POST: /Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FileUpload(FileImportDTO filedto, HttpPostedFileBase file)
        {
            if (file != null)
            {
                byte[] fileAsBytes = new byte[file.ContentLength];
                using (BinaryReader theReader = new BinaryReader(file.InputStream))
                {
                    fileAsBytes = theReader.ReadBytes(file.ContentLength);
                }
                filedto.FileUpload = fileAsBytes;
                ModelState["FileUpload"].Errors.Clear();
            }

            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PostAsJsonAsync("/api/FileImportAPI", filedto).Result;

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS + filedto.BusinessId.ToString()); //Remove the stale business item from the cache
                        CacheManager.Instance.Remove(CacheManager.CACHE_KEY_BUSINESS_LOCATION + filedto.BusinessLocationId.ToString()); //Remove the stale business location item from the cache

                        var ret = JsonConvert.DeserializeObject<LogFileDTO>(responseMessage.Content.ReadAsStringAsync().Result);

                        return Json(ret, JsonRequestBehavior.DenyGet);
                    }
                    else
                    {
                        //If and error occurred add details to model error.
                        var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);
                        ModelState.AddModelError(String.Empty, error.Message);
                    }
                }
            }

            return Json(new { Status = "Failed" }, JsonRequestBehavior.DenyGet);
        }
    }
}