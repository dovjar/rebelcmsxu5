using System;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.Trees
{
    [Tree(CorePluginConstants.ProfileTypeTreeControllerId, "Member types")]
    [UmbracoTree]
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