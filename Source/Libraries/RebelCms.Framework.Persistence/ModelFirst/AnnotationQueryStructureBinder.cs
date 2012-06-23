using System;
using System.Linq;
using System.Linq.Expressions;

using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.ModelFirst.Annotations;

namespace RebelCms.Framework.Persistence.ModelFirst
{
    using RebelCms.Framework.Linq.CriteriaGeneration.Expressions;

    using RebelCms.Framework.Linq.CriteriaGeneration.StructureMetadata;

    public class AnnotationQueryStructureBinder : DefaultQueryStructureBinder 
    {
        public override FieldSelectorExpression CreateFieldSelector(MemberExpression expression, BindingSignatureSupport reportedSignatureSupport)
        {
            var attrib = GetAutoAttribute(expression);

            return attrib == null ? base.CreateFieldSelector(expression, reportedSignatureSupport) : new FieldSelectorExpression(attrib.Alias);
        }

        public static AttributeAliasAttribute GetAutoAttribute(MemberExpression fromProperty)
        {
            var property = fromProperty.Member;
            return property.GetCustomAttributes<AttributeAliasAttribute>(true).SingleOrDefault();
        }

        public static AttributeAliasAttribute GetAutoAttribute<T, TImplementor>(Expression<Func<TImplementor, T>> fromProperty) where TImplementor : TypedEntity
        {
            var propertyExpression = fromProperty.Body as MemberExpression;
            var property = propertyExpression.Member;
            var attrib = property.GetCustomAttributes<AttributeAliasAttribute>(true).SingleOrDefault();
            if (attrib == null)
            {
                throw new InvalidOperationException(
                    "Cannot use automatic getter / setter without using an AttributeAlias annotation. Property: {0}".
                        InvariantFormat(property.Name));
            }
            return attrib;
        }
    }
}
