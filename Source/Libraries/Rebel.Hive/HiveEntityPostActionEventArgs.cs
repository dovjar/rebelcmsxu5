using System;
using System.Collections.Generic;
using System.Linq;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;

namespace Rebel.Hive
{
    using Rebel.Framework.Linq.QueryModel;
    using Rebel.Framework.Persistence.Model.Versioning;

    public class HiveSchemaPostActionEventArgs : EventArgs
    {
        public HiveSchemaPostActionEventArgs(AbstractSchemaPart schemaPart, AbstractScopedCache scopedCache)
        {
            SchemaPart = schemaPart;
            ScopedCache = scopedCache;
        }

        /// <summary>
        /// Gets or sets the schema part.
        /// </summary>
        /// <value>The schema part.</value>
        public AbstractSchemaPart SchemaPart{ get; protected set; }

        /// <summary>
        /// Gets or sets the scoped cache.
        /// </summary>
        /// <value>The scoped cache.</value>
        public AbstractScopedCache ScopedCache { get; protected set; }
    }

    public class HiveEntityPostActionEventArgs : EventArgs
    {
        public HiveEntityPostActionEventArgs(IRelatableEntity entity, AbstractScopedCache scopedCache)
        {
            Entity = entity;
            ScopedCache = scopedCache;
        }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>The entity.</value>
        public IRelatableEntity Entity { get; protected set; }

        /// <summary>
        /// Gets or sets the scoped cache.
        /// </summary>
        /// <value>The scoped cache.</value>
        public AbstractScopedCache ScopedCache { get; protected set; }
    }

    public class HiveRevisionPostActionEventArgs : EventArgs
    {
        public HiveRevisionPostActionEventArgs(IRevision<IVersionableEntity> entity, AbstractScopedCache scopedCache)
        {
            Entity = entity;
            ScopedCache = scopedCache;
        }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>The entity.</value>
        public IRevision<IVersionableEntity> Entity { get; protected set; }

        /// <summary>
        /// Gets or sets the scoped cache.
        /// </summary>
        /// <value>The scoped cache.</value>
        public AbstractScopedCache ScopedCache { get; protected set; }
    }

    public class HiveQueryResultEventArgs : EventArgs
    {
        public HiveQueryResultEventArgs(object result, QueryDescription query, AbstractScopedCache scopedCache)
            : this(Enumerable.Repeat(result, 1).ToArray(), query, scopedCache)
        {
        }

        public HiveQueryResultEventArgs(IEnumerable<object> results, QueryDescription query, AbstractScopedCache scopedCache)
        {
            Results = results;
            ScopedCache = scopedCache;
            QueryDescription = query;
        }

        /// <summary>
        /// Gets or sets the results.
        /// </summary>
        /// <value>The results.</value>
        public IEnumerable<object> Results { get; protected set; }

        /// <summary>
        /// Gets or sets the query description.
        /// </summary>
        /// <value>The query description.</value>
        public QueryDescription QueryDescription { get; protected set; }

        /// <summary>
        /// Gets or sets the scoped cache.
        /// </summary>
        /// <value>The scoped cache.</value>
        public AbstractScopedCache ScopedCache { get; protected set; }
    }
}