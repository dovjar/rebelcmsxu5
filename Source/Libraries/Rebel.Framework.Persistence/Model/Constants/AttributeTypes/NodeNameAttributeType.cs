using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeTypes
{
    public class NodeNameAttributeType : AttributeType
    {
        public const string AliasValue = "system-node-name-type";

        public NodeNameAttributeType()
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