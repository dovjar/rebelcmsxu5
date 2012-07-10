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
    [MenuItem("83177655-9F16-47BC-BD5D-B48E7E2D6CE3",
        "Sort",
        true, false,
        "Rebel.Controls.MenuItems.sortItems",
        "menu-sort")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Sort })]
    public class Sort : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "sortUrl" }; }
        }
    }
}