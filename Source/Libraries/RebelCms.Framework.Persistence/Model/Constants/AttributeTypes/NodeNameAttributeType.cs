using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.SerializationTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class NodeNameAttributeType : AttributeType
    {
        public const string AliasValue = "system-node-name-type";

        internal NodeNameAttributeType()
            : base(
                AliasValue,
                AliasValue,
                "This type represents the internal NodeName",
                new StringSerializationType())
        {
            Id = FixedHiveIds.NodeNameAttributeTypeId;
        }
    }
}