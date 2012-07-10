using System.Configuration;

namespace Rebel.Framework.Persistence.XmlStore.Config
{
    public class ProviderConfigurationSection : ConfigurationSection 
    {
        private const string PathXmlKey = "path";

        [ConfigurationProperty(PathXmlKey)]
        public string Path
        {
            get { return (string)this[PathXmlKey]; }

            set { this[PathXmlKey] = value; }
        }
    }
}
