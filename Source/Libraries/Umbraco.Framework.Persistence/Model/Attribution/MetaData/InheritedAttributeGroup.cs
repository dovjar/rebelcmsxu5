using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Persistence.Model.Attribution.MetaData
{
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(IsReference = true)]
    public class InheritedAttributeGroup : AttributeGroup
    {
        [DataMember]
        public EntitySchema Schema { get; set; }

        /// <summary>
        /// Used for infrastructure (json deserialization) only.
        /// </summary>
        public InheritedAttributeGroup()
        {
            
        }

        public InheritedAttributeGroup(AttributeGroup attributeGroup, EntitySchema schema)
            : base(attributeGroup.Alias, attributeGroup.Name, attributeGroup.Ordinal)
        {
            Id = attributeGroup.Id;
            UtcCreated = attributeGroup.UtcCreated;
            UtcModified = attributeGroup.UtcModified;
            UtcStatusChanged = attributeGroup.UtcStatusChanged;

            Schema = schema;
        }
    }
}
