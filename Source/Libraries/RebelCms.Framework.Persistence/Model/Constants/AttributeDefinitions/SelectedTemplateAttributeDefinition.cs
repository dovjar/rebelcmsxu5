using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.AttributeTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions
{

   

    public class SelectedTemplateAttributeDefinition : AttributeDefinition
    {

        public const string AliasValue = "system-internal-selected-template";

        public SelectedTemplateAttributeDefinition(AttributeGroup group)
        {
            this.Setup(AliasValue, "Selected template");
            this.AttributeType = AttributeTypeRegistry.Current.GetAttributeType(SelectedTemplateAttributeType.AliasValue) ?? new SelectedTemplateAttributeType();
            //this.AttributeType = new SelectedTemplateAttributeType();
            this.AttributeGroup = group;
        }
    }
}