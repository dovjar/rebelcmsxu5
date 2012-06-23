using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("075EAE6E-96C3-4CBC-9882-017ED1A03D65",
        "Manage hostnames",
        "RebelCms.Controls.MenuItems.hostname",
        "menu-domain")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Hostnames })]
    public class Hostname : MenuItem { }
}