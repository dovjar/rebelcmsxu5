using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;

namespace Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions
{

    public class NodeNameAttributeDefinition : AttributeDefinition
    {
        public const string AliasValue = "system-internal-node-name";

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedTemplateAttributeDefinition"/> class.
        /// </summary>
        /// <remarks>Used for deserialization only - not intended for general use.</remarks>
        public NodeNameAttributeDefinition()
        {
            this.Setup(AliasValue, "Node Name");
            this.AttributeType = new NodeNameAttributeType();
            if (AttributeTypeRegistry.Current != null)
            {
                this.AttributeType = AttributeTypeRegistry.Current.GetAttributeType(NodeNameAttributeType.AliasValue) ?? this.AttributeType;
            }
        }

        public NodeNameAttributeDefinition(AttributeGroup group)
            : this()
        {
            this.AttributeGroup = group;
        }
    }
}