using Rebel.Framework;

namespace Rebel.Hive.ProviderSupport
{
    public interface IProviderRepository<in T>
        : IReadonlyProviderRepository<T>, ICoreRepository<T> 
        where T : class, IReferenceByHiveId
    {
        IProviderTransaction Transaction { get; }
    }
}