namespace Umbraco.Framework.Caching
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(IsReference = true)]
    public class CacheEntry<T>
    {
        public CacheEntry()
        {}

        public CacheEntry(string key, ICacheValueOf<T> value)
        {
            Key = key;
            Value = value;
        }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public ICacheValueOf<T> Value { get; set; }
    }
}