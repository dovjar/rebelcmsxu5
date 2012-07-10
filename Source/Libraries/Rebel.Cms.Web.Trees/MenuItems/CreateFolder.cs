using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("37B61A49-6595-491C-92E4-28C7A37508EE",
        "Create Folder",
        "Rebel.Controls.MenuItems.CreateFolder",
        "menu-create-folder")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Create })]
    public class CreateFolder : MenuItem { }
}