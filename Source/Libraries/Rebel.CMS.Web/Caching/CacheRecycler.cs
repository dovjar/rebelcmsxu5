using System.Threading;
using System.Threading.Tasks;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Cms.Web.Caching
{
    /// <summary>
    /// This object is used for when nodes and templates are saved
    /// When a node is saved the background thread will only wam the relevant nodes
    /// </summary>
    public class CacheRecycler
    {
        private readonly string _host;
        private readonly IFrameworkCaches _caches;

        public CacheRecycler(string host, IFrameworkCaches caches)
        {
            _host = host;
            _caches = caches;
        }

        public void RecycleCacheFor(TypedEntity entity)
        {
            string niceUrl = entity.NiceUrl();

            RemoveFromLimitedProvider(entity, _caches, niceUrl);
            RemoveFromExtendedProvider(entity, _caches, niceUrl);

            RegenerateCache(niceUrl);
        }

        private static void RemoveFromExtendedProvider(TypedEntity entity, IFrameworkCaches caches, string niceUrl)
        {
            caches.ExtendedLifetime.RemoveWhereKeyMatches<UrlCacheKey>(x => x.EntityId == entity.Id);
            caches.ExtendedLifetime.RemoveWhereKeyContainsString(niceUrl);
        }

        private static void RemoveFromLimitedProvider(TypedEntity entity, IFrameworkCaches caches, string niceUrl)
        {
            caches.LimitedLifetime.RemoveWhereKeyMatches<UrlCacheKey>(x => x.EntityId == entity.Id);
            caches.LimitedLifetime.RemoveWhereKeyContainsString(niceUrl);
        }

        private delegate void AsyncMethodCaller(string node);

        private void RegenerateCache(string url)
        {
            var method = new AsyncMethodCaller(GenerateIndexesFor);
            method.BeginInvoke(url, null, null);
        }

        private void GenerateIndexesFor(string node)
        {
            Task.Factory.StartNew(() =>
                                        {
                                            var generator = new CacheWarmer(_host);
                                            generator.TraverseFrom(node);
                                        });
        }
    }
}