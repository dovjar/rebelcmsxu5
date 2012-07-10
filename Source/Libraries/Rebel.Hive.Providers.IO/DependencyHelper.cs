using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Hive.ProviderSupport;

namespace Rebel.Hive.Providers.IO
{
    public class DependencyHelper : ProviderDependencyHelper 
    {
        public DependencyHelper(Settings settings, ProviderMetadata providerMetadata) : base(providerMetadata)
        {
            Settings = settings;
        }

        public Settings Settings { get; protected set; }

        protected override void DisposeResources()
        {
            return;
        }
    }
}
