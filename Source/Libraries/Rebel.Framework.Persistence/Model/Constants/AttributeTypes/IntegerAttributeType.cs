using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class IntegerAttributeType : AttributeType
    {
        public const string AliasValue = "system-integer-type";

        public IntegerAttributeType()
            : base(
            AliasValue,
            AliasValue,
            "used internally for built in integer fields for rebel typed persistence entities",
            new IntegerSerializationType())
        {
            Id = FixedHiveIds.IntegerAttributeType;
        }
    }
}