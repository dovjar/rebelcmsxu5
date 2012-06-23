namespace RebelCms.Hive.InMemoryProvider
{
    using System;
    using System.Runtime.Caching;
    using RebelCms.Framework.DependencyManagement;
    using RebelCms.Framework.Persistence.ProviderSupport._Revised;
    using RebelCms.Hive.DependencyManagement;
    using RebelCms.Hive.ProviderSupport;

    public class CacheDemandBuilder : AbstractProviderDependencyBuilder
    {
        /// <summary>Builds the dependency demands required by this implementation. </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="context">The context for this building session containing configuration etc.</param>
        public override void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            return;
        }

        /// <summary>
        /// Initialises the provider dependency builder. This method is run by <see cref="RebelCms.Hive.DependencyManagement.ProviderDemandRunner"/> prior to it calling <see cref="AbstractProviderDependencyBuilder.Build"/>.
        /// </summary>
        /// <param name="builderContext">The builder context.</param>
        public override void Initialise(IBuilderContext builderContext)
        {
            return;
        }

        /// <summary>
        /// Gets the provider bootstrapper factory. The <see cref="AbstractProviderDependencyBuilder"/> will use this to register the <see cref="AbstractProviderBootstrapper"/> against the ProviderKey.
        /// </summary>
        /// <param name="builderContext">The builder context.</param>
        /// <returns></returns>
        public override Func<IResolutionContext, AbstractProviderBootstrapper> GetProviderBootstrapperFactory(IBuilderContext builderContext)
        {
            return null;
        }

        /// <summary>
        /// Gets the provider dependency helper factory. If a provider requires dependencies with a specific registration key, use this delegate to register a <see cref="ProviderDependencyHelper"/> with the appropriate
        /// keyed dependencies. Otherwise, if this method returns null, <see cref="ProviderDemandRunner"/> will register a <see cref="NullProviderDependencyHelper"/> in its place.
        /// </summary>
        /// <param name="builderContext">The builder context.</param>
        /// <returns></returns>
        public override Func<IResolutionContext, ProviderDependencyHelper> GetProviderDependencyHelperFactory(IBuilderContext builderContext)
        {
            return x => new DependencyHelper(x.Resolve<ProviderMetadata>(ProviderKey), new CacheHelper(new MemoryCache("hive-app-cache")));
        }
    }
}
