using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class DateTimeAttributeType : AttributeType
    {
        public const string AliasValue = "system-date-time-type";

        public DateTimeAttributeType()
            : base(
            AliasValue,
            AliasValue,
            "used internally for built in DateTime fields for rebel typed persistence entities",
            new DateTimeSerializationType())
        {
            Id = FixedHiveIds.DateTimeAttributeType;
        }
    }
}