using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class MediaPickerAttributeType : AttributeType
    {
        public const string AliasValue = "system-media-picker-type";

        internal MediaPickerAttributeType()
            : base(
            AliasValue,
            AliasValue,
            "This type represents internal system media picker",
            new StringSerializationType())
        {
            Id = FixedHiveIds.MediaPickerAttributeType;
        }
    }
}
