using System;
using System.Collections.Generic;
using System.Linq;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Hive.ProviderSupport;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Hive.ProviderGrouping
{
    using Rebel.Framework.Persistence.Model;

    public class RevisionRepositoryGroup<TFilter, T>
        : AbstractRepositoryGroup, IRevisionRepositoryGroup<TFilter, T>
        where T : class, IVersionableEntity
        where TFilter : class, IProviderTypeFilter
    {
        public RevisionRepositoryGroup(IEnumerable<AbstractRevisionRepository<T>> childRepositories, Uri idRoot, AbstractScopedCache scopedCache, RepositoryContext hiveContext)
            : base(childRepositories, idRoot, scopedCache, hiveContext)
        {
            ChildSessions = childRepositories;
        }
        
        protected IEnumerable<ProviderSupport.AbstractRevisionRepository<T>> ChildSessions { get; set; }

        protected override void DisposeResources()
        {
            ChildSessions.Dispose();
        }

        public Revision<TEntity> Get<TEntity>(HiveId entityId, HiveId revisionId) where TEntity : class, T
        {
            return ChildSessions.Get<TEntity>(entityId, revisionId, IdRoot);
        }

        public IEnumerable<Revision<TEntity>> GetAll<TEntity>() where TEntity : class, T
        {
            return ChildSessions.GetAll<TEntity>(IdRoot);
        }

        public void AddOrUpdate<TEntity>(Revision<TEntity> revision) where TEntity : class, T
        {
            ExecuteInCancellableTask(revision, TaskTriggers.Hive.Revisions.PreAddOrUpdate, TaskTriggers.Hive.Revisions.PostAddOrUpdate, () => ChildSessions.Add<TEntity>(revision, IdRoot));
        }

        public EntitySnapshot<TEntity> GetLatestSnapshot<TEntity>(HiveId hiveId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            return ChildSessions.GetLatestSnapshot<TEntity>(hiveId, IdRoot, revisionStatusType);
        }

        public Revision<TEntity> GetLatestRevision<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            return ChildSessions.GetLatestRevision<TEntity>(entityId, IdRoot, revisionStatusType);
        }

        public IEnumerable<Revision<TEntity>> GetAll<TEntity>(HiveId entityId, RevisionStatusType revisionStatusType = null) where TEntity : class, T
        {
            return ChildSessions.GetAll<TEntity>(entityId, IdRoot, revisionStatusType);
        }

        public IEnumerable<Revision<TEntity>> GetLatestRevisions<TEntity>(bool allOrNothing, RevisionStatusType revisionStatusType = null, params HiveId[] entityIds) where TEntity : class, T
        {
            return ChildSessions.GetLatestRevisions<TEntity>(allOrNothing, IdRoot, revisionStatusType, entityIds);
        }

        private void ExecuteInCancellableTask(IRevision<T> entity, string preActionTaskTrigger, string postActionTaskTrigger, Action execution)
        {
            HiveContext.FrameworkContext.TaskManager.ExecuteInCancellableTask(this, entity, preActionTaskTrigger, postActionTaskTrigger, execution, x => new HiveRevisionPreActionEventArgs(x, UnitScopedCache), x => new HiveRevisionPostActionEventArgs(x, UnitScopedCache), HiveContext.FrameworkContext);
        }
    }
}