
using Rebel.Framework.Configuration;
using Rebel.Framework.Context;
using System.Globalization;

namespace Rebel.Framework.DependencyManagement.DemandBuilders
{
    public class FrameworkSetupHttpLifetime : IDependencyDemandBuilder
    {
        #region Implementation of IDependencyDemandBuilder

        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            containerBuilder.For<DefaultConfigurationResolver>()
                .KnownAs<IConfigurationResolver>()
                .ScopedAs.Singleton();

            new LocalizationSetup().Build(containerBuilder, context);

            containerBuilder.For<DefaultFrameworkContext>().KnownAs<IFrameworkContext>()
                .ScopedAs.HttpRequest();
        }

        #endregion
    }
}