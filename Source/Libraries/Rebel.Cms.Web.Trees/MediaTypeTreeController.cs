using System;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MediaTypeTreeControllerId, "Media types")]
    [RebelTree]
    public class MediaTypeTreeController : DocumentTypeTreeController
    {
        public MediaTypeTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        protected override HiveId RootNodeId
        {
            get { return FixedHiveIds.MediaRootSchema; }
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MediaTypeEditorControllerId); }
        }

    }
}