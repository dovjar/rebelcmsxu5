using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;

namespace Rebel.Framework
{
    /// <summary>
    /// An Application Cache provider which stores its items in the HttpRuntime Cache
    /// </summary>
    public class HttpRuntimeApplicationCache : AbstractApplicationCache
    {

        private const string ContextKey = "UmbHttpRuntimeCache-ase244g-";

        #region Overrides of AbstractScopedCache

        public override T Get<T>(string key)
        {
            var realKey = ContextKey + key;
            var output = GetFromContext(realKey);
            if (output == null)
                return default(T);

            return (T) output;
        }

        /// <summary>
        /// Gets or Creates the cache item
        /// </summary>
        /// <param name="key"></param>
        /// <param name="callback">Callback to create the cache item if it doesn't exist</param>
        /// <returns></returns>
        public override T GetOrCreate<T>(string key, Func<HttpRuntimeCacheParameters<T>> callback)
        {
            var realKey = ContextKey + key;
            var output = GetFromContext(realKey);
            if (output == null)
            {
                var parameters = callback.Invoke();
                if (parameters != null)
                {
                    HttpRuntime.Cache.Add(realKey, parameters.Value, parameters.Dependencies, parameters.AbsoluteExpiration, parameters.SlidingExpiration, parameters.CacheItemPriority, parameters.OnRemoved);
                    return parameters.Value;
                }
                return default(T);
            }
            return (T)output;
        }

        public override void Create<T>(string key, T objectToCache, TimeSpan slidingExpiration) 
        {
            var realKey = ContextKey + key;
            if (GetFromContext(realKey) == null)
            {
                HttpRuntime.Cache.Add(realKey, objectToCache, null, Cache.NoAbsoluteExpiration, slidingExpiration,
                                      CacheItemPriority.High, null);
            }
        }

        public override void Remove(string key)
        {
            var realKey = ContextKey + key;
            if (HttpRuntime.Cache.Get(realKey) != null)
            {
                HttpRuntime.Cache.Remove(realKey);
            }
        }

        public override void RemoveWhereKeyContains(string key)
        {
            foreach(DictionaryEntry entry in HttpRuntime.Cache)
            {
                string entryKey = entry.Key.ToString();
                if(entryKey.Contains(key))
                {
                    HttpRuntime.Cache.Remove(entryKey);
                }
            }
        }

        /// <summary>
        /// Removes any item from the cache that match the regex pattern
        /// </summary>
        /// <param name="pattern"></param>
        public override int InvalidateItems(string pattern)
        {
            var toRemove = (from DictionaryEntry i in HttpRuntime.Cache
                             where i.Key.ToString().StartsWith(ContextKey)
                            let key = i.Key.ToString()
                            where Regex.IsMatch(key.Substring(ContextKey.Length, key.Length - ContextKey.Length), pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase)
                             select i.Key.ToString()).ToList();

            foreach (var i in toRemove)
            {
                HttpRuntime.Cache.Remove(i);
            }
            return toRemove.Count;
        }

        private static object GetFromContext(string key)
        {
            return HttpRuntime.Cache[key];
        }

        public override void ScopeComplete()
        {
            var toDispose = (from DictionaryEntry i in HttpRuntime.Cache 
                             where i.Key.ToString().StartsWith(ContextKey) 
                             select i.Key.ToString()).ToList();
            foreach(var i in toDispose)
            {
                //dispose and remove it
                var item = HttpRuntime.Cache[i];
                if (item != null && item is IDisposable)
                {
                    ((IDisposable)item).Dispose();
                }
                HttpRuntime.Cache.Remove(i);
            }
        }

        #endregion
    }
}