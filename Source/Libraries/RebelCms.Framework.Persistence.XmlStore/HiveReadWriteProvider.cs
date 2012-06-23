using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;
using RebelCms.Framework.ProviderSupport;

namespace RebelCms.Framework.Persistence.XmlStore
{
    public class HiveReadWriteProvider : AbstractHiveReadWriteProvider
    {
        public HiveReadWriteProvider(IHiveProviderSetup setup)
            : base(setup)
        {
        }
    }
}
