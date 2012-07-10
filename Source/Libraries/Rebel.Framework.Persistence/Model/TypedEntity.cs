using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Associations._Revised;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.LinqSupport;
using Rebel.Framework.Persistence.Model.Versioning;

namespace Rebel.Framework.Persistence.Model
{
    using System.Runtime.Serialization;
    using Rebel.Framework.Linq.CriteriaGeneration.StructureMetadata;

    [QueryStructureBinderOfType(typeof(PersistenceModelStructureBinder))]
    [DebuggerDisplay("Id: {Id}")]
    [Serializable]
    [DataContract(IsReference = true)]
    public class TypedEntity : AbstractEntity, IRelatableEntity, IVersionableEntity
    {
        public TypedEntity()
        {
            //v2: create the relation proxy collection
            RelationProxies = new RelationProxyCollection(this);

            //create the attributes
            Func<TypedAttribute, bool> validateAdd = x =>
            {
                //if (FixedAttributeDefinitionAliases.AllAliases.Contains(x.AttributeDefinition.Alias))
                //    return true;
                // TODO: Better validation here
                if (EntitySchema == null) return true;

                var composite = EntitySchema as CompositeEntitySchema;
                if (composite != null)
                {
                    return composite.AllAttributeDefinitions.Any(y => y.Alias == x.AttributeDefinition.Alias);
                }

                return EntitySchema.AttributeDefinitions.Any(y => y.Alias == x.AttributeDefinition.Alias);
            };

            Attributes = new TypedAttributeCollection(validateAdd);
        }


        private readonly static PropertyInfo AttributesSelector = ExpressionHelper.GetPropertyInfo<TypedEntity, TypedAttributeCollection>(x => x.Attributes);
        void AttributesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(AttributesSelector);
        }

        private TypedAttributeCollection _attributes;
        [DataMember]
        public TypedAttributeCollection Attributes
        {
            get
            {
                return _attributes;
            }
            protected set
            {
                _attributes = value;
                _attributes.CollectionChanged += AttributesCollectionChanged;
            }
        }

        /// <summary>
        /// Gets the attribute groups.
        /// </summary>
        /// <value>The attribute groups.</value>
        [IgnoreDataMember]
        public IEnumerable<AttributeGroup> AttributeGroups
        {
            get
            {
                return EntitySchema == null
                           ? Enumerable.Empty<AttributeGroup>()
                           : EntitySchema.AttributeGroups;
            }
        }

        /// <summary>
        /// Gets the attribute schema.
        /// </summary>
        /// <value>The attribute schema.</value>
        [DataMember]
        public virtual EntitySchema EntitySchema
        {
            get { return _entitySchema; }
            set
            {
                _entitySchema = value;
                OnPropertyChanged(EntitySchemaSelector);
            }
        }
        private EntitySchema _entitySchema;
        private readonly static PropertyInfo EntitySchemaSelector = ExpressionHelper.GetPropertyInfo<TypedEntity, EntitySchema>(x => x.EntitySchema);

        /// <summary>
        /// A store of relation proxies for this entity, to support enlisting relations to this entity.
        /// The relations will not be persisted until the entity is passed to a repository for saving.
        /// If <see cref="RelationProxyCollection.IsConnected"/> is <code>true</code>, this sequence may have
        /// <see cref="RelationProxy"/> objects lazily loaded by enumerating the results of calling <see cref="RelationProxyCollection.LazyLoadDelegate"/>.
        /// </summary>
        /// <value>The relation proxies.</value>
        [IgnoreDataMember]
        public RelationProxyCollection RelationProxies { get; protected set; }
    }
}