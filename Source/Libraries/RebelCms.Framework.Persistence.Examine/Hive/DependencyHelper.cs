using RebelCms.Framework.Persistence.ProviderSupport._Revised;
using RebelCms.Hive.ProviderSupport;

namespace RebelCms.Framework.Persistence.Examine.Hive
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