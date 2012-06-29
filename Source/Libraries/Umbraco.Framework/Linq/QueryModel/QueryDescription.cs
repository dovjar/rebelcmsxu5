namespace Umbraco.Framework.Linq.QueryModel
{
    using System;
    using System.Collections.Generic;

    using System.Linq;

    using System.Linq.Expressions;

    using System.Text;

    using Umbraco.Framework.Linq.CriteriaGeneration.Expressions;

    using Umbraco.Framework.Linq.CriteriaTranslation;

    public class QueryDescription
    {
        protected IList<SortClause> _sortClauses;

        protected QueryDescription()
            : this(null, new FromClause(), Expression.Empty(), new List<SortClause>(), true)
        { }

        public QueryDescription(ResultFilterClause singleResultFilter, FromClause fromClause, Expression criteria, IEnumerable<SortClause> sortClauses, bool excludeOrphans = true)
        {
            From = fromClause;
            Criteria = criteria;
            _sortClauses = new List<SortClause>(sortClauses);
            ExcludeOrphans = excludeOrphans;
            ResultFilters = new List<ResultFilterClause>();
            if (singleResultFilter != null) ResultFilters.Add(singleResultFilter);
        }

        /// <summary>
        /// The query should exclude items that have no incoming nor outgoing relations
        /// </summary>
        public bool ExcludeOrphans { get; protected set; }

        public FromClause From { get; protected set; }

        [Obsolete("Must use ResultFilters instead")]
        public ResultFilterClause ResultFilter { get; protected set; }

        public IList<ResultFilterClause> ResultFilters { get; protected set; }

        // Need better name, confusing between Filter and Criteria

        public Expression Criteria { get; protected set; }

        public IEnumerable<SortClause> SortClauses
        {
            get { return _sortClauses.OrderBy(x => x.Priority); }
            protected set { _sortClauses = new List<SortClause>(value); }
        }

        public Type GetSequenceResultType()
        {
            return ResultFilters.Where(x =>
            {
                switch (x.ResultFilterType)
                {
                    case ResultFilterType.Distinct:
                    case ResultFilterType.SkipTake:
                    case ResultFilterType.Skip:
                    case ResultFilterType.Take:
                    case ResultFilterType.Sequence:
                        return true;
                    default:
                        return false;
                }
            }).Select(x => x.ResultType).FirstOrDefault();
        }
    }

    /// <summary>
    /// Represents a <see cref="QueryDescription"/> in a form that is normalised and performant for use as a dictionary or cache key.
    /// </summary>
    public struct QueryDescriptionCacheKey
    {
        private string _toString;

        public QueryDescriptionCacheKey(QueryDescription queryDescription)
        {
            CriteriaString = CriteriaStringBuilder.GetString(queryDescription.Criteria);
            _toString = null;
        }

        /// <summary>
        /// The <see cref="QueryDescription.Criteria"/> as a string.
        /// </summary>
        public readonly string CriteriaString;

        public static implicit operator string(QueryDescriptionCacheKey key)
        {
            return key.ToString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            var casted = (QueryDescriptionCacheKey)obj;
            return casted.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return _toString ?? (_toString = CriteriaString);
        }
    }

    public class CriteriaStringBuilder : AbstractCriteriaVisitor<Expression>
    {
        protected readonly StringBuilder StringBuilder = new StringBuilder();

        public static string GetString(Expression expression)
        {
            var builder = new CriteriaStringBuilder();
            builder.Visit(expression);
            return builder.StringBuilder.ToString();
        }

        #region Overrides of AbstractCriteriaVisitor<Expression<Func<StringBuilder,bool>>>

        public override Expression VisitNoCriteriaPresent()
        {
            StringBuilder.Append("no-criteria");
            return Expression.Constant(null);
        }

        public override Expression VisitBinary(BinaryExpression node)
        {
            StringBuilder.AppendFormat("\nBinary: {0} and {1}", node.Left.Type.Name, node.Right.Type.Name);

            Visit(node.Left);
            Visit(node.Right);

            return node;
        }

        public override Expression VisitSchemaPredicate(SchemaPredicateExpression node)
        {
            StringBuilder.AppendFormat("\n{0}", node);
            return node;
        }

        public override Expression VisitFieldPredicate(FieldPredicateExpression node)
        {
            StringBuilder.AppendFormat("\n{0}", node);
            return node;
        }

        #endregion
    }
}
