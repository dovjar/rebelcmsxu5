using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;
using Rebel.Framework.Caching;
using Rebel.Framework.Diagnostics;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Cms.Web.Context
{
    using Rebel.Cms.Web.Model.BackOffice.Editors;
    using Rebel.Cms.Web.Security;
    using Rebel.Cms.Web.Security.Permissions;
    using Rebel.Framework.Persistence.Model.Versioning;
    using Rebel.Framework.Security;
    using Rebel.Hive;
    using Rebel.Hive.RepositoryTypes;

    /// <summary>
    /// The default IRenderModelFactory
    /// </summary>
    public class DefaultRenderModelFactory : IRenderModelFactory
    {
        private const int _longTime = 999;
        private readonly IRebelApplicationContext _applicationContext;
        private TimeSpan _veryLongSlidingExpiration=new TimeSpan(10, 0, 0, 0);

        public DefaultRenderModelFactory(IRebelApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <summary>
        /// Creates or returns an existing IRebelRenderModel based on the request data
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="rawUrl"></param>
        /// <returns></returns>
        public IRebelRenderModel Create(HttpContextBase httpContext, string rawUrl)
        {
            bool isPreview = false;
            bool.TryParse(httpContext.Request.QueryString[ContentEditorModel.PreviewQuerystringKey], out isPreview);

            if(isPreview)
            {
                return new RebelRenderModel(_applicationContext, () => ResolveItem(httpContext, rawUrl, isPreview));
            }
            using (DisposableTimer.TraceDuration<DefaultRenderModelFactory>("Begin find/create context", "End find/create"))
            {
                string key = string.Format("Model:{0}",rawUrl);
                return GetOrCreateRebelRenderModel(httpContext, rawUrl, key);
            }
        }

        private IRebelRenderModel GetOrCreateRebelRenderModel(HttpContextBase httpContext, string rawUrl,
                                                              string key)
        {
            var modelFromApplicationCache =
                _applicationContext.FrameworkContext.ApplicationCache.Get<RebelRenderModel>(key);

            if (modelFromApplicationCache != null)
                return modelFromApplicationCache;

            var modelFromExtendedCache = _applicationContext
                .FrameworkContext
                .Caches
                .ExtendedLifetime
                .GetOrCreate(key, () =>
                                    {
                                        LogHelper.TraceIfEnabled<DefaultRenderModelFactory>("IRebelRenderModel requires creation");
                                        return new RebelRenderModel(_applicationContext, () => ResolveItem(httpContext,rawUrl,false));
                                    }, new StaticCachePolicy(TimeSpan.FromDays(_longTime))).Value.Item;

            _applicationContext.FrameworkContext.ApplicationCache
                .Create(key, modelFromExtendedCache, _veryLongSlidingExpiration);

            return modelFromExtendedCache;
        }

        private Content ResolveItem(HttpContextBase httpContext, string requestUrl, bool isPreview)
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
                        var user = BackOfficeAuthenticationModule.GetRebelBackOfficeIdentity(httpContext);

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
