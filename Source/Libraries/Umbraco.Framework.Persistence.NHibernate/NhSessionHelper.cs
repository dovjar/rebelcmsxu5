using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using HibernatingRhinos.Profiler.Appender;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using Umbraco.Framework.Context;
using Umbraco.Framework.Linq.QueryModel;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Associations._Revised;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Versioning;
using Umbraco.Framework.Persistence.NHibernate.Dependencies;
using Umbraco.Framework.Persistence.RdbmsModel;
using Umbraco.Framework.TypeMapping;
using Attribute = Umbraco.Framework.Persistence.RdbmsModel.Attribute;
using AttributeDefinition = Umbraco.Framework.Persistence.RdbmsModel.AttributeDefinition;

namespace Umbraco.Framework.Persistence.NHibernate
{
    using System.Reflection;
    using Umbraco.Framework.Data;
    using Umbraco.Framework.Persistence.Model.Constants;
    using Umbraco.Framework.Persistence.NHibernate.Linq;
    using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings;
    using global::NHibernate.Dialect;
    using global::NHibernate.SqlCommand;

    public class NhSessionHelper : DisposableObject
    {
        internal readonly NodeVersion vFilter = null;

        private readonly NodeChangeTrackerBag _trackPostCommits = new NodeChangeTrackerBag();
        public ISession NhSession { get; protected set; }

        public NhSessionHelper(ISession nhSession, IFrameworkContext frameworkContext)
        {
            NhSession = nhSession;
            NhEventListeners.AddNodeIdHandler(this, HandleNodeIdAvailable);
            FrameworkContext = frameworkContext;
        }

        public IFrameworkContext FrameworkContext { get; protected set; }

        public void HandleNodeIdAvailable(IReferenceByGuid node, Guid id)
        {
            var persistenceEntity = _trackPostCommits.FlushWhere(x => x.Key.Id == node.Id);

            foreach (var keyValuePair in persistenceEntity)
            {
                keyValuePair.Value.Id = (HiveId)id;
            }
        }

        public NodeRelationType GetOrCreateNodeRelationType(string alias, AbstractScopedCache repositoryScopedCache)
        {
            return repositoryScopedCache.GetOrCreateTyped(
                "NodeRelationType-" + alias,
                () =>
                {
                    var existing = NhSession.QueryOver<NodeRelationType>()
                        .Where(x => x.Alias == alias)
                        .Cacheable()
                        .Take(1)
                        .SingleOrDefault<NodeRelationType>();

                    return existing ?? new NodeRelationType() { Alias = alias, Id = alias.EncodeAsGuid() };
                });
        }

        public const string ProfilerLoggingPrefix = "CUSTOM PROFILE: ";

        public Guid GetSessionId()
        {
            return ((ISessionImplementor)NhSession).SessionId;
        }

        internal static Dialect GetDialect(ISessionFactory sessionFactory)
        {
            var implementor = sessionFactory as ISessionFactoryImplementor;
            if (implementor == null) return null;
            return implementor.Dialect;
        }

        private bool RequiresAllForGtSubquery()
        {
            var dialect = GetDialect(NhSession.SessionFactory);
            if (dialect == null) return false;
            var sqlCeDialect = dialect as MsSqlCe40Dialect;
            if (sqlCeDialect != null) return true;
            return false;
        }

        internal bool IsSqlServer()
        {
            var dialect = GetDialect(NhSession.SessionFactory);
            if (dialect == null) return false;
            return ((dialect as MsSql2008Dialect) != null);
        }

        internal bool SupportsCountDistinct()
        {
            var dialect = GetDialect(NhSession.SessionFactory);
            if (dialect == null) return false;
            return ((dialect as MsSql2008Dialect) != null) || ((dialect as SQLiteDialect) != null);
        }

        public class AggregateQuery
        {
            public IQueryOver<AggregateNodeStatus, AggregateNodeStatus> GeneratedQuery { get; set; }
            public bool NodeTableIsJoinedAlready { get; set; }
        }

        public AggregateQuery GenerateAggregateStatusQuery(Guid[] restrictToNodeIds = null, RevisionStatusType restrictToStatus = null, bool latestRevisionOnly = true, IEnumerable<SortClause> sortClauses = null, HierarchyScope relationScope = HierarchyScope.Indeterminate, IEnumerable<HiveId> scopeStartIds = null, string relationTypeAlias = null, IEnumerable<HiveId> excludeParentIds = null, IEnumerable<HiveId> excludeIds = null)
        {
            if (!latestRevisionOnly)
                throw new NotImplementedException("Need to not join to AggregateNodeStatus");

            var toReturn = new AggregateQuery();
            toReturn.NodeTableIsJoinedAlready = false;

            scopeStartIds = scopeStartIds ?? Enumerable.Empty<HiveId>();
            excludeParentIds = excludeParentIds ?? Enumerable.Empty<HiveId>();
            excludeIds = excludeIds ?? Enumerable.Empty<HiveId>();

            relationTypeAlias = relationTypeAlias ?? FixedRelationTypes.DefaultRelationType.RelationName;

            restrictToNodeIds = restrictToNodeIds ?? new Guid[0];

            // We're going to get the NodeVersionId from the aggregate table

            AggregateNodeStatus pub = null;
            AggregateNodeStatus unpub = null;
            NodeVersionStatusType pubType = null;
            NodeVersionStatusType unpubType = null;

            Node node = null;
            NodeRelation relation = null;
            NodeRelationType relationType = null;

            var mainQuery = NhSession.QueryOver<AggregateNodeStatus>(() => pub);

            if (restrictToNodeIds.Any())
            {
                mainQuery = mainQuery.Where(x => x.Node.Id.IsIn(restrictToNodeIds));
            }

            if (excludeIds.Any())
            {
                var asGuid = excludeIds.Select(x => (Guid)x.Value).ToArray();
                mainQuery = mainQuery.Where(x => !x.Node.Id.IsIn(asGuid));
            }

            var alreadyJoinedRelations = false;
            if (scopeStartIds.Any())
            {
                var asGuid = scopeStartIds.Select(x => (Guid)x.Value).ToArray();
                switch (relationScope)
                {
                    case HierarchyScope.Children:
                        // The scope start id(s) are essentially "permitted parents", i.e. direct relations where the Start Node matches the id
                        mainQuery = mainQuery
                            .JoinAlias(() => pub.Node, () => node)
                            .JoinAlias(() => node.IncomingRelations, () => relation)
                            .JoinAlias(() => relation.NodeRelationType, () => relationType)
                            .Where(() => relation.StartNode.Id.IsIn(asGuid))
                            .And(() => relationType.Alias == relationTypeAlias);
                        alreadyJoinedRelations = true;
                        toReturn.NodeTableIsJoinedAlready = true;
                        break;
                }
            }

            if (excludeParentIds.Any())
            {
                var asGuid = excludeParentIds.Select(x => (Guid)x.Value).ToArray();
                if (!alreadyJoinedRelations)
                {
                    mainQuery = mainQuery
                        .JoinAlias(() => pub.Node, () => node)
                        .JoinAlias(() => node.IncomingRelations, () => relation)
                        .JoinAlias(() => relation.NodeRelationType, () => relationType);
                    alreadyJoinedRelations = true;
                }
                mainQuery = mainQuery.Where(() => !relation.StartNode.Id.IsIn(asGuid));
            }

            

            if (restrictToStatus != null)
            {
                mainQuery = mainQuery.JoinAlias(() => pub.StatusType, () => pubType);

                if (restrictToStatus.NegatedByTypes != null && restrictToStatus.NegatedByTypes.Any())
                {
                    // Add a subquery restriction for the negated types
                    // NH doesn't support joining a table to itself in this instance because
                    // we're using a composite key but only want to join on NodeId,
                    // so we have to use a subquery (pending just generating the Sql ourselves)
                    var unpubStatus = Projections.Property(() => unpub.StatusDate);
                    var maxUnpubStatus = Projections.Max(unpubStatus);
                    var benchDate = Projections.Constant(new DateTime(1980, 1, 1));
                    var coalesce = Projections.SqlFunction("coalesce", NHibernateUtil.Date, maxUnpubStatus, benchDate);
                    var types = restrictToStatus.NegatedByTypes.Select(x => x.Alias).ToArray();

                    var getMaxDate = QueryOver.Of(() => unpub)
                        .JoinAlias(() => unpub.StatusType, () => unpubType)
                        .Where(() => pub.Node == unpub.Node)
                        .And(() => unpub.StatusDate > pub.StatusDate)
                        .And(() => unpubType.Alias.IsIn(types)).Select(x => x.NodeVersion.Id)
                        .Select(coalesce);

                    if (RequiresAllForGtSubquery())
                    {
                        mainQuery = mainQuery.WithSubquery.WhereProperty(() => pub.StatusDate).GtAll(getMaxDate);
                    }
                    else
                    {
                        mainQuery = mainQuery.WithSubquery.WhereProperty(() => pub.StatusDate).Gt(getMaxDate);
                    }
                }

                mainQuery = mainQuery.And(() => pubType.Alias == restrictToStatus.Alias);
            }

            if (latestRevisionOnly && restrictToStatus == null) // Only need to filter the aggregates when a status isn't specified anyway
            {
                // Generate a top 1 query ordered by date (SqlLite doesn't support Having Max(inner.Date) = outer.Date without a group, and NH can't add a grouping without adding it to the select list)
                AggregateNodeStatus date = null;
                var topDate = QueryOver.Of(() => date)
                    .Where(() => date.Node == pub.Node)
                    .OrderBy(() => date.StatusDate).Desc
                    .Select(x => x.NodeVersion.Id)
                    .Take(1);
                mainQuery = mainQuery.WithSubquery.WhereProperty(() => pub.NodeVersion).In(topDate);
            }

            toReturn.GeneratedQuery = mainQuery;
            return toReturn;
        }

