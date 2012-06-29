using System;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Criterion;

using Umbraco.Framework.Persistence.RdbmsModel;
using Attribute = Umbraco.Framework.Persistence.RdbmsModel.Attribute;

namespace Umbraco.Framework.Persistence.NHibernate.Linq
{
    using System.Collections.Generic;
    using Umbraco.Framework.Data;
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaTranslation;
    using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
    using global::NHibernate;

    public class NhQueryOverVisitor : AbstractCriteriaVisitor<QueryOver<NodeVersion>>
    {
        private ISession _activeSession;

        public NhQueryOverVisitor(ISession activeSession)
        {
            _activeSession = activeSession;
        }

        private QueryOver<NodeVersion> _queryBuilder;

        #region Overrides of AbstractCriteriaVisitor<IQueryOver<NodeVersion>>

        public override QueryOver<NodeVersion> VisitNoCriteriaPresent()
        {
            return QueryOver.Of<NodeVersion>().Select(x => x.Id);
        }

        public override QueryOver<NodeVersion> VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            var schemaPropertyname = node.SelectorExpression.Name;
            var fieldValue = node.ValueExpression.Value.ToString();

            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    return QueryOver.Of<NodeVersion>()
                        .JoinQueryOver<AttributeSchemaDefinition>(x => x.AttributeSchemaDefinition)
                        .Where(x => x.Alias == fieldValue)
                        .Select(x => x.Id);
                case ValuePredicateType.NotEqual:
                    return QueryOver.Of<NodeVersion>()
                        .JoinQueryOver<AttributeSchemaDefinition>(x => x.AttributeSchemaDefinition)
                        .Where(x => x.Alias != fieldValue)
                        .Select(x => x.Id);
                default:
                    throw new InvalidOperationException(
                        "Cannot query an item by schema alias by any other operator than == or !=");
            }
        }

        public override QueryOver<NodeVersion> VisitFieldPredicate(FieldPredicateExpression node)
        {
            var fieldName = node.SelectorExpression.FieldName;
            var valueKey = node.SelectorExpression.ValueKey;
            var fieldValue = node.ValueExpression.Value;
            var fieldValueType = fieldValue != null ? fieldValue.GetType() : typeof(string);

            switch (fieldName.ToLowerInvariant())
            {
                case "id":
                    Guid idValue = GetIdValue(node);

                    switch (node.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.Equal:
                            return QueryOver.Of<NodeVersion>().Where(x => x.Node.Id == idValue).Select(x => x.Id);
                        case ValuePredicateType.NotEqual:
                            return QueryOver.Of<NodeVersion>().Where(x => x.Node.Id != idValue).Select(x => x.Id); ;
                        default:
                            throw new InvalidOperationException(
                                "Cannot query an item by id by any other operator than == or !=");
                    }
            }

            // First look up the types of the main field
            AttributeDefinition defAlias = null;
            AttributeType typeAlias = null;
            var attributeType = _activeSession.QueryOver<AttributeDefinition>(() => defAlias)
                .JoinAlias(() => defAlias.AttributeType, () => typeAlias)
                .Where(() => defAlias.Alias == fieldName)
                .Select(x => typeAlias.PersistenceTypeProvider)
                .List<string>();

            var typesAlreadyEstablished = new List<string>();
            var typesToQuery = new List<DataSerializationTypes>();

            foreach (var type in attributeType)
            {
                var typeName = type;
                if (typesAlreadyEstablished.Contains(typeName)) continue;
                try
                {
                    typesAlreadyEstablished.Add(typeName);
                    var persisterType = Type.GetType(typeName, false);
                    if (persisterType != null)
                    {
                        var persisterInstance = Activator.CreateInstance(persisterType) as IAttributeSerializationDefinition;
                        if (persisterInstance != null)
                        {
                            typesToQuery.Add(persisterInstance.DataSerializationType);
                        }
                    }
                }
                catch (Exception)
                {

                    throw;
                }
            }

            // U5-789
            // Workaround pending a better check of what data is actually saved
            // An issue arose because previous data had been saved in long-string,
            // but the datatype changed to be just string, therefore only string was
            // being queried despite all the data residing still in long-string
            if (typesToQuery.Contains(DataSerializationTypes.String) && !typesToQuery.Contains(DataSerializationTypes.LongString))
            {
                typesToQuery.Add(DataSerializationTypes.LongString);
            }

            NodeVersion aliasNodeVersion = null;
            Attribute aliasAttribute = null;
            AttributeDefinition aliasAttributeDefinition = null;
            AttributeStringValue aliasStringValue = null;
            AttributeLongStringValue aliasLongStringValue = null;
            AttributeIntegerValue aliasIntegerValue = null;
            AttributeDecimalValue aliasDecimalValue = null;
            NodeRelation aliasNodeRelation = null;
            AttributeDateValue aliasDateValue = null;

            QueryOver<NodeVersion, AttributeDefinition> queryExtender = QueryOver.Of<NodeVersion>(() => aliasNodeVersion)
                .JoinQueryOver<Attribute>(() => aliasNodeVersion.Attributes, () => aliasAttribute)
                .JoinQueryOver<AttributeDefinition>(() => aliasAttribute.AttributeDefinition, () => aliasAttributeDefinition);

            int numberOfMatchesEvaluated = 0;
            AbstractCriterion restrictionBuilder = null;
            foreach (var dataSerializationTypese in typesToQuery.Distinct())
            {
                AbstractCriterion restriction = null;
                Expression<Func<object>> propertyExpression = null;
                Expression<Func<object>> subkeyExpression = null;
                List<ValuePredicateType> validClauseTypes = null;
                var useLikeMatchForStrings = false;
                switch (dataSerializationTypese)
                {
                    case DataSerializationTypes.SmallInt:
                    case DataSerializationTypes.LargeInt:
                    case DataSerializationTypes.Boolean:
                        queryExtender = queryExtender.Left.JoinAlias(() => aliasAttribute.AttributeIntegerValues, () => aliasIntegerValue);
                        propertyExpression = () => aliasIntegerValue.Value;
                        subkeyExpression = () => aliasIntegerValue.ValueKey;
                        validClauseTypes = new List<ValuePredicateType>()
                            {
                                ValuePredicateType.Equal,
                                ValuePredicateType.GreaterThan,
                                ValuePredicateType.GreaterThanOrEqual,
                                ValuePredicateType.LessThan,
                                ValuePredicateType.LessThanOrEqual,
                                ValuePredicateType.NotEqual
                            };
                        break;
                    case DataSerializationTypes.Decimal:
                        queryExtender = queryExtender.Left.JoinAlias(() => aliasAttribute.AttributeDecimalValues, () => aliasDecimalValue);
                        propertyExpression = () => aliasDecimalValue.Value;
                        subkeyExpression = () => aliasDecimalValue.ValueKey;
                        validClauseTypes = new List<ValuePredicateType>()
                            {
                                ValuePredicateType.Equal,
                                ValuePredicateType.GreaterThan,
                                ValuePredicateType.GreaterThanOrEqual,
                                ValuePredicateType.LessThan,
                                ValuePredicateType.LessThanOrEqual,
                                ValuePredicateType.NotEqual
                            };
                        break;
                    case DataSerializationTypes.String:
                        queryExtender = queryExtender.Left.JoinAlias(() => aliasAttribute.AttributeStringValues, () => aliasStringValue);
                        propertyExpression = () => aliasStringValue.Value;
                        subkeyExpression = () => aliasStringValue.ValueKey;
                        validClauseTypes = new List<ValuePredicateType>()
                            {
                                ValuePredicateType.Equal,
                                ValuePredicateType.NotEqual,
                                ValuePredicateType.Contains,
                                ValuePredicateType.StartsWith,
                                ValuePredicateType.EndsWith,
                                ValuePredicateType.MatchesWildcard
                            };
                        break;
                    case DataSerializationTypes.LongString:
                        queryExtender = queryExtender.Left.JoinAlias(() => aliasAttribute.AttributeLongStringValues, () => aliasLongStringValue);
                        propertyExpression = () => aliasLongStringValue.Value;
                        subkeyExpression = () => aliasLongStringValue.ValueKey;
                        useLikeMatchForStrings = true;
                        validClauseTypes = new List<ValuePredicateType>()
                            {
                                ValuePredicateType.Equal,
                                ValuePredicateType.NotEqual,
                                ValuePredicateType.Contains,
                                ValuePredicateType.StartsWith,
                                ValuePredicateType.EndsWith,
                                ValuePredicateType.MatchesWildcard
                            };
                        break;
                    case DataSerializationTypes.Date:
                        queryExtender = queryExtender.Left.JoinAlias(() => aliasAttribute.AttributeDateValues, () => aliasDateValue);
                        propertyExpression = () => aliasDateValue.Value;
                        subkeyExpression = () => aliasDateValue.ValueKey;
                        validClauseTypes = new List<ValuePredicateType>()
                            {
                                ValuePredicateType.Equal,
                                ValuePredicateType.GreaterThan,
                                ValuePredicateType.GreaterThanOrEqual,
                                ValuePredicateType.LessThan,
                                ValuePredicateType.LessThanOrEqual,
                                ValuePredicateType.NotEqual,
                                ValuePredicateType.Empty
                            };
                        break;
                }

                if (!validClauseTypes.Contains(node.ValueExpression.ClauseType))
                {
                    throw new InvalidOperationException("A field of type {0} cannot be queried with operator {1}".InvariantFormat(dataSerializationTypese.ToString(), node.ValueExpression.ClauseType.ToString()));
                }

                switch (node.ValueExpression.ClauseType)
                {
                    case ValuePredicateType.Equal:
                        restriction = GetRestrictionEq(fieldValue, useLikeMatchForStrings, propertyExpression, subkeyExpression, valueKey);
                        break;
                    case ValuePredicateType.NotEqual:
                        restriction = !GetRestrictionEq(fieldValue, useLikeMatchForStrings, propertyExpression);
                        break;
                    case ValuePredicateType.LessThan:
                        restriction = GetRestrictionLt(fieldValue, propertyExpression);
                        break;
                    case ValuePredicateType.LessThanOrEqual:
                        restriction = GetRestrictionLtEq(fieldValue, propertyExpression);
                        break;
                    case ValuePredicateType.GreaterThan:
                        restriction = GetRestrictionGt(fieldValue, propertyExpression);
                        break;
                    case ValuePredicateType.GreaterThanOrEqual:
                        restriction = GetRestrictionGtEq(fieldValue, propertyExpression);
                        break;

                    case ValuePredicateType.Contains:
                        restriction = GetRestrictionContains(fieldValue, propertyExpression);
                        break;
                    case ValuePredicateType.StartsWith:
                        restriction = GetRestrictionStarts(fieldValue, propertyExpression);
                        break;
                    case ValuePredicateType.EndsWith:
                        restriction = GetRestrictionEnds(fieldValue, propertyExpression);
                        break;
                }

                if (restriction != null)
                {
                    if (numberOfMatchesEvaluated == 0)
                    {
                        restrictionBuilder = restriction;
                        numberOfMatchesEvaluated++;
                    }
                    else
                    {
                        restrictionBuilder = Restrictions.Or(restriction, restrictionBuilder);
                    }
                }
            }

            if (restrictionBuilder != null)
                queryExtender = queryExtender.Where(restrictionBuilder);

            queryExtender = queryExtender
                .And(x => aliasAttributeDefinition.Alias == fieldName);

            return queryExtender.Select(x => x.Id);
        }

        private static SimpleExpression GetRestrictionContains(object fieldValue, Expression<Func<object>> propertyExpression)
        {
            return Restrictions.Like(Projections.Property(propertyExpression), fieldValue as string, MatchMode.Anywhere);
        }

        private static SimpleExpression GetRestrictionStarts(object fieldValue, Expression<Func<object>> propertyExpression)
        {
            return Restrictions.Like(Projections.Property(propertyExpression), fieldValue as string, MatchMode.Start);
        }

        private static SimpleExpression GetRestrictionEnds(object fieldValue, Expression<Func<object>> propertyExpression)
        {
            return Restrictions.Like(Projections.Property(propertyExpression), fieldValue as string, MatchMode.End);
        }

        private static AbstractCriterion GetRestrictionEq(object fieldValue, bool useLikeMatchForStrings, Expression<Func<object>> propertyExpression, Expression<Func<object>> fieldSubKeySelector = null, string fieldSubKey = null)
        {
            var basic = useLikeMatchForStrings
                ? Restrictions.Like(Projections.Property(propertyExpression), fieldValue as string, MatchMode.Exact)
                : Restrictions.Eq(Projections.Property(propertyExpression), fieldValue);

            if (fieldSubKey != null && fieldSubKeySelector != null)
            {
                return Restrictions.And(Restrictions.Eq(Projections.Property(fieldSubKeySelector), fieldSubKey), basic);
            }
            return basic;
        }

        private static SimpleExpression GetRestrictionLtEq(object fieldValue, Expression<Func<object>> propertyExpression)
        {
            return Restrictions.Le(Projections.Property(propertyExpression), fieldValue);
        }

        private static SimpleExpression GetRestrictionLt(object fieldValue, Expression<Func<object>> propertyExpression)
        {
            return Restrictions.Lt(Projections.Property(propertyExpression), fieldValue);
        }

        private static SimpleExpression GetRestrictionGt(object fieldValue, Expression<Func<object>> propertyExpression)
        {
            return Restrictions.Gt(Projections.Property(propertyExpression), fieldValue);
        }

        private static SimpleExpression GetRestrictionGtEq(object fieldValue, Expression<Func<object>> propertyExpression)
        {
            return Restrictions.Ge(Projections.Property(propertyExpression), fieldValue);
        }

        private static Guid GetIdValue(FieldPredicateExpression node)
        {
            var idValue = Guid.Empty;
            if (node.ValueExpression.Type.IsAssignableFrom(typeof(HiveId)))
            {
                idValue = (Guid)((HiveId)node.ValueExpression.Value).Value;
            }
            else
            {
                Guid.TryParse(node.ValueExpression.Value.ToString(), out idValue);
            }
            return idValue;
        }

        public override QueryOver<NodeVersion> VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return QueryOver.Of<NodeVersion>()
                            .Where(Restrictions.Conjunction()
                                .Add(Subqueries.PropertyIn(Projections.Property<NodeVersion>(x => x.Id).PropertyName, left.DetachedCriteria))
                                .Add(Subqueries.PropertyIn(Projections.Property<NodeVersion>(x => x.Id).PropertyName, right.DetachedCriteria)))
                            .Select(x => x.Id);
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    {
                        return QueryOver.Of<NodeVersion>()
                            .Where(Restrictions.Disjunction()
                                .Add(Subqueries.PropertyIn(Projections.Property<NodeVersion>(x => x.Id).PropertyName, left.DetachedCriteria))
                                .Add(Subqueries.PropertyIn(Projections.Property<NodeVersion>(x => x.Id).PropertyName, right.DetachedCriteria)))
                            .Select(x => x.Id);
                    }
            }

            throw new InvalidOperationException("This provider only supports binary expressions with And, AndAlso, Or, OrElse expression types. ExpressionType was {0}".InvariantFormat(node.NodeType.ToString()));
        }

        #endregion
    }
}