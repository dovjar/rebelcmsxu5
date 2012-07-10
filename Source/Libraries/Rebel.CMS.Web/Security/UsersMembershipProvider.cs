using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Security
{
    public class UsersMembershipProvider : AbstractRebelMembershipProvider
    {
        public override Uri HiveUri
        {
            get { return new Uri("security://users"); }
        }

        public override HiveId VirtualRootId
        {
            get { return FixedHiveIds.UserVirtualRoot; }
        }
    }
}
