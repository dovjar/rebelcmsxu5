using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("CF493EF1-C290-4C37-95BD-6F106094C383",
        "Save",
        "RebelCms.Controls.MenuItems.Save",
        "menu-save")]
    public class Save : MenuItem { }
}