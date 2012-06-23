using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Hive.ProviderGrouping
{
    public interface IRevisionRepositoryGroup<out TProviderFilter, in T> : ICoreRevisionRepository<T>
        where TProviderFilter : class, IProviderTypeFilter
        where T : class, IVersionableEntity
    {
    }
}