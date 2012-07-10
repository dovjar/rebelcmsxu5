using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Cms.Web.Caching
{
    using Rebel.Framework;
    using Rebel.Framework.Caching;

    public class UrlCacheKey //: CacheKey<UrlCacheKey>
    {
        public UrlCacheKey(HiveId entityId)
        {
            EntityId = entityId;
        }

        public HiveId EntityId { get; set; }
    }
}
