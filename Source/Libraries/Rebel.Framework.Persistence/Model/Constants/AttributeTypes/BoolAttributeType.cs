using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class BoolAttributeType : AttributeType
    {
        public const string AliasValue = "system-bool-type";

        public BoolAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents internal system booleans",
                new BoolSerializationType())
        {
            Id = FixedHiveIds.BoolAttributeType;
        }
    }
}