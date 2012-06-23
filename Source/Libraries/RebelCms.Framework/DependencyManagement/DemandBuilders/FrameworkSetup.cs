using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RebelCms.Framework.Configuration;
using RebelCms.Framework.Context;
using System.Globalization;
using RebelCms.Framework.TypeMapping;

namespace RebelCms.Framework.DependencyManagement.DemandBuilders
{
    public class FrameworkSetup : IDependencyDemandBuilder
    {
        #region Implementation of IDependencyDemandBuilder

        public void Build(IContainerBuilder containerBuilder, IBuilderContext context)
        {
            containerBuilder.For<DefaultConfigurationResolver>()
                .KnownAs<IConfigurationResolver>().ScopedAs.Singleton();

            new LocalizationSetup().Build(containerBuilder, context);

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
