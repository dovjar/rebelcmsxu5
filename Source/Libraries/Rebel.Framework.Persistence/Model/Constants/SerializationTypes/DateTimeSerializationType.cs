using Rebel.Framework.Persistence.Abstractions.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Attribution;

namespace Rebel.Framework.Persistence.Model.Constants.SerializationTypes
{
    using Rebel.Framework.Data;

    public class DateTimeSerializationType : IAttributeSerializationDefinition
    {
        #region Implementation of IReferenceByName

        public string Alias { get; set; }
        public LocalizedString Name { get; set; }

        #endregion

        #region Implementation of IAttributeSerializationDefinition

        public DataSerializationTypes DataSerializationType { get { return DataSerializationTypes.Date; } }
        public dynamic Serialize(TypedAttribute value)
        {
            return (dynamic)value.DynamicValue;
        }

        #endregion
    }
}