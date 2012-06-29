using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Security;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    public class PublicAccessAuthorizeAttribute : AuthorizeAttribute, IRequiresRoutableRequestContext
    {
        private IRoutableRequestContext _routableRequestContext;
        public IRoutableRequestContext RoutableRequestContext
        {
            get { return _routableRequestContext ?? (_routableRequestContext = DependencyResolver.Current.GetService<IRoutableRequestContext>()); }
            set { _routableRequestContext = value; }
        }

        protected bool AuthorizeMember(AuthorizationContext filterContext, out HiveId redirectPageId)
        {
            if (filterContext == null) 
                throw new ArgumentNullException("filterContext");

            var entityId = HiveId.Empty;
            if (filterContext.RouteData != null
                && filterContext.RouteData.DataTokens.ContainsKey("umbraco")
                && filterContext.RouteData.DataTokens["umbraco"] != null
                && filterContext.RouteData.DataTokens["umbraco"] is IUmbracoRenderModel)
            {
                var renderModel = (IUmbracoRenderModel)filterContext.RouteData.DataTokens["umbraco"];
                entityId = renderModel.CurrentNode.Id;
            }

            if (entityId == HiveId.Empty)
            {
                redirectPageId = HiveId.Empty;
                return true;
            }

            var publicAccessInfo = RoutableRequestContext.Application.Security.PublicAccess.GetNearestPublicAccessInfo(entityId);
            if (publicAccessInfo == null)
            {
                redirectPageId = HiveId.Empty;
                return true; // No public access relation found, so treat as unprotected
            }

            if (filterContext.HttpContext.User.Identity.IsAuthenticated == false)
            {
                redirectPageId = publicAccessInfo.LoginPageId;
                return false; // Not logged in so redirect to login page
            }

            var memberName = filterContext.HttpContext.User.Identity.Name;
            var member = RoutableRequestContext.Application.Security.Members.GetByUsername(memberName);
            if (member == null)
            {
                redirectPageId = publicAccessInfo.ErrorPageId;
                return false; // Someone is logged in, but is not a valid member, redirect to error page
            }

            var publicAccessStatusResult = RoutableRequestContext.Application.Security.PublicAccess.GetPublicAccessStatus(member.Groups, entityId);
            if(!publicAccessStatusResult.CanAccess)
            {
                redirectPageId = publicAccessInfo.ErrorPageId;
                return false; // Member is logged in, but doesn't have permission to access the resource, redirect to error page
            }

            redirectPageId = HiveId.Empty;
            return true; // If you get this far, you must be logged in and have permission, so allow access
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
                throw new InvalidOperationException("Cannot use the UmbracoAuthorizeAttribute within a child action cache");

            HiveId redirectPageId;
            if (!AuthorizeMember(filterContext, out redirectPageId))
            {
                if (!redirectPageId.IsNullValueOrEmpty())
                {
                    var redirectPageUrl = RoutableRequestContext.RoutingEngine.GetUrl(redirectPageId);
                    filterContext.Result = new RedirectResult(redirectPageUrl + "?ReturnUrl=" + filterContext.HttpContext.Request.RawUrl);
                    return;
                }

                throw new HttpException((int)global::System.Net.HttpStatusCode.Unauthorized,
                    "You must login to view this resource.");
            }
        }
    }
}
