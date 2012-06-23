using RebelCms.Framework;
using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Hive.RepositoryTypes;
using System.Linq;

namespace RebelCms.Hive.ProviderGrouping
{
    using RebelCms.Framework.Linq;

    public interface IReadonlyEntityRepositoryGroup<out TFilter> : ICoreReadonlyRepository<TypedEntity>, IQueryable<TypedEntity>, IQueryContext<TypedEntity>, IRequiresFrameworkContext
        where TFilter : class, IProviderTypeFilter
    {
        IReadonlyRevisionRepositoryGroup<TFilter, TypedEntity> Revisions { get; }
        IReadonlySchemaRepositoryGroup<TFilter> Schemas { get; }
        IQueryContext<TypedEntity> QueryContext { get; }
    }
}