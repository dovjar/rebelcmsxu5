using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class TextAttributeType : AttributeType
    {
        public const string AliasValue = "system-long-string-type";

        public TextAttributeType()
            : base(
            AliasValue,
            AliasValue, 
            "used internally for built in long string fields for rebel typed persistence entities", 
            new LongStringSerializationType())
        {
            Id = FixedHiveIds.TextAttributeType;
        }
    }
}