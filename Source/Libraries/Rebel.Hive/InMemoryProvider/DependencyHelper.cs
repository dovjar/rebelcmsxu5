namespace Rebel.Hive.InMemoryProvider
{
    using Rebel.Framework;
    using Rebel.Framework.Persistence.ProviderSupport._Revised;
    using Rebel.Hive.ProviderSupport;

    public class DependencyHelper : ProviderDependencyHelper
    {
        public DependencyHelper(ProviderMetadata providerMetadata, CacheHelper cacheHelper)
            : base(providerMetadata)
        {
            Mandate.ParameterNotNull(cacheHelper, "cacheHelper");
            CacheHelper = cacheHelper;
        }

        /// <summary>
        /// Gets or sets the cache helper.
        /// </summary>
        /// <value>The helper.</value>
        public CacheHelper CacheHelper { get; protected set; }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            CacheHelper.Dispose();
        }
    }
}
