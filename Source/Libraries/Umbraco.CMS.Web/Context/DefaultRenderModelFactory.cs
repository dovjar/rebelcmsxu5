using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using Umbraco.Cms.Web.Model;
using Umbraco.Cms.Web.Routing;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Constants;

namespace Umbraco.Cms.Web.Context
{
    using Umbraco.Cms.Web.Model.BackOffice.Editors;
    using Umbraco.Cms.Web.Security;
    using Umbraco.Cms.Web.Security.Permissions;
    using Umbraco.Framework.Persistence.Model.Versioning;
    using Umbraco.Framework.Security;
    using Umbraco.Hive;
    using Umbraco.Hive.RepositoryTypes;

    /// <summary>
    /// The default IRenderModelFactory
    /// </summary>
    public class DefaultRenderModelFactory : IRenderModelFactory
    {
        private const string ResponseLifecycleCacheKey = "DefaultRenderModelFactory-b23r98ysjdnfsuk";
        private readonly IUmbracoApplicationContext _applicationContext;

        public DefaultRenderModelFactory(IUmbracoApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Creates or returns an existing IUmbracoRenderModel based on the request data
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="rawUrl"></param>
        /// <returns></returns>
        public IUmbracoRenderModel Create(HttpContextBase httpContext, string rawUrl)
        {
            using (DisposableTimer.TraceDuration<DefaultRenderModelFactory>("Begin find/create context", "End find/create"))
            {
                return _applicationContext.FrameworkContext.ScopedCache.GetOrCreateTyped<IUmbracoRenderModel>(ResponseLifecycleCacheKey, () =>
                    {
                        LogHelper.TraceIfEnabled<DefaultRenderModelFactory>("IUmbracoRenderModel requires creation");
                        var model = new UmbracoRenderModel(_applicationContext, () => ResolveItem(httpContext, rawUrl));
                        return model;
                    });
            }
        }

        private Content ResolveItem(HttpContextBase httpContext, string requestUrl)
        {
            //check if the RouteDebugger is enabled, if it is, we just return the content virtual root... or any other full TypedEntity will work.
            //this however will show a no template found page, but still allows us to debug the route.
            if (ConfigurationManager.AppSettings["RouteDebugger:Enabled"] == "true")
                return new Content(FixedEntities.ContentVirtualRoot);
            
            using (DisposableTimer.TraceDuration<DefaultRenderModelFactory>("Begin ResolveItem", "End ResolveItem"))
            {
                //Sorry, gonna need to use the resolver here because IRoutingEngine is registered with IoC as per-request
                //whereas the model factory is a singleton (SD)
                //TODO: Fix this by adding IRoutingEngine as a parameter to relevant methods (APN)
                var urlUtility = DependencyResolver.Current.GetService<IRoutingEngine>();

                //var revisionStatusType = httpContext.Request.QueryString["revisionStatusType"];
                //var actualStatusType = (revisionStatusType.IsNullOrWhiteSpace()) ? FixedStatusTypes.Published : new RevisionStatusType(revisionStatusType, revisionStatusType);

                bool isPreview = false;
                bool.TryParse(httpContext.Request.QueryString[ContentEditorModel.PreviewQuerystringKey], out isPreview);

                Content content = null;
                var fullUrlIncludingDomain = httpContext.Request.Url;
                var result = urlUtility.FindEntityByUrl(fullUrlIncludingDomain, isPreview ? null : FixedStatusTypes.Published);
                var altTemplate = "";

                if(result == null || result.Status == EntityRouteStatus.FailedNotFoundByName)
                {
                    // Couldn't resolve node, so see if it's an alt template request instead
                    // TODO: this detection could be changed to check if a template exists with the discovered alias, before calling FindEntityByUrl to improve perf.
                    if (!fullUrlIncludingDomain.AbsolutePath.Trim('/').IsNullOrWhiteSpace())
                    {
                        var url = fullUrlIncludingDomain.ToString().TrimEnd('/').Replace("/?", "?");
                        var templateAlias = url.Substring(url.LastIndexOf('/') + 1);
                        if (!string.IsNullOrWhiteSpace(fullUrlIncludingDomain.Query))
                            templateAlias = templateAlias.Replace(fullUrlIncludingDomain.Query, "");
                        var newUrl = url.Substring(0, url.LastIndexOf('/'));

                        Uri parsedNewUrl;
                        var isValidNewUrl = Uri.TryCreate(newUrl, UriKind.RelativeOrAbsolute, out parsedNewUrl);
                        if (isValidNewUrl)
                        {
                            var tempResult = urlUtility.FindEntityByUrl(parsedNewUrl, isPreview ? null : FixedStatusTypes.Published);

                            if (tempResult != null && tempResult.IsRoutable())
                            {
                                var tempContent = _applicationContext.FrameworkContext.TypeMappers.Map<Content>(tempResult.RoutableEntity);
                                if (tempContent.AlternativeTemplates.Any(x => x.Alias != null && x.Alias.Equals(templateAlias, StringComparison.InvariantCultureIgnoreCase)))
                                {
                                    content = tempContent;
                                    altTemplate = templateAlias;
                                    result = tempResult;
                                }
                            }
                        }
                    }
                }

                if (result != null && result.IsRoutable())
                {
                    var viewPermissionId = new ViewPermission().Id;
                    if (isPreview)
                    {
                        // First get the user from the backoffice membership provider, which may be different than from the site running
                        var user = BackOfficeAuthenticationModule.GetUmbracoBackOfficeIdentity(httpContext);

                        // Check user is allowed to preview
                        var userId = (user != null && user.IsAuthenticated)
                            ? user.Id
                            : HiveId.Empty;

                        PermissionResults permissionResult = this._applicationContext.Security.Permissions.GetEffectivePermissions(userId, result.RoutableEntity.Id, viewPermissionId, new CreatePermission().Id);

                        if (!permissionResult.AreAllAllowed())
                        {
                            // Redirect to insufficient permissions pages
                            throw new HttpException((int)global::System.Net.HttpStatusCode.Forbidden, "You do not have permission to view this resource.");
                        }
                    }

                    var resultId = result.RoutableEntity.Id.AsEnumerableOfOne().ToArray();

                    //using (var contentUow = _applicationContext.Hive.OpenReader<IContentStore>())
                    //using (var securityUow = _applicationContext.Hive.OpenReader<ISecurityStore>())
                    //    resultId = resultId.FilterAnonymousWithPermissions(_applicationContext.Security, contentUow, securityUow, viewPermissionId).ToArray();

                    if (resultId.Length == 0)
                        throw new HttpException((int)global::System.Net.HttpStatusCode.Forbidden, "You do not have permission to view this resource.");

                    if(content == null)
                        content = _applicationContext.FrameworkContext.TypeMappers.Map<Content>(result.RoutableEntity);

                    // Swap out the current template if an alt template is in querystring, and current node has an alternative template with that alias
                    if (string.IsNullOrWhiteSpace(altTemplate) && !string.IsNullOrWhiteSpace(httpContext.Request.QueryString["altTemplate"]))
                        altTemplate = httpContext.Request.QueryString["altTemplate"];

                    if (!string.IsNullOrWhiteSpace(altTemplate))
                        content.TrySwapTemplate(altTemplate);

                    return content;
                }

                LogHelper.TraceIfEnabled<DefaultRenderModelFactory>("Could not find item for route '{0}'", () => requestUrl);
                return null;
            }
        }
    }
}
