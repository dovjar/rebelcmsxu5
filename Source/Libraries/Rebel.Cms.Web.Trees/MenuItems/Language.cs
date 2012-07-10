using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("D9AA73D3-2993-409E-9877-0551DC7D56BB",
        "Language",
        "Rebel.Controls.MenuItems.language",
        "menu-language")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Language })]
    public class Language : MenuItem { }
}