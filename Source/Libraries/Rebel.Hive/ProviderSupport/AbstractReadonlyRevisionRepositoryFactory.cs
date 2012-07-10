using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Persistence.ProviderSupport._Revised;

namespace Rebel.Hive.ProviderSupport
{
    public abstract class AbstractReadonlyRevisionRepositoryFactory<T> 
        : AbstractReadonlyRepositoryFactory<AbstractReadonlyRevisionRepository<T>>
        where T : class, IVersionableEntity
    {
        protected AbstractReadonlyRevisionRepositoryFactory(ProviderMetadata providerMetadata, IFrameworkContext frameworkContext, ProviderDependencyHelper dependencyHelper) 
            : base(providerMetadata, frameworkContext, dependencyHelper)
        {
        }
    }
}