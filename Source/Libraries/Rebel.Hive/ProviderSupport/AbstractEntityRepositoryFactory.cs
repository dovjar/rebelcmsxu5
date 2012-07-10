using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderSupport
{
    public abstract class AbstractEntityRepositoryFactory
        : AbstractReadonlyEntityRepositoryFactory, IProviderEntityRepositoryFactory
    {
        protected AbstractEntityRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory,
            IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, schemaRepositoryFactory, revisionRepositoryFactory, frameworkContext, dependencyHelper)
        {
            RevisionRepositoryFactory = revisionRepositoryFactory;
            SchemaRepositoryFactory = schemaRepositoryFactory;
        }

        /// <summary>
        /// Gets or sets the schema session factory.
        /// </summary>
        /// <value>The schema session factory.</value>
        public new AbstractSchemaRepositoryFactory SchemaRepositoryFactory { get; protected set; }

        /// <summary>
        /// Gets or sets the revision session factory.
        /// </summary>
        /// <value>The revision session factory.</value>
        public new AbstractRevisionRepositoryFactory<TypedEntity> RevisionRepositoryFactory { get; protected set; }

        #region IProviderRepositoryFactory<AbstractEntityRepository,ProviderReadonlyEntitySession> Members

        /// <summary>
        /// Gets the session from the factory.
        /// </summary>
        /// <returns></returns>
        public abstract AbstractEntityRepository GetRepository();

        #endregion
    }
}