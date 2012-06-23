
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Model
{
    public abstract class AttributedNodeWithAlias : AttributedNode, IReferenceByName
    {
        public string Alias { get; set; }
        public LocalizedString Name { get; set; }
    }
}