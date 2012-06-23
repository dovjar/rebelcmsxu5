using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Hive.ProviderGrouping
{
    public interface IReadonlyRevisionRepositoryGroup<out TFilter, in T> : ICoreReadonlyRevisionRepository<T>
        where T : class, IVersionableEntity
        where TFilter : class, IProviderTypeFilter
    {
        
    }
}