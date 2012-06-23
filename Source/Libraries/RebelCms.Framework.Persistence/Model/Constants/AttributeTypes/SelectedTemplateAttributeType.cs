using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class SelectedTemplateAttributeType : AttributeType
    {
        public const string AliasValue = "system-selected-template-type";

        internal SelectedTemplateAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents the internal SelectedTemplate",
                new StringSerializationType())
        {
            Id = FixedHiveIds.SelectedTemplateAttributeTypeId;
        }
    }
}