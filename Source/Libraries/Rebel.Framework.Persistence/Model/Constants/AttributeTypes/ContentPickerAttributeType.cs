using Newtonsoft.Json;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class ContentPickerAttributeType : AttributeType
    {
        public const string AliasValue = "system-content-picker-type";

        public ContentPickerAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "This type represents internal system content picker", 
            new StringSerializationType())
        {
            Id = FixedHiveIds.ContentPickerAttributeType;
        }
    }
}
