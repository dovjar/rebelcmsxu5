using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Hive;

namespace Rebel.Framework.Security
{
    public interface IPublicAccessService
    {
        bool IsProtected(HiveId entityId);
        PublicAccessStatusResult GetPublicAccessStatus(HiveId memberId, HiveId entityId);
        PublicAccessStatusResult GetPublicAccessStatus(IEnumerable<HiveId> userGroupIds, HiveId entityId);
        PublicAccessInfo GetNearestPublicAccessInfo(HiveId entityId);

        IHiveManager Hive { get; }
    }
}
