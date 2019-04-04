using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using WebUI.Models;
using WebUI.Controllers.API.Security;
using WebUI.DTOs;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Services;
using WebUI.Common;
using WebUI.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.Web.Helpers;
using System.Web.Routing;
using System.Threading.Tasks;
using System.IO;
using Thinktecture.IdentityModel.Http;
using System.Web.Script.Serialization;
using System.Net;
using System.Globalization;
using System.Threading;

namespace WebUI.Controllers
{
    //[Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {
        //
        // GET: /Account/Login

        [AllowAnonymous]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Login(string returnUrl)
        {
            bool loggedInAlready = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (loggedInAlready)
                return RedirectToLocal(returnUrl);
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModelDTO model, string returnUrl)
        {
            // check if all required fields are set
            if (ModelState.IsValid)
            {
                if (ValidateUser(model.Email, WebUI.Controllers.API.Security.Crypto.Hash(model.Password), model.RememberMe))
                    return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //Refresh token required for when claims need to be updated
        public ActionResult RefreshToken(string returnAction, string returnController, string urlParams)
        {
            ValidateUser(Session[Constants.SessionUsernameKey].ToString(), Session[Constants.SessionPasswordKey].ToString());

            var nameValuePairs = (string.IsNullOrEmpty(urlParams)) ? null : HttpUtility.ParseQueryString(urlParams);

            if (returnAction == "CreateAddBusinessLocation"
                || returnAction == "RoleCreate")
            {
                return RedirectToAction(returnAction, returnController, new { businessId = Guid.Parse(nameValuePairs["businessId"]), isNew = true });
            }
            else if (returnAction == "Details" && returnController == "Business")
                return RedirectToAction(returnAction, returnController, new { Id = Guid.Parse(nameValuePairs["Id"]) });
            else if (returnAction == "ApproveEmployeeRequest" && returnController == "Employer")
               return Content("Success");
            else
                throw new Exception("RefreshToken returnAction not handled");
        }

        private bool ValidateUser(string email, string passwordHash, bool rememberMe = false)
        {
            bool success = false;

            // authenticate user
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                try
                {
                    //To validate the user we need to pass the credentials to the api using Basic Authentication
                    //if authenticated, a session token will be returned.
                    httpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(email, passwordHash);
                    var response = httpClient.GetAsync("api/token").Result;

                    if (response.IsSuccessStatusCode) //User was validated
                    {
                        var tokenResponse = response.Content.ReadAsStringAsync().Result;
                        var json = JObject.Parse(tokenResponse);
                        var token = json["access_token"].ToString();
                        var expiresIn = int.Parse(json["expires_in"].ToString());
                        var expiration = WebUI.Common.Common.DateTimeNowLocal().AddSeconds(expiresIn);

                        Session[Constants.SessionUsernameKey] = email;
                        Session[Constants.SessionPasswordKey] = passwordHash;

                        var prefNameResponse = httpClient.GetAsync("api/UserProfileAPI/GetPreferredName").Result;
                        var prefName = JsonConvert.DeserializeObject<string>(prefNameResponse.Content.ReadAsStringAsync().Result);

                        CustomPrincipalSerializeModel serializeModel = new CustomPrincipalSerializeModel()
                        {
                            Email = email,
                            FirstName = prefName
                        };
                        JavaScriptSerializer serializer = new JavaScriptSerializer();

                        string userData = serializer.Serialize(serializeModel);

                        success = true;
                        FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                          1,
                          email,  //user id
                          DateTime.Now,
                          DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                          rememberMe,  //do not remember
                          userData,
                          "/");

                        HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName,
                                                           FormsAuthentication.Encrypt(authTicket))
                        {
                            HttpOnly = true,
                            Secure = FormsAuthentication.RequireSSL,
                            Path = FormsAuthentication.FormsCookiePath,
                            Domain = FormsAuthentication.CookieDomain
                        };

                        //Store the token in a cookie so as to remove any dependencie on session cache and timeout issues
                        HttpCookie cookieToken = new HttpCookie(Constants.CookieTokenKey,
                                                           token);
                        Response.Cookies.Add(cookie);
                        Response.Cookies.Add(cookieToken);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("ValidateUser():/n [Source]: " + ex.Source + "/n[Message]: " + ex.Message + "\n[InnerExceptionSource]: " + ex.InnerException.Source + "\n[InnerExceptionMesage]: " + ex.InnerException.Message + "\n[StackTrace]: " + ex.StackTrace);
                    success = false;
                }
            }

            return success;
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.StatesList = WebUI.Common.Common.GetStates();
            RegisterModelDTO model = new RegisterModelDTO()
            {
                Address = new AddressModelDTO()
            };
            return View(model);
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModelDTO registerModelDTO)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            if (ModelState.IsValid)
            {   
                // Attempt to register the user
                try
                {
                    using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                    {
                        var responseMessage = httpClient.PostAsJsonAsync("/api/AccountAPI", registerModelDTO).Result;
                        //If registration worked, then need to send email confirmation
                        if (responseMessage.IsSuccessStatusCode)
                        {
                            var confirmationToken = JsonConvert.DeserializeObject<string>(responseMessage.Content.ReadAsStringAsync().Result);

                            var confURL = WebUI.Common.Common.GetDomainFromRequest(HttpContext.Request) + "/Account/RegisterConfirmation/" + confirmationToken.ToString();
                            MessagingService.RegistrationConfirmation(registerModelDTO.Email, registerModelDTO.FirstName, confURL);

                            return RedirectToAction("RegisterStepTwo");
                        }
                        else
                        {
                            var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);

                            ModelState.AddModelError(responseMessage.ReasonPhrase, error.Message);
                        }
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }
            // If we got this far, something failed, redisplay form
            ViewBag.StatesList = WebUI.Common.Common.GetStates();
            return View(registerModelDTO);
        }

