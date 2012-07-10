using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderSupport
{
    public class NullProviderDependencyHelper : ProviderDependencyHelper
    {
        public NullProviderDependencyHelper(ProviderMetadata providerMetadata) : base(providerMetadata)
        {
        }

        protected override void DisposeResources()
        {
            return;
        }
    }
}
