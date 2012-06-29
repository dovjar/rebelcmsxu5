using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.Security
{
    public class UsersMembershipProvider : AbstractUmbracoMembershipProvider
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
