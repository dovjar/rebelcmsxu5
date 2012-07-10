using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class SelectedTemplateAttributeType : AttributeType
    {
        public const string AliasValue = "system-selected-template-type";

        public SelectedTemplateAttributeType()
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