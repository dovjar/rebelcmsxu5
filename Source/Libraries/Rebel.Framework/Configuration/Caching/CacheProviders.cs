namespace Rebel.Framework.Configuration.Caching
{
    using System.Configuration;

    public class CacheProviders : ConfigurationElement
    {
        public const string ExtendedLifetimeXmlKey = "extendedLifetime";
        public const string LimitedLifetimeXmlKey = "limitedLifetime";

        internal void SetParent(General general)
        {
            ParentConfig = general;
        }
        protected General ParentConfig { get; private set; }

        [ConfigurationProperty(ExtendedLifetimeXmlKey, IsRequired = true)]
        public ExtendedLifetime ExtendedLifetime
        {
            get
            {
                var extendedLifetime = (ExtendedLifetime)this[ExtendedLifetimeXmlKey];
                extendedLifetime.SetParent(this.ParentConfig);
                return extendedLifetime;
            }
            set
            {
                value.IfNotNull(x => x.SetParent(this.ParentConfig));
                this[ExtendedLifetimeXmlKey] = value;
            }
        }

        [ConfigurationProperty(LimitedLifetimeXmlKey, IsRequired = true)]
        public LimitedLifetime LimitedLifetime
        {
            get
            {
                var limitedLifetime = (LimitedLifetime)this[LimitedLifetimeXmlKey];
                limitedLifetime.SetParent(this.ParentConfig);
                return limitedLifetime;
            }
            set
            {
                value.IfNotNull(x => x.SetParent(this.ParentConfig));
                this[LimitedLifetimeXmlKey] = value;
            }
        }
    }
}