        public IQueryOver<NodeVersion, NodeVersion> GenerateVersionedQuery2(Guid[] nodeIds = null, RevisionStatusType revisionStatus = null, bool limitToLatestRevision = true, IEnumerable<SortClause> sortClauses = null)
        {
            if (nodeIds != null && nodeIds.Length > 0)
                Mandate.That(nodeIds.Length < 1000, x => new ArgumentOutOfRangeException("nodeIds", "The maximum filter restriction is 1000 nodes, but {0} were supplied".InvariantFormat(nodeIds.Length)));

                // Set up some aliases for the joins in the query
                Node node = null;
                NodeVersion filterVersion = null;

                // Aliases for revision filter
                NodeVersionStatusHistory histFilter = null;
                NodeVersionStatusType typeFilter = null;

                // Set up the query which will filter by status type, if one has been provided
                QueryOver<NodeVersion, NodeVersion> combinedFilterBuilder = null;
                QueryOver<NodeVersion, NodeVersion> maxDateFilterBuilder = null;

                // Create the combined filter which may optionally have the max-date subquery if it has been created
                combinedFilterBuilder = QueryOver.Of(() => filterVersion);

                //// Specify the node-id filter on the subquery since we will be doing a top-x query on it
                //if (nodeIds != null && nodeIds.Length > 0)
                //{
                //    // Diff between SqlCe and Sql Server:
                //    // SqlCe has the same execution speed when the outer query has a list of nodeids
                //    // and the subquery just says "where outer.node == inner.node"
                //    // However, Sql Server 2008 does not have this optimisation and it's measurably slower
                //    // (20ms vs 90ms)

                //    // So that we don't waste parameter slots, if we're in SqlCe we'll specify the nodes on
                //    // the outer query and use the reference check on the inner query.
                //    // In Sql Server we'll not specify the node list on the outer query at all and only on the inner one
                //    if (IsSqlServer())
                //    {
                //        combinedFilterBuilder = combinedFilterBuilder.Where(() => vFilter.Node.Id.IsIn(nodeIds));
                //    }
                //    else
                //    {
                //        combinedFilterBuilder = combinedFilterBuilder.Where(() => filterVersion.Node == vFilter.Node);
                //    }
                //}

                // If revisionStatus is null then we'll need to separately create combinedFilterBuilder in order to add the value filters
                if (revisionStatus != null)
                {
                    // First set up the combined filter which will be used as a subselect
                    // to filter the NodeVersion table
                    var statusAlias = revisionStatus.Alias;

                    // If the supplied status has "negating statusses" then we must 
                    // essentially discount any matches for the right status, if
                    // any of the negating statusses exist that are newer.
                    // I.e., it doesn't matter if something is "published" if there's a newer
                    // "unpublished" status entry.
                    QueryOver<NodeVersionStatusHistory> excludeNegatingStatusses = null;
                    if (revisionStatus.NegatedByTypes.Any())
                    {
                        // Set up the aliases for the negation subquery
                        NodeVersionStatusHistory negHistFilter = null;
                        NodeVersionStatusType negTypeFilter = null;
                        NodeVersion negVersion = null;

                        var negatingAliases = revisionStatus.NegatedByTypes.Select(x => x.Alias).ToArray();

                        // This subquery selects the maximum date of any of the negating statusses,
                        // or an old point-in-time benchmark if the result is null (i.e. there aren't any negating statusses)
                        // The purposes is to later use this to say "select a status whose date is greater than" this subselect
                        excludeNegatingStatusses = QueryOver.Of(() => negHistFilter)
                            .JoinAlias(() => negHistFilter.NodeVersionStatusType, () => negTypeFilter)
                            .JoinAlias(() => negHistFilter.NodeVersion, () => negVersion)
                            .Where(() => negTypeFilter.Alias.IsIn(negatingAliases))
                            .And(() => filterVersion.Node == negVersion.Node)
                            .Select(Projections.SqlFunction("coalesce",
                                                            NHibernateUtil.DateTime,
                                                            Projections.Max<NodeVersionStatusHistory>(x => x.Date),
                                                            new ConstantProjection(new DateTime(1980, 1, 1))));
                    }

                    // Tell the combined filter that it must match the maximum date in the subselect
                    combinedFilterBuilder = combinedFilterBuilder
                        .JoinAlias(() => filterVersion.NodeVersionStatuses, () => histFilter)
                        .JoinAlias(() => histFilter.NodeVersionStatusType, () => typeFilter)
                        .And(() => typeFilter.Alias == statusAlias);

                    // If we're limiting to the latest revision, we need to construct a subselect that will
                    // take the maximum status date so that we can filter on that
                    if (limitToLatestRevision)
                    {
                        // Join NodeVersion to the status and type tables
                        // Don't add a precondition that the NodeId matches the outer query as it
                        // is unneccessary and slows down execution
                        NodeVersion maxVFilter = null;
                        maxDateFilterBuilder = QueryOver.Of(() => maxVFilter)
                            .JoinAlias(() => maxVFilter.NodeVersionStatuses, () => histFilter)
                            .JoinAlias(() => histFilter.NodeVersionStatusType, () => typeFilter)
                            .And(() => filterVersion.Node == maxVFilter.Node)
                            .And(() => typeFilter.Alias == statusAlias);

                        // If we have negating statusses, then the subquery should say "where the status date is greater than the maximum date of a negating status"
                        // If we don't have negating statusses, then the subquery should just say "select the max status date where the status type matches"

                        // We are going to say "where the status date is greater than" this subselect
                        if (excludeNegatingStatusses != null)
                        {
                            // We have negating statusses, so we need only find versions newer than them (if any exist).

                            // We have to use either "greater than" or "greater than all", by checking the Sql dialect
                            // It's horrible to have to check the Sql dialect when generating the query, but Nh's dialect support doesn't allow a provider-based
                            // way of telling whether the db engine supports / requires "all" to be prefix before a subquery when doing an operation.
                            // e.g. SqlCe requires "blah > all (select max(blah) from blah)", SqlServer doesn't mind, SQLite doesn't support it
                            if (RequiresAllForGtSubquery())
                                maxDateFilterBuilder =
                                    maxDateFilterBuilder.WithSubquery.WhereProperty(() => histFilter.Date).GtAll(
                                        excludeNegatingStatusses);
                            else
                                maxDateFilterBuilder =
                                    maxDateFilterBuilder.WithSubquery.WhereProperty(() => histFilter.Date).Gt(
                                        excludeNegatingStatusses);
                        }
                        else
                        {
                            // We don't have negating statusses, so the max-date filter should just select the max date
                            maxDateFilterBuilder = maxDateFilterBuilder.Select(Projections.Max(() => histFilter.Date));
                        }

                        combinedFilterBuilder = combinedFilterBuilder
                            .WithSubquery
                            .WhereProperty(() => histFilter.Date)
                            .In((maxDateFilterBuilder.Select(Projections.Max(() => histFilter.Date))));
                    }
                    else
                    {
                        if (excludeNegatingStatusses != null)
                        {
                            // If we're not limiting to the latest revision, we don't need the subselect, and can instead 
                            // directly add another clause to the combined filter only if there are negating statusses
                            // and if not, we don't add any date clause at all
                            combinedFilterBuilder =
                                combinedFilterBuilder.WithSubquery.WhereProperty(() => histFilter.Date).Gt(
                                    excludeNegatingStatusses);
                        }
                    }
                }
                else
                {
                    if (limitToLatestRevision)
                    {
                        // If we've been asked to limit to the latest revision, but no specific revision type, we need to only select the
                        // versions with maximum-dated status
                        NodeVersion maxVFilter = null;
                        maxDateFilterBuilder = QueryOver.Of(() => maxVFilter)
                            .JoinAlias(() => maxVFilter.NodeVersionStatuses, () => histFilter)
                            .And(() => filterVersion.Node == maxVFilter.Node)
                            .Select(Projections.Max(() => histFilter.Date));

                        combinedFilterBuilder = combinedFilterBuilder
                            .JoinAlias(() => filterVersion.NodeVersionStatuses, () => histFilter)
                            .WithSubquery
                            .WhereProperty(() => histFilter.Date)
                            .In((maxDateFilterBuilder.Select(Projections.Max(() => histFilter.Date))));
                    }
                    else
                    {
                        // Merely introduce a subquery so that we can limit pagesize and sort in the same
                        // method as is expected in the code which follows this if statement (to reduce
                        // the number of if statements we have, I've left certain queries such as 
                        // .OrderBy(() => histFilter.Date).Desc
                        combinedFilterBuilder = combinedFilterBuilder
                            .JoinAlias(() => filterVersion.NodeVersionStatuses, () => histFilter);
                    }
                }

                // For each of the sort clauses, add an orderby
                // TODO: Need access to the generated attribute filter here including
                // support for sorting by "modified" or "created" date
                //			if (sortClauses != null)
                //				foreach (var clause in sortClauses)
                //				{
                //					clause.FieldSelector.
                //					combinedFilterBuilder = combinedFilterBuilder.OrderBy(() => histFilter.Date).Desc;
                //				}

                // Finally add a clause to sort by the date of the version, descending
                // Because we're adding an order-by to the inner subquery (to avoid a join on the outer query when
                // we already have that join here) we need to specify a take too) - this is further down 
                // where we've added it as the subquery to the main query
                combinedFilterBuilder = combinedFilterBuilder.OrderBy(() => histFilter.Date).Desc;


                // Specify that we're only interested in the NodeVersion.Id
                combinedFilterBuilder = combinedFilterBuilder.Select(x => x.Id);

                // Construct the main selector for NodeVersion information for NH to hydrate
                // which will be filtered by the subselects we have created

                // Create aliases for joins in this outer query
                NodeVersionStatusHistory sortByHist = null;
                Attribute attribAlias = null;
                var main = NhSession.QueryOver<NodeVersion>(() => vFilter)
                    .Fetch(x => x.AttributeSchemaDefinition).Lazy
                    .Fetch(x => x.Attributes).Eager
                    // We load these eagerly rather than in a Future to avoid a separate query due to NH's slow rehydration in Futures
                    .Fetch(x => x.Node).Lazy
                    // There's a 1-m mapping between Node-NodeVersion so safe to load this with a join too rather than with a future
                    .Left.JoinAlias(() => vFilter.Attributes, () => attribAlias);
                //.Left.JoinAlias(() => attribAlias.AttributeDefinition, () => defEager)
                //.Fetch(x => attribAlias.AttributeDefinition).Eager
                //.Fetch(x => defEager).Eager;

                //.Fetch(x => x.NodeVersionStatuses).Lazy
                //.Inner.JoinQueryOver(x => vFilter.NodeVersionStatuses, () => sortByHist);

                // Add the node-id filter to the main query (even though it's also on the subquery) since it is much faster for SqlCe
                if (nodeIds != null && nodeIds.Length > 0)
                {
                    // Diff between SqlCe and Sql Server:
                    // SqlCe has the same execution speed when the outer query has a list of nodeids
                    // and the subquery just says "where outer.node == inner.node"
                    // However, Sql Server 2008 does not have this optimisation and it's measurably slower
                    // (20ms vs 90ms)

                    // So that we don't waste parameter slots, if we're in SqlCe we'll specify the nodes on
                    // the outer query and use the reference check on the inner query.
                    // In Sql Server we'll not specify the node list on the outer query at all and only on the inner one
                    if (IsSqlServer())
                    {
                        combinedFilterBuilder = combinedFilterBuilder.Where(() => filterVersion.Node.Id.IsIn(nodeIds));
                    }
                    else
                    {
                        combinedFilterBuilder = combinedFilterBuilder.Where(() => filterVersion.Node == vFilter.Node);
                        main = main.Where(() => vFilter.Node.Id.IsIn(nodeIds));
                    }
                }

                // Perf findings:
                // Removing the joins from the main query so that AttributeSchemaDefinition & Node are
                // not joined makes a massive difference to query execution speed (e.g. with 67000 revisions, it's 
                // 600ms vs 60ms)
                // However, in SqlCe, because it doesn't support batching, if we add the below as a future
                // and cache it, NHibernate actually caches the results OK
                // For Sql Server, we'll see how we get on with select n+1 loading the schemas as needed
                if (!IsSqlServer())
                {
                    AttributeSchemaDefinition schemaEager = null;
                    AttributeDefinition defEager = null;
                    AttributeGroup groupEager = null;
                    var futureAttribs = NhSession.QueryOver<AttributeSchemaDefinition>(() => schemaEager)
                        .Left.JoinAlias(() => schemaEager.AttributeDefinitions, () => defEager)
                        .Left.JoinAlias(() => schemaEager.AttributeDefinitionGroups, () => groupEager)
                        .Cacheable()
                        .CacheRegion("eager-load-schemas")
                        .Future();
                }

                // If the combined filter is not null, then ensure the outer query uses it as a filter
                if (combinedFilterBuilder != null)
                {
                    // We have something with which to filter the main selection query
                    // So ensure it has a page limit and that it is selecting the id
                    var finaliseCombinedFilter = combinedFilterBuilder.Select(x => x.Id).Take(100);
                    main = main.WithSubquery.WhereProperty(x => vFilter.Id).In(finaliseCombinedFilter);
                }



                // Join to the attributes tables
                AttributeDateValue attributeDateValue = null;
                AttributeDecimalValue attributeDecimalValue = null;
                AttributeIntegerValue attributeIntegerValue = null;
                AttributeStringValue attributeStringValue = null;
                AttributeLongStringValue attributeLongStringValue = null;
                main = main
                    //.Left.JoinAlias(() => vFilter.Attributes, () => attribAlias) // Using a left join identifies to Nh that it can reuse the loaded Attributes because Nh considers them to be unaffected by the query, otherwise it issues another select from accessing NodeVersion.Attributes
                    .Left.JoinAlias(() => attribAlias.AttributeStringValues, () => attributeStringValue)
                    .Left.JoinAlias(() => attribAlias.AttributeLongStringValues, () => attributeLongStringValue)
                    .Left.JoinAlias(() => attribAlias.AttributeIntegerValues, () => attributeIntegerValue)
                    .Left.JoinAlias(() => attribAlias.AttributeDecimalValues, () => attributeDecimalValue)
                    .Left.JoinAlias(() => attribAlias.AttributeDateValues, () => attributeDateValue);

                // Return the generated query
                return main;
        }

