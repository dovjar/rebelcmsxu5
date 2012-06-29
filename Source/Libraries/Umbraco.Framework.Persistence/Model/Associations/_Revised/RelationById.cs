using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Umbraco.Framework.Persistence.Model.Associations._Revised
{
    /// <summary>
    /// Represents a relation between two items that can be identified by <see cref="HiveId"/> values.
    /// This relation class is used for lazy-loading where only the identifiers are loaded rather than the whole entity.
    /// </summary>
    [DataContract(IsReference = true)]
    public class RelationById : AbstractEquatableObject<RelationById>, IRelationById
    {
        public RelationById()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationById"/> class.
        /// </summary>
        /// <param name="sourceId">The source id.</param>
        /// <param name="destinationId">The destination id.</param>
        /// <param name="relationType">Type of the relation.</param>
        /// <param name="ordinal">The ordinal.</param>
        /// <param name="metaData">The meta data.</param>
        public RelationById(HiveId sourceId, HiveId destinationId, AbstractRelationType relationType, int ordinal, params RelationMetaDatum[] metaData)
            : this(relationType, ordinal, metaData)
        {
            Mandate.ParameterNotEmpty(sourceId, "sourceId");
            Mandate.ParameterNotEmpty(destinationId, "destinationId");
            _sourceId = sourceId;
            _destinationId = destinationId;
            Ordinal = ordinal;
        }

        protected RelationById(AbstractRelationType type, int ordinal, params RelationMetaDatum[] metaData)
        {
            Type = type;
            Ordinal = ordinal;
            MetaData = new RelationMetaDataCollection(metaData);
        }

        private HiveId _sourceId;
        private HiveId _destinationId;

        /// <summary>
        /// Gets or sets the destination id.
        /// </summary>
        /// <value>The destination id.</value>
        [DataMember]
        public virtual HiveId DestinationId
        {
            get { return _destinationId; }
            set { _destinationId = value; }
        }

        /// <summary>
        /// Gets or sets the source id.
        /// </summary>
        /// <value>The source id.</value>
        [DataMember]
        public virtual HiveId SourceId
        {
            get { return _sourceId; }
            set { _sourceId = value; }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        [DataMember]
        public AbstractRelationType Type { get; set; }

        /// <summary>
        /// Gets or sets the meta data.
        /// </summary>
        /// <value>The meta data.</value>
        [DataMember]
        public RelationMetaDataCollection MetaData { get; set; }

        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        [DataMember]
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets the members for equality comparison.
        /// </summary>
        /// <returns></returns>
        protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.SourceId);
            yield return this.GetPropertyInfo(x => x.DestinationId);
            yield return this.GetPropertyInfo(x => x.Type);
            yield return this.GetPropertyInfo(x => x.MetaData);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. 
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public int CompareTo(int other)
        {
            return ((IComparable)Ordinal).CompareTo(other);
        }

        public bool EqualsIgnoringProviderId(IRelationById other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;

            var equals = true;

            if (Type != other.Type) equals = false;
            if (!SourceId.EqualsIgnoringProviderId(other.SourceId)) equals = false;
            if (!DestinationId.EqualsIgnoringProviderId(other.DestinationId)) equals = false;
            if (!MetaData.OrderBy(x => x.Key).SequenceEqual(other.MetaData.OrderBy(x => x.Key))) equals = false;

            return equals;
        }
    }
}