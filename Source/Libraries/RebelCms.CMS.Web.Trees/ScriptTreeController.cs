using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Framework;

using RebelCms.Framework.Persistence.Model.Constants;

namespace RebelCms.Cms.Web.Trees
{
    /// <summary>
    /// Tree controller to render out the javascript files
    /// </summary>
    [Tree(CorePluginConstants.ScriptTreeControllerId, "Scripts")]
    [RebelCmsTree]
    public class ScriptTreeController : AbstractFileSystemTreeController
    {
        public ScriptTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
        }

        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.ScriptEditorControllerId); }
        }

        protected override string HiveUriRouteMatch
        {
            get { return "storage://scripts"; }
        }
    }
}
