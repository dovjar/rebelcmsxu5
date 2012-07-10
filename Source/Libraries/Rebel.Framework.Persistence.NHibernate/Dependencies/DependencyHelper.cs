using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Hive.ProviderSupport;

namespace Rebel.Framework.Persistence.NHibernate.Dependencies
{
    public class DependencyHelper : ProviderDependencyHelper 
    {
        public DependencyHelper(NhFactoryHelper nhFactoryHelper, ProviderMetadata providerMetadata)
            : base(providerMetadata)
        {
            FactoryHelper = nhFactoryHelper;
        }

        public NhFactoryHelper FactoryHelper { get; protected set; }

        protected override void DisposeResources()
        {
            FactoryHelper.Dispose();
        }
    }
}
