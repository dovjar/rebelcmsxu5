using System.Web;

using Rebel.Framework.Context;
using Rebel.Framework.DataManagement;
using Rebel.Framework.Persistence.DataManagement;
using Rebel.Framework.Persistence.ProviderSupport;

namespace Rebel.Framework.IO
{
    public class ReadWriteUnitOfWork : AbstractReadWriteUnitOfWork
    {
        public ReadWriteUnitOfWork(AbstractDataContext dataContext)
            : base(dataContext)
        {
            Mandate.ParameterNotNull(dataContext, "dataContext");
            Mandate.ParameterCondition(dataContext is DataContext, "dataContext");


            //TODO: Is this right? Alex will know --Aaron
            
            //TODO: We need to inject some sort of ICacheProvider so that we can cache the GetFile(s) lookups occuring in the ReadWriteRepository
            _repo = new RepositoryReadWriter(dataContext as DataContext);
        }

        private readonly IRepositoryReadWriter _repo;
        
        public override void Commit()
        {
        }

        protected override void DisposeTransaction()
        {
        }

        public override IRepositoryReader ReadRepository
        {
            get { return _repo; }
        }

        public override IRepositoryReadWriter ReadWriteRepository
        {
            get { return _repo; }
        }
    }
}