using System;
using System.Collections.Generic;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Framework.Security
{
    using Rebel.Hive;

    public interface ISecurityService
    {
        /// <summary>
        /// Gets the users membership service.
        /// </summary>
        IMembershipService<User> Users { get; }

        /// <summary>
        /// Gets the members membership service.
        /// </summary>
        IMembershipService<Member> Members { get; }

        /// <summary>
        /// Gets the permissions service.
        /// </summary>
        IPermissionsService Permissions { get; }

        /// <summary>
        /// Gets the public access service.
        /// </summary>
        IPublicAccessService PublicAccess { get; }
    }
}