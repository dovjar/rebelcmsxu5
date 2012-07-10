using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("EE6805A0-2ABC-4A5B-ACB6-EF15B371E579",
        "Notify",
        true, false,
        "Rebel.Controls.MenuItems.Notify",
        "menu-notify")]
    public class Notify : MenuItem { }
}