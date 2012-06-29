using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Security.Permissions;
using Umbraco.Framework;

using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.UserGroupEditorControllerId)]
    [UmbracoEditor]
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
