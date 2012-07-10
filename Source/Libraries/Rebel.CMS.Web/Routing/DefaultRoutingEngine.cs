using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.Context;
using Rebel.Framework;
using Rebel.Framework.Diagnostics;

using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Hive.RepositoryTypes;
using Rebel.Hive;
namespace Rebel.Cms.Web.Routing
{
    using Rebel.Hive.ProviderGrouping;

    /// <summary>
    /// A utility for resolving urls and looking up entities by URL
    /// </summary>
    public class DefaultRoutingEngine : IRoutingEngine
    {
        private readonly IRebelApplicationContext _applicationContext;
        private readonly HttpContextBase _httpContext;
        public const string DomainListKey = "domain-list";
        public const string ContentUrlKey = "content-url";
        public const string HostnameUrlKey = "hostname-url";
        public const string EntityMappedKey = "entity-url";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRoutingEngine"/> class.
        /// </summary>
        /// <param name="appContext">The routable request context.</param>
        /// <param name="httpContext"></param>
        public DefaultRoutingEngine(IRebelApplicationContext appContext, HttpContextBase httpContext)
        {
            _applicationContext = appContext;
            _httpContext = httpContext;
        }

        /// <summary>
        /// Clears the cache, removes domain cache items and content-url items
        /// </summary>
        /// <param name="clearDomains">true to clear all domain cache</param>
        /// <param name="clearForIds">will clear the URL</param>
        /// <param name="clearMappedUrls">will clear the cache for all URLs mapped to entities</param>
        /// <param name="clearGeneratedUrls">clears cache for all generated urls</param>
        /// <param name="clearAll">Clears all cache</param>
        public void ClearCache(bool clearDomains = false, HiveId[] clearForIds = null, bool clearMappedUrls = false, bool clearGeneratedUrls = false, bool clearAll = false)
        {
            if (clearAll)
            {
                ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(DomainListKey);
                ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(ContentUrlKey + ".+");
                ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(HostnameUrlKey + ".+");
                ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(EntityMappedKey + ".+");
            }
            else
            {
                if (clearMappedUrls)
                {
                    ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(EntityMappedKey + ".+");
                }
                if (clearDomains)
                {
                    ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(DomainListKey);
                }
                if (clearGeneratedUrls)
                {
                    ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(ContentUrlKey + ".+");
                    ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(HostnameUrlKey + ".+");
                }
                if (clearForIds != null && clearForIds.Any())
                {
                    foreach (var id in clearForIds)
                    {
                        ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(Regex.Escape(ContentUrlKey + id) + ".*");
                        ApplicationContext.FrameworkContext.ApplicationCache.InvalidateItems(Regex.Escape(HostnameUrlKey + id) + ".*");
                    }
                }
            }
        }

