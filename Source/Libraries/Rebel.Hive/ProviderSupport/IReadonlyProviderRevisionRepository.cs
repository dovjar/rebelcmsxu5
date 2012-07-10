using System;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Associations._Revised;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderSupport
{
    public interface IReadonlyProviderRevisionRepository<in TBaseEntity>
        : ICoreReadonlyRevisionRepository<TBaseEntity>
        where TBaseEntity : class, IVersionableEntity
    {
        ProviderMetadata ProviderMetadata { get; }

        /// <summary>
        /// Gets the related entities delegate. This is used to provide returned Revisions' entities RelationProxyCollection with a delegate
        /// back to the relevant AbstractEntityRepository that may auto-load relations.
        /// </summary>
        /// <value>The related entities delegate.</value>
        Func<HiveId, RelationProxyBucket> RelatedEntitiesLoader { get; set; }
    }
}