using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class ApplicationsListPickerAttributeType : AttributeType
    {
        public const string AliasValue = "system-applications-list-picker-type";

        public ApplicationsListPickerAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "This type represents internal system applications list picker", 
            new StringSerializationType())
        {
            Id = FixedHiveIds.ApplicationsListPickerAttributeType;
        }
    }
}
