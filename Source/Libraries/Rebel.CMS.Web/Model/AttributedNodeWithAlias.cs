
using Rebel.Framework;

namespace Rebel.Cms.Web.Model
{
    public abstract class AttributedNodeWithAlias : AttributedNode, IReferenceByName
    {
        public string Alias { get; set; }
        public LocalizedString Name { get; set; }
    }
}