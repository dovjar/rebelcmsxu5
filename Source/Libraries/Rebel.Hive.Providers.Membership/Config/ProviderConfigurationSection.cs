using System.Configuration;
using Rebel.Framework.ProviderSupport;

namespace Rebel.Hive.Providers.Membership.Config
{
    /// <summary>
    /// 
    /// </summary>
    public class ProviderConfigurationSection : AbstractProviderConfigurationSection
    {
        [ConfigurationProperty("providers")]
        public ProviderCollection MembershipProviders
        {
            get { return (ProviderCollection) this["providers"]; }
            set { this["providers"] = value; }
        }
    }
}
