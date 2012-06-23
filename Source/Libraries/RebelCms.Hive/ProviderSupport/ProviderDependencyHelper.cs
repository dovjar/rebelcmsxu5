using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderSupport
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
