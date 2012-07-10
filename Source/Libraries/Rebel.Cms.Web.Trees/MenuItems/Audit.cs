using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("8977395A-9DF8-4A38-97C3-E35B36BE2151",
        "Audit",
        true, true,
        "Rebel.Controls.MenuItems.Audit",
        "menu-audit")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Audit })]
    public class Audit : MenuItem { }
}