using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("1BA85CD2-978A-4D6E-AAE4-28AFF1CE4698",
        "Package",
        "RebelCms.Controls.MenuItems.Package",
        "menu-package")]
    public class Package : MenuItem { }
}