using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("CB7CD841-A7B7-495D-B6A7-179A05A0C506",
        "Translate",
        "Rebel.Controls.MenuItems.Translate",
        "menu-translate")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Translate })]
    public class Translate : MenuItem { }
}