using RebelCms.Framework;
using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderSupport
{
    public interface IReadonlyProviderRepository<in T>
        : ICoreReadonlyRepository<T>, IReadonlyProviderRelationsRepository
        where T : class, IReferenceByHiveId
    {
        /// <summary>
        /// Gets or sets the framework context.
        /// </summary>
        /// <value>The framework context.</value>
        IFrameworkContext FrameworkContext { get; }

        /// <summary>
        /// Gets or sets the unit-scoped cache.
        /// </summary>
        /// <value>The unit-scoped cache.</value>
        AbstractScopedCache RepositoryScopedCache { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can read.
        /// </summary>
        /// <value><c>true</c> if this instance can read; otherwise, <c>false</c>.</value>
        bool CanRead { get; }
    }
}