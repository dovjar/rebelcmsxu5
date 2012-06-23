using RebelCms.Framework.Persistence.ProviderSupport;

namespace RebelCms.Framework.IO
{
    public class HiveReadWriteProvider : AbstractHiveReadWriteProvider
    {
        public HiveReadWriteProvider(IHiveProviderSetup setup)
            : base(setup)
        {
        }
    }
}