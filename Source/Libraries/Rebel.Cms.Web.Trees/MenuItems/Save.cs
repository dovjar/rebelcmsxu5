using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("CF493EF1-C290-4C37-95BD-6F106094C383",
        "Save",
        "Rebel.Controls.MenuItems.Save",
        "menu-save")]
    public class Save : MenuItem { }
}