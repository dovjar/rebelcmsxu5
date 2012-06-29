using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Caching
{
    using System.Collections.Concurrent;

    public class DictionaryCacheProvider : AbstractCacheProvider
    {
        private readonly ConcurrentDictionary<string, object> _cacheStore = new ConcurrentDictionary<string, object>();

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            foreach (var item in _cacheStore)
            {
                if (item.Value != null && item.Value is IDisposable)
                {
                    ((IDisposable)item.Value).Dispose();
                }
            }
            _cacheStore.Clear();
        }

        #endregion

        #region Overrides of AbstractCacheProvider

        public override CacheModificationResult AddOrChange<T>(string key, CacheValueOf<T> cacheObject)
        {
            CacheModificationResult toReturn = null;
            _cacheStore.AddOrUpdate(
                key,
                keyForAdding =>
                {
                    toReturn = new CacheModificationResult(false, true);
                    return cacheObject;
                },
                (keyForUpdating, existing) =>
                {
                    toReturn = new CacheModificationResult(true, false);
                    return cacheObject;
                });

            return toReturn;
        }

        public override void Clear()
        {
            _cacheStore.Clear();
        }

        public override bool Remove(string key)
        {
            object original = null;
            return _cacheStore.TryRemove(key, out original);
        }

        protected override IEnumerable<string> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            foreach (var key in _cacheStore.Keys)
            {
                var convertKey = (CacheKey<T>)key;
                if (convertKey != default(CacheKey<T>) && !ReferenceEquals(convertKey.Original, null) && predicate.Invoke(convertKey.Original))
                {
                    yield return key;
                }
            }
        }

        protected override CacheEntry<T> PerformGet<T>(string key)
        {
            var item = _cacheStore.ContainsKey(key) ? _cacheStore[key] : null;
            var casted = item as ICacheValueOf<T>;
            return casted != null ? new CacheEntry<T>(key, casted) : null;
        }

        #endregion
    }
}
