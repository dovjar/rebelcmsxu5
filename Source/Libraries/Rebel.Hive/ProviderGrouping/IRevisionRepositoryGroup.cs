using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Hive.ProviderGrouping
{
    public interface IRevisionRepositoryGroup<out TProviderFilter, in T> : ICoreRevisionRepository<T>
        where TProviderFilter : class, IProviderTypeFilter
        where T : class, IVersionableEntity
    {
    }
}