        [AllowAnonymous]
        public ActionResult RegisterStepTwo()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult RegisterConfirmation(string id)
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                //Call confirm account API method
                var responseMessage = httpClient.PutAsJsonAsync("/api/AccountAPI/ConfirmToken/" + id, string.Empty).Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                    return RedirectToAction("ConfirmationSuccess");
                }
            }
            return RedirectToAction("ConfirmationFailure");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    //Call confirm account API method
                    var responseMessage = httpClient.PostAsJsonAsync("/api/AccountAPI/VerifyUser", model).Result;
                    if (responseMessage.IsSuccessStatusCode)
                    {
                        var isValid = JsonConvert.DeserializeObject<bool>(responseMessage.Content.ReadAsStringAsync().Result);
                        if (!isValid)
                        {
                            // Don't reveal that the user does not exist or is not confirmed
                            return View("ForgotPasswordConfirmation");
                        }
                        else
                        {
                            //Generate and send a reset password token to the users email.
                            var responseMessage2 = httpClient.PostAsJsonAsync("/api/AccountAPI/ResetPasswordToken", model).Result;
                            //If registration worked, then need to send email confirmation
                            if (responseMessage2.IsSuccessStatusCode)
                            {
                                var confirmationToken = JsonConvert.DeserializeObject<string>(responseMessage2.Content.ReadAsStringAsync().Result);
                                var confURL = WebUI.Common.Common.GetDomainFromRequest(HttpContext.Request) + "/Account/ResetPassword/" + confirmationToken.ToString();
                                MessagingService.PasswordReset(model.Email, confURL);

                                return RedirectToAction("ForgotPasswordConfirmation", "Account");
                            }
                            else
                            {
                                var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage2.Content.ReadAsStringAsync().Result);

                                ModelState.AddModelError(String.Empty, error.Message);
                            }
                        }
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            ViewBag.Link = TempData["ViewBagLink"];
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string id)
        {
            ResetPasswordDTO model = new ResetPasswordDTO() { Code = id };
            return id == null ? View("Error") : View(model);
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                //Call confirm account API method
                var responseMessage = httpClient.PostAsJsonAsync("/api/AccountAPI/VerifyUser", new ForgotPasswordDTO() { Email = model.Email }).Result;
                if (responseMessage.IsSuccessStatusCode)
                {
                    var isValid = JsonConvert.DeserializeObject<bool>(responseMessage.Content.ReadAsStringAsync().Result);
                    if (!isValid)
                    {
                        // Don't reveal that the user does not exist
                        return RedirectToAction("ResetPasswordConfirmation", "Account");
                    }
                    else
                    {
                        var responseMessage2 = httpClient.PostAsJsonAsync("/api/AccountAPI/ResetPassword", model).Result;
                        if (responseMessage2.IsSuccessStatusCode)
                        {
                            return RedirectToAction("ResetPasswordConfirmation", "Account");
                        }
                        else
                        {
                            var error2 = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage2.Content.ReadAsStringAsync().Result);

                            ModelState.AddModelError(String.Empty, error2.Message);
                        }
                    }
                }
                else
                {
                    var error1 = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);

                    ModelState.AddModelError(String.Empty, error1.Message);
                }
            }
           return View(model);
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        [AllowAnonymous]
        public ActionResult ConfirmationSuccess()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult ConfirmationFailure()
        {
            return View();
        }


        //
        // POST: /Account/Disassociate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = true;//OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Manage(LocalPasswordModel model)
        //{
        //    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
        //    ViewBag.HasLocalPassword = hasLocalAccount;
        //    ViewBag.ReturnUrl = Url.Action("Manage");
        //    if (hasLocalAccount)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            // ChangePassword will throw an exception rather than return false in certain failure scenarios.
        //            bool changePasswordSucceeded;
        //            try
        //            {
        //                changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
        //            }
        //            catch (Exception)
        //            {
        //                changePasswordSucceeded = false;
        //            }

        //            if (changePasswordSucceeded)
        //            {
        //                return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
        //            }
        //            else
        //            {
        //                ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
        //            }
        //        }
        //    }
        //    else
        //    {
        //        // User does not have a local password so remove any validation errors caused by a missing
        //        // OldPassword field
        //        ModelState state = ModelState["OldPassword"];
        //        if (state != null)
        //        {
        //            state.Errors.Clear();
        //        }

        //        if (ModelState.IsValid)
        //        {
        //            try
        //            {
        //                WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
        //                return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
        //            }
        //            catch (Exception)
        //            {
        //                ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
        //            }
        //        }
        //    }

        //    // If we got this far, something failed, redisplay form
        //    return View(model);
        //}

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            FacebookClient2017.RewriteRequest();
            var regClients = OAuthWebSecurity.RegisteredClientData;
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            // Check database for entry for provider, userid
            var oAuthResult = MembershipService.ValidateOAuthUser(result, this.Session);
            if (oAuthResult.AccountLinked)
            {
                //Log the user in
                this.ValidateUser(oAuthResult.UserEmail, oAuthResult.HashedPassword);

                return RedirectToLocal(returnUrl);
            }

            //If account not already linked, then link them to the currently authenticated and logged in user
            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                //User needs to register for a rooster account, direct them to registration process.
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                ViewBag.StatesList = WebUI.Common.Common.GetStates();
                return View("ExternalLoginConfirmation", new RegisterModelDTO { ExternalLoginData = loginData });
            }
        }



        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterModelDTO model, string profileImageUrl, string returnUrl)
        {

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out string provider, out string providerUserId))
            {
                return RedirectToAction("Manage");
            }

            model.ExternalProvider = provider;
            model.ExternalProviderUserId = providerUserId;

            //Download the image for profile pic
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(profileImageUrl);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Check that the remote file was found. The ContentType
            // check is performed since a request for a non-existent
            // image file might be redirected to a 404-page, which would
            // yield the StatusCode "OK", even though the image was not
            // found.
            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                Stream inputStream = null;
                try
                {
                    inputStream = response.GetResponseStream();

                    //get the bytes from the content stream of the file
                    byte[] thePictureAsBytes = new byte[response.ContentLength];
                    using (BinaryReader theReader = new BinaryReader(inputStream))
                    {
                        inputStream = null;
                        thePictureAsBytes = theReader.ReadBytes(int.Parse(response.ContentLength.ToString()));
                    }
                    model.ImageData = thePictureAsBytes;
                    model.ImageType = response.ContentType;
                }
                finally
                {
                    if (inputStream != null)
                        inputStream.Dispose();
                }
            }


            if (ModelState.IsValid)
            {

                // Attempt to register the user
                try
                {
                    using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                    {
                        var responseMessage = httpClient.PostAsJsonAsync("/api/AccountAPI", model).Result;
                        //If registration worked, then need to send email confirmation
                        if (responseMessage.IsSuccessStatusCode)
                        {
                            var confirmationToken = JsonConvert.DeserializeObject<string>(responseMessage.Content.ReadAsStringAsync().Result);

                            var confURL = WebUI.Common.Common.GetDomainFromRequest(HttpContext.Request) + "/Account/RegisterConfirmation/" + confirmationToken.ToString();
                            MessagingService.RegistrationConfirmation(model.Email, model.FirstName, confURL);

                            return RedirectToAction("RegisterStepTwo");
                        }
                        else
                        {
                            var error = JsonConvert.DeserializeObject<System.Web.Http.HttpError>(responseMessage.Content.ReadAsStringAsync().Result);

                            ModelState.AddModelError(responseMessage.ReasonPhrase, error.Message);
                        }
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.StatesList = WebUI.Common.Common.GetStates();
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            //ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            //List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            //foreach (OAuthAccount account in accounts)
            //{
            //    AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

            //    externalLogins.Add(new ExternalLogin
            //    {
            //        Provider = account.Provider,
            //        ProviderDisplayName = clientData.DisplayName,
            //        ProviderUserId = account.ProviderUserId,
            //    });
            //}

            //ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            //return PartialView("_RemoveExternalLoginsPartial", externalLogins);

            return null;
        }

        #region Preference actions
        //
        // GET: /Business/RoleDetails/5
        [Authorize]
        public ActionResult UserPreferencesDetails()
        {
            return PartialView(GetUserPreferences());
        }

        private UserPreferencesDTO GetUserPreferences()
        {
            var userEmail = (HttpContext.User as WebUI.Common.CustomPrincipal).Email;

            UserPreferencesDTO userPrefDTO = CacheManager.Instance.Get<UserPreferencesDTO>(CacheManager.CACHE_KEY_USER_PREFS + userEmail);

            if (userPrefDTO == null) //Not found in cache
            {
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    Task<String> response = httpClient.GetStringAsync("api/UserPreferencesAPI");
                    userPrefDTO = Task.Factory.StartNew(() => JsonConvert.DeserializeObject<UserPreferencesDTO>(response.Result)).Result;
                    CacheManager.Instance.Add(CacheManager.CACHE_KEY_USER_PREFS + userEmail, userPrefDTO);
                }
            }
            return userPrefDTO;
        }

        //
        // GET: /Business/PreferencesEdit/5
        [Authorize]
        public ActionResult UserPreferencesEdit(Guid id)
        {
            return PartialView(GetUserPreferences());
        }

        [Authorize]
        public ActionResult ShowUserPreferencesImage(Guid id)
        {
            var usrPrefsDTO = GetUserPreferences();

            if (usrPrefsDTO.ImageData.Length > 0)
                return File(usrPrefsDTO.ImageData, usrPrefsDTO.ImageType);
            else
                return null;
        }

        [Authorize]
        public FileContentResult ShowCurrentUserQRCode()
        {
            using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
            {
                HttpResponseMessage response = httpClient.GetAsync("api/UserProfileAPI/GetQRCode").Result;
                byte[] qrCode = response.Content.ReadAsAsync<byte[]>().Result;
                return File(qrCode, "image/jpeg");
            }
        }

        //
        // POST: /Business/PreferencesEdit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult UserPreferencesEdit(UserPreferencesDTO usrPreferencesDTO, HttpPostedFileBase image)
        {
            var validImageTypes = new string[]
                                            {
                                                "image/gif",
                                                "image/jpeg",
                                                "image/pjpeg",
                                                "image/png"
                                            };

            if (image != null) //If no new file is being uploaded, then ignore image checks
            {
                if (image.ContentLength == 0)
                {
                    ModelState.AddModelError("ImageUpload", "This field is required");
                }
                else if (image.ContentLength > 102400)
                {
                    ModelState.AddModelError("ImageUpload", "File too large, maximum upload size is 100KB");
                }
                else if (!validImageTypes.Contains(image.ContentType))
                {
                    ModelState.AddModelError("ImageUpload", "Please choose either a GIF, JPG or PNG image.");
                }
                //get the bytes from the content stream of the file
                byte[] thePictureAsBytes = new byte[image.ContentLength];
                using (BinaryReader theReader = new BinaryReader(image.InputStream))
                {
                    thePictureAsBytes = theReader.ReadBytes(image.ContentLength);
                }
                usrPreferencesDTO.ImageData = thePictureAsBytes;
                usrPreferencesDTO.ImageType = image.ContentType;
            }

            if (ModelState.IsValid)
            {
                var userEmail = (HttpContext.User as WebUI.Common.CustomPrincipal).Email;

                //Replace with updated role
                using (HttpClientWrapper httpClient = new HttpClientWrapper(Session))
                {
                    var responseMessage = httpClient.PutAsJsonAsync("api/UserPreferencesAPI/" + usrPreferencesDTO.Id.ToString(), usrPreferencesDTO).Result;
                    responseMessage.EnsureSuccessStatusCode();
                    CacheManager.Instance.Remove(CacheManager.CACHE_KEY_USER_PREFS + userEmail);
                    return RedirectToAction("UserPreferencesDetails");
                }
            }
            return View(usrPreferencesDTO);
        }

        #endregion


        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion


    }
}
