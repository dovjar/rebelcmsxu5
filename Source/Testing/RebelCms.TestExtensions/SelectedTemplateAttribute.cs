using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model.Attribution;
using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace RebelCms.Tests.Extensions
{
    /// <summary>
    /// Represents an attribute storing the selected template id to use for the entity
    /// </summary>
    public class SelectedTemplateAttribute : TypedAttribute
    {
        public SelectedTemplateAttribute(HiveId templateId, AttributeGroup group)
            : base(new SelectedTemplateAttributeDefinition(group), templateId.IsNullValueOrEmpty() ? "" : templateId.ToString())
        { }
    }
}