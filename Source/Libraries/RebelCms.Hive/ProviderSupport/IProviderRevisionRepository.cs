using RebelCms.Framework.Persistence.Model.Versioning;

namespace RebelCms.Hive.ProviderSupport
{
    public interface IProviderRevisionRepository<in TBaseEntity>
        : IReadonlyProviderRevisionRepository<TBaseEntity>, ICoreRevisionRepository<TBaseEntity>
        where TBaseEntity : class, IVersionableEntity
    {
    }
}