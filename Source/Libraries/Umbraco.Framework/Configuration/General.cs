namespace Umbraco.Framework.Configuration
{
    using System.Configuration;
    using Umbraco.Framework.Configuration.Caching;

    public class General : ConfigurationSection
    {
        public const string ConfigXmlKey = "umbraco/framework";

        public const string CacheProvidersXmlKey = "cacheProviders";

        private static General _loadedSection;
        public static General GetFromConfigManager()
        {
            return _loadedSection ?? (_loadedSection = (General)ConfigurationManager.GetSection(ConfigXmlKey));
        }

        [ConfigurationProperty(CacheProvidersXmlKey, IsRequired = true)]
        public CacheProviders CacheProviders
        {
            get
            {
                var cacheProviders = (CacheProviders)this[CacheProvidersXmlKey];
                cacheProviders.SetParent(this);
                return cacheProviders;
            }
            set
            {
                value.IfNotNull(x => x.SetParent(this));
                this[CacheProvidersXmlKey] = value;
            }
        }

        [ConfigurationProperty("cachePolicies", IsRequired = true)]
        [ConfigurationCollection(typeof(CachePolicyElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMapAlternate)]
        public CachePolicies Policies
        {
            get { return (CachePolicies)this["cachePolicies"]; }
        }
    }
}
