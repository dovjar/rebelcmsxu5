using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7C5D12C0-6D3B-482D-B80E-B6F3FFCEF934",
        "Permissions",
        false, true,
        "Rebel.Controls.MenuItems.permissions",
        "menu-permissions")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Permissions })]
    public class Permissions : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "permissionsUrl" }; }
        }
    }
}