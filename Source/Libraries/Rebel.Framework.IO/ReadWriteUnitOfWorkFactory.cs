using System.Web;
using Rebel.Framework.Context;
using Rebel.Framework.DataManagement;
using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.ProviderSupport;

namespace Rebel.Framework.IO
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