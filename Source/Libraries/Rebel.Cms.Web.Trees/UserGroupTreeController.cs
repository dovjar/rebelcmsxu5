using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Trees.MenuItems;
using Rebel.Framework;

using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;


namespace Rebel.Cms.Web.Trees
{
    /// <summary>
    /// Tree controller to render out the data types
    /// </summary>
    [Tree(CorePluginConstants.UserGroupTreeControllerId, "User groups")]
    [RebelTree]
    public class UserGroupTreeController : AbstractUserGroupTree
    {
        public UserGroupTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }

        /// <summary>
        /// Gets the provider group root.
        /// </summary>
        public override string ProviderGroupRoot
        {
            get { return "security://user-groups"; }
        }

        /// <summary>
        /// Gets the provider group root.
        /// </summary>
        public override HiveId VirtualRoot
        {
            get { return FixedHiveIds.UserGroupVirtualRoot; }
        }

        /// <summary>
        /// Defines the member group editor
        /// </summary>
        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.UserGroupEditorControllerId); }
        }

    }
}
