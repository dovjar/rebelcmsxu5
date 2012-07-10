using System.Configuration;
using Rebel.Framework.Security.Configuration;

namespace Rebel.Cms.Web.Configuration.RebelSystem
{
    public class RebelSystemConfiguration : ConfigurationSection
    {
        public const string ConfigXmlKey = RebelSettings.GroupXmlKey + "/system";

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("public-packages", IsRequired = true)]
        public PackageRepositoryElement PublicPackageRepository
        {
            get
            {
                return (PackageRepositoryElement)base["public-packages"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("folders", IsRequired = true)]
        public RebelFoldersElement Folders
        {
            get
            {
                return (RebelFoldersElement)base["folders"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("paths", IsRequired = true)]
        public RebelPathsElement Paths
        {
            get
            {
                return (RebelPathsElement)base["paths"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("urls", IsRequired = true)]
        public RebelUrlsElement Urls
        {
            get
            {
                return (RebelUrlsElement)base["urls"];
            }
        }

        [ConfigurationProperty(RouteMatchElementCollection.CollectionXmlKey, IsRequired = true)]
        public RouteMatchElementCollection RouteMatches
        {
            get { return this[RouteMatchElementCollection.CollectionXmlKey] as RouteMatchElementCollection; }
            set
            {
                this[RouteMatchElementCollection.CollectionXmlKey] = value;
            }
        }
    }
}
