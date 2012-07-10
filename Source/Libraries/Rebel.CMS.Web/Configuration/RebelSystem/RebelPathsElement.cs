using System.Configuration;

namespace Rebel.Cms.Web.Configuration.RebelSystem
{
    public class RebelPathsElement : ConfigurationElement
    {
        private const string BackOfficeXmlKey = "backOfficePath";

        [ConfigurationProperty(BackOfficeXmlKey, IsRequired = true)]
        public string BackOfficePath
        {
            get { return this[BackOfficeXmlKey].ToString(); }
            set
            {
                this[BackOfficeXmlKey] = value;
            }
        }


        const string LocalizationPathXmlKey = "localizationPath";
        [ConfigurationProperty(LocalizationPathXmlKey, IsRequired = true)]
        public string LocalizationPath
        {
            get { return this[LocalizationPathXmlKey].ToString(); }
            set            
            {
                this[LocalizationPathXmlKey] = value;
            }
        }
    }
}