namespace RebelCms.Framework.Linq.QueryModel
{
    using System;

    using System.Linq.Expressions;

    using RebelCms.Framework.Data;

    internal class QueryDescriptionBuilder : QueryDescription
    {
        public FromClause SetFromClause(string startId, HierarchyScope hierarchyScope, RevisionStatusType revisionStatus)
        {
            From = new FromClause(startId, hierarchyScope, revisionStatus);
            return From;
        }

        public ResultFilterClause SetResultFilterClause(Type resultType, ResultFilterType resultFilterType, int selectorArgument)
        {
            ResultFilter = new ResultFilterClause(resultType, resultFilterType, selectorArgument);
            return ResultFilter;
        }

        public void SetCriteria(Expression criteria)
        {
            Criteria = criteria;
        }

        public void AddSortClause(SortClause sortClause)
        {
            _sortClauses.Add(sortClause);
        }
    }
}