using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("78FEF28C-4090-43A7-8176-27C26727DAB0",
        "Publish",
        true, false,
        "Rebel.Controls.MenuItems.publish",
        "menu-publish")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Publish })]
    public class Publish : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "publishUrl" }; }
        }
    }
}