using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.ProviderSupport;

namespace Rebel.Framework.Persistence.XmlStore
{
    public class HiveReadWriteProvider : AbstractHiveReadWriteProvider
    {
        public HiveReadWriteProvider(IHiveProviderSetup setup)
            : base(setup)
        {
        }
    }
}
