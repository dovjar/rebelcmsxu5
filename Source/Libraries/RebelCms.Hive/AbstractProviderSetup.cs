using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;
using RebelCms.Framework.ProviderSupport;

namespace RebelCms.Hive
{
    public class AbstractProviderSetup
    {
        public AbstractProviderSetup(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, AbstractProviderBootstrapper bootstrapper, int priorityOrdinal)
        {
            ProviderMetadata = providerMetadata;
            FrameworkContext = frameworkContext;
            Bootstrapper = bootstrapper;
            PriorityOrdinal = priorityOrdinal;
        }

        /// <summary>
        /// Gets or sets the priority ordinal of the provider.
        /// </summary>
        /// <value>The priority ordinal.</value>
        /// <remarks></remarks>
        public int PriorityOrdinal { get; protected set; }

        /// <summary>
        /// Gets the provider metadata.
        /// </summary>
        /// <remarks></remarks>
        public ProviderMetadata ProviderMetadata { get; protected set; }

        /// <summary>
        /// Gets or sets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; protected set; }

        /// <summary>
        /// Gets or sets the bootstrapper.
        /// </summary>
        /// <value>The bootstrapper.</value>
        /// <remarks></remarks>
        public AbstractProviderBootstrapper Bootstrapper { get; protected set; }
    }
}