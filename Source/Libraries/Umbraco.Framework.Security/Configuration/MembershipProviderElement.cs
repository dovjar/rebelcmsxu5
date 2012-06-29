using System;
using System.Configuration;

namespace Umbraco.Framework.Security.Configuration
{
    public class MembershipProviderElement : ConfigurationElement
    {
        private const string NameXmlKey = "name";
        private const string TypeXmlKey = "type";
        private const string ProviderUserKeyTypeXmlKey = "providerUserKeyType";
        private const string WildcardXmlKey = "wildcard";

        [ConfigurationProperty(NameXmlKey, IsKey = true, IsRequired = true)]
        public string Name { get { return (string)this[NameXmlKey]; } set { this[NameXmlKey] = value; } }

        [ConfigurationProperty(TypeXmlKey, IsRequired = true)]
        public string TypeString
        {
            get { return (string)this[TypeXmlKey]; } 
            set { this[TypeXmlKey] = value; }
        }

        public Type Type
        {
            get { return Type.GetType(TypeString); }
            set { TypeString = value.AssemblyQualifiedName; }
        }

        [ConfigurationProperty(ProviderUserKeyTypeXmlKey, IsRequired = true)]
        public string ProviderUserKeyTypeString
        {
            get { return (string)this[ProviderUserKeyTypeXmlKey]; }
            set { this[ProviderUserKeyTypeXmlKey] = value; }
        }

        public Type ProviderUserKeyType
        {
            get { return Type.GetType(ProviderUserKeyTypeString); }
            set { ProviderUserKeyTypeString = value.AssemblyQualifiedName; }
        }

        [ConfigurationProperty(WildcardXmlKey, IsRequired = true)]
        public string Wildcard { get { return (string)this[WildcardXmlKey]; } set { this[WildcardXmlKey] = value; } }
    }
}
