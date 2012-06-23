using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("78FEF28C-4090-43A7-8176-27C26727DAB0",
        "Publish",
        true, false,
        "RebelCms.Controls.MenuItems.publish",
        "menu-publish")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Publish })]
    public class Publish : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "publishUrl" }; }
        }
    }
}