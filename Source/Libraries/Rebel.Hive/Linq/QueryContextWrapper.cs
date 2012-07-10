namespace Rebel.Hive.Linq
{
    using Rebel.Framework.Linq;

    public class QueryContextWrapper<T> : AbstractQueryContext<T>
    {
        public QueryContextWrapper(IQueryableDataSource queryableDataSource) : base(queryableDataSource)
        {
        }
    }
}
