using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Web.Http;
using WebUI.Common;
using WebUI.Controllers.API.Security;
using WebUI.DTOs;
using WebUI.Models;

namespace WebUI.Controllers.API
{
    public class AccountAPIController : ApiController
    {
        private DataModelDbContext db = new DataModelDbContext();

        // GET api/AccountAPI/5
        //Get method used to ValidateUser upon login/authentication, should only be called on login
        internal LoginModelDTO GetAccount(string email, string passwordHash)
        {
            
            var userProfile = db.UserProfiles.FirstOrDefault(Usr => Usr.Email == email);
            if (userProfile == null)
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.ExpectationFailed, "Account email/password incorrect"));
            else if (userProfile.Membership.IsConfirmed == false)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Account is not confirmed"));
            else if (userProfile.Membership.IsLockedOut == true)
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Account is locked"));

            String hashedPassword = userProfile.Membership.Password;
            bool verificationSucceeded = (hashedPassword != null && hashedPassword == passwordHash);
            if (verificationSucceeded)
            {
                userProfile.Membership.PasswordFailuresSinceLastSuccess = 0;
                userProfile.Membership.LastLoginDate = WebUI.Common.Common.DateTimeNowLocal();
            }
            else //failed login attempt
            {
                userProfile.Membership.LastPasswordFailureDate = WebUI.Common.Common.DateTimeNowLocal();

                int failures = userProfile.Membership.PasswordFailuresSinceLastSuccess;
                if (failures < UserCredentials.MaxInvalidPasswordAttempts)
                    ++userProfile.Membership.PasswordFailuresSinceLastSuccess;
                else if (failures >= UserCredentials.MaxInvalidPasswordAttempts)
                {
                    userProfile.Membership.LastLockoutDate = WebUI.Common.Common.DateTimeNowLocal();
                    userProfile.Membership.IsLockedOut = true;
                }
            }

            db.SaveChanges();
            
