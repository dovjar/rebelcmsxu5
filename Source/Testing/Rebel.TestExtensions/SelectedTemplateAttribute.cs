using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;

namespace Rebel.Tests.Extensions
{
    /// <summary>
    /// Represents an attribute storing the selected template id to use for the entity
    /// </summary>
    public class SelectedTemplateAttribute : TypedAttribute
    {
        public SelectedTemplateAttribute()
            : base(new SelectedTemplateAttributeDefinition())
        {}

        public SelectedTemplateAttribute(HiveId templateId, AttributeGroup group)
            : base(new SelectedTemplateAttributeDefinition(group), templateId.IsNullValueOrEmpty() ? "" : templateId.ToString())
        { }
    }
}