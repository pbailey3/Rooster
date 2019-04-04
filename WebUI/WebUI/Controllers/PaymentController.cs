using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Http;
using WebUI.Common;
using WebUI.DTOs;
using WebUI.Http;

namespace WebUI.Views.Payment
{
    public class PaymentController : Controller
    {
         // GET: TokenReturn
        public ActionResult TokenReturn(string employeeId,string businessLocationId, string accessCode)
        {
            var requestURL = Request.Url;
            PaymentDetailsDTO paymentDetailsDTO = new PaymentDetailsDTO();
                    
            using (HttpClient httpClient = new HttpClient())
            {
               
                    //To validate the user we need to pass the credentials to the api using Basic Authentication
                    //if authenticated, a session token will be returned.
                    var apiUser = ConfigurationManager.AppSettings.Get("eWayRapidAPIKey");
                    var apiPassword = ConfigurationManager.AppSettings.Get("eWayRapidAPIPassword");
                    httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(apiUser, apiPassword);

                    httpClient.BaseAddress = new Uri("https://api.sandbox.ewaypayments.com");

                    HttpResponseMessage response = httpClient.GetAsync("AccessCode/" + accessCode).Result;
                    response.EnsureSuccessStatusCode();
                    var objResponse = JsonConvert.DeserializeObject<eWayAccessCodeResponseDTO>(response.Content.ReadAsStringAsync().Result);

                    var tokenCustomerID = objResponse.TokenCustomerID;

                    //Save token to database for current logged in employee, then redirect to Index
                    paymentDetailsDTO.TokenCustomerID = long.Parse(tokenCustomerID);
                    paymentDetailsDTO.EmployeeID = Guid.Parse(employeeId);
            }
                
                    using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                    {
                       var responseMessage = httpClient.PostAsJsonAsync("api/PaymentAPI/AddToken", paymentDetailsDTO).Result;
                       if (responseMessage.IsSuccessStatusCode)
                       {
                         //Successfully updated token, now redirect back to payment index
                           return RedirectToAction("Index", new  { businessLocationId = businessLocationId });
                       }
                       else
                       {
                         //TODO: handle error
                       }
                    }
             
               return View();
             
        }


        // GET: Payment
        public ActionResult Index(Guid businessLocationId)
        {
            PaymentDetailsDTO paymentDetailsDTO = new PaymentDetailsDTO();

            //Get a summary of the employes currently linked to the Employee
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/PaymentAPI/businesslocation/" + businessLocationId.ToString());
                paymentDetailsDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<PaymentDetailsDTO>(response.Result)).Result;
            }

            ViewBag.BusinessLocationId = businessLocationId;

            return PartialView(paymentDetailsDTO);
        }

        public ActionResult AddCardDetails(Guid businessLocationId)
        {
            AddCardDetailsDTO addCardDetailsDTO = new AddCardDetailsDTO();
            EmployeeDTO employeeDTO = null;

            //Get a summary of the employes currently linked to the Employee
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                Task<String> response = httpClient.GetStringAsync("api/EmployeeAPI/GetCurrentEmployee/businesslocationid/" + businessLocationId.ToString());
                employeeDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<EmployeeDTO>(response.Result)).Result;
            }

            using (HttpClient httpClient = new HttpClient())
            {
                //To validate the user we need to pass the credentials to the api using Basic Authentication
                //if authenticated, a session token will be returned.
                var apiUser = ConfigurationManager.AppSettings.Get("eWayRapidAPIKey");
                var apiPassword = ConfigurationManager.AppSettings.Get("eWayRapidAPIPassword");
                httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(apiUser, apiPassword);

                httpClient.BaseAddress = new Uri("https://api.sandbox.ewaypayments.com");

                eWayRequestDTO gatewayRequest = new eWayRequestDTO();
                gatewayRequest.CancelUrl = ConfigurationManager.AppSettings["ApiBaseURL"] + "Home/AuthIndex";
                gatewayRequest.RedirectUrl = ConfigurationManager.AppSettings["ApiBaseURL"] + "Home/AuthIndex";
               
                //gatewayRequest.CancelUrl = ConfigurationManager.AppSettings["ApiBaseURL"] + "Payment?businessLocationId=" + businessLocationId.ToString();
                //gatewayRequest.RedirectUrl = ConfigurationManager.AppSettings["ApiBaseURL"] + "Payment/TokenReturn?EmployeeId=" + employeeDTO.Id.ToString() + "&businessLocationId=" + businessLocationId.ToString();
                gatewayRequest.Method = "CreateTokenCustomer";
                gatewayRequest.TransactionType = "Purchase";

                gatewayRequest.HeaderText = "Register your credit card details. These will be kept on file for monthly billing, no payments will be taken now.";
                gatewayRequest.LogoUrl = "https://www-batdog.azurewebsites.net/Content/images/batdog-logo.png";

                gatewayRequest.Customer = new eWayCustomerRequestDTO();
                gatewayRequest.Customer.Title = "Mr.";
                gatewayRequest.Customer.FirstName = employeeDTO.FirstName;
                gatewayRequest.Customer.LastName = employeeDTO.LastName;
                gatewayRequest.Customer.Reference = employeeDTO.Id.ToString();
                gatewayRequest.Customer.Country = "au";

                HttpResponseMessage response = httpClient.PostAsJsonAsync("AccessCodesShared", gatewayRequest).Result;
                response.EnsureSuccessStatusCode();
                var objResponse = JsonConvert.DeserializeObject<eWayResponseDTO>(response.Content.ReadAsStringAsync().Result);

                addCardDetailsDTO.SharedPaymentUrl = objResponse.SharedPaymentUrl;
            }

            return PartialView(addCardDetailsDTO);
           
        }
    }
}