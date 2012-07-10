using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Hive.ProviderSupport;

namespace Rebel.Hive
{
    public sealed class UninstalledReadonlyProviderSetup : ReadonlyProviderSetup
    {
        public UninstalledReadonlyProviderSetup(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, AbstractProviderBootstrapper bootstrapper, int priorityOrdinal)
            : base(providerMetadata, frameworkContext, bootstrapper, priorityOrdinal)
        {
        }
    }

    public class ReadonlyProviderSetup : AbstractProviderSetup
    {
        protected ReadonlyProviderSetup(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, AbstractProviderBootstrapper bootstrapper, int priorityOrdinal) 
            : base(providerMetadata, frameworkContext, bootstrapper, priorityOrdinal)
        {
            Mandate.ParameterNotNull(frameworkContext, "frameworkContext");
            Mandate.ParameterNotNull(bootstrapper, "bootstrapper");
            Mandate.ParameterNotNull(providerMetadata, "providerMetadata");
        }

        public ReadonlyProviderSetup(ReadonlyProviderUnitFactory unitFactory, ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, AbstractProviderBootstrapper bootstrapper, int priorityOrdinal)
            : base(providerMetadata, frameworkContext, bootstrapper, priorityOrdinal)
        {
            ReadonlyUnitFactory = unitFactory;
        }

        /// <summary>
        /// Gets the unit factory.
        /// </summary>
        /// <remarks></remarks>
        public ReadonlyProviderUnitFactory ReadonlyUnitFactory { get; private set; }
    }
}