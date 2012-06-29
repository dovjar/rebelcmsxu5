namespace Umbraco.Framework.Linq.QueryModel
{
    using System.Collections.Generic;
    using System.Linq;
    using Umbraco.Framework.Data;

    public class FromClause
    {
        public static readonly RevisionStatusType RevisionStatusNotSpecifiedType = new RevisionStatusType("not-specified", "Not Specified");
        // This is left here for backwards compilation compatibility with Hive providers that only take into account the alias
        public static readonly string RevisionStatusNotSpecified = RevisionStatusNotSpecifiedType.Alias;

        public FromClause(IEnumerable<HiveId> startIds, HierarchyScope hierarchyScope, RevisionStatusType revisionStatus, string hierarchyType = null, IEnumerable<HiveId> requiredEntityIds = null)
            : this(startIds, Enumerable.Empty<HiveId>(), Enumerable.Empty<HiveId>(), hierarchyScope, revisionStatus, hierarchyType, requiredEntityIds)
        {
        }

        public FromClause(IEnumerable<HiveId> startIds, IEnumerable<HiveId> excludeStartIds, IEnumerable<HiveId> excludeIds, HierarchyScope hierarchyScope, RevisionStatusType revisionStatus, string hierarchyType = null, IEnumerable<HiveId> requiredEntityIds = null)
        {
            ScopeStartIds = startIds;
            ExcludeEntityIds = excludeIds;
            ExcludeParentIds = excludeStartIds;
            HierarchyScope = hierarchyScope;
            RevisionStatusType = revisionStatus;
            RequiredEntityIds = requiredEntityIds ?? Enumerable.Empty<HiveId>();
            HierarchyType = hierarchyType ?? string.Empty;
        }

        public FromClause()
            : this(Enumerable.Empty<HiveId>(), Enumerable.Empty<HiveId>(), Enumerable.Empty<HiveId>(), HierarchyScope.Indeterminate, RevisionStatusNotSpecifiedType)
        { }

        public IEnumerable<HiveId> ExcludeParentIds { get; set; }
        public IEnumerable<HiveId> ScopeStartIds { get; set; }
        public HierarchyScope HierarchyScope { get; set; }
        public string HierarchyType { get; set; }

        public IEnumerable<HiveId> RequiredEntityIds { get; set; }
        public IEnumerable<HiveId> ExcludeEntityIds { get; set; }

        public string RevisionStatus
        {
            get
            {
                return (RevisionStatusType != null) ? RevisionStatusType.Alias : null;
            }
        }

        public RevisionStatusType RevisionStatusType { get; set; }
    }
}