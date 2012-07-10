using System;
using System.Collections.Generic;
using ClientDependency.Core;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Security;

namespace Rebel.Cms.Web.Trees.MenuItems
{
    [ClientDependency(ClientDependencyType.Javascript, "Tree/MenuItems.js", "Modules")]
    [MenuItem("9F38DF60-273F-45FA-AEF5-7A74716FEC41",
        "Delete", true, true,
        "Rebel.Controls.MenuItems.deleteItem",
        "menu-delete")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Delete })]
    public class Delete : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "deleteUrl"}; }
        }
    }
}