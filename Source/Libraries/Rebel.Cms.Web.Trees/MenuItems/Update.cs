using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("11C20C07-B76D-4C87-9C92-97054CAEC092",
        "Update",
        "Rebel.Controls.MenuItems.Update",
        "menu-update")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Update })]
    public class Update : MenuItem { }
}