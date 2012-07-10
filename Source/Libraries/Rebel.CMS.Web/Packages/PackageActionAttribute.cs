using Rebel.Cms.Domain.BackOffice;
using Rebel.Framework;

namespace Rebel.Cms.Web.Packages
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