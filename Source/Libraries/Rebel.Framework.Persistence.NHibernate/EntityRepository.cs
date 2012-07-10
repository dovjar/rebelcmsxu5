using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Persistence.NHibernate.Dependencies;
using Rebel.Framework.Persistence.NHibernate.Linq;
using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Framework.Persistence.RdbmsModel;
using Rebel.Hive;
using Rebel.Hive.ProviderSupport;
using Attribute = Rebel.Framework.Persistence.RdbmsModel.Attribute;

namespace Rebel.Framework.Persistence.NHibernate
{
    using Rebel.Framework.Linq.QueryModel;

    using Rebel.Framework.Linq.ResultBinding;
    using Rebel.Framework.Persistence.NHibernate.OrmConfig.FluentMappings;

    public class EntityRepository : AbstractEntityRepository
    {
        public EntityRepository(ProviderMetadata providerMetadata,
            AbstractSchemaRepository schemas,
            AbstractRevisionRepository<TypedEntity> revisions,
            IProviderTransaction providerTransaction,
            ISession nhSession,
            IFrameworkContext frameworkContext,
            bool isReadOnly)
            : base(providerMetadata, providerTransaction, revisions, schemas, frameworkContext)
        {
            Helper = new NhSessionHelper(nhSession, frameworkContext);
            IsReadonly = isReadOnly;

#if DEBUG
            if (schemas is SchemaRepository)
            {
                var sesh = schemas as SchemaRepository;
                if (sesh.Helper.NhSession != Helper.NhSession)
                    throw new InvalidOperationException("NHibernate provider can only be used in conjunction with an NHibernate schema provider when they share the same NHibernate session");
            }
#endif
        }

        protected bool IsReadonly { get; set; }

        protected internal NhSessionHelper Helper { get; set; }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            Helper.IfNotNull(x => x.Dispose());
            Schemas.Dispose();
            Revisions.Dispose();
            Transaction.Dispose();
        }

        public override IEnumerable<T> PerformGet<T>(bool allOrNothing, params HiveId[] ids)
        {
            Mandate.ParameterNotNull(ids, "ids");
            ids.ForEach(x => Mandate.ParameterNotEmpty(x, "id"));

            // We don't just ask for the Node by id here because some other types inherit from Node
            // like AttributeSchemaDefinition. Therefore, a Node that represents a TypedEntity is said
            // to exist if a NodeVersion exists
            Guid[] nodeIds = ids.Where(x => x.Value.Type == HiveIdValueTypes.Guid).Select(x => (Guid)x.Value).ToArray();
            //var nodeVersions = Helper.GetNodeVersionsByStatusDesc(nodeIds, limitToLatestRevision: true).ToArray();
            var nodeVersionQuery = Helper.GenerateAggregateStatusQuery(nodeIds, latestRevisionOnly: true);
            var nodeVersions = Helper.GetNodeVersionsInBatches(nodeVersionQuery.GeneratedQuery, false);

            return nodeIds.Select(x => nodeVersions.SingleOrDefault(y => y.Node.Id == x)).WhereNotNull().Select(x => FrameworkContext.TypeMappers.Map<T>(x));
        }

        public override IEnumerable<T> PerformExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var direction = query.From.HierarchyScope;


            //var disconnected = new NhQueryOverVisitor(Helper.NhSession).Visit(query.Criteria);

            var revisionStatus = query.From.RevisionStatusType != FromClause.RevisionStatusNotSpecifiedType ? query.From.RevisionStatusType : null;

            var nodeIds = GetNodeIds(query);

            var aggQuery = Helper.GenerateAggregateStatusQuery(nodeIds, revisionStatus, true, query.SortClauses, query.From.HierarchyScope, query.From.ScopeStartIds, query.From.HierarchyType, query.From.ExcludeParentIds, query.From.ExcludeEntityIds);

            var filteredAggQuery = GetFilterQuery(query, aggQuery.GeneratedQuery);

            // If we have some sort clauses, and they are just for supported fields, we can apply skips / takes earlier
            var appliedSort = false;
            filteredAggQuery = ApplyPossibleDbSortOrders(query, filteredAggQuery, out appliedSort, aggQuery.NodeTableIsJoinedAlready);

