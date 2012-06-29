using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Framework.Security
{
    public class PublicAccessService : IPublicAccessService
    {
        private readonly IHiveManager _hive;
        private readonly IMembershipService<Member> _members;
        private readonly IFrameworkContext _framework;

        public PublicAccessService(IHiveManager hive, IMembershipService<Member> membersMembershipService,
            IFrameworkContext framework)
        {
            Mandate.That<NullReferenceException>(hive != null);
            Mandate.That<NullReferenceException>(membersMembershipService != null);

            _hive = hive;
            _members = membersMembershipService;
            _framework = framework;
        }

        /// <summary>
        /// Gets the hive.
        /// </summary>
        /// <value>The hive.</value>
        public IHiveManager Hive { get { return _hive; } }

        /// <summary>
        /// Determines whether the specified entity id is protected.
        /// </summary>
        /// <param name="entityId">The entity id.</param>
        /// <returns>
        ///   <c>true</c> if the specified entity id is protected; otherwise, <c>false</c>.
        /// </returns>
        public bool IsProtected(HiveId entityId)
        {
            var info = GetNearestPublicAccessInfo(entityId);
            return info != null;
        }

        /// <summary>
        /// Gets the public access status for the specified user groups.
        /// </summary>
        /// <param name="userGroupIds">The user group ids.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public PublicAccessStatusResult GetPublicAccessStatus(IEnumerable<HiveId> userGroupIds, HiveId entityId)
        {
            var publicAccessInfo = GetNearestPublicAccessInfo(entityId);
            if(publicAccessInfo == null)
            {
                // There are not public access relations, so default to allow
                return new PublicAccessStatusResult(entityId, true);
            }

            var matchingIds = publicAccessInfo.UserGroupIds.Where(allowedUserGroupId => userGroupIds.Any(x => x.Value == allowedUserGroupId.Value)).ToList();
            return matchingIds.Any() 
                ? new PublicAccessStatusResult(publicAccessInfo.EntityId, true, matchingIds) 
                : new PublicAccessStatusResult(publicAccessInfo.EntityId, false, publicAccessInfo.LoginPageId, publicAccessInfo.ErrorPageId);
        }

        /// <summary>
        /// Gets the public access status for the specified member.
        /// </summary>
        /// <param name="memberId">The member id.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public PublicAccessStatusResult GetPublicAccessStatus(HiveId memberId, HiveId entityId)
        {
            var userGroupIds = GetUserGroupIdsForMember(memberId);
            return GetPublicAccessStatus(userGroupIds, entityId);
        }

        /// <summary>
        /// Gets a list of UserGroup ids that the specified Member belongs to.
        /// </summary>
        /// <param name="memberId">The member id.</param>
        /// <returns></returns>
        protected IEnumerable<HiveId> GetUserGroupIdsForMember(HiveId memberId)
        {
            return Hive.FrameworkContext.ScopedCache.GetOrCreateTyped("ss_GetUserGroupIdsForMember_" + memberId,
                () =>
                {
                    if (memberId != null && !memberId.IsNullValueOrEmpty())
                    {
                        var user = _members.GetById(memberId, true);
                        if (user != null)
                            return user.Groups;
                    }

                    return Enumerable.Empty<HiveId>();
                });
        }

        public PublicAccessInfo GetNearestPublicAccessInfo(HiveId entityId)
        {
            return Hive.FrameworkContext.ScopedCache.GetOrCreateTyped("ss_GetNearestPublicAccessInfo_" + entityId,
                () =>
                {
                    using (var uow = Hive.OpenReader<IContentStore>())
                    {
                        // Check actual node
                        var publicAccessRelation = uow.Repositories.GetParentRelations(entityId, FixedRelationTypes.PublicAccessRelationType)
                            .SingleOrDefault();

                        if (publicAccessRelation != null)
                            return _framework.TypeMappers.Map<PublicAccessInfo>(publicAccessRelation);

                        // Check ancestors
                        var ancestorIds = uow.Repositories.GetAncestorIds(entityId, FixedRelationTypes.DefaultRelationType);
                        foreach (var ancestorId in ancestorIds)
                        {
                            publicAccessRelation = uow.Repositories.GetParentRelations(ancestorId, FixedRelationTypes.PublicAccessRelationType)
                                .SingleOrDefault();

                            if (publicAccessRelation != null)
                                return _framework.TypeMappers.Map<PublicAccessInfo>(publicAccessRelation);
                        }
                    }

                    return null;
                });
        }
    }

    
}
