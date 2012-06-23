using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("CB7CD841-A7B7-495D-B6A7-179A05A0C506",
        "Translate",
        "RebelCms.Controls.MenuItems.Translate",
        "menu-translate")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Translate })]
    public class Translate : MenuItem { }
}