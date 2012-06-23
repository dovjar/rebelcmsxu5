namespace RebelCms.Hive.InMemoryProvider
{
    using RebelCms.Framework;
    using RebelCms.Framework.Context;
    using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
    using RebelCms.Framework.Persistence.ProviderSupport._Revised;
    using RebelCms.Hive.ProviderSupport;

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