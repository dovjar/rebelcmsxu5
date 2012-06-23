using System;
using System.Collections.Generic;
using ClientDependency.Core;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.ActionFilters;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Security;

namespace RebelCms.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("9F38DF60-273F-45FA-AEF5-7A74716FEC41",
        "Delete", true, true,
        "RebelCms.Controls.MenuItems.deleteItem",
        "menu-delete")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Delete })]
    public class Delete : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "deleteUrl"}; }
        }
    }
}