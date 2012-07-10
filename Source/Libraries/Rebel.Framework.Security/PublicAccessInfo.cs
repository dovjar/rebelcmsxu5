using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Security
{
    public class PublicAccessInfo
    {
        public PublicAccessInfo()
        {
            UserGroupIds = Enumerable.Empty<HiveId>();
        }

        public HiveId EntityId { get; set; }
        public IEnumerable<HiveId> UserGroupIds { get; set; }
        public HiveId LoginPageId { get; set; }
        public HiveId ErrorPageId { get; set; }
    }
}
