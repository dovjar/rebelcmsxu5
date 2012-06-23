using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;
using RebelCms.Hive.ProviderSupport;

namespace RebelCms.Hive.Providers.IO
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
