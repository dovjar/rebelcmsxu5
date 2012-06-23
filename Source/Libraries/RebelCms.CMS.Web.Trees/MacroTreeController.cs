using System;
using System.Web.Mvc;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Framework;

using RebelCms.Framework.Persistence.Model.Constants;

namespace RebelCms.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MacroTreeControllerId, "Macros")]
    [RebelCmsTree]
    public class MacroTreeController : AbstractFileSystemTreeController
    {

        public MacroTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {

        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.MacroEditorControllerId); }
        }


        protected override string HiveUriRouteMatch
        {
            get { return "storage://macros"; }
        }

        protected override void CustomizeFileNode(TreeNode n, FormCollection queryStrings)
        {            
            base.CustomizeFileNode(n, queryStrings);
            n.Icon = "tree-developer-macro";
        }
    }
}