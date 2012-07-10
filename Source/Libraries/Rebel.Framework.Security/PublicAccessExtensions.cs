using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Framework.Security
{
    public static class PublicAccessExtensions
    {
        /// <summary>
        /// Filters the with public access.
        /// </summary>
        /// <param name="entityIds">The entity ids.</param>
        /// <param name="memberId">The member id.</param>
        /// <param name="publicAccessService">The public access service.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> FilterWithPublicAccess(this IEnumerable<HiveId> entityIds,
            HiveId memberId ,
            IPublicAccessService publicAccessService)
        {
            return memberId.IsNullValueOrEmpty()
                ? entityIds.Where(id => !publicAccessService.IsProtected(id))
                : entityIds.Where(id => publicAccessService.GetPublicAccessStatus(memberId, id).CanAccess);
        }

        /// <summary>
        /// Filters the with public access.
        /// </summary>
        /// <param name="entityIds">The entity ids.</param>
        /// <param name="member">The member.</param>
        /// <param name="publicAccessService">The public access service.</param>
        /// <returns></returns>
        public static IEnumerable<HiveId> FilterWithPublicAccess(this IEnumerable<HiveId> entityIds,
            Member member,
            IPublicAccessService publicAccessService)
        {
            return entityIds.FilterWithPublicAccess(member == null ? HiveId.Empty : member.Id, publicAccessService);
        }
    }
}
