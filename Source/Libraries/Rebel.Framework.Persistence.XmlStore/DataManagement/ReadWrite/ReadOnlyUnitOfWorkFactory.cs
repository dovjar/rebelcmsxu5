
using Rebel.Framework.Context;
using Rebel.Framework.DataManagement;
using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.ProviderSupport;

namespace Rebel.Framework.Persistence.XmlStore.DataManagement.ReadWrite
{
    public class ReadOnlyUnitOfWorkFactory : AbstractReadOnlyUnitOfWorkFactory
    {
        

        public override IReadOnlyUnitOfWork CreateForReading(IHiveProvider hiveProvider)
        {
            this.CheckThrowObjectDisposed(base.IsDisposed, "DemoData...ReadWriteUnitOfWorkFactory:CreateForReading");
            return new ReadOnlyUnitOfWork(hiveProvider.DataContextFactory.CreateDataContext(hiveProvider) as DataContext);
        }

    }
}