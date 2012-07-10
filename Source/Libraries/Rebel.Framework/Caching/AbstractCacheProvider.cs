namespace Rebel.Framework.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Caching;
    using System.Threading;
    using Newtonsoft.Json;
    using Rebel.Framework.Configuration;
    using Rebel.Framework.Configuration.Caching;
    using Rebel.Framework.Diagnostics;

    /// <summary>
    /// A cache provider used for caching data in the current scope. For example, in a web application the current scope is the current request.
    /// </summary>
    public abstract class AbstractCacheProvider : DisposableObject
    {
        protected static readonly ReaderWriterLockSlim ConfigInitLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private static General _config;
        protected virtual ICachePolicy GetCachePolicy(string key)
        {
            EnsureFrameworkConfig();

            // Figure out the type of the cache key
            Type realType = null;
            var getTypeFromJson = CacheKey.GetAndRemoveTypeFromJson(key, out realType);

            if (realType == null || getTypeFromJson.IsNullOrWhiteSpace())
                return StaticCachePolicy.CreateDefault();

            try
            {
                var jsonSerializerSettings = new JsonSerializerSettings()
                {
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ObjectCreationHandling = ObjectCreationHandling.Replace
                };
                var realKey = JsonConvert.DeserializeObject(getTypeFromJson, realType, jsonSerializerSettings) as CacheKey;
                if (realKey == null)
                {
                    return StaticCachePolicy.CreateDefault();
                }

                // Figure out which type of cache provider we are, i.e. which rules to load
                var policy = GetCachePolicyForKey(realKey);
                return policy;
            }
            catch
            {
                return StaticCachePolicy.CreateDefault();
            }
        }

        private static void EnsureFrameworkConfig()
        {
            if (_config == null)
                using (new WriteLockDisposable(ConfigInitLocker))
                {
                    if (_config == null)
                    {
                        _config = General.GetFromConfigManager() ?? new General();
                    }
                }
        }

        public virtual ICachePolicy GetCachePolicyForKey(CacheKey realKey)
        {
            EnsureFrameworkConfig();

            var extendedType = _config.CacheProviders.ExtendedLifetime.GetProviderType();

            var provider = _config.CacheProviders.LimitedLifetime;
            if (extendedType == this.GetType())
            {
                provider = _config.CacheProviders.ExtendedLifetime;
            }

            // Avoid people having to put Original. in front of all their expressions so that the dynamic expression parser can find the properties
            var tryCast = realKey as IClassKey<object>;
            if (tryCast != null)
            {
                return provider.GetPolicyFor(tryCast.Original) ?? StaticCachePolicy.CreateDefault(); ;
            }
            var policy = provider.GetPolicyFor(realKey);
            return policy ?? StaticCachePolicy.CreateDefault(); ;
        }

        public virtual CacheModificationResult AddOrChangeValue<T>(string key, T cacheObject)
        {
            var value = new CacheValueOf<T>(cacheObject);
            return AddOrChange(key, value);
        }

        //public virtual CacheModificationResult AddOrChange(string key, CacheValueOf<object> cacheObject)
        //{
        //    return AddOrChange<object>(key, cacheObject);
        //}

        public abstract CacheModificationResult AddOrChange<T>(string key, CacheValueOf<T> cacheObject);

        //public virtual CacheCreationResult<object> GetOrCreate(string key, Func<CacheValueOf<object>> callback)
        //{
        //    return GetOrCreate<object>(key, callback);
        //}

        public abstract void Clear();

        public abstract bool Remove(string key);

        public virtual int RemoveWhereKeyMatches<T>(Func<T, bool> matches)
        {
            var removed = 0;
            // Ensure sequence is executed once to avoid collection modified errors in inheriting class
            var cacheKeys = GetKeysMatching(matches).ToArray();
            cacheKeys
                .ForEach(x =>
                {
                    LogHelper.TraceIfEnabled(GetType(), "Removing item due to key delegate, key: {0}", () => x);
                    Remove(x);
                    removed++;
                });
            return removed;
        }

        protected virtual ICacheValueOf<T> EnsureItemExpired<T>(CacheEntry<T> entry)
        {
            if (entry == null) return null;
            var dateTimeOffset = entry.Value.Policy.ExpiryDate;
            var now = DateTimeOffset.Now;
            if (dateTimeOffset < now)
            {
                LogHelper.TraceIfEnabled(GetType(), "Removing item due to expiry, key: {0}", () => entry.Key);
                Remove(entry.Key);
                return null;
            }
            return entry.Value;
        }

        protected abstract IEnumerable<string> GetKeysMatching<T>(Func<T, bool> predicate);

        protected abstract CacheEntry<T> PerformGet<T>(string key);

        public virtual ICacheValueOf<T> Get<T>(string key)
        {
            var performGet = PerformGet<T>(key);
            return EnsureItemExpired(performGet);
        }

        public T GetValue<T>(string key)
        {
            var value = Get<T>(key);
            return value != null ? value.Item : default(T);
        }

        public virtual CacheCreationResult<T> GetOrCreate<T>(string key, Func<CacheValueOf<T>> callback)
        {

            var inheritorType = GetType();
            LogHelper.TraceIfEnabled(inheritorType, "In GetOrCreate for {0}", key.ToString);
            using (DisposableTimer.TraceDuration(inheritorType, "Start GetOrCreate", "End GetOrCreate"))
            {
                var existing = Get<T>(key);
                if (existing != null)
                {
                    LogHelper.TraceIfEnabled<AbstractCacheProvider>("Item existed");
                    var existingCast = existing as CacheValueOf<T>;
                    if (existingCast != null)
                    {
                        return new CacheCreationResult<T>(false, false, true, existingCast);
                    }
                    return new CacheCreationResult<T>(false, false, true, default(CacheValueOf<T>), true);
                }

                LogHelper.TraceIfEnabled(inheritorType, "Item did not exist");
                var newValue = callback.Invoke();
                var result = AddOrChange(key, newValue);
                return new CacheCreationResult<T>(result.WasUpdated, result.WasInserted, false, newValue);
            }
        }
    }
}