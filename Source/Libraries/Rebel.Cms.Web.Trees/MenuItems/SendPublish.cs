using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("58798353-9378-4BF1-8E61-B557C993511D",
        "Send to publish",
        "Rebel.Controls.MenuItems.SendPublish",
        "menu-send-publish")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.SendToPublish })]
    public class SendPublish : MenuItem { }
}