            // If we can apply skip/take in the db, do so
            var appliedSkipTakeAlready = ApplySkipOrTakeInDbIfPossible(query, ref filteredAggQuery);

            // First get the NodeVersionIds that match, then go back and load them in batches
            var matchingIds = Helper.GetResultVersionAndSchemaIds(filteredAggQuery, appliedSort);

            var batchLoader = Helper.GetNodeVersionsInBatches(matchingIds);

            IEnumerable<NodeVersion> resultBuilder = batchLoader;

            //// Need to order in memory using the materialised results because the field name is the value of a column in the resultset, not a column itself
            //// First materialise the results. Note that the Take / Skip is inefficient atm; NH bugs in Skip / Take in 3.0 causing same results irrespective of request, so enumerating whole list and skipping in memory (ouch)
            //// https://nhibernate.jira.com/browse/NH-2917
            var canApplyOrderingInDb = CanApplySortOrderInDb(query);
            if (!(canApplyOrderingInDb == ApplySortOrderInDb.CanApplyAll || canApplyOrderingInDb == ApplySortOrderInDb.NoOrderingRequired))
                resultBuilder = OrderMaterialisedResults(query, resultBuilder);

            if (!appliedSkipTakeAlready)
            {
                foreach (var filter in query.ResultFilters)
                {
                    switch (filter.ResultFilterType)
                    {
                        case ResultFilterType.Skip:
                            resultBuilder = resultBuilder.Skip(filter.SkipCount);
                            break;
                        case ResultFilterType.Take:
                            resultBuilder = resultBuilder.Take(filter.TakeCount);
                            break;
                    }
                }
            }

            var getResultType = query.GetSequenceResultType();

            if (getResultType != null && TypeFinder.IsTypeAssignableFrom(getResultType, typeof(T)))
            {
                var nodeVersions = resultBuilder.Distinct();
                return nodeVersions.Select(node => FrameworkContext.TypeMappers.Map<T>(node));
            }

