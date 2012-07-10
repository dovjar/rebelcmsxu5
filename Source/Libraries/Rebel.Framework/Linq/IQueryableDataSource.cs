namespace Rebel.Framework.Linq
{
    using System.Collections.Generic;

    using Rebel.Framework.Context;

    using Rebel.Framework.Linq.QueryModel;

    using Rebel.Framework.Linq.ResultBinding;

    public interface IQueryableDataSource : IRequiresFrameworkContext
    {
        T ExecuteScalar<T>(QueryDescription query, ObjectBinder objectBinder);
        T ExecuteSingle<T>(QueryDescription query, ObjectBinder objectBinder);
        IEnumerable<T> ExecuteMany<T>(QueryDescription query, ObjectBinder objectBinder);
    }
}