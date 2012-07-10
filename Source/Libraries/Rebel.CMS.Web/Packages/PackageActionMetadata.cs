using System.Collections.Generic;
using Rebel.Cms.Domain.BackOffice;
using Rebel.Foundation;
using Rebel.Framework;

namespace Rebel.Cms.Web.Packages
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
