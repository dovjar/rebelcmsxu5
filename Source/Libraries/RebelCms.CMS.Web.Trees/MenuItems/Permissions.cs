using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7C5D12C0-6D3B-482D-B80E-B6F3FFCEF934",
        "Permissions",
        false, true,
        "RebelCms.Controls.MenuItems.permissions",
        "menu-permissions")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Permissions })]
    public class Permissions : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "permissionsUrl" }; }
        }
    }
}