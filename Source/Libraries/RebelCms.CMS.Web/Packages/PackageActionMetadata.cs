using System.Collections.Generic;
using RebelCms.Cms.Domain.BackOffice;
using RebelCms.Foundation;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Packages
{

    public class PackageActionMetadata : PluginMetadataComposition
    {
        public PackageActionMetadata(IDictionary<string, object> obj)
            : base(obj)
        {
        }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}
