using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("0C177931-2812-483F-85AB-320EEBCBF217",
        "Send to Translate",
        false, true,
        "Rebel.Controls.MenuItems.SendTranslate",
        "menu-send-translate")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.SendToTranslate })]
    public class SendTranslate : MenuItem { }
}