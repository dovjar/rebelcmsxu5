using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Hive.ProviderSupport;

namespace Rebel.Hive.Providers.IO
{
    [DemandsDependencies(typeof(ProviderDemandBuilder))]
    public class EntityRepositoryFactory : AbstractEntityRepositoryFactory
    {
        internal EntityRepositoryFactory(ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory, Settings settings,
            IFrameworkContext frameworkContext)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, new DependencyHelper(settings, providerMetadata))
        {
        }

        public EntityRepositoryFactory(
            ProviderMetadata providerMetadata,
            AbstractRevisionRepositoryFactory<TypedEntity> revisionRepositoryFactory,
            AbstractSchemaRepositoryFactory schemaRepositoryFactory,
            IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, revisionRepositoryFactory, schemaRepositoryFactory, frameworkContext, dependencyHelper)
        {
        }

        public DependencyHelper IoDependencyHelper { get { return base.DependencyHelper as DependencyHelper; }}

        public override AbstractReadonlyEntityRepository GetReadonlyRepository()
        {
            return new EntityRepository(ProviderMetadata, IoDependencyHelper.Settings, FrameworkContext);
        }

        public override AbstractEntityRepository GetRepository()
        {
            return new EntityRepository(ProviderMetadata, IoDependencyHelper.Settings, FrameworkContext);
        }

        protected override void DisposeResources()
        {
            return;
        }
    }
}