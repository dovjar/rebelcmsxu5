using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class BytesAttributeType : AttributeType
    {
        public const string AliasValue = "system-bytearray-type";

        internal BytesAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents an internal system byte array",
                new ByteArraySerializationType())
        {
            Id = FixedHiveIds.ByteArrayAttributeType;
        }
    }
}