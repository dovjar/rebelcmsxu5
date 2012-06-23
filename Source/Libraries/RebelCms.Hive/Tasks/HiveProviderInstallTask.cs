using RebelCms.Framework.Context;
using RebelCms.Framework.Tasks;

namespace RebelCms.Hive.Tasks
{
    public abstract class HiveProviderInstallTask : ProviderInstallTask
    {
        private readonly IHiveManager _coreManager;

        protected HiveProviderInstallTask(IFrameworkContext context, IHiveManager coreManager) : base(context)
        {
            _coreManager = coreManager;
        }

        public IHiveManager CoreManager
        {
            get { return _coreManager; }
        }
    }
}
