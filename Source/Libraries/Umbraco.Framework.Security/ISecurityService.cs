using System;
using System.Collections.Generic;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Framework.Security
{
    using Umbraco.Hive;

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