using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Framework;

using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Cms.Web.Trees
{
    /// <summary>
    /// Tree controller to render out the javascript files
    /// </summary>
    [Tree(CorePluginConstants.ScriptTreeControllerId, "Scripts")]
    [RebelTree]
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
