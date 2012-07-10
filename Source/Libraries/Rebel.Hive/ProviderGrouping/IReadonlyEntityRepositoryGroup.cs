using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model;
using Rebel.Hive.RepositoryTypes;
using System.Linq;

namespace Rebel.Hive.ProviderGrouping
{
    using Rebel.Framework.Linq;

    public interface IReadonlyEntityRepositoryGroup<out TFilter> : ICoreReadonlyRepository<TypedEntity>, IQueryable<TypedEntity>, IQueryContext<TypedEntity>, IRequiresFrameworkContext
        where TFilter : class, IProviderTypeFilter
    {
        IReadonlyRevisionRepositoryGroup<TFilter, TypedEntity> Revisions { get; }
        IReadonlySchemaRepositoryGroup<TFilter> Schemas { get; }
        IQueryContext<TypedEntity> QueryContext { get; }
    }
}