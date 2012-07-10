using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("9712B4C2-4524-4804-8C4F-33B098EED813",
        "Delete", true, true,
        "Rebel.Controls.MenuItems.deleteItem",
        "menu-delete")]
    public class UnsecuredDelete : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "deleteUrl" }; }
        }
    }
}