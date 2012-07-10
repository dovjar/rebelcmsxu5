using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.ProviderSupport;

namespace Rebel.Framework.Persistence.MockedInMemory
{
    public class Reader : ReaderBase
    {
        private Reader()
            : base()
        {
        }

        public Reader(IReadRepositoryUnitOfWorkFactory unitOfWorkFactory)
            : base(unitOfWorkFactory)
        {
        }
    }
}