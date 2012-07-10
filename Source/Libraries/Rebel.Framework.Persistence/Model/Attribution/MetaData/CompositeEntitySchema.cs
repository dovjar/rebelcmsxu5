using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Rebel.Framework.Persistence.Model.Associations._Revised;

namespace Rebel.Framework.Persistence.Model.Attribution.MetaData
{
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract(IsReference = true)]
    public class CompositeEntitySchema : EntitySchema
    {
        [DataMember]
        public EntityCollection<InheritedAttributeDefinition> InheritedAttributeDefinitions { get; set; }

        [DataMember]
        public EntityCollection<InheritedAttributeGroup> InheritedAttributeGroups { get; set; }

        [IgnoreDataMember]
        public IEnumerable<AttributeType> InheritedAttributeTypes 
        {
            get { return InheritedAttributeDefinitions.Select(x => x.AttributeType).Distinct(); }
        }

        [IgnoreDataMember]
        public IEnumerable<InheritedAttributeDefinition> AllAttributeDefinitions
        {
            get
            {
                return AttributeDefinitions.Select(x => new InheritedAttributeDefinition(x, this)).Concat(InheritedAttributeDefinitions).Distinct();
            }
        }

        [IgnoreDataMember]
        public IEnumerable<InheritedAttributeGroup> AllAttributeGroups
        {
            get
            {
                return AttributeGroups.Select(x => new InheritedAttributeGroup(x, this)).Concat(InheritedAttributeGroups).Distinct();
            }
        }

        /// <summary>
        /// Used for infrastructure (json deserialization) only. Initializes a new instance of the <see cref="CompositeEntitySchema"/> class.
        /// </summary>
        public CompositeEntitySchema()
        {
            
        }

        public CompositeEntitySchema(EntitySchema schema, IEnumerable<EntitySchema> ancestorSchemas, bool copyRelationProxiesFromSchema = false)
            : base(schema.Alias, schema.Name)
        {
            //TODO: Need to move this into a mapper, but not currently one available at the right level
            Id = schema.Id;
            SchemaType = SchemaType;
            AttributeGroups.AddRange(schema.AttributeGroups);
            AttributeDefinitions.AddRange(schema.AttributeDefinitions);
            UtcCreated = schema.UtcCreated;
            UtcModified = schema.UtcModified;
            UtcStatusChanged = schema.UtcStatusChanged;

            var inheritedDefsDict = new Dictionary<string, InheritedAttributeDefinition>();
            var inheritedDefs = ancestorSchemas
                .SelectMany(entitySchema => entitySchema.AttributeDefinitions.Select(definition => new InheritedAttributeDefinition(definition, entitySchema))).ToArray();
            foreach (var def in inheritedDefs)
            {
                if (!inheritedDefsDict.ContainsKey(def.Alias))
                    inheritedDefsDict.Add(def.Alias, def);
            }

            InheritedAttributeDefinitions = new EntityCollection<InheritedAttributeDefinition>(inheritedDefsDict.Values);

            // Need to only show the inherited groups that are exposed by the filtered inherited attribute definitions, but also include empty
            // groups so that they have a chance to have some definitions added
            var allLinkedGroups = ancestorSchemas.SelectMany(x => x.AttributeDefinitions.Select(y => new InheritedAttributeGroup(y.AttributeGroup, x)));
            var allKnownGroups = ancestorSchemas.SelectMany(x => x.AttributeGroups.Select(y => new InheritedAttributeGroup(y, x)));
            var unlinkedGroups = allKnownGroups.Except(allLinkedGroups);

            var inheritedGroups =
                InheritedAttributeDefinitions.Select(x => new InheritedAttributeGroup(x.AttributeGroup, x.Schema)).Union
                    (unlinkedGroups);

            InheritedAttributeGroups = new EntityCollection<InheritedAttributeGroup>(inheritedGroups);

            RelationProxies.LazyLoadDelegate = schema.RelationProxies.LazyLoadDelegate;
            XmlConfiguration = schema.XmlConfiguration;

            if (copyRelationProxiesFromSchema)
            {
                foreach (var proxies in schema.RelationProxies.GetManualParentProxies())
                {
                    RelationProxies.EnlistParent(proxies.Item.Source, proxies.Item.Type, proxies.Item.Ordinal, proxies.Item.MetaData.ToArray());
                }
                foreach (var proxies in schema.RelationProxies.GetManualChildProxies())
                {
                    RelationProxies.EnlistParent(proxies.Item.Source, proxies.Item.Type, proxies.Item.Ordinal, proxies.Item.MetaData.ToArray());
                }
            }
        }
    }
}
