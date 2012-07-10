using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("B75585C9-44AC-4397-9326-A0D8B58A027D",
        "Empty recycle bin", false, true,
        "Rebel.Controls.MenuItems.emptyBin",
        "menu-empty-bin")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.EmptyRecycleBin })]
    public class EmptyRecycleBin : MenuItem { }
}