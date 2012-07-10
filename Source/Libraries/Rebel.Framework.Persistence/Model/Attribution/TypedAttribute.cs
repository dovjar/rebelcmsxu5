using System;
using System.Collections.Generic;
using System.Diagnostics;

using System.Linq;
using System.Reflection;
using Rebel.Framework.Linq.CriteriaGeneration.StructureMetadata;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;

namespace Rebel.Framework.Persistence.Model.Attribution
{
    using System.Runtime.Serialization;

    [DebuggerDisplay("{this.ToDebugString()}")]
    [Serializable]
    [DataContract(IsReference = true)]
    public class TypedAttribute : AbstractEntity
    {
        private AttributeDefinition _attributeDefinition;
        private TypedAttributeValueCollection _values = new TypedAttributeValueCollection();

        public TypedAttribute()
        {
            _values.CollectionChanged += ValuesCollectionChanged;
        }

        public TypedAttribute(AttributeDefinition attributeDefinition)
            : this()
        {
            _attributeDefinition = attributeDefinition;            
        }

        public TypedAttribute(AttributeDefinition attributeDefinition, dynamic defaultValue)
            : this(attributeDefinition)
        {
            DynamicValue = defaultValue;
        }

        protected string ToDebugString()
        {
            var attributeType = AttributeDefinition.AttributeType;
            return string.Concat(typeof(TypedAttribute).Name, " Def Id ", _attributeDefinition.Id, " (type:" + attributeType.Alias + ", type id: " + attributeType.Id + ") Alias: ", AttributeDefinition.Alias, ": ", Values.ToString());
        }

        /// <summary>
        /// Gets or sets the type of the attribute.
        /// </summary>
        /// <value>The type of the attribute.</value>
        [DataMember]
        public virtual AttributeDefinition AttributeDefinition
        {
            get { return _attributeDefinition; }
            set
            {
                _attributeDefinition = value;
                OnPropertyChanged(AttributeDefinitionSelector);
            }
        }
        private readonly static PropertyInfo AttributeDefinitionSelector = ExpressionHelper.GetPropertyInfo<TypedAttribute, AttributeDefinition>(x => x.AttributeDefinition);

        /// <summary>
        /// Tries the get the default value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AttemptTuple<T> TryGetDefaultValue<T>()
        {
            if (Values.Any())
            {
                var val = Values.GetDefaultValue();
                if (val != null)
                {
                    return val.TryConvertTo<T>();
                };
                return Values.Count() == 1 ? Values.Single().Value.TryConvertTo<T>() : Values.TryConvertTo<T>();
            }
            return AttemptTuple<T>.False;
        }

        /// <summary>
        /// Gets or sets the default value of the attribute. This is a shortcut property which stores the value as a default in the <see cref="Values"/> dictionary.
        /// </summary>
        /// <value>The value.</value>
        /// <remarks>
        /// This will try to return the value from the dictionary with the default key, if it is null and there is an item in the dictionary it will return that singular item,
        /// if there is more than 1 item in the dictionary, it will simply return the entire dictionary.
        /// 
        /// Setting this value will only set the value in the Values dictionary based on the default key
        /// </remarks>
        [IgnoreDataMember]
        public dynamic DynamicValue
        {
            get { return GetDefaultValue(); }
            set
            {
                // Need to explicitly cast value to object, as there seems to be a framework bug
                // where it can cause an index out of bounds exception if implied implicitly.
                SetDefaultValue((object)value);
            }
        }

        public object GetDefaultValue()
        {
            if (Values.Any())
            {
                var val = Values.GetDefaultValue();
                if (val != null)
                    return val;
                return Values.Count() == 1 ? Values.First().Value : Values;
            }
            return null;
        }

        public void SetDefaultValue(object value)
        {
            Values.SetDefaultValue(value);
        }

        /// <summary>
        /// Gets or sets the values of the attribute.
        /// </summary>
        /// <value>The values.</value>
        [MapsToInnerAliasForQuerying]
        [DataMember]
        public TypedAttributeValueCollection Values
        {
            get { return _values; }
            set
            {
                _values.IfNotNull(x => x.CollectionChanged -= ValuesCollectionChanged);
                _values = value;
                _values.CollectionChanged += ValuesCollectionChanged;
            }
        }

        public static bool operator ==(TypedAttribute left, object other)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(other, null)) return true;
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(other, null)) return false;
            if (other is TypedAttribute)
                return left.Equals(other as TypedAttribute);

            return left.DynamicValue == other;
        }

        public static bool operator !=(TypedAttribute left, object other)
        {
            return !(left == other);
        }


        /// <summary>
        /// Handles when the Values collection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ValuesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(ValuesSelector);
        }

        private readonly static PropertyInfo ValuesSelector = ExpressionHelper.GetPropertyInfo<TypedAttribute, TypedAttributeValueCollection>(x => x.Values);

    }
}