        //public IQueryOver<NodeVersion, NodeVersion> GenerateVersionedQuery(Guid[] nodeIds = null, RevisionStatusType revisionStatus = null, bool limitToLatestRevision = true, IEnumerable<SortClause> sortClauses = null)
        //{
        //    Node node = null;
        //    NodeVersion subSelectVersion = null;
        //    NodeVersionStatusHistory subSelectTopStatus = null;
        //    NodeVersion outerVersionSelect = null;
        //    NodeVersionStatusType subSelectStatusType = null;

        //    // First define the subselection of the top 1 version items when joined and sorted by version status history in date-descending order
        //    // We also add a clause to say "where the outer selected version's node id equals the subselected node id" since it's selecting the top 1
        //    // so we want it to be the 1 latest-date item relevant for each row of the outer select
        //    var subSelectTopStatusByDate = QueryOver.Of(() => subSelectTopStatus)
        //        //.Where(() => subSelectTopStatus.NodeVersion.Id == outerVersionSelect.Id);
        //        .JoinQueryOver(() => subSelectTopStatus.NodeVersion, () => subSelectVersion)
        //        .JoinQueryOver(() => subSelectVersion.Node, () => node)
        //        .Where(() => subSelectTopStatus.NodeVersion.Id == subSelectVersion.Id)
        //        .And(() => outerVersionSelect.Node.Id == node.Id);

