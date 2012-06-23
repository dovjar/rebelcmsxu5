using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("B75585C9-44AC-4397-9326-A0D8B58A027D",
        "Empty recycle bin", false, true,
        "RebelCms.Controls.MenuItems.emptyBin",
        "menu-empty-bin")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.EmptyRecycleBin })]
    public class EmptyRecycleBin : MenuItem { }
}