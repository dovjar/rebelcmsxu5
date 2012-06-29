using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Security.Configuration
{
    public class SecurityConfiguration : ConfigurationSection
    {
        public const string ConfigXmlKey = "security";

        private const string BackofficeCookieXmlKey = "backofficeCookie";

        [ConfigurationProperty(BackofficeCookieXmlKey, IsRequired = true)]
        public BackofficeCookieElement BackofficeCookie
        {
            get { return this[BackofficeCookieXmlKey] as BackofficeCookieElement; }
            set
            {
                this[BackofficeCookieXmlKey] = value;
            }
        }

        [ConfigurationProperty(MembershipProviderElementCollection.CollectionXmlKey, IsRequired = true)]
        public MembershipProviderElementCollection MembershipProviders
        {
            get { return this[MembershipProviderElementCollection.CollectionXmlKey] as MembershipProviderElementCollection; }
            set
            {
                this[MembershipProviderElementCollection.CollectionXmlKey] = value;
            }
        }
    }
}
