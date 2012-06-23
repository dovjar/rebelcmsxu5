
using RebelCms.Framework.Context;
using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;

namespace RebelCms.Framework.Persistence.XmlStore.DataManagement.ReadWrite
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