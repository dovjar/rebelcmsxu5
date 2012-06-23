using RebelCms.Framework.Context;
using RebelCms.Framework.Persistence.ProviderSupport;

namespace RebelCms.Framework.Persistence.DemoData
{
    public class Manager : PersistenceManagerBase
    {
        public Manager(string @alias, IPersistenceReadWriter reader, IPersistenceReadWriter readWriter, IFrameworkContext frameworkContext)
            : base(@alias, reader, readWriter, frameworkContext)
        { }
    }
}
