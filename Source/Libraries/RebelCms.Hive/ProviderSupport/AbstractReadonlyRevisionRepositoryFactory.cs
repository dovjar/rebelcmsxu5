using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.Model.Versioning;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;

namespace RebelCms.Hive.ProviderSupport
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