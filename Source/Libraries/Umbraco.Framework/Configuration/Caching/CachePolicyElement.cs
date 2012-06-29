namespace Umbraco.Framework.Configuration.Caching
{
    using System;
    using System.Configuration;
    using Umbraco.Framework.Caching;

    public class CachePolicyElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("durationSeconds", IsRequired = true)]
        public int DurationSeconds
        {
            get { return (int)this["durationSeconds"]; }
            set { base["durationSeconds"] = value; }
        }

        public ICachePolicy ToPolicy()
        {
            return new StaticCachePolicy(TimeSpan.FromSeconds(DurationSeconds));
        }
    }
}