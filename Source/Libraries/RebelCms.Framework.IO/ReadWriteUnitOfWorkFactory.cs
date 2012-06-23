using System.Web;
using RebelCms.Framework.Context;
using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;

namespace RebelCms.Framework.IO
{
    public class ReadWriteUnitOfWorkFactory : AbstractReadWriteUnitOfWorkFactory
    {

        public override IReadOnlyUnitOfWork CreateForReading(IHiveProvider hiveProvider)
        {
            return new ReadWriteUnitOfWork(hiveProvider.DataContextFactory.CreateDataContext(hiveProvider));
        }

        public override IReadWriteUnitOfWork CreateForReadWriting(IHiveProvider hiveProvider)
        {
            return new ReadWriteUnitOfWork(hiveProvider.DataContextFactory.CreateDataContext(hiveProvider));
        }
    }
}