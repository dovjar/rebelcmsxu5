using System;
using System.Diagnostics.Contracts;
using RebelCms.Framework.Data.Common;
using RebelCms.Framework.EntityGraph.Domain.Versioning;

namespace RebelCms.Framework.EntityGraph.Domain.Entity
{
    [ContractClassFor(typeof (IEntity))]
    internal abstract class IEntityCodeContract : IEntity
    {
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(UtcCreated < DateTime.UtcNow.AddDays(1));
            Contract.Invariant(UtcModified >= UtcCreated);
        }

        #region Implementation of ITracksConcurrency

        public IConcurrencyToken ConcurrencyToken { get; set; }

        #endregion

        #region Implementation of IEntity

        public IEntityStatus Status { get; set; }
        public DateTime UtcCreated { get; set; }
        public DateTime UtcModified { get; set; }
        public DateTime UtcStatusChanged { get; set; }
        public IMappedIdentifier Id { get; set; }
        public IRevisionData Revision { get; set; }

        #endregion
    }
}