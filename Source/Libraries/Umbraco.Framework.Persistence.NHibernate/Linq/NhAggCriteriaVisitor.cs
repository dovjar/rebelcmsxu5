using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Persistence.NHibernate.Linq
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;
    using Umbraco.Framework.Data;
    using Umbraco.Framework.Diagnostics;
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;
    using Umbraco.Framework.Linq.CriteriaTranslation;
    using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
    using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings;
    using Umbraco.Framework.Persistence.RdbmsModel;
    using global::NHibernate.Criterion;
    using global::NHibernate.SqlCommand;

    public class NhAggCriteriaVisitor : AbstractCriteriaVisitor<FilterResult, FilterResult, FieldFilterResult, SchemaFilterResult>
    {
        private static readonly ConcurrentDictionary<string, DataSerializationTypes> TypesAlreadyDiscovered = new ConcurrentDictionary<string, DataSerializationTypes>();

        private readonly NhSessionHelper _activeSession;

        public NhAggCriteriaVisitor(NhSessionHelper activeSession)
        {
            _activeSession = activeSession;
        }

        public FilterResult GenerateFilterQuery(System.Linq.Expressions.Expression fromExpression)
        {
            var filterResult = Visit(fromExpression);

            // Before sending it back, check if we need to add the joins if it wasn't a binary
            var schemaFilter = filterResult as SchemaFilterResult;
            var fieldFilter = filterResult as FieldFilterResult;
            if (filterResult.Subquery == null)
            {
                filterResult.Subquery = QueryOver.Of(() => agg);
            }

            // Add the joins (provided there's any point)
            if (schemaFilter != null || fieldFilter != null)
                foreach (var queryJoin in filterResult.Joins)
                {
                    filterResult.Subquery = filterResult.Subquery.JoinAlias(queryJoin.Path, queryJoin.Alias, queryJoin.JoinType);
                }

            if (schemaFilter != null && schemaFilter.NhCriterion != null)
            {
                filterResult.Subquery = filterResult.Subquery.Where(schemaFilter.NhCriterion);
            }
            else if (fieldFilter != null && fieldFilter.NhCriterion != null)
            {
                filterResult.Subquery = filterResult.Subquery.Where(fieldFilter.NhCriterion);
            }

            return filterResult;
        }

        public override FilterResult VisitNoCriteriaPresent()
        {
            return new FilterResult();
        }

        NodeVersion version = null;
        AttributeSchemaDefinition schema = null;
        AggregateNodeStatus agg = null;

        public override SchemaFilterResult VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            var fieldValue = node.ValueExpression.Value.ToString();

            var toReturn = new SchemaFilterResult();
            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    toReturn.NhCriterion = Restrictions.Eq(Projections.Property(() => schema.Alias), fieldValue);
                    toReturn.Joins.Add(new Join(() => agg.NodeVersion, () => version, JoinType.InnerJoin));
                    toReturn.Joins.Add(new Join(() => version.AttributeSchemaDefinition, () => schema, JoinType.InnerJoin));
                    break;
                case ValuePredicateType.NotEqual:
                    toReturn.NhCriterion = !Restrictions.Eq(Projections.Property(() => schema.Alias), fieldValue);
                    toReturn.Joins.Add(new Join(() => agg.NodeVersion, () => version, JoinType.InnerJoin));
                    toReturn.Joins.Add(new Join(() => version.AttributeSchemaDefinition, () => schema, JoinType.InnerJoin));
                    break;
                default:
                    throw new InvalidOperationException("Cannot query an item by schema alias by any other operator than == or !=");
            }

            return toReturn;
        }

        public override FieldFilterResult VisitFieldPredicate(FieldPredicateExpression node)
        {
            var fieldName = node.SelectorExpression.FieldName;
            var valueKey = node.SelectorExpression.ValueKey;
            var fieldValue = node.ValueExpression.Value;
            var fieldValueType = fieldValue != null ? fieldValue.GetType() : typeof(string);
            var toReturn = new FieldFilterResult();

            RdbmsModel.Attribute att = null;
            Node outerNode = null;
            AttributeStringValue stringVal = null;
            AttributeLongStringValue longStrVal = null;
            AttributeIntegerValue integerVal = null;
            AttributeDecimalValue decimalVal = null;
            AttributeDateValue dateVal = null;
            AttributeDefinition def = null;

            // First check for special cases, typically stuff that can be queried
            // that isn't necessarily a value in any of the Attribute*Value tables
            switch (fieldName.ToLowerInvariant())
            {
                case "utccreated":
                    DateTimeOffset createdDtoValue = ParseDateTimeOffset(fieldValue);
                    if (createdDtoValue == default(DateTime)) break;

                    toReturn.NhCriterion = CreateRestriction(node.ValueExpression.ClauseType,
                        () => outerNode.DateCreated, createdDtoValue);

                    if (toReturn.NhCriterion != null)
                    {
                        toReturn.Joins.Add(new Join(() => agg.Node, () => outerNode, JoinType.InnerJoin));
                    }

                    break;
                case "utcmodified":
                    DateTimeOffset modifiedDtoValue = ParseDateTimeOffset(fieldValue);
                    if (modifiedDtoValue == default(DateTime)) break;

                    toReturn.NhCriterion = CreateRestriction(node.ValueExpression.ClauseType,
                        () => agg.StatusDate, modifiedDtoValue);

                    break;
                case "id":
                    Guid idValue = GetIdValue(node);

                    switch (node.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.Equal:
                            toReturn.NhCriterion = Restrictions.Eq(Projections.Property(() => agg.Node.Id), idValue);
                            break;
                        case ValuePredicateType.NotEqual:
                            toReturn.NhCriterion = !Restrictions.Eq(Projections.Property(() => agg.Node.Id), idValue);
                            break;
                        default:
                            throw new InvalidOperationException("Cannot query an item by id by any other operator than == or !=");
                    }
                    break;
                case "system-internal-selected-template":
                    //TODO Pending property editors getting involved in query modification prior to being passed to hive provider,
                    //manually check for queries against a template here
                    if (valueKey == "TemplateId" && fieldValue != null)
                    {
                        var tryParseResult = HiveId.TryParse(fieldValue.ToString());
                        if (!tryParseResult.Success || tryParseResult.Result.ProviderGroupRoot == null || tryParseResult.Result.ProviderId == null ||
                            (tryParseResult.Result.ProviderGroupRoot.AbsoluteUri != "storage://" && tryParseResult.Result.ProviderId != "templates"))
                        {
                            var normalisedFieldValue = "/" + fieldValue.ToString().TrimStart("/").TrimEnd(".") + ".";
                            // Need to convert the value into the serialized form that a HiveId would use
                            var newValue = new HiveId("storage", "templates", new HiveIdValue(normalisedFieldValue)).ToString(HiveIdFormatStyle.UriSafe);
                            fieldValue = newValue;
                        }
                        else
                        {
                            fieldValue = tryParseResult.Result.ToString(HiveIdFormatStyle.UriSafe);
                        }
                    }
                    break;
            }

            if (toReturn.NhCriterion != null)
            {
                // The special-case handling above has already set the criterion,
                // so we don't have to evaluate field values in this pass and can return
                return toReturn;
            }

            // Establish which Attribute*Value tables to query
            // First look up the types of the main field
            AttributeDefinition defAlias = null;
            AttributeType typeAlias = null;
            var attributeType = _activeSession.NhSession.QueryOver<AttributeDefinition>(() => defAlias)
                .JoinAlias(() => defAlias.AttributeType, () => typeAlias)
                .Where(() => defAlias.Alias == fieldName)
                .Select(Projections.Distinct(Projections.Property(() => typeAlias.PersistenceTypeProvider)))
                .Cacheable()
                .List<string>();

            foreach (var type in attributeType)
            {
                var typeName = type;

                // Ensure we don't do unneccessary calls to Activator.CreateInstance,
                // (_typesAlreadyEstablished lives for the lifetime of the visitor)
                // but still make sure we populate the toReturn.ValueTypesToQuery for this
                // visit to a field predicate
                if (TypesAlreadyDiscovered.ContainsKey(typeName))
                {
                    var dst = TypesAlreadyDiscovered[typeName];
                    if (toReturn.ValueTypesToQuery.Contains(dst)) continue;
                    toReturn.ValueTypesToQuery.Add(dst);
                }
                try
                {
                    var persisterType = Type.GetType(typeName, false);
                    if (persisterType != null)
                    {
                        var persisterInstance = Activator.CreateInstance(persisterType) as IAttributeSerializationDefinition;
                        if (persisterInstance != null && !toReturn.ValueTypesToQuery.Contains(persisterInstance.DataSerializationType))
                        {
                            toReturn.ValueTypesToQuery.Add(persisterInstance.DataSerializationType);
                            TypesAlreadyDiscovered.TryAdd(typeName, persisterInstance.DataSerializationType);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<NhCriteriaVisitor>("Error while trying to decide which value-tables to join & query", ex);
                    throw;
                }
            }

            // U5-789
            // Workaround pending a better check of what data is actually saved
            // An issue arose because previous data had been saved in long-string,
            // but the datatype changed to be just string, therefore only string was
            // being queried despite all the data residing still in long-string
            if (toReturn.ValueTypesToQuery.Contains(DataSerializationTypes.String) && !toReturn.ValueTypesToQuery.Contains(DataSerializationTypes.LongString))
            {
                toReturn.ValueTypesToQuery.Add(DataSerializationTypes.LongString);
            }

            // Now go through the types that we've found, and set up the expressions for the criteria
            int numberOfMatchesEvaluated = 0;
            AbstractCriterion restrictionBuilder = null;
            foreach (var dataSerializationTypese in toReturn.ValueTypesToQuery.Distinct())
            {
                AbstractCriterion restriction = null;
                global::System.Linq.Expressions.Expression<Func<object>> propertyExpression = null;
                global::System.Linq.Expressions.Expression<Func<object>> subkeyExpression = null;
                List<ValuePredicateType> validClauseTypes = null;
                var useLikeMatchForStrings = false;
                switch (dataSerializationTypese)
                {
                    case DataSerializationTypes.SmallInt:
                    case DataSerializationTypes.LargeInt:
                    case DataSerializationTypes.Boolean:
                        propertyExpression = () => integerVal.Value;
                        subkeyExpression = () => integerVal.ValueKey;
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
                        propertyExpression = () => decimalVal.Value;
                        subkeyExpression = () => decimalVal.ValueKey;
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
                        propertyExpression = () => stringVal.Value;
                        subkeyExpression = () => stringVal.ValueKey;
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
                        propertyExpression = () => longStrVal.Value;
                        subkeyExpression = () => longStrVal.ValueKey;
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
                        propertyExpression = () => dateVal.Value;
                        subkeyExpression = () => dateVal.ValueKey;
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

                // Based on the clause type, generate an NH criterion
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

                // We might be dealing with multiple restrictions even for one field (e.g. if it's been stored once in String table, and another time in LongString)
                if (restriction != null)
                {
                    if (numberOfMatchesEvaluated == 0)
                    {
                        restrictionBuilder = restriction;
                        numberOfMatchesEvaluated++;
                    }
                    else
                    {
                        // If we're doing a second or later evaluation, turn the restriction into an Or to combine them all
                        restrictionBuilder = Restrictions.Or(restriction, restrictionBuilder);
                    }
                }
            }

            // Add the field name restriction
            var fieldNameRestriction = Restrictions.Eq(Projections.Property(() => def.Alias), fieldName);
            if (restrictionBuilder != null)
            {
                restrictionBuilder = Restrictions.And(restrictionBuilder, fieldNameRestriction);
            }
            else
            {
                restrictionBuilder = fieldNameRestriction;
            }

            // Build the query which will use the restrictions we've just generated
            var query = QueryOver.Of(() => agg);

            // Set up the basic joins (not that we're adding these to a Joins collection on our own type, not just doing
            // them on an NH query, so that we can optimise or merge the joins once the entire expression tree is evaluated
            // for example inside VisitBinary)
            toReturn.Joins.Add(new Join(() => agg.NodeVersion, () => version, JoinType.InnerJoin));
            toReturn.Joins.Add(new Join(() => version.Attributes, () => att, JoinType.InnerJoin));
            toReturn.Joins.Add(new Join(() => att.AttributeDefinition, () => def, JoinType.InnerJoin));

            // Set up the joins for the value tables - only add joins for the tables that we know we want to actually query based on
            // what VisitField might have encountered and added to toReturn.ValueTypesToQuery
            foreach (var dataSerializationTypese in toReturn.ValueTypesToQuery.Distinct())
            {
                Expression<Func<object>> path = null;
                Expression<Func<object>> alias = null;
                switch (dataSerializationTypese)
                {
                    case DataSerializationTypes.SmallInt:
                    case DataSerializationTypes.LargeInt:
                    case DataSerializationTypes.Boolean:
                        path = () => att.AttributeIntegerValues;
                        alias = () => integerVal;
                        break;
                    case DataSerializationTypes.Decimal:
                        path = () => att.AttributeDecimalValues;
                        alias = () => decimalVal;
                        break;
                    case DataSerializationTypes.String:
                        path = () => att.AttributeStringValues;
                        alias = () => stringVal;
                        break;
                    case DataSerializationTypes.LongString:
                        path = () => att.AttributeLongStringValues;
                        alias = () => longStrVal;
                        break;
                    case DataSerializationTypes.Date:
                        path = () => att.AttributeDateValues;
                        alias = () => dateVal;
                        break;
                }
                toReturn.Joins.Add(new Join(path, alias));
            }

            toReturn.Subquery = query
                .Where(restrictionBuilder)
                .Select(x => x.NodeVersion.Id);

            toReturn.NhCriterion = restrictionBuilder;
            return toReturn;
        }

        private static ICriterion GetAutoSubqueryCriterion(FilterResult result)
        {
            if (result == null) return null;
            var bin = result as BinaryFilterResult;
            if (bin != null)
            {
                if (bin.IsAndOperation && bin.Subquery != null)
                {
                    return Subqueries.WhereProperty<AggregateNodeStatus>(x => x.NodeVersion.Id).In(bin.Subquery.Select(x => x.NodeVersion.Id));
                }
            }
            return GetCriterion(result);
        }

        private static ICriterion GetCriterion(FilterResult result)
        {
            if (result == null) return null;
            var bin = result as BinaryFilterResult;
            if (bin != null)
                return bin.NhCriterion;
            var field = result as FieldFilterResult;
            if (field != null)
                return field.NhCriterion;
            var schema = result as SchemaFilterResult;
            if (schema != null)
                return schema.NhCriterion;
            return result.SimpleNhCriterion;
        }

        public override FilterResult VisitBinary(BinaryExpression node)
        {
            var left = Visit(node.Left);
            var right = Visit(node.Right);

            if (left == null && right != null)
            {
                return right;
            }
            if (left != null && right == null)
            {
                return left;
            }

            AggregateNodeStatus agg = null;

            // Combine a distinct set of all the joins in the left and right sides, we might use this later
            var allJoins = left.Joins.Concat(right.Joins)
                .DistinctBy(x => x.Path.ToString() + x.Alias.ToString() + x.JoinType.ToString()).ToArray();

            var newQuery = QueryOver.Of<AggregateNodeStatus>(() => agg);
            foreach (var allJoin in allJoins)
            {
                newQuery = newQuery.JoinAlias(allJoin.Path, allJoin.Alias, allJoin.JoinType);
            }

            // If we're returning a binary result, we need to let the upper branch in the tree know if this is
            // and And or an Or binary. If it's an And binary, then we're going to have subqueries, because in the EVM
            // datamodel you can't do "and" expressions on the same join because different values occupy different rows.
            // If we have subqueries, we need to be aware of that when combining the "outer" binary bearing in mind there
            // may be several compound binaries in the expression tree and thus this method might be recursively walking
            // the tree (depth-first of course)

            var isConjunction = false;
            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    isConjunction = true;
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    break;
                default:
                    throw new InvalidOperationException("This provider only supports binary expressions with And, AndAlso, Or, OrElse expression types. ExpressionType was {0}".InvariantFormat(node.NodeType.ToString()));
            }

            var toReturn = new BinaryFilterResult(isConjunction);
            var leftCriterion = GetCriterion(left);
            var rightCriterion = GetCriterion(right);
            if (toReturn.IsAndOperation)
            {

                // To do an "and", we'll construct the left and right queries, and the output
                // query will be "where left also has a subquery matching right"
                ConjoinAsAndOperation(rightCriterion, toReturn, agg, left, leftCriterion, right, allJoins, newQuery);
            }
            else
            {
                // To do an "or", we can combine the queries into one, provided either side isn't a Binary that is an "and"
                // in which case we need to add it as a subquery as its simple criterion won't have enough info
                bool leftIsSubquery = false;
                bool rightIsSubquery = false;

                var leftAsBinary = left as BinaryFilterResult;
                var rightAsBinary = right as BinaryFilterResult;
                if (leftAsBinary != null && leftAsBinary.IsAndOperation)
                    leftIsSubquery = true;
                if (rightAsBinary != null && rightAsBinary.IsAndOperation)
                    rightIsSubquery = true;

                if (leftIsSubquery && rightIsSubquery)
                {
                    ConjoinAsAndOperation(rightCriterion, toReturn, agg, left, leftCriterion, right, allJoins, newQuery);
                }
                else if (rightIsSubquery)
                {
                    toReturn.Subquery = newQuery;
                    var rightSubquery = Subqueries.WhereProperty<AggregateNodeStatus>(x => x.NodeVersion.Id).In(right.Subquery.Select(x => x.NodeVersion.Id));
                    if (leftCriterion != null)
                    {
                        var disjunction = Restrictions.Disjunction().Add(leftCriterion).Add(rightSubquery);
                        toReturn.Subquery = toReturn.Subquery.Where(disjunction);
                        toReturn.Joins = left.Joins;
                    }
                    else
                    {
                        toReturn.Subquery = toReturn.Subquery.Where(rightSubquery);
                    }
                }
                else if (leftIsSubquery)
                {
                    toReturn.Subquery = newQuery;

                    var leftSubquery = Subqueries.WhereProperty<AggregateNodeStatus>(x => x.NodeVersion.Id).In(left.Subquery.Select(x => x.NodeVersion.Id));
                    if (rightCriterion != null)
                    {
                        var disjunction = Restrictions.Disjunction().Add(rightCriterion).Add(leftSubquery);
                        toReturn.Subquery = toReturn.Subquery.Where(disjunction);
                        toReturn.Joins = right.Joins;
                    }
                    else
                    {
                        toReturn.Subquery = toReturn.Subquery.Where(leftSubquery);
                    }
                }
                else
                {
                    var orBoth = Restrictions.Disjunction()
                        .Add(leftCriterion)
                        .Add(rightCriterion);
                    newQuery = newQuery.Where(orBoth);
                    toReturn.NhCriterion = orBoth;

                    toReturn.Joins = new List<Join>(allJoins);
                    toReturn.Subquery = newQuery;
                }
            }
            return toReturn;
        }

        private static void ConjoinAsAndOperation(ICriterion rightCriterion, BinaryFilterResult toReturn, AggregateNodeStatus agg, FilterResult left, ICriterion leftCriterion, FilterResult right, Join[] allJoins, QueryOver<AggregateNodeStatus, AggregateNodeStatus> newQuery)
        {
            // If either side is a binary, it'll already have a subquery generated, so can use that,
            // otherwise we need to make one
            var leftAsBinary = left as BinaryFilterResult;
            var rightAsBinary = right as BinaryFilterResult;
            var leftAsSchema = left as SchemaFilterResult;
            var rightAsSchema = right as SchemaFilterResult;
            var addedLeftSchema = false;
            var addedRightSchema = false;

            if (leftAsSchema != null && rightAsSchema != null)
            {
                toReturn.Joins = allJoins;
                var addSchemaJoin = Restrictions.Conjunction()
                    .Add(leftAsSchema.NhCriterion)
                    .Add(rightAsSchema.NhCriterion);
                toReturn.Subquery = newQuery.Where(addSchemaJoin);
            }
            else if (leftAsSchema != null)
            {
                toReturn.Joins = allJoins;

                var theQuery = QueryOver.Of(() => agg);
                if (rightCriterion != null)
                    theQuery = theQuery.Where(rightCriterion);
                foreach (var rightJoin in allJoins)
                {
                    theQuery = theQuery.JoinAlias(rightJoin.Path, rightJoin.Alias, rightJoin.JoinType);
                }

                // One side is a schema restriction, and we can generate faster sql by avoiding
                // a subquery and just adding the schema restriction to the field restriction(s)
                var addSchemaJoin = Restrictions.Conjunction()
                .Add(leftAsSchema.NhCriterion)
                .Add(GetAutoSubqueryCriterion(right));

                theQuery = theQuery.Where(addSchemaJoin);
                toReturn.Subquery = theQuery;
                return;
                addedLeftSchema = true;
            }
            else if (rightAsSchema != null)
            {
                toReturn.Joins = allJoins;

                var theQuery = QueryOver.Of(() => agg);
                if (leftCriterion != null)
                    theQuery = theQuery.Where(rightCriterion);
                foreach (var rightJoin in allJoins)
                {
                    theQuery = theQuery.JoinAlias(rightJoin.Path, rightJoin.Alias, rightJoin.JoinType);
                }

                // One side is a schema restriction, and we can generate faster sql by avoiding
                // a subquery and just adding the schema restriction to the field restriction(s)
                var addSchemaJoin = Restrictions.Conjunction()
                .Add(GetAutoSubqueryCriterion(left))
                .Add(rightAsSchema.NhCriterion);

                theQuery = theQuery.Where(addSchemaJoin);
                toReturn.Subquery = theQuery;
                return;
                addedRightSchema = true;
            }

            if (leftAsBinary == null && !addedLeftSchema)
            {
                left.Subquery = QueryOver.Of(() => agg);

                if (leftCriterion != null)
                    left.Subquery = left.Subquery.Where(leftCriterion);
                foreach (var leftJoin in left.Joins)
                {
                    left.Subquery = left.Subquery.JoinAlias(leftJoin.Path, leftJoin.Alias, leftJoin.JoinType);
                }
            }

            if (rightAsBinary == null && !addedRightSchema)
            {
                right.Subquery = QueryOver.Of(() => agg);
                if (rightCriterion != null)
                    right.Subquery = right.Subquery.Where(rightCriterion);
                foreach (var rightJoin in right.Joins)
                {
                    right.Subquery = right.Subquery.JoinAlias(rightJoin.Path, rightJoin.Alias, rightJoin.JoinType);
                }
            }

            toReturn.Joins = new List<Join>(left.Joins);
            toReturn.Subquery = left
                .Subquery
                .WithSubquery.WhereProperty(x => x.NodeVersion.Id).In(right.Subquery.Select(x => x.NodeVersion.Id));
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

        private static DateTimeOffset ParseDateTimeOffset(object value)
        {
            var dtoValue = default(DateTimeOffset);
            if (value is DateTimeOffset)
            {
                dtoValue = (DateTimeOffset)value;
            }
            else if (value is DateTime)
            {
                dtoValue = new DateTimeOffset((DateTime)value);
            }
            return dtoValue;
        }

        private static SimpleExpression CreateRestriction(ValuePredicateType clauseType, Expression<Func<object>> projection, object value)
        {
            switch (clauseType)
            {
                case ValuePredicateType.LessThanOrEqual:
                    return Restrictions.Le(Projections.Property(projection), value);
                case ValuePredicateType.LessThan:
                    return Restrictions.Lt(Projections.Property(projection), value);
                case ValuePredicateType.Equal:
                    return Restrictions.Eq(Projections.Property(projection), value);
                case ValuePredicateType.GreaterThan:
                    return Restrictions.Gt(Projections.Property(projection), value);
                case ValuePredicateType.GreaterThanOrEqual:
                    return Restrictions.Ge(Projections.Property(projection), value);
                default:
                    throw new InvalidOperationException("Unsupport clause type");
            }
        }
    }

    public class FilterResult
    {
        public FilterResult()
        {
            Joins = new List<Join>();
        }

        public ICriterion SimpleNhCriterion { get; set; }
        public QueryOver<AggregateNodeStatus, AggregateNodeStatus> Subquery { get; set; }
        public IList<Join> Joins { get; set; }
    }

    public class FieldFilterResult : FilterResult
    {
        public FieldFilterResult()
        {
            ValueTypesToQuery = new List<DataSerializationTypes>();
        }

        public ICriterion NhCriterion { get; set; }
        public IList<DataSerializationTypes> ValueTypesToQuery { get; protected set; }
    }

    [DebuggerDisplay("{Path.ToString()} {Alias.ToString()}")]
    public class Join : AbstractEquatableObject<Join>
    {
        public Join(Expression<Func<object>> path, Expression<Func<object>> @alias, JoinType joinType = JoinType.LeftOuterJoin)
        {
            Path = path;
            Alias = alias;
            JoinType = joinType;
        }

        public Expression<Func<object>> Path { get; set; }

        public Expression<Func<object>> Alias { get; set; }

        public JoinType JoinType { get; set; }

        protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
        {
            yield return this.GetPropertyInfo(x => x.Path);
            yield return this.GetPropertyInfo(x => x.Alias);
            yield return this.GetPropertyInfo(x => x.JoinType);
        }
    }

    public class SchemaFilterResult : FilterResult
    {
        public ICriterion NhCriterion { get; set; }
    }

    public class BinaryFilterResult : FilterResult
    {
        public BinaryFilterResult(bool isConjunction)
        {
            IsAndOperation = isConjunction;
        }

        public ICriterion NhCriterion { get; set; }
        public bool IsAndOperation { get; set; }
    }
}
