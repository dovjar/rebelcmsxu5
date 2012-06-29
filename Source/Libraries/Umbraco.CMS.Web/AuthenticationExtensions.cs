using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using Umbraco.Cms.Web.Configuration;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web
{
    /// <summary>
    /// Extensions to create and renew and remove authentication tickets for the Umbraco back office
    /// </summary>
    public static class AuthenticationExtensions
    {
        private static readonly UmbracoSettings _settings;

        static AuthenticationExtensions()
        {
            _settings = UmbracoSettings.GetSettings();
        }

        /// <summary>
        /// This clears the forms authentication cookie
        /// </summary>
        public static void UmbracoLogout(this HttpContextBase http)
        {
            Logout(http, _settings.BackofficeCookieName);
        }

        /// <summary>
        /// This clears the forms authentication cookie
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        public static void Logout(this HttpContextBase http, string cookieName)
        {
            //remove from the request
            http.Request.Cookies.Remove(cookieName);

            //expire from the response
            var formsCookie = http.Response.Cookies[cookieName];
            if (formsCookie != null)
            {                
                //this will expire immediately and be removed from the browser
                formsCookie.Expires = DateTime.Now.AddYears(-1);
            }
            else
            {
                //ensure there's def an expired cookie
                http.Response.Cookies.Add(new HttpCookie(cookieName) { Expires = DateTime.Now.AddYears(-1) });
            }
        }

        /// <summary>
        /// Renews the Umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <returns></returns>
        public static bool RenewUmbracoAuthTicket(this HttpContextBase http)
        {
            return RenewAuthTicket(http, _settings.BackofficeCookieName, _settings.BackofficeCookieDomain, 60);
        }

        /// <summary>
        /// Creates the umbraco authentication ticket
        /// </summary>
        /// <param name="http"></param>
        /// <param name="userdata"></param>
        internal static void CreateUmbracoAuthTicket(this HttpContextBase http, UserData userdata)
        {
            var userDataString = (new JavaScriptSerializer()).Serialize(userdata);

            CreateAuthTicket(http, userdata.Username, userDataString, userdata.SessionTimeout, userdata.SessionTimeout, "/", _settings.BackofficeCookieName, _settings.BackofficeCookieDomain);
        }

        public static FormsAuthenticationTicket GetUmbracoAuthTicket(this HttpContextBase http)
        {
            return GetAuthTicket(http, _settings.BackofficeCookieName);
        }

        public static FormsAuthenticationTicket GetAuthTicket(this HttpContextBase http, string cookieName)
        {
            var formsCookie = http.Request.Cookies[cookieName];
            if (formsCookie == null)
            {
                return null;
            }
            //get the ticket
            try
            {
                return FormsAuthentication.Decrypt(formsCookie.Value);
            }
            catch (Exception)
            {
                //occurs when decryption fails
                http.Logout(cookieName);
                return null;
            }
        }

        /// <summary>
        /// Renews the forms authentication ticket & cookie
        /// </summary>
        /// <param name="http"></param>
        /// <param name="cookieName"></param>
        /// <param name="cookieDomain"></param>
        /// <param name="minutesPersisted"></param>
        /// <returns></returns>
        public static bool RenewAuthTicket(this HttpContextBase http, string cookieName, string cookieDomain, int minutesPersisted)
        {
            //get the ticket
            var ticket = GetAuthTicket(http, cookieName);
            //renew the ticket
            var renewed = FormsAuthentication.RenewTicketIfOld(ticket);
            if (renewed == null)
            {
                return false;
            }
            //encrypt it
            var hash = FormsAuthentication.Encrypt(renewed);
            //write it to the response
            var cookie = new HttpCookie(cookieName, hash)
            {
                Expires = DateTime.Now.AddMinutes(minutesPersisted),
                Domain = cookieDomain
            };
            //rewrite the cooke
            http.Response.Cookies.Remove(cookieName);
            http.Response.Cookies.Add(cookie);
            return true;
        }

        /// <summary>
        /// Creates a custom FormsAuthentication ticket with the data specified
        /// </summary>
        /// <param name="http">The HTTP.</param>
        /// <param name="username">The username.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="loginTimeoutMins">The login timeout mins.</param>
        /// <param name="minutesPersisted">The minutes persisted.</param>
        /// <param name="cookiePath">The cookie path.</param>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="cookieDomain">The cookie domain.</param>
        public static void CreateAuthTicket(this HttpContextBase http, 
                                  string username, 
                                  string userData, 
                                  int loginTimeoutMins, 
                                  int minutesPersisted, 
                                  string cookiePath,
                                  string cookieName,
                                  string cookieDomain)
        {
            // Create a new ticket used for authentication
            var ticket = new FormsAuthenticationTicket(
                4,
                username,
                DateTime.Now,
                DateTime.Now.AddMinutes(loginTimeoutMins),
                true,
                userData,
                cookiePath
                );

            // Encrypt the cookie using the machine key for secure transport
            var hash = FormsAuthentication.Encrypt(ticket);
            var cookie = new HttpCookie(
                cookieName,
                hash)
            {
                Expires = DateTime.Now.AddMinutes(minutesPersisted),
                Domain = cookieDomain
            };
            
            http.Response.Cookies.Add(cookie);            
        }
    }
}