using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Persistence.Model.Constants.Schemas;
using Umbraco.Framework.Security.Configuration;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Framework.Security
{
    public class SecurityService : ISecurityService
    {
        public SecurityService(IMembershipService<User> usersMembershipService, IMembershipService<Member> membersMembershipService,
            IPermissionsService permissionsService, IPublicAccessService publicAccessService)
        {
            Mandate.That<NullReferenceException>(usersMembershipService != null);
            Mandate.That<NullReferenceException>(membersMembershipService != null);
            Mandate.That<NullReferenceException>(permissionsService != null);
            Mandate.That<NullReferenceException>(publicAccessService != null);

            Users = usersMembershipService;
            Members = membersMembershipService;
            Permissions = permissionsService;
            PublicAccess = publicAccessService;
        }

        /// <summary>
        /// Gets the users membership service.
        /// </summary>
        public IMembershipService<User> Users { get; protected set; }

        /// <summary>
        /// Gets the members membership service.
        /// </summary>
        public IMembershipService<Member> Members { get; protected set; }

        /// <summary>
        /// Gets the permissions service.
        /// </summary>
        public IPermissionsService Permissions { get; protected set; }

        /// <summary>
        /// Gets the public access service.
        /// </summary>
        public IPublicAccessService PublicAccess { get; protected set; }

    }
}
