using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Routing;
using Thinktecture.IdentityModel.WebApi;
using WebUI.Common;
using WebUI.Controllers.API.Security;
using Thinktecture.IdentityModel.WebApi.Authentication.Handler;


namespace WebUI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //CorsConfiguration corsConfig = new CorsConfiguration();
            //corsConfig.AllowAll();
            //var corsHandler = new CorsMessageHandler(corsConfig, config);
            //config.MessageHandlers.Add(corsHandler);

            // authentication configuration for identity controller
            var authentication = CreateAuthenticationConfiguration();
            config.MessageHandlers.Add(new AuthenticationHandler(authentication));
            //Add exception logging
            config.Services.Add(typeof(IExceptionLogger), new TraceExceptionLogger());
            
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "ActionApi",
                routeTemplate: "api/{controller}/{action}/{id}"//,
                //defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            

            config.Routes.MapHttpRoute("DefaultApiWithAction", "api/{controller}/{action}");
            config.Routes.MapHttpRoute("DefaultApiGet", "api/{controller}", new { action = "Get" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) });
            config.Routes.MapHttpRoute("DefaultApiPost", "api/{controller}", new { action = "Post" }, new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) });

            config.Routes.MapHttpRoute(
               name: "BusinessRole",
               routeTemplate: "api/BusinessAPI/Business/{businessId}/Role/{roleId}",
               defaults: new { controller = "BusinessAPI" }
           );

            config.Routes.MapHttpRoute(
                name: "EmployeeRole",
                routeTemplate: "api/EmployeeAPI/Employee/{employeeId}/Role/{roleId}",
                defaults: new { controller = "EmployeeAPI" }
            );
           
            //  config.Routes.MapHttpRoute(
            //    name: "AccountLogin",
            //    routeTemplate: "api/AccountAPI/Email/{email}/Password/{password}",
            //    defaults: new { controller = "AccountAPI" }
            //);

            //  config.Routes.MapHttpRoute(
            //    name: "AccountLogin",
            //    routeTemplate: "api/AccountAPI/ConfirmToken",
            //    defaults: new { controller = "AccountAPI" }
            //);


            // add global authorization filter
            //Only hit on request, not always - //config.Filters.Add(new ClaimsAuthorizeAttribute());

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
        }

        private static AuthenticationConfiguration CreateAuthenticationConfiguration()
        {
            TimeSpan sessionTokenTimespanSeconds = TimeSpan.FromSeconds(Double.Parse(ConfigurationManager.AppSettings.Get("SessionTokenLifetimeSeconds")));
            var authentication = new AuthenticationConfiguration
            {
                ClaimsAuthenticationManager = new ClaimsTransformer(),
#if DEBUG
                RequireSsl = false, 
#else
                 RequireSsl = true, 
#endif
                EnableSessionToken = true,
                SessionToken = new SessionTokenConfiguration
                {
                    SigningKey = Convert.FromBase64String(ConfigurationManager.AppSettings.Get("APISessionTokenSigningKey")),
                    DefaultTokenLifetime = sessionTokenTimespanSeconds
                }
            };

            #region Basic Authentication
            authentication.AddBasicAuthentication(UserCredentials.ValidateBasic,UserCredentials.GetUserRoles);
            #endregion
            
            #region IdentityServer JWT
            //authentication.AddJsonWebToken(
            //    issuer: Constants.IdSrv.IssuerUri,
            //    audience: Constants.Audience,
            //    signingKey: Constants.IdSrv.SigningKey);

            //authentication.AddMsftJsonWebToken(
            //    issuer: Constants.IdSrv.IssuerUri,
            //    audience: Constants.Audience,
            //    signingKey: Constants.IdSrv.SigningKey);
            #endregion

            #region Access Control Service JWT
            //authentication.AddJsonWebToken(
            //    issuer: Constants.ACS.IssuerUri,
            //    audience: Constants.Audience,
            //    signingKey: Constants.ACS.SigningKey,
            //    scheme: Constants.ACS.Scheme);
            #endregion

            #region IdentityServer SAML
            //authentication.AddSaml2(
            //    issuerThumbprint: Constants.IdSrv.SigningCertThumbprint,
            //    issuerName: Constants.IdSrv.IssuerUri,
            //    audienceUri: Constants.Realm,
            //    certificateValidator: X509CertificateValidator.None,
            //    options: AuthenticationOptions.ForAuthorizationHeader(Constants.IdSrv.SamlScheme),
            //    scheme: AuthenticationScheme.SchemeOnly(Constants.IdSrv.SamlScheme));
            #endregion

            #region Client Certificates
            //authentication.AddClientCertificate(ClientCertificateMode.ChainValidation);
            #endregion

            return authentication;
        }
    }
}