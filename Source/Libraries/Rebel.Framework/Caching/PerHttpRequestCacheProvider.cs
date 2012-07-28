using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Caching
{
    using System.Threading;
    using System.Web;

    public class PerHttpRequestCacheProvider : AbstractCacheProvider
    {
        private readonly HttpContextBase _fixedContext;

        public PerHttpRequestCacheProvider()
        {}

        public PerHttpRequestCacheProvider(HttpContextBase context)
        {
            _fixedContext = context;
        }

        /// <summary>
        /// Gets an <see cref="HttpContextWrapper"/> for either the instance passed in (e.g. for unit testing) or from the <see cref="HttpContext.Current"/> scoped singleton.
        /// </summary>
        /// <returns></returns>
        protected HttpContextBase GetCurrent()
        {
            // We cannot accept new HttpContextWrapper(HttpContext.Current) as a ctor parameter otherwise we'll never get the new "Current" instance for other threads / requests.
            if (_fixedContext != null) return _fixedContext;
            return new HttpContextWrapper(HttpContext.Current);
        }

        public static PerHttpRequestCacheProvider Default
        {
            get
            {
                return new PerHttpRequestCacheProvider();
            }
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            foreach (var result in GetCurrent().Items.Values.OfType<IDisposable>())
            {
                result.Dispose();
            }
        }

        #endregion

        #region Overrides of AbstractCacheProvider

        public override CacheModificationResult AddOrChange<T>(string key, CacheValueOf<T> cacheObject)
        {
            var exists = Get<T>(key) != null;
            var items = GetCurrent().Items;
            if (exists)
            {
                items[key] = cacheObject;
                return new CacheModificationResult(true, false);
            }
            items.Add(key, cacheObject);
            return new CacheModificationResult(false, true);
        }

        public override void Clear()
        {
            var items = GetCurrent().Items;
            items.Keys.OfType<CacheKey>().ToArray().ForEach(items.Remove);
        }

        public override bool Remove(string key)
        {
            var items = GetCurrent().Items;
            if (!items.Contains(key)) return false;
            items.Remove(key);
            return true;
        }

        protected override IEnumerable<string> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            foreach (var key in GetCurrent().Items.Keys)
            {
                var keyAsString = key as string;
                if (keyAsString == null) continue;
                var convertKey = (CacheKey<T>)keyAsString;
                if (convertKey != default(CacheKey<T>) && !ReferenceEquals(convertKey.Original, null) && predicate.Invoke(convertKey.Original))
                {
                    yield return keyAsString;
                }
            }
        }

        protected override IEnumerable<string> GetKeysMatching(string containing)
        {
            return GetCurrent().Items.Keys.OfType<string>().Where(keyAsString => keyAsString.Contains(containing));
        }

        protected override CacheEntry<T> PerformGet<T>(string key)
        {
            var item = GetCurrent().Items[key] as ICacheValueOf<T>;
            return item != null ? new CacheEntry<T>(key, item) : null;
        }

        #endregion
    }
}
