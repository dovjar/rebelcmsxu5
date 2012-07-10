using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7B0926DD-A5CB-49BD-B60B-3C3FBC886603",
        "Rollback",
        false, true,
        "Rebel.Controls.MenuItems.rollback",
        "menu-rollback")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Rollback })]
    public class Rollback : MenuItem { }
}