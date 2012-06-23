using RebelCms.Framework.Data.Common;

namespace RebelCms.Framework.EntityGraph.Domain.Entity.Attribution.MetaData
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TAllowedType"></typeparam>
    public interface IAttributeSerializationDefinition : IEntity, IReferenceByAlias
    {
        /// <summary>
        ///   Gets or sets the type of the data serialization.
        /// </summary>
        /// <value>The type of the data serialization.</value>
        DataSerializationTypes DataSerializationType { get; set; }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        dynamic Serialize(ITypedAttribute value);
    }
}