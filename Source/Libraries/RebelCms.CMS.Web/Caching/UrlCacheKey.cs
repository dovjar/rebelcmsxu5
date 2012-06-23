using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RebelCms.Cms.Web.Caching
{
    using RebelCms.Framework;
    using RebelCms.Framework.Caching;

    public class UrlCacheKey //: CacheKey<UrlCacheKey>
    {
        public UrlCacheKey(HiveId entityId)
        {
            EntityId = entityId;
        }

        public HiveId EntityId { get; set; }
    }
}
