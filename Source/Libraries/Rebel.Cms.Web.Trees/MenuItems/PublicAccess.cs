using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("7AF3CE17-5FEA-4C20-B0F0-08676519194F",
        "Public Access",
        true, true,
        "Rebel.Controls.MenuItems.publicAccess",
        "menu-protect")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.PublicAccess })]
    public class PublicAccess : MenuItem { }
}