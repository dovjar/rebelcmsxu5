using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Hive.ProviderGrouping
{
    public interface IReadonlyRevisionRepositoryGroup<out TFilter, in T> : ICoreReadonlyRevisionRepository<T>
        where T : class, IVersionableEntity
        where TFilter : class, IProviderTypeFilter
    {
        
    }
}