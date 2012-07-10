using Rebel.Framework.Context;
using Rebel.Framework.Persistence.ProviderSupport;

namespace Rebel.Framework.Persistence.DemoData
{
    public class Manager : PersistenceManagerBase
    {
        public Manager(string @alias, IPersistenceReadWriter reader, IPersistenceReadWriter readWriter, IFrameworkContext frameworkContext)
            : base(@alias, reader, readWriter, frameworkContext)
        { }
    }
}
