using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("022FD04F-44FF-47FC-B6CC-83FDBD276F55",
        "Copy",
        false, true,
        "RebelCms.Controls.MenuItems.copyItem",
        "menu-copy")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Copy })]
    public class Copy : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "copyUrl" }; }
        }
    }
}