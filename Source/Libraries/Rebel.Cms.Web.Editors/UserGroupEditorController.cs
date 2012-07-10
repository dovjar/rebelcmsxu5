using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Security.Permissions;
using Rebel.Framework;

using Rebel.Framework.Localization;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Cms.Web.Editors
{
    [Editor(CorePluginConstants.UserGroupEditorControllerId)]
    [RebelEditor]
    [SupportClientNotifications]
    public class UserGroupEditorController : AbstractUserGroupEditorController
    {
        public UserGroupEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("security://user-groups"));

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route security://user-groups"));
        }

        private GroupUnitFactory _hive;
        public override GroupUnitFactory Hive
        {
            get { return _hive; }
        }

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
        /// Gets the type of the user.
        /// </summary>
        /// <value>
        /// The type of the user.
        /// </value>
        public override UserType UserType
        {
            get { return UserType.User; }
        }
    }
}
