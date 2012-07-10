using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Hive.Caching
{
    using System.Linq.Expressions;
    using Rebel.Framework;
    using Rebel.Framework.Caching;
    using Rebel.Framework.Data;
    using Rebel.Framework.Linq.CriteriaGeneration.Expressions;
    using Rebel.Framework.Linq.QueryModel;
    using Rebel.Framework.Persistence.Model.Associations;

    public class HiveRelationCacheKey //: CacheKey<HiveRelationCacheKey>
    {
        public enum RepositoryTypes
        {
            Entity = 0,
            Schema = 2
        }

        public HiveRelationCacheKey()
        {
        }

        public HiveRelationCacheKey(RepositoryTypes repositoryType, HiveId entityId, Direction direction, RelationType relationType)
        {
            RepositoryType = repositoryType;
            EntityId = entityId;
            Direction = direction;
            RelationType = relationType;
        }

        public RepositoryTypes RepositoryType { get; set; }

        public HiveId EntityId { get; set; }

        public Direction Direction { get; set; }

        public RelationType RelationType { get; set; }
    }

    public class HiveQueryCacheKey //: CacheKey<QueryDescription>
    {
        public HiveQueryCacheKey() // Used for deserialization from Json
        {}

        public HiveQueryCacheKey(QueryDescription queryDescription)
            : this(queryDescription.From, queryDescription.ResultFilters, queryDescription.Criteria, queryDescription.ExcludeOrphans, queryDescription.SortClauses.ToArray())//, queryDescription.Criteria as FieldPredicateExpression)
        {

        }

        public HiveQueryCacheKey(FromClause fromClause, IEnumerable<ResultFilterClause> resultFilterClauses, Expression criteria, bool excludeOrphans, params SortClause[] sortClauses)
        {
            From = fromClause;
            ResultFilters = resultFilterClauses;
            Criteria = criteria;
            SortClauses = sortClauses;
            ExcludeOrphans = excludeOrphans;
        }

        public bool ExcludeOrphans { get; set; }

        public FromClause From { get; set; }

        public IEnumerable<ResultFilterClause> ResultFilters { get; set; }

        public IEnumerable<SortClause> SortClauses { get; set; }

        // TODO: FPE can de/serialize but a BinaryExpression of two or more can't, easily fixable with a custom tree but clearing cache by criteria
        // a bit of an edge case (e.g. where schema type == blah)
        //public FieldPredicateExpression Criteria { get; set; }

        // For now it's here as obviously it needs serializing in order to generate a unique cache key, but you can't deserialize it
        public Expression Criteria { get; protected set; }
    }
}
