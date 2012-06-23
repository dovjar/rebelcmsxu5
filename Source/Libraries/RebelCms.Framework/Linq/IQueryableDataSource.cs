namespace RebelCms.Framework.Linq
{
    using System.Collections.Generic;

    using RebelCms.Framework.Context;

    using RebelCms.Framework.Linq.QueryModel;

    using RebelCms.Framework.Linq.ResultBinding;

    public interface IQueryableDataSource : IRequiresFrameworkContext
    {
        T ExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder);
        T ExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder);
        IEnumerable<T> ExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder);
    }
}