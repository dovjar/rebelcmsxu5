using RebelCms.Cms.Domain.BackOffice;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Packages
{
    /// <summary>
    /// Attribute that defines a Package Action
    /// </summary>
    public class PackageActionAttribute : PluginAttribute
    {
        public string Name { get; private set; }

        public PackageActionAttribute(string id, string name) 
            : base(id)
        {
            Name = name;
        }
    }
}