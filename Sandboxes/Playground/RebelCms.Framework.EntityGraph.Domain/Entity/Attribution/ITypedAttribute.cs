using System.Diagnostics.Contracts;
using RebelCms.Framework.EntityGraph.Domain.Entity.Attribution.MetaData;
using RebelCms.Framework.EntityGraph.Domain.Entity.Graph;

namespace RebelCms.Framework.EntityGraph.Domain.Entity.Attribution
{
    /// <summary>
    /// Represents a strongly-typed attribute value
    /// </summary>
    public interface ITypedAttribute : IEntityVertex // Note may need to change this to IEntity if it's too difficult to have hierarchical attributes
    {
        /// <summary>
        /// Gets or sets the type of the attribute.
        /// </summary>
        /// <value>The type of the attribute.</value>
        IAttributeTypeDefinition AttributeType { get; set; }

        /// <summary>
        /// Gets or sets the value of the attribute.
        /// </summary>
        /// <value>The value.</value>
        dynamic Value { get; set; }
    }
}