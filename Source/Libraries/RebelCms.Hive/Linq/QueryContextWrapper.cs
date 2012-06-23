namespace RebelCms.Hive.Linq
{
    using RebelCms.Framework.Linq;

    public class QueryContextWrapper<T> : AbstractQueryContext<T>
    {
        public QueryContextWrapper(IQueryableDataSource queryableDataSource) : base(queryableDataSource)
        {
        }
    }
}
