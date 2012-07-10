using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("022FD04F-44FF-47FC-B6CC-83FDBD276F55",
        "Copy",
        false, true,
        "Rebel.Controls.MenuItems.copyItem",
        "menu-copy")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Copy })]
    public class Copy : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "copyUrl" }; }
        }
    }
}