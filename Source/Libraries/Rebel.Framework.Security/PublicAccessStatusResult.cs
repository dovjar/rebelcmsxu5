using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Security
{
    public class PublicAccessStatusResult
    {
        public PublicAccessStatusResult(HiveId sourceId, bool canAccess)
        {
            SourceId = sourceId;
            CanAccess = canAccess;
        }

        public PublicAccessStatusResult(HiveId sourceId, bool canAccess, IEnumerable<HiveId> userGroupIds)
        {
            SourceId = sourceId;
            CanAccess = canAccess;
            UserGroupIds = userGroupIds;
        }

        public PublicAccessStatusResult(HiveId sourceId, bool canAccess, HiveId loginPageId, HiveId errorPageId)
        {
            SourceId = sourceId;
            CanAccess = canAccess;
            LoginPageId = loginPageId;
            ErrorPageId = errorPageId;
        }

        public bool CanAccess { get; protected set; }
        public HiveId SourceId { get; protected set; }
        public IEnumerable<HiveId> UserGroupIds { get; protected set; }
        public HiveId LoginPageId { get; set; }
        public HiveId ErrorPageId { get; set; }
    }
}
