using System.Threading;
using Rebel.Cms.Web.Mvc;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Cms.Web.Caching
{
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

        private static void RemoveFromExtendedProvider(string templateName, IFrameworkCaches caches)
        {
            caches.ExtendedLifetime.RemoveWhereKeyContainsString(templateName);
        }

        private static void RemoveFromLimitedProvider(string templateName, IFrameworkCaches caches)
        {
            caches.LimitedLifetime.RemoveWhereKeyContainsString(templateName);
        }

        private delegate void AsyncMethodCaller(string node);

        private void RegenerateCache(string url)
        {
            var method = new AsyncMethodCaller(GenerateIndexesFor);
            method.BeginInvoke(url, null, null);
        }

        private void RegenerateCache()
        {
            var method = new AsyncMethodCaller(GenerateIndexesFor);
            method.BeginInvoke("/", null, null);
        }

        private void GenerateIndexesFor(string node)
        {
            var thread = new Thread(() =>
                                        {
                                            var generator = new IndexRegenerator(_host);
                                            generator.Go(node);
                                        }) {IsBackground = true};
            thread.Start();
        }

        public void RecycleCacheFor(string templateName)
        {
            RemoveFromLimitedProvider(templateName, _caches);
            RemoveFromExtendedProvider(templateName, _caches);
            RegenerateCache();
        }
    }
}