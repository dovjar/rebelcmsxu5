using Rebel.Framework.Persistence.Model;

namespace Rebel.Hive.ProviderSupport
{
    public interface IProviderReadonlyEntityRepositoryFactory : IProviderReadonlyRepositoryFactory<AbstractReadonlyEntityRepository>
    {
        /// <summary>
        /// Gets the schema session factory.
        /// </summary>
        /// <value>The schema session factory.</value>
        AbstractReadonlySchemaRepositoryFactory SchemaRepositoryFactory { get; }

        /// <summary>
        /// Gets the revision session factory.
        /// </summary>
        /// <value>The revision session factory.</value>
        AbstractReadonlyRevisionRepositoryFactory<TypedEntity> RevisionRepositoryFactory { get; }
    }
}