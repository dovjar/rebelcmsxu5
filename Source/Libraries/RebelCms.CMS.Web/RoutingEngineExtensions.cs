using System;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Routing;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Cms.Web;
using RebelCms.Hive.RepositoryTypes;
using RebelCms.Hive;

namespace RebelCms.Cms.Web
{
    using RebelCms.Cms.Web.Caching;
    using RebelCms.Framework.Caching;

    public static class RoutingEngineExtensions
    {
        /// <summary>
        /// Resolves the url for the specified id.
        /// </summary>
        /// <param name="routingEngine"></param>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static string GetUrl(this IRoutingEngine routingEngine, HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var applicationContext = DependencyResolver.Current.GetService<IRebelCmsApplicationContext>();

            var hive = applicationContext.Hive.GetReader<IContentStore>(id.ToUri());
            if (hive != null)
            {
                var key = CacheKey.Create(new UrlCacheKey(id));
                var item = hive.HiveContext.GenerationScopedCache.GetOrCreate(key, () =>
                    {
                        using (var uow = hive.CreateReadonly())
                        {
                            var entity = uow.Repositories.Get<TypedEntity>(id);
                            if (entity == null)
                                throw new NullReferenceException("Could not find a TypedEntity in the repository for a content item with id " + id.ToFriendlyString());

                            return routingEngine.GetUrlForEntity(entity);
                        }
                    });

                var urlResult = item.Value.Item;

                ////return from scoped cache so we don't have to lookup in the same session
                //var urlResult = applicationContext.FrameworkContext.ScopedCache.GetOrCreateTyped<UrlResolutionResult>("nice-url-" + id, () =>
                //    {
                //        using (var uow = hive.CreateReadonly())
                //        {
                //            var entity = uow.Repositories.Get<TypedEntity>(id);
                //            if (entity == null)
                //                throw new NullReferenceException("Could not find a TypedEntity in the repository for a content item with id " + id.ToFriendlyString());

                //            return routingEngine.GetUrlForEntity(entity);
                //        }
                //    });
                if (urlResult.IsSuccess())
                {
                    return urlResult.Url;
                }

                //return a hashed url with the status
                return "#" + urlResult.Status;
            }

            return id.ToString();
        }
    }
}