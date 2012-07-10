namespace Rebel.Framework.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Caching;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(IsReference = true)]
    public class StaticCachePolicy : AbstractEquatableObject<StaticCachePolicy>, ICachePolicy
    {
        public static StaticCachePolicy CreateDefault()
        {
            return new StaticCachePolicy(TimeSpan.FromMinutes(5));
        }

        public StaticCachePolicy()
        {}

        public StaticCachePolicy(TimeSpan slidingExpiration)
        {
            _slidingExpiry = slidingExpiration;

            SetFixedAbsoluteExpiry();
        }

        private void SetFixedAbsoluteExpiry()
        {
            _fixedAbsoluteExpiry = GenerateExpiryDate();
        }

        public StaticCachePolicy(ICachePolicy otherPolicy)
        {
            _priority = otherPolicy.Priority;
            _absoluteExpiry = otherPolicy.ExpiryDate;
        }

        public StaticCachePolicy(CacheItemPolicy fromPolicy)
        {
            _priority = fromPolicy.Priority;
            _slidingExpiry = fromPolicy.SlidingExpiration;
            _absoluteExpiry = fromPolicy.AbsoluteExpiration;
            //EntryUpdated = fromPolicy.UpdateCallback;
            //EntryRemoved = fromPolicy.RemovedCallback;

            SetFixedAbsoluteExpiry();
        }

        private CacheItemPriority _priority = CacheItemPriority.Default;
        [DataMember]
        public CacheItemPriority Priority
        {
            get { return _priority; }
            set { _priority = value; }
        }

        private readonly TimeSpan _slidingExpiry = ObjectCache.NoSlidingExpiration;
        private readonly DateTimeOffset _absoluteExpiry = ObjectCache.InfiniteAbsoluteExpiration;
        private DateTimeOffset _fixedAbsoluteExpiry = ObjectCache.InfiniteAbsoluteExpiration;

        [DataMember]
        public DateTimeOffset ExpiryDate
        {
            get { return _fixedAbsoluteExpiry; }
            set { _fixedAbsoluteExpiry = value; }
        }

        [IgnoreDataMember]
        public bool IsExpired
        {
            get
            {
                return ExpiryDate <= DateTimeOffset.Now;
            }
        }

        private DateTimeOffset GenerateExpiryDate()
        {
            if (_absoluteExpiry == ObjectCache.InfiniteAbsoluteExpiration && _slidingExpiry != ObjectCache.NoSlidingExpiration)
            {
                return DateTimeOffset.Now.Add(_slidingExpiry);
            }
            return _absoluteExpiry;
        }

        //public CacheEntryUpdateCallback EntryUpdated { get; set; }

        //public CacheEntryRemovedCallback EntryRemoved { get; set; }

        public CacheItemPolicy ToCacheItemPolicy()
        {
            return new CacheItemPolicy()
                {
                    AbsoluteExpiration = ExpiryDate,
                    Priority = Priority
                    //RemovedCallback = EntryRemoved,
                    //UpdateCallback = EntryUpdated
                };
        }

        /// <summary>
        /// Gets the natural id members.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.Priority);
            yield return this.GetPropertyInfo(x => x.ExpiryDate);
        }
    }
}