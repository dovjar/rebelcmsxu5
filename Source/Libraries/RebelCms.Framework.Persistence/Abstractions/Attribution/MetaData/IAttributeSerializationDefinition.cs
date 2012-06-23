
using RebelCms.Framework.Data.Common;
using RebelCms.Framework.Persistence.Model.Attribution;

namespace RebelCms.Framework.Persistence.Abstractions.Attribution.MetaData
{
    using RebelCms.Framework.Data;

    public interface IAttributeSerializationDefinition : IReferenceByName
    {
        /// <summary>
        ///   Gets or sets the type of the data serialization.
        /// </summary>
        /// <value>The type of the data serialization.</value>
        DataSerializationTypes DataSerializationType { get; }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        dynamic Serialize(TypedAttribute value);
    }
}