using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Hive.ProviderSupport;

namespace Rebel.Hive
{
    public sealed class UninstalledProviderSetup : ProviderSetup
    {
        public UninstalledProviderSetup(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, AbstractProviderBootstrapper bootstrapper, int priorityOrdinal) 
            : base(providerMetadata, frameworkContext, bootstrapper, priorityOrdinal)
        {
        }
    }

    public class ProviderSetup : AbstractProviderSetup
    {
        internal ProviderSetup(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, AbstractProviderBootstrapper bootstrapper, int priorityOrdinal) 
            : base(providerMetadata, frameworkContext, bootstrapper, priorityOrdinal)
        {
            Mandate.ParameterNotNull(frameworkContext, "frameworkContext");
            Mandate.ParameterNotNull(bootstrapper, "bootstrapper");
            Mandate.ParameterNotNull(providerMetadata, "providerMetadata");
        }

        public ProviderSetup(ProviderUnitFactory unitFactory, ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, AbstractProviderBootstrapper bootstrapper, int priorityOrdinal)
            : base(providerMetadata, frameworkContext, bootstrapper, priorityOrdinal)
        {
            Mandate.ParameterNotNull(unitFactory, "unitFactory");

            UnitFactory = unitFactory;
        }

        /// <summary>
        /// Gets the unit factory.
        /// </summary>
        /// <remarks></remarks>
        public ProviderUnitFactory UnitFactory { get; private set; }
    }
}
