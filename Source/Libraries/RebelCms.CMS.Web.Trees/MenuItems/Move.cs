using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("A3940031-5A80-4633-BB7C-2D2D2521E95F", 
        "Move",
        true, false,
        "RebelCms.Controls.MenuItems.moveItem",
        "menu-move")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Move })]
    public class Move : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "moveUrl" }; }
        }
    }
}