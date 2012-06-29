using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Security;
using Umbraco.Hive.ProviderGrouping;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.MemberGroupEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class MemberGroupEditorController : AbstractUserGroupEditorController
    {
        public MemberGroupEditorController(IBackOfficeRequestContext requestContext) 
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter(new Uri("security://member-groups"));

            Mandate.That(_hive != null, x => new NullReferenceException("Could not find hive provider for route security://member-groups"));
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
            get { return "security://member-groups"; }
        }

        /// <summary>
        /// Gets the provider group root.
        /// </summary>
        public override HiveId VirtualRoot
        {
            get { return FixedHiveIds.MemberGroupVirtualRoot; }
        }

        /// <summary>
        /// Gets the type of the user.
        /// </summary>
        /// <value>
        /// The type of the user.
        /// </value>
        public override UserType UserType
        {
            get { return UserType.Member; }
        }
    }
}
