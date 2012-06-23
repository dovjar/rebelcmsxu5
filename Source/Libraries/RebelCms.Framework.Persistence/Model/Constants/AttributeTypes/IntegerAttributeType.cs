using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class IntegerAttributeType : AttributeType
    {
        public const string AliasValue = "system-integer-type";

        internal IntegerAttributeType()
            : base(
            AliasValue,
            AliasValue,
            "used internally for built in integer fields for RebelCms typed persistence entities",
            new IntegerSerializationType())
        {
            Id = FixedHiveIds.IntegerAttributeType;
        }
    }
}