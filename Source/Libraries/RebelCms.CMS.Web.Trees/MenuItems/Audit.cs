using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("8977395A-9DF8-4A38-97C3-E35B36BE2151",
        "Audit",
        true, true,
        "RebelCms.Controls.MenuItems.Audit",
        "menu-audit")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Audit })]
    public class Audit : MenuItem { }
}