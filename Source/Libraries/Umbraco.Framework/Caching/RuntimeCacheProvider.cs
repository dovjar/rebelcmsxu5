using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Caching
{
    using System.Collections.Concurrent;
    using System.Runtime.Caching;
    using System.Threading;
    using Umbraco.Framework.Diagnostics;

    public class NonCachingProvider : AbstractCacheProvider
    {
        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            return;
        }

        public override CacheModificationResult AddOrChange<T>(string key, CacheValueOf<T> cacheObject)
        {
            return new CacheModificationResult(false, false);
        }

        public override void Clear()
        {
            return;
        }

        public override bool Remove(string key)
        {
            return true;
        }

        protected override IEnumerable<string> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            yield break;
        }

        protected override CacheEntry<T> PerformGet<T>(string key)
        {
            return null;
        }
    }

    public class RuntimeCacheProvider : AbstractCacheProvider
    {
        public RuntimeCacheProvider()
        {
            _memoryCache = new MemoryCache("in-memory");
        }

        public RuntimeCacheProvider(ObjectCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        private static readonly ReaderWriterLockSlim StaticLocker = new ReaderWriterLockSlim();
        private static RuntimeCacheProvider _singleInstance;
        public static RuntimeCacheProvider Default
        {
            get
            {
                using (new WriteLockDisposable(StaticLocker))
                {
                    if (_singleInstance == null) _singleInstance = new RuntimeCacheProvider();
                }
                return _singleInstance;
            }
        }

        private ObjectCache _memoryCache;
        private ConcurrentDictionary<string, string> _keyTracker = new ConcurrentDictionary<string, string>();

        public override CacheModificationResult AddOrChange<T>(string key, CacheValueOf<T> cacheObject)
        {
            //using (new WriteLockDisposable(ThreadLocker))
            //{
            var exists = _memoryCache.GetCacheItem(key) != null;
            var cacheItemPolicy = cacheObject.Policy.ToCacheItemPolicy();

            var entryDate = DateTime.Now;
            var policyExpiry = cacheItemPolicy.AbsoluteExpiration.Subtract(entryDate);
            cacheItemPolicy.RemovedCallback +=
                arguments =>
                LogHelper.TraceIfEnabled(
                    GetType(),
                    "Item was removed from cache ({0}). Policy had {1}s to run when entered at {2}. Key: {3}",
                    () => arguments.RemovedReason.ToString(),
                    () => policyExpiry.TotalSeconds.ToString(),
                    () => entryDate.ToString(),
                    () => key);

            _keyTracker.TryAdd(key, key);

            if (exists)
            {
                LogHelper.TraceIfEnabled(GetType(), "Updating item with {0} left to run", () => policyExpiry.TotalSeconds.ToString());
                _memoryCache.Set(key, cacheObject, cacheItemPolicy);
                return new CacheModificationResult(true, false);
            }
            var diff = cacheObject.Policy.ExpiryDate.Subtract(DateTimeOffset.Now);
            LogHelper.TraceIfEnabled(GetType(), "Adding item with {0} left to run", () => policyExpiry.TotalSeconds.ToString());
            _memoryCache.Add(key, cacheObject, cacheItemPolicy);
            //}
            return new CacheModificationResult(false, true);
        }

        public override void Clear()
        {
            _memoryCache.Select(x => x.Key).ToArray().ForEach(x => _memoryCache.Remove(x));
            _memoryCache.DisposeIfDisposable();
            _memoryCache = new MemoryCache("in-memory");
        }

        public override bool Remove(string key)
        {
            string throwaway = null;
            var keyBeSure = _keyTracker.TryGetValue(key, out throwaway);
            object itemRemoved = _memoryCache.Remove(key);
            _keyTracker.TryRemove(key, out throwaway);

            return itemRemoved != null;
        }

        protected override CacheEntry<T> PerformGet<T>(string key)
        {
            var item = _memoryCache.Get(key);
            var casted = item as ICacheValueOf<T>;
            return casted != null ? new CacheEntry<T>(key, casted) : null;
        }

        protected override IEnumerable<string> GetKeysMatching<T>(Func<T, bool> predicate)
        {
            foreach (var key in _keyTracker.Keys)
            {
                var convertKey = (CacheKey<T>)key;
                if (convertKey != default(CacheKey<T>) && !ReferenceEquals(convertKey.Original, null) && predicate.Invoke(convertKey.Original))
                {
                    yield return key;
                }
            }
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _memoryCache.DisposeIfDisposable();
        }

        #endregion
    }
}
