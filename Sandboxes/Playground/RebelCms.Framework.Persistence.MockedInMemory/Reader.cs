using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;

namespace RebelCms.Framework.Persistence.MockedInMemory
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