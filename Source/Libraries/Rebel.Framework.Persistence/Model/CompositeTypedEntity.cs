using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;

namespace Rebel.Framework.Persistence.Model
{
    public class CompositeTypedEntity : TypedEntity
    {
        public CompositeEntitySchema CompositeEntitySchema { get; set; }

        public TypedAttributeCollection InheritedAttributes { get; private set; }

        public IEnumerable<AttributeGroup> InheritedAttributeGroups
        {
            get
            {
                return CompositeEntitySchema == null
                           ? Enumerable.Empty<AttributeGroup>()
                           : CompositeEntitySchema.InheritedAttributeGroups;
            }
        }

        public CompositeTypedEntity(TypedEntity entity)
        {
            Attributes.Clear();
            entity.Attributes.ForEach(x => Attributes.Add(x));

            RelationProxies.LazyLoadDelegate = entity.RelationProxies.LazyLoadDelegate;
        }
    }
}
