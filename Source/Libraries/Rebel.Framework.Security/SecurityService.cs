using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security.Configuration;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Framework.Security
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
