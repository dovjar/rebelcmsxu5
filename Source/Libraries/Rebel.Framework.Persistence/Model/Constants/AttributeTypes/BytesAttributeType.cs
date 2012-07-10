using Newtonsoft.Json;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class BytesAttributeType : AttributeType
    {
        public const string AliasValue = "system-bytearray-type";

        public BytesAttributeType()
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