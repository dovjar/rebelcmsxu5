using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7B0926DD-A5CB-49BD-B60B-3C3FBC886603",
        "Rollback",
        false, true,
        "RebelCms.Controls.MenuItems.rollback",
        "menu-rollback")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Rollback })]
    public class Rollback : MenuItem { }
}