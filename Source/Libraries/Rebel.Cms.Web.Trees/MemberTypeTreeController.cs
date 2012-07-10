using System;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Trees
{
    [Tree(CorePluginConstants.ProfileTypeTreeControllerId, "Member types")]
    [RebelTree]
    public class MemberTypeTreeController : DocumentTypeTreeController
    {
        public MemberTypeTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.MasterMemberProfileSchema; }
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MemberTypeEditorControllerId); }
        }

    }
}