using RebelCms.Framework.Persistence.Model.Attribution.MetaData;
using RebelCms.Framework.Persistence.Model.Constants.AttributeTypes;

namespace RebelCms.Framework.Persistence.Model.Constants.AttributeDefinitions
{

    public class NodeNameAttributeDefinition : AttributeDefinition
    {
        public const string AliasValue = "system-internal-node-name";

        public NodeNameAttributeDefinition(AttributeGroup group)
        {
            this.Setup(AliasValue, "Node Name");
            this.AttributeType = AttributeTypeRegistry.Current.GetAttributeType(NodeNameAttributeType.AliasValue) ?? new NodeNameAttributeType();
            //this.AttributeType = new NodeNameAttributeType();
            this.AttributeGroup = group;
        }
    }
}