using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class ReadOnlyAttributeType : AttributeType
    {
        public const string AliasValue = "system-read-only-type";

        public ReadOnlyAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents internal system read only values",
                new StringSerializationType())
        {
            Id = FixedHiveIds.ReadOnlyAttributeType;
        }
    }
}
