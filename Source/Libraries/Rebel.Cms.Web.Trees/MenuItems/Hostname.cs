using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("075EAE6E-96C3-4CBC-9882-017ED1A03D65",
        "Manage hostnames",
        "Rebel.Controls.MenuItems.hostname",
        "menu-domain")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Hostnames })]
    public class Hostname : MenuItem { }
}