        //    int takeCount = limitToLatestRevision ? 1 : 999;

        //    // Now we need to add a filter for the revision status type, if one was supplied
        //    QueryOver<NodeVersionStatusHistory> subSelectTopStatusByDateWithFilter = null;
        //    QueryOver<NodeVersionStatusHistory> excludeNegatingStatusses = null;
        //    if (revisionStatus != null)
        //    {
        //        var statusAlias = revisionStatus.Alias;

        //        if (revisionStatus.NegatedByTypes.Any())
        //        {
        //            NodeVersionStatusHistory negateHistory = null;
        //            NodeVersionStatusType negateType = null;
        //            NodeVersion negateVersion = null;
        //            var negatingAliases = revisionStatus.NegatedByTypes.Select(x => x.Alias).ToArray();

        //            //var first = negatingAliases.First();
        //            excludeNegatingStatusses = QueryOver.Of(() => negateHistory)
        //                .JoinAlias(() => negateHistory.NodeVersionStatusType, () => negateType)
        //                .JoinAlias(() => negateHistory.NodeVersion, () => negateVersion)
        //                .Where(() => negateType.Alias.IsIn(negatingAliases))
        //                .And(() => outerVersionSelect.Node == negateVersion.Node)
        //                .Select(Projections.SqlFunction("coalesce", NHibernateUtil.DateTime, Projections.Max<NodeVersionStatusHistory>(x => x.Date), new ConstantProjection(new DateTime(1981, 8, 1))));
        //        }

        //        var subSelectBuilder = subSelectTopStatusByDate
        //            .And(() => subSelectStatusType.Alias == statusAlias)
        //            .JoinQueryOver(x => subSelectTopStatus.NodeVersionStatusType, () => subSelectStatusType)
        //            .OrderBy(() => subSelectTopStatus.Date).Desc;

        //        if (excludeNegatingStatusses != null)
        //        {
        //            // Yeah, I know, horrible to check Sql dialect when generating query, but Nh's dialect support doesn't allow a provider-based
        //            // way of telling whether the db engine supports / requires "all" to be prefix before a subquery when doing an operation.
        //            // e.g. SqlCe requires "blah > all (select max(blah) from blah)", SqlServer doesn't mind, SQLite doesn't support it
        //            if (RequiresAllForGtSubquery())
        //                subSelectBuilder = subSelectBuilder.WithSubquery.WhereProperty(() => subSelectTopStatus.Date).GtAll(excludeNegatingStatusses);
        //            else
        //                subSelectBuilder = subSelectBuilder.WithSubquery.WhereProperty(() => subSelectTopStatus.Date).Gt(excludeNegatingStatusses);
        //        }

        //        subSelectTopStatusByDateWithFilter = subSelectBuilder.Select(x => subSelectTopStatus.NodeVersion.Id).Take(takeCount);
        //        // We have to include a Take here for compatibility with SqlServerCe
        //    }
        //    else
        //    {
        //        subSelectTopStatusByDateWithFilter = subSelectTopStatusByDate
        //            .OrderBy(() => subSelectTopStatus.Date).Desc
        //            .Select(x => subSelectTopStatus.NodeVersion.Id).Take(takeCount);
        //        // We have to include a Take here for compatibility with SqlServerCe
        //    }

        //    NodeVersionStatusHistory outerHistoryForSort = null;
        //    IQueryOver<NodeVersion, NodeVersionStatusHistory> outerQuery = NhSession.QueryOver<NodeVersion>(
        //        () => outerVersionSelect)
        //        //.Fetch(x => x.AttributeSchemaDefinition).Eager
        //        .Fetch(x => x.Attributes).Eager
        //        // We load these eagerly rather than in a Future to avoid a separate query
        //        .Fetch(x => x.Node).Eager
        //        // There's a 1-m mapping between Node-NodeVersion so safe to load this with a join too rather than with a future
        //        .Inner.JoinQueryOver(x => outerHistoryForSort.NodeVersionStatusType, () => subSelectStatusType)
        //        .Inner.JoinQueryOver(x => outerVersionSelect.NodeVersionStatuses, () => outerHistoryForSort);

        //    NodeVersion innerVersion = null;
        //    NodeVersionStatusHistory innerHistory = null;
        //    NodeVersionStatusType innerType = null;
        //    var buildInnerHistorySubQuery = QueryOver.Of<NodeVersionStatusHistory>(() => innerHistory)
        //        .JoinQueryOver(() => innerHistory.NodeVersion, () => innerVersion)
        //        .Where(() => innerVersion.Node == outerVersionSelect.Node);

        //    if (revisionStatus != null)
        //    {
        //        var statusAlias = revisionStatus.Alias;
        //        buildInnerHistorySubQuery = buildInnerHistorySubQuery
        //            .JoinAlias(() => innerHistory.NodeVersionStatusType, () => innerType)
        //            .And(() => innerType.Alias == statusAlias);

        //        // Yeah, I know, horrible to check Sql dialect when generating query, but Nh's dialect support doesn't allow a provider-based
        //        // way of telling whether the db engine supports / requires "all" to be prefix before a subquery when doing an operation.
        //        // e.g. SqlCe requires "blah > all (select max(blah) from blah)", SqlServer doesn't mind, SQLite doesn't support it
        //        if (excludeNegatingStatusses != null)
        //            if (RequiresAllForGtSubquery())
        //            {
        //                buildInnerHistorySubQuery = buildInnerHistorySubQuery.WithSubquery.WhereProperty(() => innerHistory.Date).GtAll(excludeNegatingStatusses);
        //            }
        //            else
        //            {
        //                buildInnerHistorySubQuery = buildInnerHistorySubQuery.WithSubquery.WhereProperty(() => innerHistory.Date).Gt(excludeNegatingStatusses);
        //            }
        //    }

        //    var subQueryOfHistory = buildInnerHistorySubQuery.Select(Projections.Max(() => innerHistory.Date));

        //    var getVersionIds = NhSession.QueryOver<NodeVersionStatusHistory>(() => innerHistory)
        //        .JoinAlias(() => innerHistory.NodeVersion, () => outerVersionSelect)
        //        .OrderBy(() => innerHistory.Date).Desc;

        //    if (nodeIds != null && nodeIds.Any()) getVersionIds = getVersionIds.And(() => outerVersionSelect.Node.Id.IsIn(nodeIds));

        //    if (limitToLatestRevision) getVersionIds = getVersionIds.WithSubquery.WhereProperty(() => innerHistory.Date).In(subQueryOfHistory);

        //    var versionIds = getVersionIds
        //        .Select(x => x.NodeVersion.Id)
        //        .List<Guid>()
        //        .Distinct()
        //        .ToArray();

        //    // TODO: detect if more than 2000 ids returned and add it to a subquery instead
        //    var queryToReturn = NhSession.QueryOver<NodeVersion>(() => outerVersionSelect)
        //        .Fetch(x => x.Attributes).Eager
        //        .Fetch(x => x.Node).Eager
        //        .Where(() => outerVersionSelect.Id.IsIn(versionIds));

