using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Framework.Persistence.Model.Attribution.MetaData
{
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(IsReference = true)]
    public class InheritedAttributeDefinition : AttributeDefinition
    {
        [DataMember]
        public EntitySchema Schema { get; set; }

        /// <summary>
        /// Used for infrastructure (json deserialization) only.
        /// </summary>
        public InheritedAttributeDefinition()
        {
            
        }

        public InheritedAttributeDefinition(AttributeDefinition attributeDefinition, EntitySchema schema)
            : base(attributeDefinition.Alias, attributeDefinition.Name)
        {
            Id = attributeDefinition.Id;
            Description = attributeDefinition.Description;
            AttributeType = attributeDefinition.AttributeType;
            Ordinal = attributeDefinition.Ordinal;
            RenderTypeProviderConfigOverride = attributeDefinition.RenderTypeProviderConfigOverride;
            AttributeGroup = attributeDefinition.AttributeGroup != null ? new InheritedAttributeGroup(attributeDefinition.AttributeGroup, schema) : null;
            UtcCreated = attributeDefinition.UtcCreated;
            UtcModified = attributeDefinition.UtcModified;
            UtcStatusChanged = attributeDefinition.UtcStatusChanged;

            Schema = schema;
        }
    }
}
