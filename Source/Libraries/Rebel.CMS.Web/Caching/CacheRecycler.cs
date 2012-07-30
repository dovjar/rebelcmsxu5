using System.Threading;
using System.Threading.Tasks;
using Rebel.Framework;
using Rebel.Framework.Caching;
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
        private readonly IFrameworkContext frameworkContext;

        public CacheRecycler(string host, IFrameworkContext frameworkContext)
        {
            _host = host;
            this.frameworkContext = frameworkContext;
        }

        public void RecycleCacheFor(TypedEntity entity)
        {
            string niceUrl = entity.NiceUrl();

            RemoveFromProvider(entity, frameworkContext.Caches.LimitedLifetime, niceUrl);
            RemoveFromProvider(entity, frameworkContext.Caches.ExtendedLifetime, niceUrl);
            RemoveFromProvider(frameworkContext.ApplicationCache, niceUrl);

            RegenerateCache(niceUrl);
        }

        private static void RemoveFromProvider(TypedEntity entity, AbstractCacheProvider cache, string niceUrl)
        {
            cache.RemoveWhereKeyMatches<UrlCacheKey>(x => x.EntityId == entity.Id);
            cache.RemoveWhereKeyContainsString(niceUrl);
        }

        private static void RemoveFromProvider(AbstractApplicationCache cache, string niceUrl)
        {
            cache.RemoveWhereKeyContains(niceUrl);
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