namespace Umbraco.Framework.Persistence.NHibernate.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Umbraco.Framework.Data;
    using Umbraco.Framework.Diagnostics;
    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;
    using Umbraco.Framework.Linq.CriteriaTranslation;
    using Umbraco.Framework.Persistence.Abstractions.Attribution.MetaData;
    using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings;
    using Umbraco.Framework.Persistence.RdbmsModel;
    using global::NHibernate.Criterion;
    using Attribute = Umbraco.Framework.Persistence.RdbmsModel.Attribute;

    public class NhAggregateCriteriaVisitor : AbstractCriteriaVisitor<ICriterion>
    {
        private NhSessionHelper _activeSession;

        public NhAggregateCriteriaVisitor(NhSessionHelper activeSession)
        {
            _activeSession = activeSession;
            _typesToQuery = new List<DataSerializationTypes>();
            _typesAlreadyEstablished = new List<string>();
        }

        private NodeVersion version = null;
        private Node outerNode = null;
        private AggregateNodeStatus agg = null;
        private Attribute att = null;
        private AttributeDefinition def = null;
        private AttributeSchemaDefinition schema = null;
        private AttributeStringValue stringVal = null;
        private AttributeLongStringValue longStrVal = null;
        private AttributeIntegerValue integerVal = null;
        private AttributeDecimalValue decimalVal = null;
        private AttributeDateValue dateVal = null;
        private readonly List<DataSerializationTypes> _typesToQuery;
        private readonly List<string> _typesAlreadyEstablished;

        public class FilterResult
        {
            internal FilterResult(QueryOver<AggregateNodeStatus> subquery, string[] requiredSchemaAliases, Guid[] requiredNodeIds)
            {
                Subquery = subquery;
                RequiredSchemaAliases = requiredSchemaAliases;
                RequiredNodeIds = requiredNodeIds;
            }

            public QueryOver<AggregateNodeStatus> Subquery { get; protected set; }

            /// <summary>
            /// As a preliminary optimisation option, if we know some schema aliases are required,
            /// we can provide them back to the caller so that it can add them as an "or" chain
            /// to the outer query to give the db engine more hints (500ms vs 330ms in SqlCe)
            /// </summary>
            public string[] RequiredSchemaAliases { get; protected set; }

            /// <summary>
            /// Same deal as <see cref="RequiredSchemaAliases"/> but for node ids
            /// </summary>
            public Guid[] RequiredNodeIds { get; protected set; }
        }

        private FilterResult GenerateQueryOver(ICriterion fromCriterion, Guid[] nodeIds = null)
        {
            if (fromCriterion == null) return null;

            // Generate a query which joins all the value tables to Attribute
            // and uses the generated criterion, selecting the Id, so that the
            // caller can use that as a subquery
            var query = QueryOver.Of(() => agg)
                .JoinAlias(() => agg.NodeVersion, () => version)
                .JoinAlias(() => version.Attributes, () => att)
                .JoinAlias(() => att.AttributeDefinition, () => def);

            // If we've found required node ids while visiting the expression
            nodeIds = nodeIds ?? new Guid[0];
            nodeIds = _negatingNodeIdsExist ? nodeIds : nodeIds.Concat(_discoveredRequiredNodeIds).Distinct().ToArray();

            //// If we've been given some nodeIds, then add that to the filter, 
            //// as it saves the db engine scanning irrelevant nodes out of scope
            //// (even though what we're constructing here is a subquery - 110ms to 23ms in a SqlCe test)
            //if (nodeIds != null && nodeIds.Length > 0)
            //{
            //    // Since the nodeIds will be added as parameters to the outer query that this query is going to filter,
            //    // then rather than specifically adding the nodeIds, which wastes potential parameter slots,
            //    // add a condition with reference to the outer node alias

            //    // TODO: Because NH only uses alias variables in order to infer the alias name and type,
            //    // using _activeSession.vFilter doesn't work (it looks for a join-alias called "_activeSession.vFilter")
            //    // so just to act as a reminder pending using a string constant alias, here's the variable declared -
            //    // it should have the same name (vFilter)!
            //    NodeVersion vFilter = _activeSession.vFilter;
            //    query = query.Where(() => vFilter.Node == version.Node);
            //    //query = query.Where(x => x.Node.Id.IsIn(nodeIds));
            //}

            // Only join to the schema table if VisitSchemaPredicate says so
            if (_requiresSchemaTableJoin)
            {
                query = query.JoinAlias(() => def.AttributeSchemaDefinition, () => schema);
            }

            // Only join to the node table if VisitFieldPredicate says so
            if (_requiresOuterNodeJoin)
            {
                query = query.JoinAlias(() => agg.Node, () => outerNode);
            }

            // Only add joins for the tables that we know we want to actually query based on
            // what VisitField might have encountered and added to _typesToQuery
            foreach (var dataSerializationTypese in _typesToQuery.Distinct())
            {
                switch (dataSerializationTypese)
                {
                    case DataSerializationTypes.SmallInt:
                    case DataSerializationTypes.LargeInt:
                    case DataSerializationTypes.Boolean:
                        query = query.Left.JoinAlias(() => att.AttributeIntegerValues, () => integerVal);
                        break;
                    case DataSerializationTypes.Decimal:
                        query = query.Left.JoinAlias(() => att.AttributeDecimalValues, () => decimalVal);
                        break;
                    case DataSerializationTypes.String:
                        query = query.Left.JoinAlias(() => att.AttributeStringValues, () => stringVal);
                        break;
                    case DataSerializationTypes.LongString:
                        query = query.Left.JoinAlias(() => att.AttributeLongStringValues, () => longStrVal);
                        break;
                    case DataSerializationTypes.Date:
                        query = query.Left.JoinAlias(() => att.AttributeDateValues, () => dateVal);
                        break;
                }
            }

            var returnQuery = query
                .Where(fromCriterion)
                .Select(x => x.NodeVersion.Id);

            // We only provide the "required" schema aliases back to the caller if there
            // were no expressions saying "where schema alias != blah" as at the moment
            // the code which can modify the outer query isn't advanced enough to 
            // handle the case where some aliases are to be excluded
            var aliases = _someSchemaAliasesAreExcluded ? null : RequiredSchemaAliases.Select(x => x.ValueExpression.Value.ToString()).ToArray();
            return new FilterResult(returnQuery, aliases, nodeIds);
        }

        public FilterResult GenerateFilterQuery(System.Linq.Expressions.Expression fromExpression, Guid[] nodeIds = null)
        {
            var criterion = Visit(fromExpression);

            if (criterion == null) return new FilterResult(null, null, null);

            return GenerateQueryOver(criterion, nodeIds);
        }

        public override ICriterion VisitNoCriteriaPresent()
        {
            return null;
        }

        private bool _requiresSchemaTableJoin = false;
        private List<SchemaPredicateExpression> RequiredSchemaAliases = new List<SchemaPredicateExpression>();
        private bool _someSchemaAliasesAreExcluded = false;
        public override ICriterion VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            var schemaPropertyname = node.SelectorExpression.Name;
            var fieldValue = node.ValueExpression.Value.ToString();

            switch (node.ValueExpression.ClauseType)
            {
                case ValuePredicateType.Equal:
                    _requiresSchemaTableJoin = true;
                    // Since this is an 'equal' clause, add it to the RequiredSchemaAliases list
                    RequiredSchemaAliases.Add(node);
                    return Restrictions.Eq(Projections.Property(() => schema.Alias), fieldValue);
                case ValuePredicateType.NotEqual:
                    _requiresSchemaTableJoin = true;
                    _someSchemaAliasesAreExcluded = true;
                    return !Restrictions.Eq(Projections.Property(() => schema.Alias), fieldValue);
                default:
                    throw new InvalidOperationException(
                        "Cannot query an item by schema alias by any other operator than == or !=");
            }
        }

        private List<Guid> _discoveredRequiredNodeIds = new List<Guid>();
        private bool _negatingNodeIdsExist = false;
        private bool _requiresOuterNodeJoin = false;
        public override ICriterion VisitFieldPredicate(FieldPredicateExpression node)
        {
            var fieldName = node.SelectorExpression.FieldName;
            var valueKey = node.SelectorExpression.ValueKey;
            var fieldValue = node.ValueExpression.Value;
            var fieldValueType = fieldValue != null ? fieldValue.GetType() : typeof(string);

            switch (fieldName.ToLowerInvariant())
            {
                case "utccreated":
                    var dateValue = (DateTimeOffset)fieldValue;
                    if (dateValue == default(DateTimeOffset)) break;
                    _requiresOuterNodeJoin = true;
                    switch (node.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.LessThanOrEqual:
                            return Restrictions.Le(Projections.Property(() => outerNode.DateCreated), dateValue);
                    }
                    break;
                case "id":
                    Guid idValue = GetIdValue(node);

                    switch (node.ValueExpression.ClauseType)
                    {
                        case ValuePredicateType.Equal:
                            _discoveredRequiredNodeIds.Add(idValue);
                            return Restrictions.Eq(Projections.Property(() => version.Node.Id), idValue);
                        case ValuePredicateType.NotEqual:
                            _negatingNodeIdsExist = true;
                            return !Restrictions.Eq(Projections.Property(() => version.Node.Id), idValue);
                        default:
                            throw new InvalidOperationException(
                                "Cannot query an item by id by any other operator than == or !=");
                    }
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

            // First look up the types of the main field
            AttributeDefinition defAlias = null;
            AttributeType typeAlias = null;
            var attributeType = _activeSession.NhSession.QueryOver<AttributeDefinition>(() => defAlias)
                .JoinAlias(() => defAlias.AttributeType, () => typeAlias)
                .Where(() => defAlias.Alias == fieldName)
                .Select(x => typeAlias.PersistenceTypeProvider)
                .List<string>();

            foreach (var type in attributeType)
            {
                var typeName = type;
                if (_typesAlreadyEstablished.Contains(typeName)) continue;
                try
                {
                    _typesAlreadyEstablished.Add(typeName);
                    var persisterType = Type.GetType(typeName, false);
                    if (persisterType != null)
                    {
                        var persisterInstance = Activator.CreateInstance(persisterType) as IAttributeSerializationDefinition;
                        if (persisterInstance != null && !_typesToQuery.Contains(persisterInstance.DataSerializationType))
                        {
                            _typesToQuery.Add(persisterInstance.DataSerializationType);
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
            if (_typesToQuery.Contains(DataSerializationTypes.String) && !_typesToQuery.Contains(DataSerializationTypes.LongString))
            {
                _typesToQuery.Add(DataSerializationTypes.LongString);
            }

            //NodeVersion aliasNodeVersion = null;
            //Attribute aliasAttribute = null;
            //AttributeDefinition aliasAttributeDefinition = null;
            //AttributeStringValue aliasStringValue = null;
            //AttributeLongStringValue aliasLongStringValue = null;
            //AttributeIntegerValue aliasIntegerValue = null;
            //AttributeDecimalValue aliasDecimalValue = null;
            //NodeRelation aliasNodeRelation = null;
            //AttributeDateValue aliasDateValue = null;

            //QueryOver<NodeVersion, AttributeDefinition> queryExtender = QueryOver.Of<NodeVersion>(() => aliasNodeVersion)
            //    .JoinQueryOver<Attribute>(() => aliasNodeVersion.Attributes, () => aliasAttribute)
            //    .JoinQueryOver<AttributeDefinition>(() => aliasAttribute.AttributeDefinition, () => aliasAttributeDefinition);

            int numberOfMatchesEvaluated = 0;
            AbstractCriterion restrictionBuilder = null;
            foreach (var dataSerializationTypese in _typesToQuery.Distinct())
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

            var fieldNameRestriction = Restrictions.Eq(Projections.Property(() => def.Alias), fieldName);
            if (restrictionBuilder != null)
            {
                restrictionBuilder = Restrictions.And(restrictionBuilder, fieldNameRestriction);
            }
            else
            {
                restrictionBuilder = fieldNameRestriction;
            }

            return restrictionBuilder;
        }

        public override ICriterion VisitBinary(BinaryExpression node)
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



            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    // We need to split the query into two in order to evaluate both sides
                    // TODO: (Investigate) Another option is to add more joins to the single main query returned
                    // by GenerateFilterQuery, one set of joins for each side of this binary
                    var allLeft = GenerateQueryOver(left).Subquery;
                    var allRight = GenerateQueryOver(right).Subquery;

                    var combined = new Conjunction()
                        .Add(Subqueries.PropertyIn(Projections.Property<AggregateNodeStatus>(x => x.NodeVersion.Id).PropertyName,
                                                   allLeft.DetachedCriteria))
                        .Add(Subqueries.PropertyIn(Projections.Property<AggregateNodeStatus>(x => x.NodeVersion.Id).PropertyName,
                                                   allRight.DetachedCriteria));

                    return combined;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return Restrictions.Disjunction()
                        .Add(left)
                        .Add(right);
            }

            throw new InvalidOperationException("This provider only supports binary expressions with And, AndAlso, Or, OrElse expression types. ExpressionType was {0}".InvariantFormat(node.NodeType.ToString()));

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
    }
}