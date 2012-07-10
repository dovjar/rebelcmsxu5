using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace Rebel.Cms.Web.Security
{
    public class RoleProviderAuthenticationModule : IHttpModule
    {
        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += AuthenticateRequest;
        }

        /// <summary>
        /// Authenticates the request.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void AuthenticateRequest(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var http = new HttpContextWrapper(app.Context);

            if (http.User == null || http.User is RolePrincipal) 
                return;

            var principal = new RolePrincipal(http.User.Identity);

            app.Context.User = principal;
            Thread.CurrentPrincipal = principal;
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        { }
    }
}
