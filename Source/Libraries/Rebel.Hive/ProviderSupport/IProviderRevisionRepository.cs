using Rebel.Framework.Persistence.Model.Versioning;

namespace Rebel.Hive.ProviderSupport
{
    public interface IProviderRevisionRepository<in TBaseEntity>
        : IReadonlyProviderRevisionRepository<TBaseEntity>, ICoreRevisionRepository<TBaseEntity>
        where TBaseEntity : class, IVersionableEntity
    {
    }
}