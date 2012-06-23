using System;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Constants;

namespace RebelCms.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MediaTypeTreeControllerId, "Media types")]
    [RebelCmsTree]
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