        //    if (sortClauses == null) sortClauses = Enumerable.Empty<SortClause>();

        //    Attribute attribAlias = null;
        //    AttributeDateValue attributeDateValue = null;
        //    AttributeDecimalValue attributeDecimalValue = null;
        //    AttributeIntegerValue attributeIntegerValue = null;
        //    AttributeStringValue attributeStringValue = null;
        //    AttributeLongStringValue attributeLongStringValue = null;
        //    queryToReturn = queryToReturn
        //        .Left.JoinAlias(() => outerVersionSelect.Attributes, () => attribAlias) // Using a left join identifies to Nh that it can reuse the loaded Attributes because Nh considers them to be unaffected by the query, otherwise it issues another select from accessing NodeVersion.Attributes
        //        .Left.JoinAlias(() => attribAlias.AttributeStringValues, () => attributeStringValue)
        //        .Left.JoinAlias(() => attribAlias.AttributeLongStringValues, () => attributeLongStringValue)
        //        .Left.JoinAlias(() => attribAlias.AttributeIntegerValues, () => attributeIntegerValue)
        //        .Left.JoinAlias(() => attribAlias.AttributeDecimalValues, () => attributeDecimalValue)
        //        .Left.JoinAlias(() => attribAlias.AttributeDateValues, () => attributeDateValue);

        //    return queryToReturn;
        //}



        public IEnumerable<NodeVersion> GetNodeVersionsByStatusDesc(Guid[] nodeIds = null, RevisionStatusType revisionStatus = null, bool limitToLatestRevision = true)
        {
                var query2 = GenerateVersionedQuery2(nodeIds, revisionStatus, limitToLatestRevision);
                // We execute via a call to Future rather than just List so that other multi-criterias in query2 will be honoured
                var nv2 = query2.Future().ToList();
                return nv2.Distinct();
        }

