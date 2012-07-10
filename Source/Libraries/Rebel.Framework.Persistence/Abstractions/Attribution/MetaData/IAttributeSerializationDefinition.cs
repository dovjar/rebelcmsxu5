
using Rebel.Framework.Data.Common;
using Rebel.Framework.Persistence.Model.Attribution;

namespace Rebel.Framework.Persistence.Abstractions.Attribution.MetaData
{
    using Rebel.Framework.Data;

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