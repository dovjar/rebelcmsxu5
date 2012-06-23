using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderSupport
{
    public abstract class AbstractReadonlySchemaRepositoryFactory
        : AbstractReadonlyRepositoryFactory<AbstractReadonlySchemaRepository>
    {
        protected AbstractReadonlySchemaRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractReadonlyRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory,
            IFrameworkContext frameworkContext,
            ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
            RevisionRepositoryFactory = revisionRepositoryFactory;
        }

        public AbstractReadonlyRevisionRepositoryFactory<EntitySchema> RevisionRepositoryFactory { get; set; }
    }
}