using System;
using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Attribution;

namespace Umbraco.Framework.Persistence.Model.Constants.SerializationTypes
{
    using Umbraco.Framework.Data;

    public class IntegerSerializationType : IAttributeSerializationDefinition
    {
        #region Implementation of IReferenceByName

        public string Alias { get; set; }
        public LocalizedString Name { get; set; }

        #endregion

        #region Implementation of IAttributeSerializationDefinition

        public DataSerializationTypes DataSerializationType { get { return DataSerializationTypes.LargeInt; } }
        public dynamic Serialize(TypedAttribute value)
        {
            return (dynamic)value.DynamicValue;
        }

        #endregion
    }

    public class DecimalSerializationType : IAttributeSerializationDefinition
    {
        public string Alias { get; set; }
        public LocalizedString Name { get; set; }

        public DataSerializationTypes DataSerializationType
        {
            get
            {
                return DataSerializationTypes.Decimal;
            }
        }

        public dynamic Serialize(TypedAttribute value)
        {
            return (dynamic)value.DynamicValue;
        }
    }

    [Obsolete("Do not use; deserialization from datastore requires parameterless constructor for serialization types")]
    public class GeneralSerializationType : IAttributeSerializationDefinition
    {
        public GeneralSerializationType()
            : this(DataSerializationTypes.SmallInt)
        { }

        public GeneralSerializationType(DataSerializationTypes dataSerializationType)
        {
            DataSerializationType = dataSerializationType;
        }

        public string Alias { get; set; }
        public LocalizedString Name { get; set; }

        public DataSerializationTypes DataSerializationType { get; protected set; }

        public dynamic Serialize(TypedAttribute value)
        {
            return (dynamic)value.DynamicValue;
        }
    }
}