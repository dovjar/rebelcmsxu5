
using System.Web.Hosting;

using Rebel.Framework.Context;
using Rebel.Framework.DataManagement;
using Rebel.Framework.DependencyManagement;
using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.XmlStore.DataManagement;
using Rebel.Framework.Persistence.XmlStore.DataManagement.ReadWrite;

namespace Rebel.Framework.Persistence.XmlStore.DependencyDemandBuilders
{
    public class XmlProviderDemandBuilder : AbstractProviderDependencyBuilder
    {
        public XmlProviderDemandBuilder()
        {
        }

        public XmlProviderDemandBuilder(string providerKey)
        {
            ProviderKey = providerKey;
        }

        /// <summary>
        /// Builds the dependency demands required by this implementation.
        /// </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="builderContext">The builder context.</param>
        public override void Build(IContainerBuilder containerBuilder, IBuilderContext builderContext)
        {
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");
            Mandate.ParameterNotNull(builderContext, "builderContext");

            // Configure type injection for this provider's implementation of the main interfaces
            containerBuilder.ForFactory(context => new DataContextFactory(HostingEnvironment.MapPath("~/App_Data/rebel.config")))
                .Named<AbstractDataContextFactory>(ProviderKey)
                .ScopedAs.Singleton();
            
            containerBuilder.For<ReadOnlyUnitOfWorkFactory>()
                .Named<IReadOnlyUnitOfWorkFactory>(ProviderKey);

            containerBuilder.For<ReadWriteUnitOfWorkFactory>()
                .Named<IReadWriteUnitOfWorkFactory>(ProviderKey);
        }

       
    }
}