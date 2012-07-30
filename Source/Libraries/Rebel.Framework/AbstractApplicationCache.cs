using System;

namespace Rebel.Framework
{
    /// <summary>
    /// A cache provider used for caching data in the current application which persists across scope changes.
    /// </summary>
    public abstract class AbstractApplicationCache : DisposableObject
    {
        public abstract T Get<T>(string key);
        public abstract T GetOrCreate<T>(string key, Func<HttpRuntimeCacheParameters<T>> callback);
        public abstract void Create<T>(string key, T objectToCache, TimeSpan slidingExpiration);

        public abstract void Remove(string key);
        public abstract void RemoveWhereKeyContains(string key);

        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns>Returns the number of items removed that matched the pattern</returns>
        public abstract int InvalidateItems(string pattern);
        
        public abstract void ScopeComplete();

        protected override void DisposeResources()
        {
            ScopeComplete();
        }
    }
}