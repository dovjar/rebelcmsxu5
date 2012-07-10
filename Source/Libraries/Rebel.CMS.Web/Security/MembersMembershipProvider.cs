using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Security
{
    using Rebel.Cms.Web.Context;
    using Rebel.Hive;

    public class MembersMembershipProvider : AbstractRebelMembershipProvider
    {
        public MembersMembershipProvider()
        {

        }

        public MembersMembershipProvider(IHiveManager hiveManager, IRebelApplicationContext appContext)
        {
            base.HiveManager = hiveManager;
            this.AppContext = appContext;
        }

        public override Uri HiveUri
        {
            get { return new Uri("security://members"); }
        }

        public override HiveId VirtualRootId
        {
            get { return FixedHiveIds.MemberVirtualRoot; }
        }
    }
}
