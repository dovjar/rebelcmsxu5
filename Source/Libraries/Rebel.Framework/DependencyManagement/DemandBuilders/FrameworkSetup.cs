using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rebel.Framework.Configuration;
using Rebel.Framework.Context;
using System.Globalization;
using Rebel.Framework.TypeMapping;

namespace Rebel.Framework.DependencyManagement.DemandBuilders
{
    using System.Configuration;
    using Rebel.Framework.Caching;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Serialization;

    public class FrameworkSetup : IDependencyDemandBuilder
    {
        #region Implementation of IDependencyDemandBuilder

        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            containerBuilder.For<DefaultConfigurationResolver>()
                .KnownAs<IConfigurationResolver>().ScopedAs.Singleton();

            new LocalizationSetup().Build(containerBuilder, context);

            // Go into configuration and figure out the required providers for the caches
            var config = General.GetFromConfigManager();
            if (config == null)
            {
                string warning = "Could not find the Rebel Framework configuration. Ensure that a configSection element is declared in the application's configuration, of type '{0}' using the xml element path of '{1}'. Using defaults for now."
                        .InvariantFormat(typeof (General).FullName, General.ConfigXmlKey);
                LogHelper.Warn<FrameworkSetup>(warning);
                config = new General();
            }

            var extended = config.CacheProviders.ExtendedLifetime.IfNotNull(x => x.GetProviderType());
            Type extendedType = extended ?? typeof(RuntimeCacheProvider);
            var limited = config.CacheProviders.LimitedLifetime.IfNotNull(x => x.GetProviderType()); ;
            Type limitedType = limited ?? typeof(PerHttpRequestCacheProvider);

            DemandsDependenciesDemandRunniner.Run(containerBuilder, extendedType);
            DemandsDependenciesDemandRunniner.Run(containerBuilder, limitedType);

            containerBuilder.For(extendedType).Named<AbstractCacheProvider>("extended").ScopedAs.Singleton();
            containerBuilder.For(limitedType).Named<AbstractCacheProvider>("limited").ScopedAs.Singleton();

            containerBuilder
                .ForFactory(x => new DefaultFrameworkCaches(x.Resolve<AbstractCacheProvider>("limited"), x.Resolve<AbstractCacheProvider>("extended")))
                //.ForFactory(x => new DefaultFrameworkCaches(null, null))
                .KnownAs<IFrameworkCaches>()
                .ScopedAs
                .Singleton();

            containerBuilder.For<ServiceStackSerialiser>().KnownAs<ISerializer>().ScopedAs.Singleton();
            containerBuilder.For<SerializationService>().KnownAs<AbstractSerializationService>().ScopedAs.Singleton();

            containerBuilder.For<DefaultFrameworkContext>().KnownAs<IFrameworkContext>().ScopedAs.Singleton();
            
            containerBuilder
                .For<MappingEngineCollection>()
                .KnownAsSelf()
                .OnActivated((ctx, x) => x.Configure()) //once it's created, then we call Configure
                .ScopedAs.Singleton();
        }

        #endregion
    }
}
