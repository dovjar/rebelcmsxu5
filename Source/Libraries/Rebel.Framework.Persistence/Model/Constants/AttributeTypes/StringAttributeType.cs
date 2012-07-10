using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class StringAttributeType : AttributeType
    {
        public const string AliasValue = "system-string-type";

        public StringAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "This type represents internal system text", 
            new StringSerializationType())
        {
            Id = FixedHiveIds.StringAttributeType;
        }
    }
}