        /// <summary>
        /// Finds a TypedEntity based on the Uri
        /// </summary>
        /// <param name="fullUrlIncludingDomain"></param>
        /// <param name="revisionStatusType"></param>
        /// <returns></returns>
        public EntityRouteResult FindEntityByUrl(Uri fullUrlIncludingDomain, RevisionStatusType revisionStatusType = null)
        {
            Mandate.ParameterNotNull(fullUrlIncludingDomain.Scheme, "Scheme");

            var statusTypeCacheKey = (revisionStatusType != null) ? revisionStatusType.Alias : string.Empty;

            //cache key is full uri except query string and revision status
            var cacheKey = EntityMappedKey + "-" + fullUrlIncludingDomain.GetLeftPart(UriPartial.Path) + "-" + statusTypeCacheKey;

            //TODO: We need to change how the NiceUrls are cached because if we store them in one entry as a dictionary in cache, then
            // we can do a reverse lookup to see if we've already generated the URL for the entity which may match the fullUrlIncludingDomain,
            // if so, then all we have to do is return the entity with the cached ID.

            return ApplicationContext.FrameworkContext.ApplicationCache
                .GetOrCreate(cacheKey, () =>
                    {
                        using (DisposableTimer.Start(timer =>
                            LogHelper.TraceIfEnabled<DefaultRoutingEngine>("FindEntityByUrl (not from cache) for URL {0} took {1}ms", () => fullUrlIncludingDomain, () => timer)))
                        {
                            ReadonlyGroupUnitFactory<IContentStore> persistenceManager;

                            using (DisposableTimer.TraceDuration<DefaultRoutingEngine>("FindEntityByUrl: Getting a reader", "FindEntityByUrl: Got a reader"))
                                persistenceManager = ApplicationContext.Hive.GetReader<IContentStore>(fullUrlIncludingDomain);

                            if (persistenceManager != null)
                            {
                                IReadonlyGroupUnit<IContentStore> readonlyGroupUnit;
                                using (DisposableTimer.TraceDuration<DefaultRoutingEngine>("FindEntityByUrl: Opening a reader", "FindEntityByUrl: Opened a reader"))
                                    readonlyGroupUnit = persistenceManager.CreateReadonly();

                                using (var uow = readonlyGroupUnit)
                                {
                                    //first, lets check if it's an ID URL
                                    var trimmedAppPath = _httpContext.Request.ApplicationPath.TrimEnd('/');
                                    var appPathLength = trimmedAppPath.Length;

                                    // gate-check for the incoming url because some code (e.g. the alt-template checker) could be unaware of vdirs etc.
                                    var absolutePath = fullUrlIncludingDomain.AbsolutePath;
                                    var path = (absolutePath.Length < appPathLength) ? absolutePath : absolutePath.Substring(appPathLength, absolutePath.Length - appPathLength);

                                    var urlId = HiveId.TryParse(path.TrimStart('/'));
                                    if (urlId.Success && urlId.Result.ProviderGroupRoot != null)
                                    {
                                        LogHelper.TraceIfEnabled<DefaultRoutingEngine>("In FindEntityByUrl: Resolving entity by Id URL (Id: {0} ", () => urlId.Result.ToFriendlyString());
                                        try
                                        {
                                            //var entityById = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(urlId.Result, revisionStatusType);
                                            var entityById = uow.Repositories.OfRevisionType(revisionStatusType).InIds(urlId.Result.AsEnumerableOfOne()).FirstOrDefault();
                                            if (entityById == null)
                                            {
                                                LogHelper.Warn<DefaultRoutingEngine>("In FindEntityByUrl: Resolving entity by Id URL failed (Id: {0} ", urlId.Result.ToFriendlyString());
                                                return null;
                                            }
                                            return new HttpRuntimeCacheParameters<EntityRouteResult>(new EntityRouteResult(entityById, EntityRouteStatus.SuccessById));
                                        }
                                        catch (ArgumentException)
                                        {
                                            //this occurs if the Id parsed but 'not really'
                                            return null;
                                        }
                                    }

                                    TypedEntity lastItemFound;
                                    //is the current requesting hostname/port in our list ?
                                    if (DomainList.ContainsHostname(fullUrlIncludingDomain.Authority))
                                    {
                                        //domain found so get the first item assigned to this domain
                                        LogHelper.TraceIfEnabled<DefaultRoutingEngine>("In FindEntityByUrl: Resolving entity by Domain URL {0}", () => fullUrlIncludingDomain.Authority);
                                        var hostnameEntry = DomainList[fullUrlIncludingDomain.Authority];
                                        //lastRevisionFound = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(hostnameEntry.ContentId, revisionStatusType);
                                        lastItemFound = uow.Repositories.OfRevisionType(revisionStatusType).InIds(hostnameEntry.ContentId.AsEnumerableOfOne()).FirstOrDefault();
                                        Mandate.That(lastItemFound != null, x => new InvalidOperationException("Could not find an entity with a revision status of '" + revisionStatusType.Alias + "', having a hostname '" + fullUrlIncludingDomain.Authority + "' and id: " + hostnameEntry.ContentId));
                                    }
                                    else
                                    {
                                        //no domain found for the current request, so we need to find the first routable node that doesn't require a domain
                                        LogHelper.TraceIfEnabled<DefaultRoutingEngine>("In FindEntityByUrl: Resolving entity by Non-Domain URL");
                                        //var root = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(FixedHiveIds.ContentVirtualRoot, revisionStatusType);
                                        //Mandate.That(root != null, x => new InvalidOperationException("Could not find the content root"));
                                        var domainListIds = DomainList.Select(d => d.ContentId).ToArray();

                                        var firstLevelRelations =
                                            uow.Repositories.GetChildRelations(FixedHiveIds.ContentVirtualRoot, FixedRelationTypes.DefaultRelationType).OrderBy(
                                                x => x.Ordinal).ToArray();

                                        //try to find a first level node that doesn't exist in our domain list
                                        var firstNonHostnameEntity = firstLevelRelations.FirstOrDefault(x => !domainListIds.Contains(x.DestinationId));

                                        // Issue #U5-112
                                        // If we have found no entities that are NOT assigned to a domain, given that we have already tried to find
                                        // content matching the current request's domain, we cannot route any content, therefore return null

                                        // Also return null if there is no content
                                        if (firstNonHostnameEntity == null || !firstLevelRelations.Any())
                                            return null;

                                        var idToUse = firstNonHostnameEntity.DestinationId;
                                        using (DisposableTimer.TraceDuration<DefaultRoutingEngine>("FindEntityByUrl: Querying for " + idToUse, "FindEntityByUrl: Query"))
                                            lastItemFound = uow.Repositories.OfRevisionType(revisionStatusType).InIds(idToUse.AsEnumerableOfOne()).FirstOrDefault();

                                        ////if there is no first level node anywhere, then there is no content. Show a friendly message
                                        //if (firstNonHostnameEntity == null && !firstLevelRelations.Any())
                                        //    return null;

                                        ////if we have a first level node not assigned to a domain, use the first, otherwise if all nodes are assigned to domains, then just use the first
                                        //var idToUse = firstNonHostnameEntity == null ? firstLevelRelations.First().DestinationId : firstNonHostnameEntity.DestinationId;
                                        //lastItemFound = uow.Repositories.OfRevisionType(revisionStatusType).InIds(idToUse.AsEnumerableOfOne()).FirstOrDefault();
                                    }


                                    // Now we will have the path from the current application root like:
                                    //      /this/is/a/path/to/a/document
                                    // Now we need to walk down the tree
                                    if (lastItemFound != null && !string.IsNullOrWhiteSpace(path) && path != "/")
                                    {
                                        using (DisposableTimer.TraceDuration<DefaultRoutingEngine>("FindEntityByUrl: Calling GetEntityByPath for " + lastItemFound.Id + " " + path, "FindEntityByUrl: GetEntityByPath"))
                                            lastItemFound = uow
                                                .Repositories
                                                .GetEntityByPath<TypedEntity>(lastItemFound.Id,
                                                                              path,
                                                                              revisionStatusType,
                                                                              true);
                                    }

                                    if (lastItemFound == null)
                                        return new HttpRuntimeCacheParameters<EntityRouteResult>(
                                                new EntityRouteResult(null, EntityRouteStatus.FailedNotFoundByName));

                                    return new HttpRuntimeCacheParameters<EntityRouteResult>(
                                        new EntityRouteResult(lastItemFound, EntityRouteStatus.SuccessWithoutHostname));
                                }
                            }

                            return null;
                        }
                    });
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// This takes into account the current host name in the request. If the current host name matches a host name
        /// defined in the domain list for the entity being looked up, then the hostname of the current request will 
        /// be used, otherwise the primary (first ordinal) domain will be used for the url.
        /// </remarks>
        public UrlResolutionResult GetUrlForEntity(TypedEntity entity)
        {
            Mandate.ParameterNotNull(entity, "entity");

            using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
            {
                if (entity.IsContent(uow))
                    return GetContentUrl(entity);

                if (entity.IsMedia(uow))
                    return GetMediaUrl(entity);
            }
            throw new NotSupportedException("Unknown entity type");
        }

        /// <summary>
        /// Returns the list of domains and their asigned hive ids
        /// </summary>
        /// <remarks>
        /// The domain list is stored in application cache
        /// </remarks>
        public ReadonlyHostnameCollection DomainList
        {
            get
            {
                return ApplicationContext
                    .FrameworkContext.ApplicationCache
                    .GetOrCreate(DomainListKey,
                                 () =>
                                 {
                                     using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
                                     {
                                         //Gets all latest hostname revisions with it's relation to content

                                         //TODO: This needs to be changed so that we can request the latest hostnames
                                         //in a query, otherwise this may end up returning A LOT of data
                                         var allHostnameEntities = (from e in uow.Repositories.QueryContext.Query()
                                                                    where e.EntitySchema.Alias == HostnameSchema.SchemaAlias
                                                                    select e).ToArray();
                                         var latestHostnames = (from revision in allHostnameEntities
                                                                group revision by revision.Id
                                                                    into g
                                                                    select g.OrderByDescending(t => t.UtcStatusChanged).First()).ToArray();

                                         var d = new HostnameCollection();
                                         foreach (var e in latestHostnames)
                                         {
                                             var name = e.Attribute<string>(HostnameSchema.HostnameAlias);

                                             var relation = uow.Repositories.GetParentRelations(e.Id, FixedRelationTypes.HostnameRelationType).FirstOrDefault();

                                             // Check if the relation is null, so that we account for hostnames that are orphaned and not linked to anywhere
                                             if (relation != null)
                                             {
                                                 if (d.Any(x => x.Hostname == name))
                                                     throw new InvalidOperationException(string.Format("Hostname '{0}' is already assigned to another node.", name));

                                                 d.Add(new HostnameEntry(name, relation.SourceId, relation.Ordinal));
                                             }
                                         }
                                         return new HttpRuntimeCacheParameters<ReadonlyHostnameCollection>(new ReadonlyHostnameCollection(d));
                                     }
                                 });
            }
        }

        public IRebelApplicationContext ApplicationContext
        {
            get
            {
                return _applicationContext;
            }
        }

        /// <summary>
        /// Resolves the url for the specified entity with its full domain paths based on all hostnames assigned.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>All URLs for the entity based on all of its hostnames assigned, if no hostnames are assigned in this entities branch, then a null value is returned</returns>
        public UrlResolutionResult[] GetAllUrlsForEntity(TypedEntity entity)
        {
            //return from cache if its there, otherwise go get it
            return ApplicationContext.FrameworkContext.ApplicationCache
                .GetOrCreate(HostnameUrlKey + entity.Id, () =>
                    {
                        using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
                        {
                            var ancestorsOrSelf =
                                uow.Repositories.GetAncestorsOrSelf(entity, FixedRelationTypes.DefaultRelationType)
                                    .Where(x => !x.Id.IsSystem()).Cast<TypedEntity>().ToArray();

                            // (APN Oct 2011) In latest codebase ancestors are returned in reverse document order, same as Xml
                            var reverse = ancestorsOrSelf.Reverse();

                            var nonDomainurl = GetNonDomainUrl(reverse);
                            var domainUrls = GetDomainUrls(reverse);

                            var results = new[] { nonDomainurl }
                                .Union(domainUrls)
                                .Where(x => x != null)
                                .ToList();

                            //if there's no results, then it can't be routed to as it requires a domain
                            if (!results.Any())
                            {
                                results.Add(new UrlResolutionResult(string.Empty, UrlResolutionStatus.FailedRequiresHostname));
                            }

                            return new HttpRuntimeCacheParameters<UrlResolutionResult[]>(results.ToArray());
                        }
                    });
        }

        /// <summary>
        /// Returns a list of URLs with the domains assigned based on the list of ancestorsOrSelf. If no domain is assigned to the branch that the entity exists in a null value is returned.
        /// </summary>
        /// <param name="ancestorsOrSelf">The ancestorsOrSelf list to create the URL for</param>
        /// <returns></returns>        
        protected IEnumerable<UrlResolutionResult> GetDomainUrls(IEnumerable<TypedEntity> ancestorsOrSelf)
        {
            var reversed = ancestorsOrSelf.Reverse();
            var parts = new List<string>();

            //need to figure out a domain assigned to this node
            foreach (var a in reversed)
            {
                if (DomainList.ContainsContentId(a.Id))
                {
                    //ok, we've found a node with a domain assigned, return a list of URLs with all domains assigned to this id
                    return DomainList[a.Id]
                        .Select(d =>
                            new UrlResolutionResult(
                                (d.Hostname + _httpContext.Request.ApplicationPath.TrimEnd('/') + "/" + string.Join("/", Enumerable.Reverse(parts).ToArray())).TrimEnd('/'),
                                UrlResolutionStatus.SuccessWithHostname))
                        .ToArray();
                }
                else
                {
                    var urlAlias = a.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName");
                    Mandate.That(!urlAlias.IsNullOrWhiteSpace(), x => new InvalidOperationException("UrlName cannot be empty"));
                    parts.Add(urlAlias);
                }
            }

            //this will occur if no domains are found
            return Enumerable.Empty<UrlResolutionResult>();
        }

        /// <summary>
        /// Returns the non-domain URL for the 'self' node in ancestorsOrSelf or null if this node cannot
        /// exist in a non-domain branch.
        /// </summary>
        /// <param name="ancestorsOrSelf"></param>
        /// <returns></returns>
        protected UrlResolutionResult GetNonDomainUrl(IEnumerable<TypedEntity> ancestorsOrSelf)
        {
            using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
            {
                var domainListIds = DomainList.Select(x => x.ContentId);

                var contentRootExists = uow.Repositories.Exists<TypedEntity>(FixedHiveIds.ContentVirtualRoot);
                Mandate.That(contentRootExists, x => new NullReferenceException("No ContentVirtualRoot exists!"));

                var firstLevelRelations = uow.Repositories
                    .GetChildRelations(FixedHiveIds.ContentVirtualRoot, FixedRelationTypes.DefaultRelationType)
                    .OrderBy(x => x.Ordinal)
                    .ToArray();

                var firstLevelIds = firstLevelRelations.Select(x => x.DestinationId).ToArray();
                var ancestorIds = ancestorsOrSelf.Select(x => x.Id).ToArray();

                //returns true if the branch passed in is part of the first branch found that doesn't have a domain assigned
                Func<IEnumerable<HiveId>, bool> isFirstBranchNotAssignedADomain = branchIds =>
                    {
                        var firstLevelIdsWithoutDomains = firstLevelIds.Where(x => !domainListIds.Contains(x));
                        var firstBranchWithoutDomain = firstLevelIdsWithoutDomains.First();
                        return branchIds.Contains(firstBranchWithoutDomain);
                    };

                //if there's only one node, 
                // OR the current branch isnt't assigned to a domain and its part of the first branch that isn't assigned to a domain, 
                // OR all first level nodes are assigned to domains and the requesting node is part of the very first branch

                if (firstLevelRelations.Count() == 1
                    || (!domainListIds.ContainsAny(ancestorIds) && isFirstBranchNotAssignedADomain(ancestorIds))
                    || (domainListIds.ContainsAll(firstLevelIds) && ancestorIds.Contains(firstLevelIds.First())))
                {
                    //if there's only one node, just pick it, otherwise get the first without hostnames
                    var firstLevelRelationWithoutHostname = firstLevelRelations.Count() == 1
                        ? firstLevelRelations.First()
                        : firstLevelRelations.FirstOrDefault(x => !domainListIds.Contains(x.DestinationId));

                    if (firstLevelRelationWithoutHostname == null)
                    {
                        // This can happen if multiple roots exist, each with a hostname assigned
                        // Therefore the current entity does not have a non-domain url, but that's
                        // still a valid scenario, just not one that GetNonDomainUrl can handle
                        return new UrlResolutionResult(string.Empty, UrlResolutionStatus.OnlyDomainUrlsValid);

                        ////NOTE: I don't think this ever will happen! The initial thought wa that the relations api will ony return the latest published versions but this is not true, they return the latest version

                        ////can't find a first routable branch, so must not be published
                        //return new UrlResolutionResult(string.Empty, UrlResolutionStatus.FailedNotPublished);
                    }

                    //ok, now we can give it a URL

                    var appPath = _httpContext.Request.ApplicationPath.TrimEnd('/');
                    var level = 0;
                    var urlBuilder = new StringBuilder();
                    urlBuilder.Append(appPath);

                    foreach (var e in ancestorsOrSelf)
                    {
                        if (level > 0)
                        {
                            var urlAlias = e.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName");
                            if (!string.IsNullOrEmpty(urlAlias))
                            {
                                urlBuilder.Append("/");
                                urlBuilder.Append(urlAlias);
                            }
                        }
                        level++;
                    }

                    var url = "/" + urlBuilder.ToString().Trim('/');

                    return new UrlResolutionResult(url, UrlResolutionStatus.SuccessWithoutHostname);
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the content URL.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        /// <remarks>
        /// This takes into account the current host name in the request. If the current host name matches a host name
        /// defined in the domain list for the entity being looked up, then the hostname of the current request will 
        /// be used, otherwise the primary (first ordinal) domain will be used for the url.
        /// </remarks>
        protected UrlResolutionResult GetContentUrl(TypedEntity entity)
        {
            Mandate.ParameterNotNull(entity, "entity");

            //the cache key must include the host since it must be unique depending on what the current host/port is
            var cacheKey = ContentUrlKey + entity.Id + _httpContext.Request.Url.Authority;

            //return from cache if its there, otherwise go get it
            return ApplicationContext.FrameworkContext.ApplicationCache
                .GetOrCreate(cacheKey, () =>
                    {
                        using (var uow = ApplicationContext.Hive.OpenReader<IContentStore>())
                        {
                            //need to check if this node is in a branch with an assigned domain       

                            // get the ancestor ids except system ones
                            var allAncestorIds = uow.Repositories.GetAncestorIds(entity.Id, FixedRelationTypes.DefaultRelationType).ToArray();

                            var ancestorIds = allAncestorIds
                                .Where(x => !x.IsSystem())
                                .ToArray();

                            // load the published ancestors
                            var ancestors = uow.Repositories
                                .OfRevisionType(FixedStatusTypes.Published)
                                .InIds(ancestorIds)
                                .ToArray();

                            // add current one to mimic "ancestors or self" but without reloading it
                            ancestors = entity.AsEnumerableOfOne().Concat(ancestors).ToArray();

                            // (APN Oct 2011) In latest codebase ancestors are returned in reverse document order, same as Xml
                            var reverse = ancestors.Reverse();

                            var nonDomainUrl = GetNonDomainUrl(reverse);
                            if (nonDomainUrl != null && nonDomainUrl.Status != UrlResolutionStatus.OnlyDomainUrlsValid)
                            {
                                return new HttpRuntimeCacheParameters<UrlResolutionResult>(nonDomainUrl);
                            }

                            //this is a branch that has a hostname assigned, so need to route by that
                            var domainUrls = GetDomainUrls(reverse);
                            if (domainUrls == null || !domainUrls.Any())
                            {
                                return new HttpRuntimeCacheParameters<UrlResolutionResult>(new UrlResolutionResult(string.Empty, UrlResolutionStatus.FailedRequiresHostname));
                            }

                            //now we need to determine what the current authority is, and if it matches one of the domain URLs, then we can remove the authority since
                            //we'll be returning a relative URL to the current request

                            var relativeHostUrl = domainUrls.Where(x => x.Url.StartsWith(_httpContext.Request.Url.Authority)).FirstOrDefault();
                            if (relativeHostUrl != null)
                            {
                                //return a relative URL for the current request authority
                                //var url = relativeHostUrl.Url.Substring(0, _httpContext.Request.Url.Authority.Length);
                                var url = relativeHostUrl.Url.Substring(_httpContext.Request.Url.Authority.Length, relativeHostUrl.Url.Length - _httpContext.Request.Url.Authority.Length);
                                return new HttpRuntimeCacheParameters<UrlResolutionResult>(new UrlResolutionResult(url, UrlResolutionStatus.SuccessWithoutHostname));
                            }

                            //if the request has come in on a domain that isn't in this entity's domain urls, then return the first domain url assigned to it
                            return new HttpRuntimeCacheParameters<UrlResolutionResult>(domainUrls.First());

                        }
                    });
        }

        /// <summary>
        /// Gets the media URL.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        protected UrlResolutionResult GetMediaUrl(TypedEntity entity)
        {
            Mandate.ParameterNotNull(entity, "entity");

            var requestContext = new RequestContext(_httpContext, new RouteData());
            var urlHelper = new UrlHelper(requestContext);

            return new UrlResolutionResult(urlHelper.GetMediaUrl(entity), UrlResolutionStatus.SuccessWithoutHostname);
        }
    }
}
