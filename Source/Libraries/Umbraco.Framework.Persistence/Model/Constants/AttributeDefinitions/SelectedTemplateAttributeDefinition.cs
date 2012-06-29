using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions
{

   

    public class SelectedTemplateAttributeDefinition : AttributeDefinition
    {

        public const string AliasValue = "system-internal-selected-template";

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedTemplateAttributeDefinition"/> class.
        /// </summary>
        /// <remarks>Used for deserialization only - not intended for general use.</remarks>
        public SelectedTemplateAttributeDefinition()
        {
            this.Setup(AliasValue, "Selected template");
            this.AttributeType = new SelectedTemplateAttributeType();
            if (AttributeTypeRegistry.Current != null)
            {
                this.AttributeType = AttributeTypeRegistry.Current.GetAttributeType(SelectedTemplateAttributeType.AliasValue) ?? this.AttributeType;
            }
        }

        public SelectedTemplateAttributeDefinition(AttributeGroup group)
            : this()
        {
            this.AttributeGroup = group;
        }
    }
}