using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("D9AA73D3-2993-409E-9877-0551DC7D56BB",
        "Language",
        "RebelCms.Controls.MenuItems.language",
        "menu-language")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Language })]
    public class Language : MenuItem { }
}