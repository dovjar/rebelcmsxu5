using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.ProviderSupport;

namespace Rebel.Framework.Persistence.XmlStore
{
    public class HiveReadProvider : AbstractHiveReadProvider
    {
        public HiveReadProvider(IHiveProviderSetup setup)
            : base(setup)
        {
        }
    }
}