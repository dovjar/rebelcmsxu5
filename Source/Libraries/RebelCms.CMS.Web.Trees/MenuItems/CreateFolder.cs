using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("37B61A49-6595-491C-92E4-28C7A37508EE",
        "Create Folder",
        "RebelCms.Controls.MenuItems.CreateFolder",
        "menu-create-folder")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Create })]
    public class CreateFolder : MenuItem { }
}