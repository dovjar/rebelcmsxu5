using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderSupport
{
    public abstract class AbstractSchemaRepositoryFactory
        : AbstractReadonlySchemaRepositoryFactory,
          IProviderRepositoryFactory<AbstractSchemaRepository, AbstractReadonlySchemaRepository>
    {
        protected AbstractSchemaRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory,
            IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, dependencyHelper)
        {
            RevisionRepositoryFactory = revisionRepositoryFactory;
        }

        public new AbstractRevisionRepositoryFactory<EntitySchema> RevisionRepositoryFactory { get; set; }

        #region IProviderRepositoryFactory<AbstractSchemaRepository,AbstractReadonlySchemaRepository> Members

        public abstract AbstractSchemaRepository GetRepository();

        #endregion
    }
}