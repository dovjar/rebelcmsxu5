

using RebelCms.Framework.Context;
using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Persistence.DataManagement;
using RebelCms.Framework.Persistence.ProviderSupport;

namespace RebelCms.Framework.Persistence.XmlStore.DataManagement.ReadWrite
{
    public class ReadOnlyUnitOfWork : AbstractReadOnlyUnitOfWork
    {
        private readonly ITransaction _transaction;

        public ReadOnlyUnitOfWork(AbstractDataContext dataContext)
            : base(dataContext)
        {
            Mandate.ParameterNotNull(dataContext, "dataContext");
            Mandate.ParameterCondition(dataContext is DataContext, "dataContext");

            _repo = new RepositoryReader(dataContext as DataContext);
            _transaction = DataContext.BeginTransaction();
        }

        private readonly IRepositoryReader _repo;

        protected override void DisposeTransaction()
        {
            if (_transaction != null) _transaction.Dispose();
        }      

        public override IRepositoryReader ReadRepository
        {
            get { return _repo; }
        }
    }
}