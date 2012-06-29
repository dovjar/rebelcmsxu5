using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Security.Configuration
{
    public class BackofficeCookieElement : ConfigurationElement
    {
        private const string NameXmlKey = "name";
        private const string DomainTypeXmlKey = "domainType";
        private const string DomainXmlKey = "domain";

        [ConfigurationProperty(NameXmlKey, IsRequired = true)]
        public string Name { get { return (string)this[NameXmlKey]; } set { this[NameXmlKey] = value; } }

        [ConfigurationProperty(DomainTypeXmlKey, IsRequired = true)]
        public string DomainTypeString
        {
            get { return (string)this[DomainTypeXmlKey]; }
            set { this[DomainTypeXmlKey] = value; }
        }

        [ConfigurationProperty(DomainTypeXmlKey, IsRequired = true)]
        public BackofficeCookieDomainType DomainType
        {
            get { return (BackofficeCookieDomainType)Enum.Parse(typeof(BackofficeCookieDomainType), DomainTypeString); }
            set { DomainTypeString = value.ToString(); }
        }

        [ConfigurationProperty(DomainXmlKey, IsRequired = false, DefaultValue = "")]
        public string Domain { get { return (string)this[DomainXmlKey]; } set { this[DomainXmlKey] = value; } }
    }
}
