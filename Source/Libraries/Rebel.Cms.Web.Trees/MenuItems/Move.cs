using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("A3940031-5A80-4633-BB7C-2D2D2521E95F", 
        "Move",
        true, false,
        "Rebel.Controls.MenuItems.moveItem",
        "menu-move")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Move })]
    public class Move : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "moveUrl" }; }
        }
    }
}