using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderSupport
{
    public abstract class ProviderDependencyHelper : DisposableObject
    {
        protected ProviderDependencyHelper(ProviderMetadata providerMetadata)
        {
            ProviderMetadata = providerMetadata;
        }

        public ProviderMetadata ProviderMetadata { get; protected set; }
    }
}
