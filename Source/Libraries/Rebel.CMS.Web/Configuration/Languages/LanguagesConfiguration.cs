using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Rebel.Cms.Web.Configuration.Languages
{
    public class LanguagesConfiguration : ConfigurationSection
    {
        public const string ConfigXmlKey = RebelSettings.GroupXmlKey + "/languages";

        [ConfigurationCollection(typeof(LanguagesCollection), AddItemName = "language")]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public LanguagesCollection Languages
        {
            get
            {
                return (LanguagesCollection)base[""];
            }
        }
    }
}
