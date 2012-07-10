namespace Rebel.Hive.InMemoryProvider
{
    using Rebel.Framework;
    using Rebel.Framework.Context;
    using Rebel.Framework.Persistence.Model.Attribution.MetaData;
    using Rebel.Framework.Persistence.ProviderSupport._Revised;
    using Rebel.Hive.ProviderSupport;

    public class SchemaRepositoryFactory : AbstractSchemaRepositoryFactory
    {
        public SchemaRepositoryFactory(ProviderMetadata providerMetadata, AbstractRevisionRepositoryFactory<EntitySchema> revisionRepositoryFactory, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper) : base(providerMetadata, revisionRepositoryFactory, frameworkContext, dependencyHelper)
        {
        }

        protected DependencyHelper CacheDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            CacheDependencyHelper.Dispose();
        }

        /// <summary>
        /// Gets an <see cref="AbstractReadonlySchemaRepository"/>. It will have only read operations.
        /// </summary>
        /// <returns></returns>
        public override AbstractReadonlySchemaRepository GetReadonlyRepository()
        {
            return GetRepository();
        }

        public override AbstractSchemaRepository GetRepository()
        {
            return new SchemaRepository(ProviderMetadata, FrameworkContext, CacheDependencyHelper);
        }
    }
}