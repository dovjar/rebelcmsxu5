using Rebel.Framework.Persistence.Model;

namespace Rebel.Hive.ProviderSupport
{
    using Rebel.Framework.Linq;

    public interface IReadonlyEntityRepository 
        : IReadonlyProviderRepository<TypedEntity>, IQueryableDataSource
    {
        /// <summary>
        /// Gets the revisions session.
        /// </summary>
        /// <value>The revisions.</value>
        AbstractReadonlyRevisionRepository<TypedEntity> Revisions { get; }

        /// <summary>
        /// Gets the schemas session.
        /// </summary>
        /// <value>The schemas.</value>
        AbstractReadonlySchemaRepository Schemas { get; }
    }
}