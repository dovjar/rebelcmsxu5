using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MemberGroupTreeControllerId, "Member groups")]
    [RebelTree]
    public class MemberGroupTreeController : AbstractUserGroupTree
    {
        public MemberGroupTreeController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        { }

        /// <summary>
        /// Gets the provider group root.
        /// </summary>
        public override string ProviderGroupRoot
        {
            get { return "security://member-groups"; }
        }

        /// <summary>
        /// Gets the provider group root.
        /// </summary>
        public override HiveId VirtualRoot
        {
            get { return FixedHiveIds.MemberGroupVirtualRoot; }
        }

        /// <summary>
        /// Defines the member group editor
        /// </summary>
        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MemberGroupEditorControllerId); }
        }
    }
}
