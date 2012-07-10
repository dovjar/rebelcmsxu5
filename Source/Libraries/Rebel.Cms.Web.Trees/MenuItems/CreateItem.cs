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
    [MenuItem("D0635D22-B66C-4DC9-8442-3325CE5D5EE9", "Create", false, true, "Rebel.Controls.MenuItems.createItem", "menu-create")]
    [RebelAuthorize(Permissions = new[] { FixedPermissionIds.Create })]
    public class CreateItem : RequiresDataKeyMenuItem
    {
        public override string[] RequiredKeys
        {
            get { return new[] { "createUrl" }; }
        }
    }
}
