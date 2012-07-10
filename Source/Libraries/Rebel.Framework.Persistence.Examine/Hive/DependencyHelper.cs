using Rebel.Framework.Persistence.ProviderSupport._Revised;
using Rebel.Hive.ProviderSupport;

namespace Rebel.Framework.Persistence.Examine.Hive
{
    public class DependencyHelper : ProviderDependencyHelper
    {
        public ExamineHelper ExamineHelper { get; private set; }

        public DependencyHelper(ExamineHelper helper, ProviderMetadata providerMetadata)
            : base(providerMetadata)
        {
            ExamineHelper = helper;
        }

        protected override void DisposeResources()
        {
            ExamineHelper.Dispose();
        }
    }
}