using System;
using System.Collections.Generic;
using System.Threading;

using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.Persistence.XmlStore.DataManagement;

namespace Rebel.Framework.Persistence.XmlStore
{
    public class RepositoryReadWriter : RepositoryReader, IRepositoryReadWriter
    {
        private readonly DataContext _dataContext;
        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();

        public RepositoryReadWriter(DataContext dataContext) : base(dataContext)
        {
        }


        #region Implementation of IRepositoryReadWriter

        public void AddOrUpdate(AbstractEntity persistedEntity)
        {
            using (new WriteLockDisposable(_locker))
            {
                throw new NotImplementedException();
            }
        }

        public void Delete<T>(HiveId entityId)
        {
            throw new NotImplementedException();
        }

        public void AddOrUpdate<T>(Revision<T> revision) where T : TypedEntity
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
