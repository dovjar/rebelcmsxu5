using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.Security
{
    using Umbraco.Cms.Web.Context;
    using Umbraco.Hive;

    public class MembersMembershipProvider : AbstractUmbracoMembershipProvider
    {
        public MembersMembershipProvider()
        {

        }

        public MembersMembershipProvider(IHiveManager hiveManager, IUmbracoApplicationContext appContext)
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
