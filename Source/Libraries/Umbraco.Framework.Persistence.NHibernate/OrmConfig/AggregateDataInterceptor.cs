using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Framework.Persistence.NHibernate.OrmConfig
{
    using Umbraco.Framework.Persistence.RdbmsModel;
    using global::NHibernate;
    using global::NHibernate.Dialect;

    [Serializable]
    public class AggregateDataInterceptor : EmptyInterceptor
    {
        private ISession _nhSession;
        private readonly ConcurrentHashedCollection<Guid> _modifiedVersionIds = new ConcurrentHashedCollection<Guid>();

        public override void SetSession(ISession session)
        {
            _nhSession = session;
        }

        public override void PostFlush(System.Collections.ICollection entities)
        {
            // After a flush, keep the aggregates up to date
            // We do this after a flush so that if other queries are run in the same
            // transaction, the aggregate table will have been updated before those
            // selects get run in the db engine
            try
            {
                UpdateAggregatesPostFlush(_modifiedVersionIds, _nhSession);
            }
            finally
            {
                base.PostFlush(entities);
            }
        }

        public static void UpdateAggregatesPostFlush(ConcurrentHashedCollection<Guid> modifiedVersionIds, ISession nhSession)
        {
            // After a flush, keep the aggregates up to date
            // We do this after a flush so that if other queries are run in the same
            // transaction, the aggregate table will have been updated before those
            // selects get run in the db engine
            try
            {
                var updatedNodeIds = modifiedVersionIds.ToArray();

                NhSessionHelper.DeleteAggregateForNodeIds(updatedNodeIds, nhSession);
                NhSessionHelper.RunAggregateForNodeIds(updatedNodeIds, "AggregateNodeStatus_PerNode", nhSession);
            }
            finally
            {
                modifiedVersionIds.Clear();
            }
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, global::NHibernate.Type.IType[] types)
        {
            var casted = entity as NodeVersion;
            if (casted != null)
            {
                _modifiedVersionIds.Add(casted.Node.Id);
            }
            return base.OnSave(entity, id, state, propertyNames, types);
        }

        public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, global::NHibernate.Type.IType[] types)
        {
            // We only care about NodeVersion as that's the only thing that has an aggregation
            var casted = entity as NodeVersion;
            if (casted != null)
            {
                // We have to issue deletes to the aggregate now before we hit PostFlush, as obviously PostFlush might issue
                // statements that refer to items that need to be deleted and could cause an FK constraint error
                NhSessionHelper.DeleteAggregateForNodeIds(casted.Node.Id.AsEnumerableOfOne(), _nhSession);
            }

            base.OnDelete(entity, id, state, propertyNames, types);
        }

        public static void CheckNodeVersionId(object entity, ConcurrentHashedCollection<Guid> modifiedVersionIds)
        {
            var casted = entity as NodeVersion;
            if (casted != null)
            {
                modifiedVersionIds.Add(casted.Node.Id);
            }
        }

        public static void OnDeleteCheckAggregate(object entity, ISession nhSession)
        {
            // We only care about NodeVersion as that's the only thing that has an aggregation
            var casted = entity as NodeVersion;
            if (casted != null)
            {
                // We have to issue deletes to the aggregate now before we hit PostFlush, as obviously PostFlush might issue
                // statements that refer to items that need to be deleted and could cause an FK constraint error
                NhSessionHelper.DeleteAggregateForNodeIds(casted.Node.Id.AsEnumerableOfOne(), nhSession);
            }
        }
    }
}
