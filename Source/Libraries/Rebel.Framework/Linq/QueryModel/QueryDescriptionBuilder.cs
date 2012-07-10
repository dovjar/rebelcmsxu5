namespace Rebel.Framework.Linq.QueryModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using Rebel.Framework.Data;

    public class QueryDescriptionBuilder : QueryDescription
    {
        public FromClause SetFromClause(HierarchyScope hierarchyScope, RevisionStatusType revisionStatus, IEnumerable<HiveId> excludeParentIds = null, IEnumerable<HiveId> excludeIds = null, params HiveId[] scopeStartIds)
        {
            excludeParentIds = excludeParentIds ?? Enumerable.Empty<HiveId>();
            excludeIds = excludeIds ?? Enumerable.Empty<HiveId>();
            From = new FromClause(scopeStartIds, excludeParentIds, excludeIds, hierarchyScope, revisionStatus);
            return From;
        }

        public ResultFilterClause AddResultFilter(Type resultType, ResultFilterType resultFilterType, int selectorArgument)
        {
            var filter = new ResultFilterClause(resultType, resultFilterType, selectorArgument);
            ResultFilters.Add(filter);
            return filter;
        }

        public ResultFilterClause AddSkipResultFilter(Type resultType, int skipCount)
        {
            var filter = new ResultFilterClause(resultType, ResultFilterType.Skip, 0)
            {
                SkipCount = skipCount
            };
            ResultFilters.Add(filter);
            return filter;
        }

        public ResultFilterClause AddTakeResultFilter(Type resultType, int takeCount)
        {
            var filter = new ResultFilterClause(resultType, ResultFilterType.Take, 0)
            {
                TakeCount = takeCount
            };
            ResultFilters.Add(filter);
            return filter;
        }

        public ResultFilterClause AddSkipTakeResultFilter(Type resultType, int skipCount, int takeCount)
        {
            var filter = new ResultFilterClause(resultType, ResultFilterType.Take, 0)
            {
                SkipCount = skipCount,
                TakeCount = takeCount
            };
            ResultFilters.Add(filter);
            return filter;
        }

        public ResultFilterClause ResetResultFilters(Type resultType, ResultFilterType resultFilterType, int selectorArgument)
        {
            ResultFilters.Clear();
            var resultFilterClause = new ResultFilterClause(resultType, resultFilterType, selectorArgument);
            ResultFilters.Add(resultFilterClause);
            return resultFilterClause;
        }

        //public ResultFilterClause SetResultFilterClause(Type resultType, ResultFilterType resultFilterType, int selectorArgument)
        //{
        //    ResultFilter = new ResultFilterClause(resultType, resultFilterType, selectorArgument);
        //    return ResultFilter;
        //}

        //public ResultFilterClause SetSkipResultFilter(Type resultType, int skipCount)
        //{
        //    ResultFilter = new ResultFilterClause(resultType, ResultFilterType.Skip, 0)
        //        {
        //            SkipCount = skipCount
        //        };
        //    return ResultFilter;
        //}

        //public ResultFilterClause SetTakeResultFilter(Type resultType, int takeCount)
        //{
        //    if (ResultFilter != null && ResultFilter.ResultFilterType == ResultFilterType.Skip)
        //    {
        //        ResultFilter.ResultFilterType = ResultFilterType.SkipTake;
        //        ResultFilter.TakeCount = takeCount;
        //    }
        //    else
        //    {
        //        ResultFilter = new ResultFilterClause(resultType, ResultFilterType.Take, 0)
        //        {
        //            TakeCount = takeCount
        //        };
        //    }
            
        //    return ResultFilter;
        //}

        //public ResultFilterClause SetSkipTakeResultFilter(Type resultType, int skipCount, int takeCount)
        //{
        //    ResultFilter = new ResultFilterClause(resultType, ResultFilterType.Take, 0)
        //    {
        //        SkipCount = skipCount,
        //        TakeCount = takeCount
        //    };
        //    return ResultFilter;
        //}

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