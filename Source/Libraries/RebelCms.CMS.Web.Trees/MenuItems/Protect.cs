using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7AF3CE17-5FEA-4C20-B0F0-08676519194F",
        "Protect",
        true, true,
        "RebelCms.Controls.MenuItems.Protect",
        "menu-protect")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.PublicAccess })]
    public class Protect : MenuItem { }
}