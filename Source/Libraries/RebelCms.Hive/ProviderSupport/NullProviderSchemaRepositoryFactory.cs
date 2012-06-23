using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderSupport
{
    public sealed class NullProviderSchemaRepositoryFactory : AbstractSchemaRepositoryFactory
    {
        public NullProviderSchemaRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext)
            : this(providerMetadata, new NullProviderRevisionRepositoryFactory<EntitySchema>(providerMetadata, frameworkContext), frameworkContext)
        {
            
        }

        public NullProviderSchemaRepositoryFactory(ProviderMetadata providerMetadata, AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory, IFrameworkContext frameworkContext) 
            : base(providerMetadata, revisionRepositoryFactory, frameworkContext, new NullProviderDependencyHelper(providerMetadata))
        {
        }

        protected override void DisposeResources()
        {
            RevisionRepositoryFactory.Dispose();
        }

        public override AbstractReadonlySchemaRepository GetReadonlyRepository()
        {
            return GetRepository();
        }

        public override AbstractSchemaRepository GetRepository()
        {
            return new NullProviderSchemaRepository(ProviderMetadata, FrameworkContext);
        }
    }
}