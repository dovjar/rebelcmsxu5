using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderSupport
{
    public abstract class AbstractRevisionRepositoryFactory<T>
        : AbstractReadonlyRevisionRepositoryFactory<T>, 
          IProviderRepositoryFactory<AbstractRevisionRepository<T>, AbstractReadonlyRevisionRepository<T>>
        where T : class, IVersionableEntity
    {
        protected AbstractRevisionRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper)
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
        }

        public abstract AbstractRevisionRepository<T> GetRepository();
    }
}