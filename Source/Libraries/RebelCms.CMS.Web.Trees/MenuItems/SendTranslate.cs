using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("0C177931-2812-483F-85AB-320EEBCBF217",
        "Send to Translate",
        false, true,
        "RebelCms.Controls.MenuItems.SendTranslate",
        "menu-send-translate")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.SendToTranslate })]
    public class SendTranslate : MenuItem { }
}