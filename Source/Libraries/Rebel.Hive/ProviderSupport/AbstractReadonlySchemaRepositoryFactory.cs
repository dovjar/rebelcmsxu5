using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderSupport
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