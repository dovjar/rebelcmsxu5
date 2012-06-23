using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("58798353-9378-4BF1-8E61-B557C993511D",
        "Send to publish",
        "RebelCms.Controls.MenuItems.SendPublish",
        "menu-send-publish")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.SendToPublish })]
    public class SendPublish : MenuItem { }
}