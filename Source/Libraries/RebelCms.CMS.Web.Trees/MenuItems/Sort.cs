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
    [MenuItem("83177655-9F16-47BC-BD5D-B48E7E2D6CE3",
        "Sort",
        true, false,
        "RebelCms.Controls.MenuItems.sortItems",
        "menu-sort")]
    [RebelCmsAuthorize(Permissions = new[] { FixedPermissionIds.Sort })]
    public class Sort : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "sortUrl" }; }
        }
    }
}