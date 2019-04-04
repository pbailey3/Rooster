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
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Models;
using FeatureToggle;

namespace WebUI.Controllers.API
{
    [Authorize(Roles = Constants.RoleBusinessLocationManager + "," + Constants.RoleBusinessAdmin)]
    public class PaymentAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/<controller>
        [HttpGet]
        [Route("api/PaymentAPI/businesslocation/{businesslocationid}")]
        public PaymentDetailsDTO GetPaymentDetails(Guid businesslocationid)
        {
            if (Is<PaymentFeature>.Enabled)
            {
                var busLocation = db.BusinessLocations.Find(businesslocationid);
                if (busLocation != null)
                {
                    if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", busLocation.Id.ToString()))
                        return MapperFacade.MapperConfiguration.Map<PaymentDetails, PaymentDetailsDTO>(busLocation.PaymentDetails);
                    else
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }

        [HttpPost]
        [Route("api/PaymentAPI/AddToken")]
        public void AddTokenDetails([FromBody]PaymentDetailsDTO paymentDetailsDTO)
        {
            if (Is<PaymentFeature>.Enabled)
            {
                var employee = db.Employees.Find(paymentDetailsDTO.EmployeeID);
                if (employee != null)
                {
                    if (ClaimsAuthorization.CheckAccess("Put", "BusinessLocationId", employee.BusinessLocation.Id.ToString()))
                    {
                        var paymentdetails = new PaymentDetails
                        {
                            Id = Guid.NewGuid()
                        };

                        if (employee.PaymentDetails != null)
                            paymentdetails = employee.PaymentDetails;

                        paymentdetails.TokenCustomerID = paymentDetailsDTO.TokenCustomerID;
                        paymentdetails.BusinessLocation = employee.BusinessLocation;
                        paymentdetails.CreatedDate = WebUI.Common.Common.DateTimeNowLocal();

                        //If there is another payment detail from a different employee then delete
                        if (employee.BusinessLocation.PaymentDetails != null
                            && employee.BusinessLocation.PaymentDetails.Employee.Id != employee.Id)
                        {
                            //employee.BusinessLocation.PaymentDetails.
                            db.Entry(employee.BusinessLocation.PaymentDetails).State = EntityState.Deleted;
                        }

                        employee.PaymentDetails = paymentdetails;

                        db.SaveChanges();
                    }
                    else
                        throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.Unauthorized));
                }
                else
                    throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            else
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotImplemented));
        }
    }
}