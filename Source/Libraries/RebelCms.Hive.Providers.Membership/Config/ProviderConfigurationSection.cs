using System.Configuration;
using RebelCms.Framework.ProviderSupport;

namespace RebelCms.Hive.Providers.Membership.Config
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
