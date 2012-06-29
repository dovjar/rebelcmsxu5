using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Security;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// Ensures authorization occurs for the action or controller, if not this redirects to the correct login URL for the back-office
    /// </summary>
    /// <remarks>
    /// If the user is authorized, this renews their authentication ticket 
    /// </remarks>
    public class UmbracoAuthorizeAttribute : AuthorizeAttribute, IRequiresRoutableRequestContext
    {
        private string[] _permissions = new string[0];
        private string _idParameterName = "id";

        private IRoutableRequestContext _routableRequestContext;
        public IRoutableRequestContext RoutableRequestContext
        {
            get { return _routableRequestContext ?? (_routableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>()); }
            set { _routableRequestContext = value; }
        }

        /// <summary>
        /// Gets or sets the permissions.
        /// </summary>
        /// <value>
        /// The permissions.
        /// </value>
        public string[] Permissions
        {
            get { return _permissions; }
            set { _permissions = value; }
        }

        /// <summary>
        /// Gets or sets the name of the id field.
        /// </summary>
        /// <value>
        /// The name of the id field.
        /// </value>
        public string IdParameterName
        {
            get { return _idParameterName; }
            set { _idParameterName = value; }
        }

        /// <summary>
        /// Authorizes the user identity.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns></returns>
        protected bool AuthorizeIdentity(HttpContextBase httpContext)
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            if (!base.AuthorizeCore(httpContext))
                return false; // Basic authenticate tests

            if (!(httpContext.User.Identity is IUmbracoIdentity))
                return false; // Must be an UmbracoIdentity

            var umbracoIdentity = (IUmbracoIdentity)httpContext.User.Identity;
            if (umbracoIdentity.Roles.Contains(UmbracoInstallAuthorizeAttribute.InstallRoleName))
                return false; // Must not be a member of the Install role

            return true;
        }

        /// <summary>
        /// Authorizes the permissions.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        protected bool AuthorizePermissions(HttpContextBase httpContext, HiveId entityId = default(HiveId))
        {
            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            if (entityId == HiveId.Empty)
                entityId = FixedHiveIds.SystemRoot;

            var userId = (httpContext.User.Identity.IsAuthenticated && httpContext.User.Identity is IUmbracoIdentity)
                ? ((IUmbracoIdentity)httpContext.User.Identity).Id
                : HiveId.Empty;

            if (RoutableRequestContext == null 
                || _permissions == null 
                || _permissions.Length > 0 
                && !CheckPermissions(entityId, userId))
                return false; // Must have a valid permission or be Administrator

            return true;
        }

        private bool CheckPermissions(HiveId entityId, HiveId userId)
        {
            return RoutableRequestContext.Application.Security.Permissions.GetEffectivePermissions(userId, entityId, _permissions.Select(x => new Guid(x)).ToArray()).AreAllAllowed();
        }

        /// <summary>
        /// Called when a process requests authorization.
        /// </summary>
        /// <param name="filterContext">The filter context, which encapsulates information for using <see cref="T:System.Web.Mvc.AuthorizeAttribute"/>.</param>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="filterContext"/> parameter is null.</exception>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                throw new InvalidOperationException("Cannot use the UmbracoAuthorizeAttribute within a child action cache");

            if (!AuthorizeIdentity(filterContext.HttpContext))
            {
                throw new HttpException((int)global::System.Net.HttpStatusCode.Unauthorized,
                "You must login to view this resource.");
            }

            var routeId = (filterContext.RouteData != null && filterContext.RouteData.Values != null && filterContext.RouteData.Values.ContainsKey(IdParameterName))
                ? filterContext.RouteData.Values[IdParameterName].ToString()
                : filterContext.HttpContext.Request[IdParameterName];

            if (!AuthorizePermissions(filterContext.HttpContext, new HiveId(routeId)))
            {
                throw new HttpException((int) global::System.Net.HttpStatusCode.Forbidden,
                                        "You do not have permission to view this resource.");
            }

            // Passed authentication
            var cache = filterContext.HttpContext.Response.Cache;
            cache.SetProxyMaxAge(new TimeSpan(0L));
            cache.AddValidationCallback(CacheValidateHandler, null);
        }

        /// <summary>
        /// Caches the validate handler.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="data">The data.</param>
        /// <param name="validationStatus">The validation status.</param>
        private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
        {
            validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
        }

        /// <summary>
        /// Determines whether the specified HTTP context is authorized.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns>
        ///   <c>true</c> if the specified HTTP context is authorized; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAuthorized(HttpContextBase httpContext, HiveId entityId = default(HiveId))
        {
            return AuthorizeIdentity(httpContext) && AuthorizePermissions(httpContext, entityId);
        }
    }
}
