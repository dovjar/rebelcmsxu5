using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("11C20C07-B76D-4C87-9C92-97054CAEC092",
        "Update",
        "RebelCms.Controls.MenuItems.Update",
        "menu-update")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Update })]
    public class Update : MenuItem { }
}