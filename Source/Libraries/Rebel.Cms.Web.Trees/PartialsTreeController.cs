using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;

namespace Rebel.Cms.Web.Trees
{
    /// <summary>
    /// Tree controller to render out the javascript files
    /// </summary>
    [Tree(CorePluginConstants.PartialsTreeControllerId, "Partials")]
    [RebelTree]
    public class PartialsTreeController : AbstractFileSystemTreeController
    {
        public PartialsTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.PartialsEditorControllerId); }
        }

        protected override string HiveUriRouteMatch
        {
            get { return "storage://partials"; }
        }

        protected override void CustomizeFileNode(TreeNode n, global::System.Web.Mvc.FormCollection queryStrings)
        {
            base.CustomizeFileNode(n, queryStrings);
            n.Icon = "tree-template";
        }
    }
}
