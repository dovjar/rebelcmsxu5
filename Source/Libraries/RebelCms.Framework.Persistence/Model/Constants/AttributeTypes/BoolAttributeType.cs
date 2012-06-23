using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class BoolAttributeType : AttributeType
    {
        public const string AliasValue = "system-bool-type";

        internal BoolAttributeType()
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