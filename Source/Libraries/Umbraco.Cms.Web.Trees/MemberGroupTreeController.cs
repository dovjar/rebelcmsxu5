using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MemberGroupTreeControllerId, "Member groups")]
    [UmbracoTree]
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
