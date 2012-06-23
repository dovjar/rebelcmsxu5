using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class DateTimeAttributeType : AttributeType
    {
        public const string AliasValue = "system-date-time-type";

        internal DateTimeAttributeType()
            : base(
            AliasValue,
            AliasValue,
            "used internally for built in DateTime fields for RebelCms typed persistence entities",
            new DateTimeSerializationType())
        {
            Id = FixedHiveIds.DateTimeAttributeType;
        }
    }
}