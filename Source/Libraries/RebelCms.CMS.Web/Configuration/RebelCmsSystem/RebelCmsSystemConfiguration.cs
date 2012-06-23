using System.Configuration;

namespace RebelCms.Cms.Web.Configuration.RebelCmsSystem
{
    public class RebelCmsSystemConfiguration : ConfigurationSection
    {
        public const string ConfigXmlKey = RebelCmsSettings.GroupXmlKey + "/system";

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("public-packages", IsRequired = true)]
        public PublicPackageRepositoryElement PublicPackageRepository
        {
            get
            {
                return (PublicPackageRepositoryElement)base["public-packages"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("folders", IsRequired = true)]
        public RebelCmsFoldersElement Folders
        {
            get
            {
                return (RebelCmsFoldersElement)base["folders"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("paths", IsRequired = true)]
        public RebelCmsPathsElement Paths
        {
            get
            {
                return (RebelCmsPathsElement)base["paths"];
            }
        }

        [ConfigurationCollection(typeof(NameValueConfigurationCollection))]
        [ConfigurationProperty("urls", IsRequired = true)]
        public RebelCmsUrlsElement Urls
        {
            get
            {
                return (RebelCmsUrlsElement)base["urls"];
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
