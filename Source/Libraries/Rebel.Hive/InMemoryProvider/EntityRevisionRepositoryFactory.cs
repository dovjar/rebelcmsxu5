namespace Rebel.Hive.InMemoryProvider
{
    using Rebel.Framework;
    using Rebel.Framework.Context;
    using Rebel.Framework.Persistence.Model;
    using Rebel.Framework.Persistence.ProviderSupport._Revised;
    using Rebel.Hive.ProviderSupport;

    public class EntityRevisionRepositoryFactory : AbstractRevisionRepositoryFactory<TypedEntity>
    {
        public EntityRevisionRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper) 
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
        }

        protected DependencyHelper CacheDependencyHelper { get { return base.DependencyHelper as DependencyHelper; } }

        public override AbstractRevisionRepository<TypedEntity> GetRepository()
        {
            return new EntityRevisionRepository(ProviderMetadata, FrameworkContext, CacheDependencyHelper);
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            CacheDependencyHelper.Dispose();
        }

        /// <summary>
        /// Gets an <see cref="AbstractReadonlyRevisionRepository{TypedEntity}"/>. It will have only read operations.
        /// </summary>
        /// <returns></returns>
        public override AbstractReadonlyRevisionRepository<TypedEntity> GetReadonlyRepository()
        {
            return GetRepository();
        }
    }
}