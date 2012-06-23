using System.Configuration;

namespace RebelCms.Cms.Web.Configuration.Dashboards
{
    public class DashboardApplicationElement : ConfigurationElement, IDashboardApplication
    {

        [ConfigurationProperty("app", IsRequired = true)]
        public string ApplicationAlias
        {
            get
            {
                return (string)this["app"];
            }
            set
            {
                this["app"] = value;
            }
        }
    }
}