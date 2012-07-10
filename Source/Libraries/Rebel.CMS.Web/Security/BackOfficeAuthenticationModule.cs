using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Routing;
using System.Web.ClientServices;
using System.Web.Script.Serialization;
using System.Web.Security;
using Rebel.Cms.Web.Configuration;

namespace Rebel.Cms.Web.Security
{
    /// <summary>
    /// Authentication module to manage the back-office custom forms authentication
    /// </summary>
    /// <remarks>
    /// This also manages the temporary custom login for the Installer
    /// </remarks>
    public class BackOfficeAuthenticationModule: IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += AuthenticateRequest;
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Authenticates the request by reading the FormsAuthentication cookie and setting the 
        /// context and thread principle object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void AuthenticateRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var http = new HttpContextWrapper(app.Context);

            //we need to determine if the path being requested is an rebel path, if not don't do anything
            var settings = RebelSettings.GetSettings();
            var backOfficeRoutePath = string.Concat(settings.RebelPaths.BackOfficePath, "/");
            var installerRoutePath = string.Concat("Install", "/");

            //Retreive Route Url
            var routeUrl = "";
            var routeData = RouteTable.Routes.GetRouteData(http);
            if (routeData != null) {
                var route = routeData.Route as Route;
                if(route != null) {
                    routeUrl = route.Url;
                }
            }

            if (routeUrl.StartsWith(installerRoutePath, StringComparison.CurrentCultureIgnoreCase) ||
                routeUrl.StartsWith(backOfficeRoutePath, StringComparison.CurrentCultureIgnoreCase))
            {
                var ticket = http.GetRebelAuthTicket();
                if(ticket != null && !ticket.Expired && http.RenewRebelAuthTicket()) 
                {   
                    //create the Rebel user identity 
                    var identity = new RebelBackOfficeIdentity(ticket);

                    //set the principal object
                    var principal = new GenericPrincipal(identity, identity.Roles);

                    app.Context.User = principal;
                    Thread.CurrentPrincipal = principal;
                }
            }
        }

        public static RebelBackOfficeIdentity GetRebelBackOfficeIdentity(HttpContextBase http)
        {
            var ticket = http.GetRebelAuthTicket();
            if (ticket != null && !ticket.Expired && http.RenewRebelAuthTicket())
            {
                return new RebelBackOfficeIdentity(ticket);
            }
            return null;
        }
    }
}