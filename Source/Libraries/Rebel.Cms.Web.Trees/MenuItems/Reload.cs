using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;

namespace Rebel.Cms.Web.Trees.MenuItems
{

    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("FAEEA1A6-5EEE-408C-98E7-22168335A246",
        "Refresh", true, false,
        "Rebel.Controls.MenuItems.reloadChildren",
        "menu-refresh")]
    public class Reload : MenuItem { }
}