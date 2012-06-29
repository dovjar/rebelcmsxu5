using System;
using System.Linq;
using System.Linq.Expressions;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.ModelFirst.Annotations;

namespace Umbraco.Framework.Persistence.ModelFirst
{
    using System.Collections.Generic;
    using Umbraco.Framework.Persistence.Model.Attribution;

    public class CustomTypedEntity<TImplementor> : CustomTypedEntity
        where TImplementor : TypedEntity
    {
        protected T BaseAutoGet<T>(Expression<Func<TImplementor, T>> fromProperty, T defaultValue = default(T))
        {
            var attrib = AnnotationQueryStructureBinder.GetAutoAttribute(fromProperty);
            return this.Attribute<T>(attrib.Alias, defaultValue);
        }

        protected void BaseAutoSet<T>(Expression<Func<TImplementor, T>> forProperty, T value)
        {
            var attrib = AnnotationQueryStructureBinder.GetAutoAttribute(forProperty);
            base.BaseAutoSet(attrib.Alias, value);
        }

        protected T BaseAutoGetInner<T>(Expression<Func<TImplementor, T>> fromProperty, string valueKey, T defaultValue = default(T))
        {
            var attrib = AnnotationQueryStructureBinder.GetAutoAttribute(fromProperty);
            return this.InnerAttribute<T>(attrib.Alias, valueKey, defaultValue);
        }

        protected void BaseAutoSetInner<T>(Expression<Func<TImplementor, T>> forProperty, string valueKey, T value)
        {
            var attrib = AnnotationQueryStructureBinder.GetAutoAttribute(forProperty);
            base.BaseAutoSetInner(attrib.Alias, valueKey, value);
        }
    }

    public class CustomTypedEntity : TypedEntity
    {
        public object this[string alias]
        {
            get { return Attributes[alias].GetDefaultValue(); }
            set { Attributes[alias].SetDefaultValue(value); }
        }

        protected T BaseAutoGet<T>(string attributeAlias, T defaultValue = default(T))
        {
            return this.Attribute<T>(attributeAlias, defaultValue);
        }

        protected void BaseAutoSet<T>(string attributeAlias, T value)
        {
            if (!this.Attributes.ContainsKey(attributeAlias))
                throw new KeyNotFoundException("'{0}' isn't a valid alias as it doesn't exist on the object yet".InvariantFormat(attributeAlias));
            this.Attributes[attributeAlias].SetDefaultValue(value);
        }

        protected T BaseAutoGetInner<T>(string attributeAlias, string valueKey, T defaultValue = default(T))
        {
            return this.InnerAttribute<T>(attributeAlias, valueKey, defaultValue);
        }

        protected void BaseAutoSetInner<T>(string attributeAlias, string valueKey, T value)
        {
            try
            {
                var attr = this.Attributes[attributeAlias];
                if (attr.Values.ContainsKey(valueKey))
                    attr.Values[valueKey] = value;
                else
                    attr.Values.Add(valueKey, value);
            }
            catch (KeyNotFoundException ex)
            {
                throw new KeyNotFoundException("'{0}' isn't a valid alias as it doesn't exist on the object yet".InvariantFormat(attributeAlias), ex);
            }
        }
    }
}