            return Enumerable.Empty<T>();
        }

        enum ApplySortOrderInDb
        {
            CanApplyAll,
            CannotApplyAll,
            NoOrderingRequired
        }

        private static ApplySortOrderInDb CanApplySortOrderInDb(QueryDescription query)
        {
            var dbLevelSortOrders = new[] { "utccreated", "utcmodified", "utcstatuschanged" };
            if (query.SortClauses.Any())
            {
                var allArePossibleInDb = query.SortClauses.All(x => dbLevelSortOrders.Contains(x.FieldSelector.FieldName.ToLowerInvariant()));
                return allArePossibleInDb ? ApplySortOrderInDb.CanApplyAll : ApplySortOrderInDb.CannotApplyAll;
            }
            return ApplySortOrderInDb.NoOrderingRequired;
        }

        private static bool ApplySkipOrTakeInDbIfPossible(QueryDescription query, ref IQueryOver<AggregateNodeStatus, AggregateNodeStatus> filteredAggQuery)
        {
            bool modified = false;
            if (CanApplySortOrderInDb(query) != ApplySortOrderInDb.CannotApplyAll)
            {
                foreach (var filter in query.ResultFilters)
                {
                    switch (filter.ResultFilterType)
                    {
                        case ResultFilterType.Skip:
                            filteredAggQuery =
                                (IQueryOver<AggregateNodeStatus, AggregateNodeStatus>)
                                filteredAggQuery.Skip(filter.SkipCount).Take(9999);
                            modified = true;
                            break;
                        case ResultFilterType.Take:
                            filteredAggQuery =
                                (IQueryOver<AggregateNodeStatus, AggregateNodeStatus>)
                                filteredAggQuery.Take(filter.TakeCount);
                            modified = true;
                            break;
                    }
                }
            }
            return modified;
        }

        private static IQueryOver<AggregateNodeStatus, AggregateNodeStatus> ApplyPossibleDbSortOrders(QueryDescription query, IQueryOver<AggregateNodeStatus, AggregateNodeStatus> filteredAggQuery, out bool appliedSort, bool nodeIsJoinedAlready = false)
        {
            appliedSort = false;
            if (query.SortClauses.Any())
            {
                Node node = null;

                foreach (var sortClause in query.SortClauses)
                {
                    switch (sortClause.FieldSelector.FieldName.ToLowerInvariant())
                    {
                        case "utccreated":
                            var joined = nodeIsJoinedAlready ? filteredAggQuery : filteredAggQuery.JoinAlias(x => x.Node, () => node);
                            nodeIsJoinedAlready = true;

                            var createOrderBuilder = joined.OrderBy(() => node.DateCreated);

                            filteredAggQuery = (sortClause.Direction == SortDirection.Ascending)
                                                   ? createOrderBuilder.Asc
                                                   : createOrderBuilder.Desc;

                            appliedSort = true;
                            break;
                        case "utcmodified":
                        case "utcstatuschanged":
                            var modifiedOrderBuilder = filteredAggQuery.OrderBy(x => x.StatusDate);

                            filteredAggQuery = (sortClause.Direction == SortDirection.Ascending)
                                                   ? modifiedOrderBuilder.Asc
                                                   : modifiedOrderBuilder.Desc;

                            appliedSort = true;
                            break;
                    }
                }
            }
            return filteredAggQuery;
        }

        private IQueryOver<AggregateNodeStatus, AggregateNodeStatus> GetFilterQuery(QueryDescription query, IQueryOver<AggregateNodeStatus, AggregateNodeStatus> aggQuery)
        {
            var newStyle = new NhAggCriteriaVisitor(Helper).GenerateFilterQuery(query.Criteria);
            IQueryOver<AggregateNodeStatus, AggregateNodeStatus> filteredNewStyle = null;
            if (newStyle.SimpleNhCriterion == null && newStyle.Subquery == null)
            {
                // No criteria found
                filteredNewStyle = aggQuery.Clone();
            }
            else
            {
                filteredNewStyle =
                    aggQuery
                        .Clone()
                        .WithSubquery
                        .WhereProperty(x => x.NodeVersion.Id).In(newStyle.Subquery.Select(x => x.NodeVersion.Id));
            }
            return filteredNewStyle;
        }

        private static IQueryOver<AggregateNodeStatus, AggregateNodeStatus> AddSchemasToOuterQuery(IQueryOver<AggregateNodeStatus, AggregateNodeStatus> aggQuery, NhAggregateCriteriaVisitor.FilterResult filteringSubquery)
        {
            if (filteringSubquery.RequiredSchemaAliases != null && filteringSubquery.RequiredSchemaAliases.Any())
            {
                AttributeSchemaDefinition schema = null;
                NodeVersion v = null;
                aggQuery = aggQuery
                    .JoinAlias(x => x.NodeVersion, () => v)
                    .JoinAlias(() => v.AttributeSchemaDefinition, () => schema)
                    .Where(() => schema.Alias.IsIn(filteringSubquery.RequiredSchemaAliases.ToArray()));
            }
            if (filteringSubquery.RequiredNodeIds != null && filteringSubquery.RequiredNodeIds.Any())
            {
                aggQuery = aggQuery
                    .Where(x => x.Node.Id.IsIn(filteringSubquery.RequiredNodeIds.ToArray()));
            }
            return aggQuery;
        }

        private static IQueryOver<NodeVersion, NodeVersion> AddSchemasToOuterQuery(IQueryOver<NodeVersion, NodeVersion> mainLoaderQuery, NhCriteriaVisitor.FilterResult filteringSubquery)
        {
            if (filteringSubquery.RequiredSchemaAliases != null && filteringSubquery.RequiredSchemaAliases.Any())
            {
                AttributeSchemaDefinition schema = null;
                mainLoaderQuery = mainLoaderQuery
                    .JoinAlias(x => x.AttributeSchemaDefinition, () => schema)
                    .Where(() => schema.Alias.IsIn(filteringSubquery.RequiredSchemaAliases.ToArray()));
            }
            if (filteringSubquery.RequiredNodeIds != null && filteringSubquery.RequiredNodeIds.Any())
            {
                mainLoaderQuery = mainLoaderQuery
                    .Where(x => x.Node.Id.IsIn(filteringSubquery.RequiredNodeIds.ToArray()));
            }
            return mainLoaderQuery;
        }

        private static IEnumerable<NodeVersion> OrderMaterialisedResults(QueryDescription query, IEnumerable<NodeVersion> resultBuilder)
        {
            IOrderedEnumerable<NodeVersion> orderedResultBuilder = null;

            foreach (var source in query.SortClauses.OrderBy(x => x.Priority))
            {
                if (orderedResultBuilder != null)
                    if (source.Direction == SortDirection.Ascending)
                        orderedResultBuilder = orderedResultBuilder.ThenBy(x => x,
                                                                           new AttributeOrderingComparer(
                                                                               source.FieldSelector.FieldName,
                                                                               source.FieldSelector.ValueKey));
                    else
                        orderedResultBuilder = orderedResultBuilder.ThenByDescending(x => x,
                                                                                     new AttributeOrderingComparer(
                                                                                         source.FieldSelector.FieldName,
                                                                                         source.FieldSelector.ValueKey));
                else if (source.Direction == SortDirection.Ascending)
                    orderedResultBuilder = resultBuilder.OrderBy(x => x,
                                                                 new AttributeOrderingComparer(source.FieldSelector.FieldName,
                                                                                               source.FieldSelector.ValueKey));
                else
                    orderedResultBuilder = resultBuilder.OrderByDescending(x => x,
                                                                           new AttributeOrderingComparer(
                                                                               source.FieldSelector.FieldName,
                                                                               source.FieldSelector.ValueKey));
            }

            // If we've prepared an ordering, execute it here
            resultBuilder = orderedResultBuilder != null ? orderedResultBuilder.ToArray() : resultBuilder;
            return resultBuilder;
        }

        class AttributeOrderingComparer : IComparer<NodeVersion>
        {
            private readonly string _fieldAlias;
            private string _fieldSubAlias;

            public AttributeOrderingComparer(string fieldAlias, string fieldSubAlias = null)
            {
                _fieldAlias = fieldAlias;
                _fieldSubAlias = fieldSubAlias ?? "Value";
            }

            public int Compare(NodeVersion x, NodeVersion y)
            {
                int comparison;

                var leftAttrib = GetFirstAttribute(x);
                var rightAttrib = GetFirstAttribute(y);

                if (BreakEarly(leftAttrib, rightAttrib, out comparison)) return comparison;

                var leftLongStringVal = GetFirstLongString(leftAttrib);
                var rightLongStringVal = GetFirstLongString(rightAttrib);

                if (BreakEarly(leftLongStringVal, rightLongStringVal, out comparison,
                    (left, right) => String.Compare(left.Value, right.Value, StringComparison.InvariantCultureIgnoreCase)))
                    return comparison;


                var leftStringVal = GetFirstString(leftAttrib);
                var rightStringVal = GetFirstString(rightAttrib);

                if (BreakEarly(leftStringVal, rightStringVal, out comparison,
                    (left, right) => String.Compare(left.Value, right.Value, StringComparison.InvariantCultureIgnoreCase)))
                    return comparison;

                var leftDateVal = GetFirstDate(leftAttrib);
                var rightDateVal = GetFirstDate(rightAttrib);

                if (BreakEarly(leftDateVal, rightDateVal, out comparison,
                    (left, right) => DateTimeOffset.Compare(left.Value, right.Value)))
                    return comparison;

                return 0;
            }

            private static AttributeLongStringValue GetFirstLongString(Attribute leftAttrib)
            {
                if (leftAttrib == null || leftAttrib.AttributeLongStringValues == null) return null;
                return leftAttrib.AttributeLongStringValues.FirstOrDefault();
            }

            private static AttributeStringValue GetFirstString(Attribute leftAttrib)
            {
                if (leftAttrib == null || leftAttrib.AttributeStringValues == null) return null;
                return leftAttrib.AttributeStringValues.FirstOrDefault();
            }

            private static AttributeDateValue GetFirstDate(Attribute leftAttrib)
            {
                if (leftAttrib == null || leftAttrib.AttributeDateValues == null) return null;
                return leftAttrib.AttributeDateValues.FirstOrDefault();
            }

            private static bool BreakEarly<T>(
                T leftVal,
                T rightVal,
                out int compare,
                Func<T, T, int> comparisonIfBothNotNull = null)
                where T : AbstractEquatableObject<T>
            {
                if (rightVal == null && leftVal == null)
                {
                    compare = 0;
                    return false;
                }

                if (rightVal == null)
                {
                    compare = 1;
                    return true;
                }

                if (leftVal == null)
                {
                    compare = -1;
                    return true;
                }
                compare = comparisonIfBothNotNull != null ? comparisonIfBothNotNull.Invoke(leftVal, rightVal) : 0;
                return compare != 0;
            }

            private Attribute GetFirstAttribute(NodeVersion x)
            {
                return x.Attributes.FirstOrDefault(att => att.AttributeDefinition.Alias.InvariantEquals(_fieldAlias));
            }
        }

        private static Guid[] GetNodeIds(QueryDescription query)
        {
            var entityIds = query.From.RequiredEntityIds;

            var nodeIds = entityIds.Any() ? entityIds.Select(x => (Guid)x.Value).ToArray() : null;
            return nodeIds;
        }

        public override T PerformExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var revisionStatus = query.From.RevisionStatusType != FromClause.RevisionStatusNotSpecifiedType ? query.From.RevisionStatusType : null;

            NodeVersion outerVersionSelectorAlias = null;
            var nodeIds = GetNodeIds(query);

            var aggQuery = Helper.GenerateAggregateStatusQuery(nodeIds, revisionStatus, true, query.SortClauses, query.From.HierarchyScope, query.From.ScopeStartIds, query.From.HierarchyType, query.From.ExcludeParentIds, query.From.ExcludeEntityIds);

            var filteredAggQuery = GetFilterQuery(query, aggQuery.GeneratedQuery);

            // If we're combining with a Skip or Take, we can't do a count inside the db
            var requiresLoadingIds = false;
            foreach (var filter in query.ResultFilters)
            {
                switch (filter.ResultFilterType)
                {
                    case ResultFilterType.Take:
                        filteredAggQuery = (IQueryOver<AggregateNodeStatus, AggregateNodeStatus>)filteredAggQuery.Take(filter.TakeCount);
                        requiresLoadingIds = true;
                        break;
                    case ResultFilterType.Skip:
                        filteredAggQuery = (IQueryOver<AggregateNodeStatus, AggregateNodeStatus>)filteredAggQuery.Skip(filter.SkipCount).Take(99999); // Must include a Take in order for NH to generate the offset
                        requiresLoadingIds = true;
                        break;
                    case ResultFilterType.Count:
                        var count = GetCount(filteredAggQuery, requiresLoadingIds);

                        return (T)(object)count;
                    case ResultFilterType.Any:
                        var any = GetCount(filteredAggQuery, requiresLoadingIds) > 0;

                        return (T)(object)any;
                    case ResultFilterType.All:
                        var countAll = GetCount(aggQuery.GeneratedQuery, requiresLoadingIds);
                        var countFiltered = GetCount(filteredAggQuery, requiresLoadingIds);
                        var all = countAll == countFiltered;

                        return (T)(object)all;
                }
            }

            var many = ExecuteMany<T>(query, objectBinder);

            return many.Single();
        }

        private int GetCount(IQueryOver<AggregateNodeStatus, AggregateNodeStatus> aggQuery, bool requiresLoadingIds)
        {
            if (requiresLoadingIds)
            {
                // We can't do a count in the db engine and must load the distinct Ids instead
                var ids = aggQuery
                    .Select(Projections.Distinct(Projections.Property<AggregateNodeStatus>(x => x.Node.Id)))
                    .Future<Guid>();
                return ids.Distinct().Count();
            }

            int count;
            if (Helper.SupportsCountDistinct())
            {
                // Set the aggregate to return a count of the distinct NodeIds
                aggQuery = aggQuery.Select(Projections.CountDistinct<AggregateNodeStatus>(x => x.Node.Id));
                count = aggQuery.FutureValue<int>().Value;
            }
            else
            {
                // SqlCe doesn't support distinct in aggregates, so create a subquery
                // The ideal subquery for SqlCe would be
                // select count(*) from (select distinct NodeId ...)
                // However you can't select from a derived table in NH so we have to do a 
                // select count(*) where exists (select distinct NodeId .. where versionid & statusid match)
                // We could revert back to the ideal version if we used manual Sql concatenation

                // Create the distinct NodeId projection
                aggQuery = aggQuery.Select(Projections.Distinct(Projections.Property<AggregateNodeStatus>(x => x.Node.Id)));

                // Add some criteria to the inner aggregate query to tie it to the new outer one we'll create for the count
                AggregateNodeStatus outer = null;

                aggQuery = aggQuery
                    .Where(x => x.NodeVersion == outer.NodeVersion)
                    .And(x => x.StatusType == outer.StatusType);

                var outerQuery = Helper.NhSession.QueryOver(() => outer)
                    .Select(Projections.Count<AggregateNodeStatus>(x => x.Node.Id))
                    .WithSubquery.WhereExists((QueryOver<AggregateNodeStatus>)aggQuery);

                count = outerQuery.FutureValue<int>().Value;
            }
            return count;
        }

        public override T PerformExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder)
        {
            var revisionStatus = query.From.RevisionStatusType != FromClause.RevisionStatusNotSpecifiedType ? query.From.RevisionStatusType : null;
            var nodeIds = GetNodeIds(query);

            var aggQuery = Helper.GenerateAggregateStatusQuery(nodeIds, revisionStatus, true, query.SortClauses, query.From.HierarchyScope, query.From.ScopeStartIds, query.From.HierarchyType, query.From.ExcludeParentIds, query.From.ExcludeEntityIds);

            var filteredAggQuery = GetFilterQuery(query, aggQuery.GeneratedQuery);

            // If we have some sort clauses, and they are just for supported fields, we can apply skips / takes earlier
            var appliedSort = false;
            filteredAggQuery = ApplyPossibleDbSortOrders(query, filteredAggQuery, out appliedSort, aggQuery.NodeTableIsJoinedAlready);

            // If we can apply skip/take in the db, do so
            var appliedSkipTakeAlready = ApplySkipOrTakeInDbIfPossible(query, ref filteredAggQuery);

            // First get the NodeVersionIds that match, then go back and load them in batches
            var matchingIds = Helper.GetResultVersionAndSchemaIds(filteredAggQuery, appliedSort);

            var canApplyOrderingInDb = CanApplySortOrderInDb(query);

            foreach (var resultFilter in query.ResultFilters)
            {
                switch (resultFilter.ResultFilterType)
                {
                    case ResultFilterType.Single:
                    case ResultFilterType.SingleOrDefault:
                        try
                        {
                            var resultCount = matchingIds.Count();
                            if (resultCount > 1)
                                throw new NonUniqueResultException(resultCount);

                            var singleResult = Helper.GetNodeVersionsInBatches(matchingIds.Take(1)).FirstOrDefault();

                            if (ReferenceEquals(singleResult, null))
                            {
                                if (resultFilter.ResultFilterType == ResultFilterType.Single)
                                {
                                    throw new InvalidOperationException("Sequence contains 0 elements but query specified exactly 1 must be present");
                                }
                                return default(T);
                            }

                            return FrameworkContext.TypeMappers.Map<T>(singleResult);
                        }
                        catch (NonUniqueResultException ex)
                        {
                            const string nastyNhExceptionMessage = "query did not return a unique result: ";
                            var getNumberFromNastyNHMessage = ex.Message.Replace(nastyNhExceptionMessage, "");
                            throw new InvalidOperationException("Sequence contains {0} elements but query specified exactly 1 must be present.".InvariantFormat(getNumberFromNastyNHMessage), ex);
                        }
                    case ResultFilterType.First:
                    case ResultFilterType.FirstOrDefault:
                        var exceptionForFirst = new InvalidOperationException("Sequence contains 0 elements when non-null First element was required");

                        if (canApplyOrderingInDb == ApplySortOrderInDb.CanApplyAll || canApplyOrderingInDb == ApplySortOrderInDb.NoOrderingRequired)
                        {
                            var firstOrDefault = matchingIds.FirstOrDefault();
                            matchingIds = (firstOrDefault == null)
                                              ? Enumerable.Empty<NhSessionHelper.VersionAndSchemaId>()
                                              : firstOrDefault.AsEnumerableOfOne();
                        }

                        if (!matchingIds.Any() && resultFilter.ResultFilterType == ResultFilterType.First)
                        {
                            throw exceptionForFirst;
                        }

                        var resultsFirst = Helper.GetNodeVersionsInBatches(matchingIds);
                        if (!(canApplyOrderingInDb == ApplySortOrderInDb.CanApplyAll || canApplyOrderingInDb == ApplySortOrderInDb.NoOrderingRequired))
                            resultsFirst = OrderMaterialisedResults(query, resultsFirst);
                        var firstItem = resultsFirst.FirstOrDefault();

                        if (ReferenceEquals(firstItem, null))
                        {
                            if (resultFilter.ResultFilterType == ResultFilterType.First)
                            {
                                throw exceptionForFirst;
                            }
                            return default(T);
                        }

                        return FrameworkContext.TypeMappers.Map<T>(firstItem);
                    case ResultFilterType.Last:
                    case ResultFilterType.LastOrDefault:
                        var exceptionForLast = new InvalidOperationException("Sequence contains 0 elements when non-null Last element was required");

                        if (canApplyOrderingInDb == ApplySortOrderInDb.CanApplyAll || canApplyOrderingInDb == ApplySortOrderInDb.NoOrderingRequired)
                        {
                            var lastOrDefault = matchingIds.LastOrDefault();
                            matchingIds = (lastOrDefault == null)
                                              ? Enumerable.Empty<NhSessionHelper.VersionAndSchemaId>()
                                              : lastOrDefault.AsEnumerableOfOne();
                        }

                        if (!matchingIds.Any() && resultFilter.ResultFilterType == ResultFilterType.Last)
                        {
                            throw exceptionForLast;
                        }

                        var resultsLast = Helper.GetNodeVersionsInBatches(matchingIds);
                        if (!(canApplyOrderingInDb == ApplySortOrderInDb.CanApplyAll || canApplyOrderingInDb == ApplySortOrderInDb.NoOrderingRequired))
                            resultsLast = OrderMaterialisedResults(query, resultsLast);
                        var lastItem = resultsLast.LastOrDefault();

                        if (ReferenceEquals(lastItem, null))
                        {
                            if (resultFilter.ResultFilterType == ResultFilterType.First)
                            {
                                throw exceptionForLast;
                            }
                            return default(T);
                        }

                        return FrameworkContext.TypeMappers.Map<T>(lastItem);
                }
            }



            return ExecuteMany<T>(query, objectBinder).FirstOrDefault();
        }

        public override IEnumerable<T> PerformGetAll<T>()
        {
            //var outerQuery = Helper.GetNodeVersionsByStatusDesc();
            //return outerQuery.Select(x => FrameworkContext.TypeMappers.Map<T>(x));
            var agg = Helper.GenerateAggregateStatusQuery();
            var matches = Helper.GetNodeVersionsInBatches(agg.GeneratedQuery, false);
            return matches.Select(x => FrameworkContext.TypeMappers.Map<T>(x));
        }

        public override bool Exists<TEntity>(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            var value = (Guid)id.Value;

            var qo = Helper.NhSession.QueryOver<NodeVersion>()
                .Where(x => x.Node.Id == value)
                .Select(Projections.RowCount())
                .Cacheable()
                .SingleOrDefault<int>();

            return qo > 0;
        }

        public override bool CanReadRelations
        {
            get { return true; }
        }

        public override IEnumerable<IRelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            return Helper.PerformGetParentRelations(childId, relationType);
        }

        public override IRelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            return Helper.PerformFindRelation(sourceId, destinationId, relationType);
        }

        public override IEnumerable<IRelationById> PerformGetAncestorRelations(HiveId descendentId, RelationType relationType = null)
        {
            var parents = GetParentRelations(descendentId, relationType).ToArray();
            return parents.SelectRecursive(x => GetParentRelations(x.SourceId, relationType));
        }

        public override IEnumerable<IRelationById> PerformGetDescendentRelations(HiveId ancestorId, RelationType relationType = null)
        {
            var childRelations = GetChildRelations(ancestorId, relationType).ToArray();
            return childRelations.SelectRecursive(x =>
                                                      {
                                                          var childRelationsSub = GetChildRelations(x.DestinationId, relationType).ToArray();
                                                          return childRelationsSub;
                                                      });
        }

        public override IEnumerable<IRelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            return Helper.PerformGetChildRelations(parentId, relationType);
        }

        //public override IEnumerable<IRelationById> PerformGetBranchRelations(HiveId siblingId, RelationType relationType = null)
        //{
        //    return Helper.PerformGetBranchRelations(siblingId, relationType);
        //}

        protected override void PerformAddOrUpdate(TypedEntity entity)
        {
            Mandate.ParameterNotNull(entity, "persistedEntity");

                // Note that it should be the caller's responsibility to add to revisions but the Cms backoffice code needs to change
                // to do that, so this is included to avoid breaking assumptions about auto-created versions until then
                if (Revisions.CanWrite)
                {
                    var newRevision = new Revision<TypedEntity>(entity);
                    Revisions.AddOrUpdate(newRevision);
                    return;
                }
                else
                {
                    if (TryUpdateExisting(entity)) return;

                    Helper.MapAndMerge(entity, FrameworkContext.TypeMappers);
                }
        }

        private bool TryUpdateExisting(AbstractEntity persistedEntity)
        {
            var mappers = FrameworkContext.TypeMappers;

            // Get the entity with matching Id, provided the incoming Id is not null / empty
            if (!persistedEntity.Id.IsNullValueOrEmpty())
            {
                Type rdbmsType;
                if (mappers.TryGetDestinationType(persistedEntity.GetType(), typeof(IReferenceByGuid), out rdbmsType))
                {
                    //// Temp hack for testing
                    //if (typeof(NodeVersion) == rdbmsType && typeof(TypedEntity) == persistedEntity.GetType())
                    //{
                    //    rdbmsType = typeof(Node);

                    //    var nodeVersions = global::NHibernate.Linq.LinqExtensionMethods.Query<NodeVersion>(InnerDataContext.NhibernateSession).Where(x => x.Node.Id == persistedEntity.Id.AsGuid);
                    //    var firstOrDefault = nodeVersions.FirstOrDefault();
                    //    if (firstOrDefault == null) return false;

                    //    var latest = GetMostRecentVersionFromQuery(firstOrDefault.Node);
                    //    if (latest != null)
                    //    {
                    //        mappers.Map(persistedEntity, latest, persistedEntity.GetType(), latest.GetType());
                    //        //InnerDataContext.NhibernateSession.Evict(latest);
                    //        latest = InnerDataContext.NhibernateSession.Merge(latest) as NodeVersion;
                    //        //InnerDataContext.NhibernateSession.SaveOrUpdate(existingEntity);
                    //        mappers.Map(latest, persistedEntity, latest.GetType(), persistedEntity.GetType());
                    //        SetOutgoingId(persistedEntity);
                    //        //_trackNodePostCommits.Add((IReferenceByGuid)existingEntity, persistedEntity);
                    //        return true;
                    //    }
                    //}

                    var existingEntity = Helper.NhSession.Get(rdbmsType, (Guid)persistedEntity.Id.Value);
                    if (existingEntity != null)
                    {
                        mappers.Map(persistedEntity, existingEntity, persistedEntity.GetType(), existingEntity.GetType());
                        existingEntity = Helper.NhSession.Merge(existingEntity);
                        //InnerDataContext.NhibernateSession.SaveOrUpdate(existingEntity);
                        mappers.Map(existingEntity, persistedEntity, existingEntity.GetType(), persistedEntity.GetType());
                        // ##API2: Disabled: SetOutgoingId(persistedEntity);
                        //_trackNodePostCommits.Add((IReferenceByGuid)existingEntity, persistedEntity);
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void PerformDelete<T>(HiveId id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            if (id.Value.Type != HiveIdValueTypes.Guid) return;

            // We don't issue a direct-to-db deletion because otherwise NH can't keep track
            // of any cascading deletes
            // InnerDataContext.NhibernateSession.Delete(destinationType, entityId.AsGuid);

            object nhObject;

            nhObject = Helper.NhSession.Get<Node>((Guid)id.Value);
            var node = (Node)nhObject;
            node.NodeVersions.EnsureClearedWithProxy();
            Helper.RemoveRelationsBiDirectional(node);

            Helper.NhSession.Delete(nhObject);
        }

        public override bool CanWriteRelations
        {
            get { return true; }
        }

        protected override void PerformAddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item)
        {
            Helper.AddRelation(item, this.RepositoryScopedCache);
        }

        protected override void PerformRemoveRelation(IRelationById item)
        {
            Helper.RemoveRelation(item, this.RepositoryScopedCache);
        }


    }
}