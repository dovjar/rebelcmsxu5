using System;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Framework;

using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Cms.Web.Trees
{
    [Tree(CorePluginConstants.MacroTreeControllerId, "Macros")]
    [RebelTree]
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