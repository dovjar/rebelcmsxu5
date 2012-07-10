namespace Rebel.Framework.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(IsReference = true)]
    public class CacheValueOf<T> : AbstractEquatableObject<CacheValueOf<T>>, ICacheValueOf<T>
    {
        /// <summary>
        /// Default ctor used for json serialization
        /// </summary>
        public CacheValueOf()
        {
            
        }

        public CacheValueOf(T value)
            : this(value, StaticCachePolicy.CreateDefault())
        {
        }

        public CacheValueOf(T value, ICachePolicy policy)
        {
            Item = value;
            Policy = policy;
        }

        [DataMember]
        public T Item { get; set; }

        [DataMember]
        public ICachePolicy Policy { get; set; }

        /// <summary>
        /// Gets the natural id members.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.Item);
            yield return this.GetPropertyInfo(x => x.Policy);
        }
    }
}