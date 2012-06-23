using System.Collections.Generic;
using System.Configuration;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Configuration.ApplicationSettings
{
    public class TreeConfiguration : ConfigurationSection, ITreeCollection
    {
        public const string ConfigXmlKey = RebelCmsSettings.GroupXmlKey + "/trees";

        [ConfigurationCollection(typeof(TreeCollection))]
        [ConfigurationProperty("", IsDefaultCollection = true, IsRequired = true)]
        public TreeCollection Trees
        {
            get
            {
                return (TreeCollection)base[""];
            }
        }
        
        IEnumerable<ITree> ITreeCollection.Trees
        {
            get { return Trees.OnlyLocalConfig<ITree>(); }
        }
    }
}