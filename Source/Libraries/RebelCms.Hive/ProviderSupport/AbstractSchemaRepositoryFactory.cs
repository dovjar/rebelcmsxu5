using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderSupport
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