        /// <summary>
        /// Adds the small attribute values to the current session as a "future" or multi-criteria.
        /// Strings and LongStrings are excluded as a trade-off between cartesian product size (string or longstring value * number of attributes)
        /// vs the cost of obtaining a sink for sending a new Sql statement.
        /// </summary>
        /// <param name="versionIds">The version ids.</param>
        public void AddSmallAttributeValueFuturesToSession(Guid[] versionIds)
        {
            Attribute attribAlias = null;
            AttributeIntegerValue integerLoader = null;
            NhSession.QueryOver(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeIntegerValues, () => integerLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            AttributeDecimalValue decimalLoader = null;
            NhSession.QueryOver(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeDecimalValues, () => decimalLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            AttributeDateValue dateLoader = null;
            NhSession.QueryOver(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeDateValues, () => dateLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();
        }

        public void AddAttributeValueFuturesToSession(Guid[] versionIds)
        {
            Attribute attribAlias = null;
            Attribute aliasForString = null;
            AttributeStringValue stringsLoader = null;
            var strings = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeStringValues, () => stringsLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            //Attribute aliasForLongString = null;
            //AttributeStringValue longStringsLoader = null;
            //var longStrings = NhSession.QueryOver<Attribute>(() => attribAlias)
            //    .Left.JoinAlias(() => attribAlias.AttributeLongStringValues, () => longStringsLoader)
            //    .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
            //    .Future<Attribute>();

            Attribute aliasForInteger = null;
            AttributeIntegerValue integerLoader = null;
            var integers = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeIntegerValues, () => integerLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            Attribute aliasForDecimal = null;
            AttributeDecimalValue decimalLoader = null;
            var decimals = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeDecimalValues, () => decimalLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();

            Attribute aliasForDate = null;
            AttributeDateValue dateLoader = null;
            var dates = NhSession.QueryOver<Attribute>(() => attribAlias)
                .Left.JoinAlias(() => attribAlias.AttributeDateValues, () => dateLoader)
                .Where(() => attribAlias.NodeVersion.Id.IsIn(versionIds))
                .Future<Attribute>();
        }

        public void RemoveRelation(IRelationById item, AbstractScopedCache repositoryScopedCache)
        {
            var sessionIdAsString = GetSessionId().ToString("n");
            using (DisposableTimer.TraceDuration<NhSessionHelper>("In RemoveRelation for session " + sessionIdAsString, "End RemoveRelation for session " + sessionIdAsString))
            {
                var relationType = GetOrCreateNodeRelationType(item.Type.RelationName, repositoryScopedCache);

                // Nh should handle this for us but got an error with mappings atm in SqlCe (APN 09/11/11)
                // Clear session cache to make sure it loads all of the tags in order to delete them all
                NhSession.Flush();

                // Clear the repository cache of this relation in case one has been added and then removed in the same unit of work
                var cacheKey = GenerateCacheKeyForRelation(item, relationType);
                repositoryScopedCache.InvalidateItems(cacheKey);

                var sourceIdValue = (Guid)item.SourceId.Value;
                var destinationIdValue = (Guid)item.DestinationId.Value;

                var existingRelation = GetDbRelation(relationType.Alias, sourceIdValue, destinationIdValue).ToArray();

                if (existingRelation.Any())
                {
                    existingRelation.ForEach(x => NhSession.Delete(x));
                }
            }
        }

        private static string GenerateCacheKeyForRelation(IRelationById item, NodeRelationType nodeRelationType)
        {
            return GenerateCacheKeyForRelation(nodeRelationType, item.SourceId, item.DestinationId);
        }

        private static string GenerateCacheKeyForRelation(NodeRelationType nodeRelationType, HiveId sourceId, HiveId destinationId)
        {
            return "NodeRelation-" + sourceId.Value + "|" + destinationId.Value + "|" + nodeRelationType.Alias;
        }

        public void AddRelation(IReadonlyRelation<IRelatableEntity, IRelatableEntity> item, AbstractScopedCache repositoryScopedCache)
        {
            var sessionIdAsString = GetSessionId().ToString("n");
            using (DisposableTimer.TraceDuration<NhSessionHelper>("In AddRelation for session " + sessionIdAsString, "End AddRelation for session " + sessionIdAsString))
            {
                // Get the source and destination items from the Nh session
                var sourceId = (Guid)item.SourceId.Value;
                var destinationId = (Guid)item.DestinationId.Value;

                if (sourceId == Guid.Empty || destinationId == Guid.Empty)
                {
                    string extraMessage = String.Empty;
                    if (sourceId == Guid.Empty) extraMessage = "Source has no id";
                    if (destinationId == Guid.Empty) extraMessage += "Destination has no id";
                    throw new InvalidOperationException(
                        "Some ids missing when trying to add a relation. Before adding a relation, you must call AddOrUpdate with those items (to generate Ids) or provide ids directly.\n{2}"
                            .InvariantFormat(item.SourceId, item.DestinationId, extraMessage));
                }

                var sourceNode = NhSession.Get<Node>(sourceId);
                var destNode = NhSession.Get<Node>(destinationId);

                // Check the Nh session is already aware of the items
                if (sourceNode == null || destNode == null)
                {
                    string extraMessage = String.Empty;
                    if (sourceNode == null) extraMessage = "Source {0} cannot be found.\n".InvariantFormat(item.SourceId.Value);
                    if (destNode == null) extraMessage += "Destination {0} cannot be found.".InvariantFormat(item.DestinationId.Value);
                    throw new InvalidOperationException(
                        "Before adding a relation between source {0} and destination {1}, you must call AddOrUpdate with those items or they must already exist in the datastore.\n{2}"
                            .InvariantFormat(item.SourceId, item.DestinationId, extraMessage));
                }

                //var checkExists = sourceNode.OutgoingRelations.FirstOrDefault(x => x.NodeRelationType.Alias == item.Type.RelationName)
                //    ??
                //    destNode.IncomingRelations.FirstOrDefault(x => x.NodeRelationType.Alias == item.Type.RelationName);

                //if (checkExists == null)
                //{
                //    var relationType = GetOrCreateNodeRelationType(item.Type.RelationName, repositoryScopedCache);
                //    checkExists = new NodeRelation { StartNode = sourceNode, EndNode = destNode, NodeRelationType = relationType, Ordinal = item.Ordinal };
                //    sourceNode.OutgoingRelations.Add(checkExists);
                //    destNode.IncomingRelations.Add(checkExists);
                //    NhSession.SaveOrUpdate(checkExists);
                //    NhSession.Merge(sourceNode);
                //    NhSession.Merge(destNode);
                //    NhSession.Flush();
                //}
                //else
                //{
                //    checkExists.Ordinal = item.Ordinal;
                //}

                // Try to load an existing relation of the same type between the two
                var relationType = GetOrCreateNodeRelationType(item.Type.RelationName, repositoryScopedCache);

                NodeRelation relationToReturn = NhSession
                    .QueryOver<NodeRelation>()
                    .Where(x => x.StartNode.Id == sourceId && x.EndNode.Id == destinationId && x.NodeRelationType == relationType)
                    .Cacheable()
                    .SingleOrDefault();

                // Avoid a duplicate by checking if one already exists
                if (relationToReturn != null)
                {
                    // Make sure existing relation has ordinal
                    relationToReturn.Ordinal = item.Ordinal;
                }
                else
                {
                    // Create a new relation
                    relationToReturn = new NodeRelation { StartNode = sourceNode, EndNode = destNode, NodeRelationType = relationType, Ordinal = item.Ordinal };
                    relationToReturn = NhSession.Merge(relationToReturn) as NodeRelation;
                }

                // Ensure metadata correct on existing or new entity
                CreateAndAddRelationTags(item, relationToReturn);
            }
        }

        private static void CreateAndAddRelationTags(IReadonlyRelation<IRelatableEntity, IRelatableEntity> incomingRelation, NodeRelation dbRelation)
        {
            dbRelation.NodeRelationTags.Clear();
            var newRelationMetadata = incomingRelation.MetaData.Select(x => new NodeRelationTag() { Name = x.Key, Value = x.Value, NodeRelation = dbRelation });
            newRelationMetadata.ForEach(x => dbRelation.NodeRelationTags.Add(x));
        }

        public void RemoveRelationsBiDirectional(Node node)
        {
            // For each relation, we need to remove the relation for the related node otherwise we get exceptions when deleting 
            // because of the way that Cascades are setup: MergeSaveAllDeleteOrphan.
            // If we don't do this we will get the exception that "deleted object would be re-saved by cascade" and this is because
            // the node on the other end of the relation is being saved as well because of our cascade options... 
            // To me, it would make more sense to change the cascade options to something else so that when you save a node it 
            // doesn't go re-save every other node that it's related to, however at this point in time it seems that we must have 'merge'
            // at least enabled but we also need delete and FluentNHibernate currently only has MergeSaveAllDeleteOrphan... perhaps it 
            // would be better to just have MergeDeleteOrphan ??
            foreach (var r in node.OutgoingRelations)
                r.EndNode.IncomingRelations.RemoveAll(x => x.StartNode.Id == node.Id);
            foreach (var r in node.OutgoingRelationCaches)
                r.EndNode.IncomingRelationCaches.RemoveAll(x => x.StartNode.Id == node.Id);
            foreach (var r in node.IncomingRelations)
                r.StartNode.OutgoingRelations.RemoveAll(x => x.EndNode.Id == node.Id);
            foreach (var r in node.IncomingRelationCaches)
                r.StartNode.OutgoingRelationCaches.RemoveAll(x => x.EndNode.Id == node.Id);

            node.ClearAllRelationsWithProxy();
        }

        public void MapAndMerge(AbstractEntity entity, MappingEngineCollection mappers)
        {
            using (DisposableTimer.TraceDuration<NhSessionHelper>("Start MapAndMerge for entity " + entity.Id, "End MapAndMerge"))
            {
                var rdbmsEntity = mappers.MapToIntent<IReferenceByGuid>(entity);

                // Track ID generation on the Rdbms object so that it can be pinged to the AbstractEntity upon Save/Update commit
                rdbmsEntity = NhSession.Merge(rdbmsEntity) as IReferenceByGuid;

                //InnerDataContext.NhibernateSession.SaveOrUpdate(rdbmsEntity);
                mappers.Map(rdbmsEntity, entity, rdbmsEntity.GetType(), entity.GetType());
            }
        }

        public void MapAndMerge<T>(Revision<T> entity, MappingEngineCollection mappers) where T : class, IVersionableEntity
        {
            HiveId hiveId = entity.MetaData != null ? entity.MetaData.Id : HiveId.Empty;
            HiveId entityId = entity.Item != null ? entity.Item.Id : HiveId.Empty;
            using (DisposableTimer.TraceDuration<NhSessionHelper>("Start MapAndMerge for revision " + hiveId + " entity " + entityId, "End MapAndMerge"))
            {
                var rdbmsEntity = mappers.MapToIntent<IReferenceByGuid>(entity);

                // Track ID generation on the Rdbms object so that it can be pinged to the AbstractEntity upon Save/Update commit
                rdbmsEntity = NhSession.Merge(rdbmsEntity) as IReferenceByGuid;

                // 16th Jan 12 (APN) NH is not flushing if the above merged entity is queried before the transaction is committed, despite
                // the flushmode being Auto. So, explicit call to Flush here pending a bugfix/ better solution
                NhSession.Flush();

                //InnerDataContext.NhibernateSession.SaveOrUpdate(rdbmsEntity);
                mappers.Map(rdbmsEntity, entity, rdbmsEntity.GetType(), entity.GetType());
            }
        }

        public RelationById PerformFindRelation(HiveId sourceId, HiveId destinationId, RelationType relationType)
        {
            if (sourceId.Value.Type != HiveIdValueTypes.Guid || destinationId.Value.Type != HiveIdValueTypes.Guid)
                return null;

                // Reference the values here because otherwise it gets lost in NH's query cache
                var sourceValue = (Guid)sourceId.Value;
                var destValue = (Guid)destinationId.Value;
                var relationName = relationType.RelationName;

                var firstRelation = GetDbRelation(relationName, sourceValue, destValue).FirstOrDefault();

                return firstRelation != null ? MapNodeRelation(firstRelation) : null;
        }

        private IEnumerable<NodeRelation> GetDbRelation(string relationName, Guid sourceValue, Guid destValue)
        {
            return NhSession.QueryOver<NodeRelation>()
                .Where(x => x.StartNode.Id == sourceValue)
                .And(x => x.EndNode.Id == destValue)
                .Fetch(x => x.StartNode).Lazy
                .Fetch(x => x.EndNode).Lazy
                .Fetch(x => x.NodeRelationTags).Eager
                .Fetch(x => x.NodeRelationType).Eager
                .JoinQueryOver(x => x.NodeRelationType)
                .Where(x => x.Alias == relationName)
                .Cacheable()
                .List()
                .Distinct(); // This query generates a cartesian product of the NodeRelationTags and NH 'handily' gives us x * NodeRelations so can't do a Take(1)
        }

        public IEnumerable<RelationById> PerformGetParentRelations(HiveId childId, RelationType relationType = null)
        {
            if (childId.Value.Type != HiveIdValueTypes.Guid) return Enumerable.Empty<RelationById>();
            var value = (Guid)childId.Value;

            //// Experiment (disabled for now due to errors with NH not returning fresh results when querying for relations in the same session)
            //// Scenario:
            //// In a loop of 5 items, the parent is requested. The parent is always the same for these 5 items
            //// but since the query is "get relation where end node == current id" they are different
            //// queries and so miss the cache.
            //// However, since NH can fetch collections in batches, here instead of issuing a direct query for the relation
            //// we instead get the Node and then access the IncomingRelations collection and rely on NH's collection
            //// loader (and cache) to reduce the number of queries for us
            //var node = NhSession.Get<Node>(value, LockMode.None);
            //return node.IncomingRelations
            //    .Where(x => relationType == null || x.NodeRelationType.Alias == relationType.RelationName)
            //    .Select(MapNodeRelation).ToList();



                // Reference the value here because otherwise it gets lost in NH's query cache

                var query = NhSession.QueryOver<NodeRelation>()
                    .Where(x => x.EndNode.Id == value)
                    .Fetch(x => x.StartNode).Eager
                    .Fetch(x => x.EndNode).Lazy
                    .Fetch(x => x.NodeRelationTags).Eager
                    .Fetch(x => x.NodeRelationType).Eager;

                if (relationType != null)
                {
                    var relationName = relationType.RelationName;
                    var nodeRelations = query
                        .JoinQueryOver<NodeRelationType>(x => x.NodeRelationType)
                        .Where(x => x.Alias == relationName)
                        .Cacheable()
                        .List()
                        .Distinct();
                    return nodeRelations.Select(MapNodeRelation).ToList();
                }

                var relations = query
                    .Cacheable()
                    .List()
                    .Distinct();

                return relations.Select(MapNodeRelation).ToList();
        }

        public IEnumerable<RelationById> PerformGetChildRelations(HiveId parentId, RelationType relationType = null)
        {
            if (parentId.Value.Type != HiveIdValueTypes.Guid) return Enumerable.Empty<RelationById>();

                var value = (Guid)parentId.Value;

                //// Experiment (disabled for now due to errors with NH not returning fresh results when querying for relations in the same session)
                //// Scenario:
                //// In a loop of 5 items, the parent is requested. The parent is always the same for these 5 items
                //// but since the query is "get relation where end node == current id" they are different
                //// queries and so miss the cache.
                //// However, since NH can fetch collections in batches, here instead of issuing a direct query for the relation
                //// we instead get the Node and then access the IncomingRelations collection and rely on NH's collection
                //// loader (and cache) to reduce the number of queries for us
                //var node = NhSession.Get<Node>(value, LockMode.None);
                //return node.OutgoingRelations
                //    .Where(x => relationType == null || x.NodeRelationType.Alias == relationType.RelationName)
                //    .Select(MapNodeRelation).ToList();







                // Reference the value here because otherwise it gets lost in NH's query cache

                var query = NhSession.QueryOver<NodeRelation>()
                    .Where(x => x.StartNode.Id == value)
                    .Fetch(x => x.StartNode).Lazy
                    .Fetch(x => x.EndNode).Eager
                    .Fetch(x => x.NodeRelationTags).Eager
                    .Fetch(x => x.NodeRelationType).Eager;

                if (relationType != null)
                {
                    var relationName = relationType.RelationName;
                    var listWithType = query.JoinQueryOver<NodeRelationType>(x => x.NodeRelationType)
                        .Where(x => x.Alias == relationName)
                        .Cacheable()
                        .List()
                        .Distinct();
                    return listWithType.Select(MapNodeRelation);
                }

                var list = query
                    .Cacheable()
                    .List()
                    .Distinct();

                return list.Select(MapNodeRelation);
        }

        public IEnumerable<RelationById> PerformGetBranchRelations(HiveId siblingId, RelationType relationType = null)
        {
            if (siblingId.Value.Type != HiveIdValueTypes.Guid) return Enumerable.Empty<RelationById>();

                var value = (Guid)siblingId.Value;

                //    // Experiment (disabled for now due to errors with NH not returning fresh results when querying for relations in the same session)
            //    // Scenario:
            //    // In a loop of 5 items, the parent is requested. The parent is always the same for these 5 items
            //    // but since the query is "get relation where end node == current id" they are different
            //    // queries and so miss the cache.
            //    // However, since NH can fetch collections in batches, here instead of issuing a direct query for the relation
            //    // we instead get the Node and then access the IncomingRelations collection and rely on NH's collection
            //    // loader (and cache) to reduce the number of queries for us
            //    var node = NhSession.Get<Node>(value, LockMode.None);

            //return node
            //    .IncomingRelations
            //    .Where(x => relationType == null || x.NodeRelationType.Alias == relationType.RelationName)
            //    .SelectMany(x => x.StartNode.OutgoingRelations.Where(y => relationType == null || y.NodeRelationType.Alias == relationType.RelationName))
            //    .Select(MapNodeRelation)
            //    .ToList();


                // Reference the value here because otherwise it gets lost in NH's query cache

                var query = NhSession.QueryOver<NodeRelation>()
                   .Fetch(x => x.StartNode).Lazy
                   .Fetch(x => x.EndNode).Lazy
                   .Fetch(x => x.NodeRelationTags).Eager
                   .Fetch(x => x.NodeRelationType).Eager;

                var parentQuery = QueryOver.Of<NodeRelation>().Where(x => x.EndNode.Id == value);

                if (relationType != null)
                {
                    var copyRelationName = relationType.RelationName;

                    var parentQueryWithType = parentQuery
                        .JoinQueryOver(x => x.NodeRelationType)
                        .Where(x => x.Alias == copyRelationName);

                    return query.WithSubquery.WhereProperty(x => x.StartNode.Id).In(parentQueryWithType.Select(x => x.StartNode.Id).Take(1))
                        .CacheRegion("Relations").Cacheable().List().Select(MapNodeRelation);
                }
                else
                {
                    return query.WithSubquery.WhereProperty(x => x.StartNode.Id).In(parentQuery.Select(x => x.StartNode.Id).Take(1))
                        .CacheRegion("Relations").Cacheable().List().Select(MapNodeRelation);
                }
        }

        private class SimpleRelation
        {
            public Guid StartNodeId { get; set; }
            public Guid EndNodeId { get; set; }
            public int Ordinal { get; set; }
            public RelationMetaDatum[] NodeRelationTags { get; set; }
        }

        private static RelationById MapNodeRelation(SimpleRelation nodeRelation, string aliasName)
        {
            return new RelationById(
                new HiveId(nodeRelation.StartNodeId),
                new HiveId(nodeRelation.EndNodeId),
                new RelationType(aliasName),
                nodeRelation.Ordinal,
                nodeRelation.NodeRelationTags);
        }

        private RelationById MapNodeRelation(NodeRelation nodeRelation)
        {
            IRelatableEntity startMapped;
            IRelatableEntity endMapped;

            var nodeRelationTags = MapRelationTags(nodeRelation);

            return new RelationById(new HiveId(nodeRelation.StartNode.Id), new HiveId(nodeRelation.EndNode.Id), new RelationType(nodeRelation.NodeRelationType.Alias), nodeRelation.Ordinal, nodeRelationTags);

            if (nodeRelation.StartNode is AttributeSchemaDefinition)
            {
                startMapped = FrameworkContext.TypeMappers.MapToIntent<EntitySchema>(nodeRelation.StartNode);
            }
            else
            {
                startMapped =
                    FrameworkContext.TypeMappers.Map<TypedEntity>(
                        nodeRelation.StartNode.NodeVersions.OrderByDescending(x => x.DateCreated).FirstOrDefault());
            }

            if (nodeRelation.EndNode is AttributeSchemaDefinition)
            {
                endMapped = FrameworkContext.TypeMappers.MapToIntent<EntitySchema>(nodeRelation.EndNode);
            }
            else
            {
                endMapped =
                    FrameworkContext.TypeMappers.Map<TypedEntity>(
                        nodeRelation.EndNode.NodeVersions.OrderByDescending(x => x.DateCreated).FirstOrDefault());
            }

            //TODO: Need to move to RdbmsModelMapper but doesn't seem to be wired up at the moment
            var nodeRelations =
                nodeRelation.NodeRelationTags.Select(x => new RelationMetaDatum(x.Name, x.Value)).ToArray();

            return new Relation(new RelationType(nodeRelation.NodeRelationType.Alias), startMapped, endMapped,
                                nodeRelation.Ordinal, nodeRelations);
        }

        private static RelationMetaDatum[] MapRelationTags(NodeRelation nodeRelation)
        {
            var nodeRelationTags =
                nodeRelation.NodeRelationTags.Select(x => new RelationMetaDatum(x.Name, x.Value)).ToArray();
            return nodeRelationTags;
        }

        #region Overrides of DisposableObject

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            NhEventListeners.RemoveNodeIdHandler(this);
        }

        #endregion

        public class VersionAndSchemaId : AbstractEquatableObject<VersionAndSchemaId>
        {
            public Guid NodeId { get; set; }
            public Guid VersionId { get; set; }
            public Guid SchemaId { get; set; }

            /// <summary>
            /// Gets the natural id members.
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
            {
                yield return this.GetPropertyInfo(x => x.NodeId);
                yield return this.GetPropertyInfo(x => x.VersionId);
                yield return this.GetPropertyInfo(x => x.SchemaId);
            }
        }

        public IEnumerable<NodeVersion> GetNodeVersionsInBatches(IEnumerable<VersionAndSchemaId> idPairs)
        {
            var batchSchemaLoader = new List<AttributeSchemaDefinition>();
            var schemaIds = idPairs.Select(x => x.SchemaId).Distinct().ToArray();
            var nodeIds = idPairs.Select(x => x.NodeId).Distinct().ToArray();

            // Load the schema now (the definitions and groups will be loaded in batches according to 
            // the batch size set in the classmapping)
            NhSession.QueryOver<AttributeSchemaDefinition>()
                .Fetch(x => x.AttributeDefinitionGroups).Lazy
                .Fetch(x => x.AttributeDefinitions).Lazy
                .Where(x => x.Id.IsIn(schemaIds))
                .Cacheable()
                .List();

            var idsForBatching = idPairs.Distinct().ToArray().InGroupsOf(200);
            var batchLoader = new List<NodeVersion>();

            foreach (var batch in idsForBatching)
            {
                var thisIdBatch = batch.ToArray();
                var versionIds = thisIdBatch.Select(x => x.VersionId).ToArray();
                var nodeIdBatch = thisIdBatch.Select(x => x.NodeId).ToArray();

                AddSmallAttributeValueFuturesToSession(versionIds);

                NodeVersion version = null;
                NodeVersionStatusHistory history = null;
                var thisBatch = NhSession.QueryOver<NodeVersion>(() => version)
                    .Fetch(x => x.NodeVersionStatuses).Lazy // NodeVersionStatusses are batch-loaded by NH in a single query, so can avoid a cartesian by excluding them here and waiting for the mapping to cause them to load in a batch later
                    .Fetch(x => x.Attributes).Lazy // The Attribute mapping allows for batch-loading, so lazy-load Attributes as we can't load them in the multicriteria / future without joining to NodeVersion anyway and repeating the rows
                    .Fetch(x => x.Node).Eager
                    .Where(x => x.Id.IsIn(versionIds))
                    .Future()
                    .ToList().Distinct();

                batchLoader.AddRange(thisBatch);
            }

            // Ensure we return in the same order
            return idPairs.Select(x => batchLoader.Single(y => y.Id == x.VersionId));
        }

        public IEnumerable<NodeVersion> GetNodeVersionsInBatches(IEnumerable<Guid> matchingIds)
        {
            var batchLoader = new List<NodeVersion>();
            var batchedIds = matchingIds.InGroupsOf(200);
            foreach (var batchedId in batchedIds)
            {
                var versionIds = batchedId.ToArray();
                AddAttributeValueFuturesToSession(versionIds);

                var thisBatch = NhSession.QueryOver<NodeVersion>()
                    .Fetch(x => x.Attributes).Lazy
                    .Where(x => x.Id.IsIn(versionIds))
                    .Future()
                    .ToList().Distinct();

                batchLoader.AddRange(thisBatch);
            }
            // Ensure we return in the same order
            return matchingIds.Select(x => batchLoader.Single(y => y.Id == x));
        }

        public IEnumerable<NodeVersion> GetNodeVersionsInBatches(IQueryOver<AggregateNodeStatus, AggregateNodeStatus> filteredAggQuery, bool queryHasOrderings)
        {
            var matches = GetResultVersionAndSchemaIds(filteredAggQuery, queryHasOrderings);
            return GetNodeVersionsInBatches(matches);
        }

        public IEnumerable<VersionAndSchemaId> GetResultVersionAndSchemaIds(IQueryOver<AggregateNodeStatus, AggregateNodeStatus> filteredAggQuery, bool queryHasOrderings)
        {
            //var versionIds = GetResultVersionIds(filteredAggQuery, queryHasOrderings);
            NodeVersion version = null;
            var schemaIds = filteredAggQuery
                .JoinAlias(x => x.NodeVersion, () => version)
                .Select(x => version.AttributeSchemaDefinition.Id, x => x.NodeVersion.Id, x => x.Node.Id)
                .Future<object[]>().Distinct().ToList().Select(x => new[] { (Guid)x[0], (Guid)x[1], (Guid)x[2] }).ToArray();

            //var schemaIds = NhSession.QueryOver<NodeVersion>()
            //    .Where(x => x.Id.IsIn(versionIds.ToArray()))
            //    .Select(x => x.AttributeSchemaDefinition.Id, x => x.Id, x => x.Node.Id)
            //    .List<object[]>().Distinct().Select(x => new[] { (Guid)x[0], (Guid)x[1], (Guid)x[2] }).ToArray();

            var forReturn = schemaIds.Select(x => new VersionAndSchemaId()
                {
                    SchemaId = x[0],
                    VersionId = x[1],
                    NodeId = x[2]
                }).ToArray();

            return schemaIds.Select(x => forReturn.FirstOrDefault(y => y.VersionId == x[1]));
        }

        public IEnumerable<Guid> GetResultVersionIds(IQueryOver<AggregateNodeStatus, AggregateNodeStatus> filteredAggQuery, bool queryHasOrderings)
        {
            // We can't do a select-distinct in the db without also knowing for sure if there were any orderings applied to the query
            // so do the select-distinct in ram instead
            return filteredAggQuery
                    .Select(Projections.Property<AggregateNodeStatus>(x => x.NodeVersion.Id))
                    .Future<Guid>().Distinct().ToList();

            //if (!queryHasOrderings)
            //{
            //    return filteredAggQuery
            //        .Select(Projections.Distinct(Projections.Property<AggregateNodeStatus>(x => x.NodeVersion.Id)))
            //        .Future<Guid>().ToList();
            //}
            //else
            //{
            //    // Can't select-distinct without including the orderings, so do a distinct in ram
            //    return filteredAggQuery
            //        .Select(Projections.Property<AggregateNodeStatus>(x => x.NodeVersion.Id))
            //        .Future<Guid>().Distinct().ToList();
            //}
        }

        public static void RunAggregateForNodeIds(IEnumerable<Guid> nodeIds, string queryName, ISession nhSession)
        {
            // Split the ids into batches of 500 to stay well clear of the parameter limit
            var updatedBatches = nodeIds.InGroupsOf(500);

            foreach (var updatedBatch in updatedBatches)
            {
                var batchOfIds = updatedBatch.ToArray();
                if (batchOfIds.Any())
                {
                    /* 
                     * This is pretty horrible, but there you go.
                     * SqlLite has an issue where even though the named query is a "select distinct" it can
                     * still return duplicates from these named queries which causes a constraint violation
                     * when trying to insert into the AggregateNodeStatus table.
                     * I tried adding "where exists ()" to the end of the queries to avoid adding stuff that was
                     * already in the table for compatibility with other db engines, but it still didn't prevent
                     * duplicates. So for now, we check if the dialect is SqlLite, and if so, modify the named
                     * query to replace "insert into" with "insert or ignore into" which will avoid adding rows
                     * if they already exist. Given that we're also issuing deletes prior to the insertion,
                     * this clause should effectively be the same as what happens in SqlCe / SS2008 where the inner
                     * query should only have returned distincs anyway.
                     */
                    var namedQuery = nhSession.GetNamedQuery(queryName);
                    var dialect = GetDialect(nhSession.SessionFactory);
                    if (dialect is SQLiteDialect)
                    {
                        var queryString = namedQuery.QueryString.Replace("Insert Into", "insert or ignore into");
                        namedQuery = nhSession.CreateSQLQuery(queryString);
                    }

                    var sql = namedQuery
                        .SetParameterList("nodeIds", batchOfIds);

                    sql.ExecuteUpdate();
                }
            }
        }

        public static void DeleteAggregateForNodeIds(IEnumerable<Guid> deletedNodeIds, ISession nhSession)
        {
            RunAggregateForNodeIds(deletedNodeIds, "ClearAggregateNodeStatus_PerNode", nhSession);
        }
    }
}