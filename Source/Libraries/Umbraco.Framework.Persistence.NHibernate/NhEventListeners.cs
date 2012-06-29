using System;
using System.Collections;
using System.Collections.Generic;
using Iesi.Collections;
using NHibernate.Event;
using NHibernate.Event.Default;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Persistence.RdbmsModel;

namespace Umbraco.Framework.Persistence.NHibernate
{
    using System.Collections.Concurrent;
    using Umbraco.Framework.Persistence.NHibernate.OrmConfig;

    [Serializable]
    public class NhEventListeners : 
        global::NHibernate.Event.IPostInsertEventListener, 
        global::NHibernate.Event.IPostUpdateEventListener, 
        global::NHibernate.Event.IDeleteEventListener, 
        IPreDeleteEventListener,
        ISaveOrUpdateEventListener, 
        IFlushEventListener, 
        IMergeEventListener, 
        IEvictEventListener//, global::NHibernate.Event.IMergeEventListener, ISaveOrUpdateEventListener
    {
        //public static Action<IReferenceByGuid, Guid> NodeIdAvailable;

        private static readonly ConcurrentDictionary<DisposableObject, Action<IReferenceByGuid, Guid>> NodeIdHandlers = new ConcurrentDictionary<DisposableObject, Action<IReferenceByGuid, Guid>>();
        public static void AddNodeIdHandler(DisposableObject caller, Action<IReferenceByGuid, Guid> method)
        {
            EnsureDisposedHandlersRemoved();
            NodeIdHandlers.AddOrUpdate(caller, method, (key, existing) => method);
        }

        public static void RemoveNodeIdHandler(DisposableObject caller)
        {
            EnsureDisposedHandlersRemoved();
            NodeIdHandlers.RemoveAll(x => ReferenceEquals(x.Key, caller));
        }

        protected static void EnsureDisposedHandlersRemoved()
        {
            NodeIdHandlers.RemoveAll(x => x.Key == null || x.Key.IsDisposed);
        }

        protected static void OnNodeIdAvailable(IReferenceByGuid referenceByGuid, Guid guid)
        {
            EnsureDisposedHandlersRemoved();
            NodeIdHandlers.ForEach(x => x.Value.Invoke(referenceByGuid, guid));
        }


        private readonly DefaultMergeEventListener _defaultMergeListener;
        private readonly DefaultSaveOrUpdateEventListener _defaultSaveListener;
        private readonly DefaultDeleteEventListener _defaultDeleteListener;
        private readonly DefaultFlushEventListener _defaultFlushListener;
        private readonly DefaultMergeEventListener _defaultMergeEventListener;
        private readonly DefaultEvictEventListener _defaultEvictEventListener;


        [ThreadStatic] private static ConcurrentHashedCollection<Guid> _modifiedVersionIds;

        private ConcurrentHashedCollection<Guid> EnsureVersionIdsInitialised()
        {
            return _modifiedVersionIds ?? (_modifiedVersionIds = new ConcurrentHashedCollection<Guid>());
        }

        private List<AttributeDefinition> _debug_bodyTextCount = new List<AttributeDefinition>();

        public NhEventListeners()
        {
            _defaultMergeListener = new DefaultMergeEventListener();
            _defaultSaveListener = new DefaultSaveOrUpdateEventListener();
            _defaultDeleteListener = new DefaultDeleteEventListener();
            _defaultFlushListener = new DefaultFlushEventListener();
            _defaultMergeEventListener = new DefaultMergeEventListener();
            _defaultEvictEventListener = new DefaultEvictEventListener();
        }

        public void OnPostInsert(PostInsertEvent @event)
        {
            LogHelper.TraceIfEnabled<NhEventListeners>("OnPostInsert for '{0}' with id '{1}'",
                                                     () => @event.Entity.GetType().Name, () => @event.Id);

            AggregateDataInterceptor.CheckNodeVersionId(@event.Entity, EnsureVersionIdsInitialised());

            var referenceByGuid = @event.Entity as IReferenceByGuid;
            if (referenceByGuid == null)
            {
                LogHelper.TraceIfEnabled<NhEventListeners>("Not raising event because casting entity to IReferenceByGuid resulted in null value");
                return;
            }

            //if (NodeIdAvailable != null) NodeIdAvailable(referenceByGuid, (Guid)@event.Id);
            OnNodeIdAvailable(referenceByGuid, (Guid)@event.Id);
        }

        public void OnPostUpdate(PostUpdateEvent @event)
        {
            LogHelper.TraceIfEnabled<NhEventListeners>("OnPostUpdate for '{0}' with id '{1}'",
                                                     () => @event.Entity.GetType().Name, () => @event.Id);

            AggregateDataInterceptor.CheckNodeVersionId(@event.Entity, EnsureVersionIdsInitialised());
        }

        // Commented out for perf, can enable if you want to debug a tricky NH exception
        //private ConcurrentQueue<string> _saveQueue = new ConcurrentQueue<string>();

        public void OnSaveOrUpdate(SaveOrUpdateEvent @event)
        {

            try
            {
                _defaultSaveListener.OnSaveOrUpdate(@event);
                //_saveQueue.Enqueue(usefulEntityName + " at " + DateTimeOffset.UtcNow.TimeOfDay.ToString());
                //// Trim the save queue to avoid it getting too big
                //if (_saveQueue.Count > 10) _saveQueue = new ConcurrentQueue<string>(_saveQueue.Reverse().Take(10).Reverse());
            }
            catch (Exception ex)
            {
                var cascadeHelperMsg = string.Empty;
                var actionQueueDeletions = string.Empty;
                //if (ex.Message.Contains("cascade"))
                //{
                //    cascadeHelperMsg = "\nPreviously tried:\n" + string.Concat(_saveQueue.Select(x => x + "\n"));
                //    actionQueueDeletions = "\nDeletions queued:\n" + string.Concat(@event.Session.ActionQueue.CloneDeletions().Select(x => GetUsefulEntityName(x.Instance, x.EntityName, x.Id) + "\n"));
                //}

                var usefulEntityName = GetUsefulEntityName(@event.Entity, @event.EntityName, @event.Entry != null ? @event.Entry.Id : null);
                throw new InvalidOperationException("Error trying to save {0}. {1}{2}".InvariantFormat(usefulEntityName, cascadeHelperMsg, actionQueueDeletions), ex);
            }
        }


        private static string GetUsefulEntityName(object entity, string entityName, object id)
        {
            var alias = (entity as IReferenceByAlias != null) ? " aliassed as '" + ((IReferenceByAlias)entity).Alias + "'" : string.Empty;
            if (id == null) id = (entity as IReferenceByGuid != null) ? ((IReferenceByGuid) entity).Id.ToString("N") : "(null)";
            return "{0}{1} with id {2}".InvariantFormat(entityName, alias, id);
        }

        /// <summary>
        /// Handle the given delete event. 
        /// </summary>
        /// <param name="event">The delete event to be handled. </param>
        public void OnDelete(DeleteEvent @event)
        {
            try
            {
                try
                {
                    AggregateDataInterceptor.OnDeleteCheckAggregate(@event.Entity, @event.Session);
                }
                finally
                {
                    _defaultDeleteListener.OnDelete(@event);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Error while trying to delete a {0} with id {1}", @event.EntityName, @event.Session.GetIdentifier(@event.Entity)), ex);
            }
        }

        public void OnDelete(DeleteEvent @event, ISet transientEntities)
        {
            try
            {
                try
                {
                    AggregateDataInterceptor.OnDeleteCheckAggregate(@event.Entity, @event.Session);
                }
                finally
                {
                    _defaultDeleteListener.OnDelete(@event, transientEntities);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Error while trying to delete a {0} with id {1}", @event.EntityName, @event.Session.GetIdentifier(@event.Entity)), ex);
            }
        }

        /// <summary>
        /// Handle the given flush event. 
        /// </summary>
        /// <param name="event">The flush event to be handled.</param>
        public void OnFlush(FlushEvent @event)
        {
            try
            {
                try
                {
                    _defaultFlushListener.OnFlush(@event);
                }
                finally
                {
                    AggregateDataInterceptor.UpdateAggregatesPostFlush(EnsureVersionIdsInitialised(), @event.Session);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error trying to flush", ex);
            }
        }

        /// <summary>
        /// Handle the given merge event. 
        /// </summary>
        /// <param name="event">The merge event to be handled. </param>
        public void OnMerge(MergeEvent @event)
        {
            try
            {
                _defaultMergeEventListener.OnMerge(@event);
            }
            catch (Exception ex)
            {

                throw new InvalidOperationException("Error trying to merge: {0}".InvariantFormat(GetUsefulEntityName(@event.Entity, @event.EntityName, null)), ex);
            }
        }

        /// <summary>
        /// Handle the given merge event. 
        /// </summary>
        /// <param name="event">The merge event to be handled. </param><param name="copiedAlready"/>
        public void OnMerge(MergeEvent @event, IDictionary copiedAlready)
        {
            try
            {
                _defaultMergeEventListener.OnMerge(@event, copiedAlready);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error trying to merge: {0}".InvariantFormat(GetUsefulEntityName(@event.Entity, @event.EntityName, null)), ex);
            }
        }

        public void OnEvict(EvictEvent @event)
        {
            try
            {
                _defaultEvictEventListener.OnEvict(@event);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error trying to evict: {0}".InvariantFormat(GetUsefulEntityName(@event.Entity, null, null)), ex);
            }
        }

        /// <summary>
        /// Return true if the operation should be vetoed
        /// </summary>
        /// <param name="event"/>
        public bool OnPreDelete(PreDeleteEvent @event)
        {
            try
            {
                AggregateDataInterceptor.OnDeleteCheckAggregate(@event.Entity, @event.Session);
                return false;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("Error trying to flush aggregates during pre-delete phase: {0}".InvariantFormat(GetUsefulEntityName(@event.Entity, null, null)), ex);
            }
        }
    }
}
