using System;
using System.Linq;
using System.Linq.Expressions;

using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.ModelFirst.Annotations;

namespace Rebel.Framework.Persistence.ModelFirst
{
    using Rebel.Framework.Linq.CriteriaGeneration.Expressions;

    using Rebel.Framework.Linq.CriteriaGeneration.StructureMetadata;
    using Rebel.Framework.Persistence.Model.LinqSupport;

    public class AnnotationQueryStructureBinder : PersistenceModelStructureBinder 
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