            if (verificationSucceeded)
                return new LoginModelDTO { Email = userProfile.Email };
            else
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Invalid password"));
        }

        //Internal method to retrieve user roles linked to an email address
        internal string[] GetUserRoles(string email)
        {
            List<string> roles = new List<string>();

            var userProfile = db.UserProfiles.FirstOrDefault(Usr => Usr.Email == email);
            roles.AddRange(userProfile.security_Roles.Select(r => r.Name));

            //If user is an admin of any business, give the the Business admin role
            if (userProfile.Employees.Any(r => r.IsAdmin == true))
                roles.Add(Constants.RoleBusinessAdmin);

            //If user is a manager of any business, give the the Business Manager role
            if (userProfile.Employees.Any(r => r.ManagerBusinessLocations.Count > 0))
                roles.Add(Constants.RoleBusinessLocationManager);

             return roles.ToArray<string>();
        }

        //Internal method to retrieve user roles linked to an email address
        internal List<Claim> GetUserClaims(string email)
        {
            List<Claim> claimList = new List<Claim>();

            var userProfile = db.UserProfiles.FirstOrDefault(Usr => Usr.Email == email);
            
            //Get all businesses that profile is an employee of
            var membershipQuery = from r in userProfile.Employees
                            select r.BusinessLocation.Business.Id.ToString();

            //Get list of businesses which user is an admin of
            var adminQuery = (from r in userProfile.Employees
                               where r.IsAdmin == true
                                select r.BusinessLocation.Business.Id.ToString()).Distinct();

            claimList.Add(new Claim(Constants.ClaimFirstName, userProfile.FirstName));
            claimList.Add(new Claim(Constants.ClaimLastName, userProfile.LastName));

            foreach (var busId in membershipQuery)
                claimList.Add(new Claim(Constants.ClaimBusinessEmployee, busId));

            //Add all business locations which the user is a location manager for
            foreach (Employee emp in userProfile.Employees)
                foreach(BusinessLocation bloc in emp.ManagerBusinessLocations)
                    claimList.Add(new Claim(Constants.ClaimBusinessLocationManager, bloc.Id.ToString()));
    
            foreach (var busId in adminQuery)
                claimList.Add(new Claim(Constants.ClaimBusinessAdmin, busId));

            return claimList;
        }

        //Method to call to confirm the account activation token and to activate the users account
        //PUT api/AccountAPI/ConfirmToken/{id}
        [ActionName("ConfirmToken")]
        public HttpResponseMessage PutConfirmToken(string id)
        {
            if (db.security_Membership.SingleOrDefault(m => m.ConfirmationToken == id) == null)
                return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Cofirmation token does not exist");
            else
            {
                var secMembership = db.security_Membership.Single(m => m.ConfirmationToken == id);

                //Remove token and set IsConfrimed to true
                secMembership.ConfirmationToken = null;
                secMembership.IsConfirmed = true;

                db.Entry(secMembership).State = EntityState.Modified;

                db.SaveChanges();
            }

            //Update account to be confirmed
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [Route("api/AccountAPI/ResetPassword")]
        public HttpResponseMessage ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var userProfile = db.UserProfiles.FirstOrDefault(Usr => Usr.Email == resetPasswordDTO.Email);

            if (userProfile != null && userProfile.Membership.IsConfirmed.GetValueOrDefault(false))
            {
                if (userProfile.Membership.PasswordVerificationToken == resetPasswordDTO.Code
                    && userProfile.Membership.PasswordVerificationTokenExpirationDate >= WebUI.Common.Common.DateTimeNowLocal())
                {
                    userProfile.Membership.PasswordVerificationToken = null;
                    userProfile.Membership.PasswordVerificationTokenExpirationDate = null;
                    userProfile.Membership.Password = Crypto.Hash(resetPasswordDTO.Password);
                    userProfile.Membership.PasswordFailuresSinceLastSuccess = 0;
                    userProfile.Membership.PasswordChangedDate = WebUI.Common.Common.DateTimeNowLocal();

                    db.SaveChanges();
                }
                else
                    return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Token invalid or expired. You can request a new token by using the forgotten password link again.");

            }
            else
                return Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Invalid request");

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        [HttpPost]
        [Route("api/AccountAPI/VerifyUser")]
        public bool VerifyUser(ForgotPasswordDTO forgotPasswordDTO)
        {
            bool isValid = false;

            var userProfile = db.UserProfiles.FirstOrDefault(Usr => Usr.Email == forgotPasswordDTO.Email);

            if (userProfile != null && userProfile.Membership.IsConfirmed.GetValueOrDefault(false))
                isValid = true;

            return isValid;
        }

        // POST api/AccountAPI
        // Reset the users password
        [HttpPost]
        [Route("api/AccountAPI/ResetPasswordToken")]
        public string ResetPasswordToken(ForgotPasswordDTO forgotPasswordDTO)
        {
            //Check for valid unused email address.
            var userProfile = db.UserProfiles.FirstOrDefault(Usr => Usr.Email == forgotPasswordDTO.Email);

            if (userProfile == null || !userProfile.Membership.IsConfirmed.GetValueOrDefault(false))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "Invalid request"));
            else
            {
                //Create a reset password token
                var confirmationToken = Crypto.GenerateToken();
                var resetPasswordExpiry = WebUI.Common.Common.DateTimeNowLocal().AddMinutes(10); //ten minutes for token to be valid before expiry

                userProfile.Membership.PasswordVerificationToken = confirmationToken;
                userProfile.Membership.PasswordVerificationTokenExpirationDate = resetPasswordExpiry;

                db.SaveChanges();

                return confirmationToken;
            }
        }

        // POST api/AccountAPI
        // Register a new account method
        public HttpResponseMessage PostAccount(RegisterModelDTO registerModelDTO)
        {
            if (ModelState.IsValid)
            {
                //Check for valid unused email address.
                if(db.UserProfiles.Where(usr => usr.Email == registerModelDTO.Email).Any())
                    return Request.CreateErrorResponse(HttpStatusCode.Conflict, "Email address already exists");
                
                var userProfile = MapperFacade.MapperConfiguration.Map<RegisterModelDTO, UserProfile>(registerModelDTO);
                userProfile.Id = Guid.NewGuid(); //Assign new ID on save.

                //By default set current address to be same as home address
                userProfile.CurrentAddress = new Address
                {
                    Line1 = userProfile.Address.Line1,
                    Suburb = userProfile.Address.Suburb,
                    State = userProfile.Address.State,
                    Postcode = userProfile.Address.Postcode,
                    Line2 = userProfile.Address.Line2,
                    PlaceId = userProfile.Address.PlaceId,
                    PlaceLatitude = userProfile.Address.PlaceLatitude,
                    PlaceLongitude = userProfile.Address.PlaceLongitude
                };
              

                var confirmationToken = Crypto.GenerateToken();


                //Create QR code for the employee
                System.Drawing.Bitmap qrCodeImage = WebUI.Common.Common.GenerateQR(userProfile.Id.ToString());

                using (System.IO.MemoryStream memory = new System.IO.MemoryStream())
                {
                    qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
                    userProfile.QRCode = memory.ToArray();
                }

                //Set up corresponding Membership entry
                userProfile.Membership = new security_Membership
                {
                    CreateDate = WebUI.Common.Common.DateTimeNowLocal(),
                    IsConfirmed = false,//true,
                    ConfirmationToken = confirmationToken,
                    Password = Crypto.Hash(registerModelDTO.Password),
                    PasswordFailuresSinceLastSuccess = 0,
                    PasswordChangedDate = WebUI.Common.Common.DateTimeNowLocal(),
                };

                userProfile.UserPreferences = new UserPreferences
                {
                    InternalAvailableShifts = true,
                    ExternalShiftInfo = true,
                    ExternalAvailableShifts = true,
                    DistanceTravel = 10,
                    ShiftReminderLength = 24,
                    NotifyByApp = true,
                    NotifyByEmail = true,
                    NotifyBySMS = true,
                    TimeFormat24Hr = false,
                    MonthCalView = false,
                    ImageData = (registerModelDTO.ImageData == null? new Byte [0] : registerModelDTO.ImageData),
                    ImageType = registerModelDTO.ImageType
                };

                //If there is external hashed login data (OAuth)
                if (!String.IsNullOrEmpty(registerModelDTO.ExternalProvider) && !String.IsNullOrEmpty(registerModelDTO.ExternalProviderUserId))
                {
                    userProfile.OAuthMemberships.Add(new security_OAuthMembership
                    {
                        Provider = registerModelDTO.ExternalProvider,
                        ProviderUserId = registerModelDTO.ExternalProviderUserId
                    });
                }

                db.UserProfiles.Add(userProfile);
                
                db.SaveChanges();

                MessagingService.RegistrationAdminConfirmation(userProfile.Email, userProfile.FirstName, userProfile.LastName);

                return Request.CreateResponse(HttpStatusCode.OK, confirmationToken);
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }
        }

        #region OAuth Functions
        [Route("api/AccountAPI/ValidateOAuthUser")]
        //Checks to see if the Oauth user is already linked to an existing registerd user account
        public OAuthLoginModelResultDTO ValidateOAuthUser(OAuthLoginModelDTO oauthLoginModelDTO)
        {
            if (string.IsNullOrEmpty(oauthLoginModelDTO.ProviderName))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "providerName is null or empty"));

            if (string.IsNullOrEmpty(oauthLoginModelDTO.ProviderUserID))
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.ExpectationFailed, "providerUserID is null or empty"));

            var openAuthAccount = this.db.security_OAuthMembership.SingleOrDefault(a => a.Provider == oauthLoginModelDTO.ProviderName && a.ProviderUserId == oauthLoginModelDTO.ProviderUserID);

            var result = new OAuthLoginModelResultDTO
            {
                ProviderName = oauthLoginModelDTO.ProviderName,
                ProviderUserID = oauthLoginModelDTO.ProviderUserID
            };

            if (openAuthAccount != null)
            {
                
                result.HashedPassword = openAuthAccount.UserProfile.Membership.Password;
                result.UserEmail = openAuthAccount.UserProfile.Email;
                result.AccountLinked = true;
            }
            else
                result.AccountLinked = false;

            return result;
        }
        #endregion

    }
}
