namespace Rebel.Framework.Caching
{
    using System;
    using System.Runtime.Caching;

    public interface ICachePolicy
    {
        CacheItemPriority Priority { get; set; }

        DateTimeOffset ExpiryDate { get; set; }

        //CacheEntryUpdateCallback EntryUpdated { get; set; }

        //CacheEntryRemovedCallback EntryRemoved { get; set; }

        CacheItemPolicy ToCacheItemPolicy();
    }
}