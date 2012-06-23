using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;
using RebelCms.Framework.ProviderSupport;

namespace RebelCms.Framework.Persistence.XmlStore
{
    public class HiveReadProvider : AbstractHiveReadProvider
    {
        public HiveReadProvider(IHiveProviderSetup setup)
            : base(setup)
        {
        }
    }
}