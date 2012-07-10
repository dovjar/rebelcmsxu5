using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderSupport
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