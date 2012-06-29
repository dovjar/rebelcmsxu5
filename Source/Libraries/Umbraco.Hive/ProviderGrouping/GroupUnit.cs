using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Tasks;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Hive.RepositoryTypes;

namespace Umbraco.Hive.ProviderGrouping
{
    using Umbraco.Framework.Persistence.Model;
    using Umbraco.Framework.Persistence.Model.Versioning;

    public class GroupUnit<TFilter> : DisposableObject, IGroupUnit<TFilter> where TFilter : class, IProviderTypeFilter
    {
        public GroupUnit(IEnumerable<ProviderUnit> providerUnits, Uri idRoot, AbstractScopedCache scopedCache, bool canDisposeCache, IFrameworkContext frameworkContext, RepositoryContext hiveContext)
        {
            // Only check that providerUnits is not null, it's OK if it's empty - might be a group unit for a group that can't do anything :)
            Mandate.ParameterNotNull(providerUnits, "providerUnits");
            Mandate.ParameterNotNull(idRoot, "idRoot");
            Mandate.ParameterNotNull(scopedCache, "scopedCache");

            ProviderUnits = providerUnits;
            IdRoot = idRoot;

            _canDisposeCache = canDisposeCache;
            UnitScopedCache = scopedCache;

            FrameworkContext = frameworkContext ?? providerUnits.First().EntityRepository.FrameworkContext;

            WasAbandoned = false;

            var enumerable = ProviderUnits.ToArray();
            var entityRepositories = enumerable.Select(y =>
                {
                    y.EntityRepository.HiveContext = hiveContext;
                    return y.EntityRepository;
                }).ToList();

            var providerEntityRevisionRepositories = enumerable.Select(y =>
                {
                    y.EntityRepository.Revisions.HiveContext = hiveContext;
                    return y.EntityRepository.Revisions;
                }).ToList();

            var schemaRepositories = enumerable.Select(y =>
                {
                    y.EntityRepository.Schemas.HiveContext = hiveContext;
                    return y.EntityRepository.Schemas;
                }).ToList();

            var providerSchemaRevisionRepositories = enumerable.Select(y =>
                {
                    y.EntityRepository.Schemas.Revisions.HiveContext = hiveContext;
                    return y.EntityRepository.Schemas.Revisions;
                }).ToList();

            _entityRepository = new EntityRepositoryGroup<TFilter>(entityRepositories,
                                                                   providerEntityRevisionRepositories,
                                                                   schemaRepositories,
                                                                   providerSchemaRevisionRepositories,
                                                                   IdRoot,
                                                                   UnitScopedCache,
                                                                   FrameworkContext,
                                                                   hiveContext);
        }

        public AbstractScopedCache UnitScopedCache { get; protected set; }
        public Uri IdRoot { get; protected set; }

        protected IEnumerable<ProviderUnit> ProviderUnits { get; set; }

        private readonly IEntityRepositoryGroup<TFilter> _entityRepository;
        private readonly bool _canDisposeCache;

        public IEntityRepositoryGroup<TFilter> Repositories
        {
            get
            {
                return _entityRepository;
            }
        }

        public void Complete()
        {

            // Execute all 'Pre' tasks
            foreach (var providerUnit in ProviderUnits)
            {
                // Raise any pre-complete triggers and abandon the unit if they signal we should cancel

                // First, raise schema triggers
                Func<AbstractSchemaPart, TaskExecutionContext> runSchemaTask = schemaObject => FrameworkContext
                    .TaskManager
                    .ExecuteInContext(TaskTriggers.Hive.Schemas.PreAddOrUpdateOnUnitComplete,
                        this,
                        new TaskEventArgs(FrameworkContext, new HiveSchemaPreActionEventArgs(schemaObject, UnitScopedCache)));

                if (providerUnit.EntityRepository.Schemas.EntitiesAddedOrUpdated
                    .Select(runSchemaTask)
                    .Any(context => context.Cancel))
                {
                    Abandon();
                    return;
                }

                // Second, regular entity triggers
                Func<IRelatableEntity, TaskExecutionContext> runEntityTask = typedEntity => FrameworkContext
                    .TaskManager
                    .ExecuteInContext(TaskTriggers.Hive.PreAddOrUpdateOnUnitComplete, 
                        this, 
                        new TaskEventArgs(FrameworkContext, new HiveEntityPreActionEventArgs(typedEntity, UnitScopedCache)));

                if (providerUnit.EntityRepository.EntitiesAddedOrUpdated
                    .Select(runEntityTask)
                    .Any(context => context.Cancel))
                {
                    Abandon();
                    return;
                }

                // Third, raise revision triggers
                Func<IRevision<IVersionableEntity>, TaskExecutionContext> runRevisionTask = typedEntity => FrameworkContext
                    .TaskManager
                    .ExecuteInContext(TaskTriggers.Hive.Revisions.PreAddOrUpdateOnUnitComplete,
                        this,
                        new TaskEventArgs(FrameworkContext, new HiveRevisionPreActionEventArgs(typedEntity, UnitScopedCache)));

                if (providerUnit.EntityRepository.Revisions.RevisionsAddedOrUdpated.ToList()
                    .Select(runRevisionTask)
                    .Any(context => context.Cancel))
                {
                    Abandon();
                    return;
                }
                
                // Fifth, raise entity deletion triggers
                Func<HiveId, TaskExecutionContext> runEntityDeletionTask = id => FrameworkContext
                    .TaskManager
                    .ExecuteInContext(TaskTriggers.Hive.PreDeleteOnUnitComplete,
                        this,
                        new TaskEventArgs(FrameworkContext, new HiveEntityPreDeletionEventArgs(id, UnitScopedCache)));

                if (providerUnit.EntityRepository.EntitiesDeleted
                    .Select(runEntityDeletionTask)
                    .Any(context => context.Cancel))
                {
                    Abandon();
                    return;
                }
            }

            //Raise 'post' triggers
            foreach (var providerUnit in ProviderUnits)
            {
                providerUnit.Complete();

                //First, raise schema triggers
                foreach (var schemaPart in providerUnit.EntityRepository.Schemas.EntitiesAddedOrUpdated)
                {
                    FrameworkContext.TaskManager.ExecuteInContext(
                        TaskTriggers.Hive.Schemas.PostAddOrUpdateOnUnitComplete,
                        this,
                        new TaskEventArgs(FrameworkContext, new HiveSchemaPostActionEventArgs(schemaPart, UnitScopedCache)));
                }

                //Second, raise entity triggers
                foreach (var typedEntity in providerUnit.EntityRepository.EntitiesAddedOrUpdated)
                {
                    FrameworkContext.TaskManager.ExecuteInContext(
                        TaskTriggers.Hive.PostAddOrUpdateOnUnitComplete,
                        this,
                        new TaskEventArgs(FrameworkContext, new HiveEntityPostActionEventArgs(typedEntity, UnitScopedCache)));
                }

                //Third, raise revision triggers
                foreach (var typedEntity in providerUnit.EntityRepository.Revisions.RevisionsAddedOrUdpated)
                {
                    FrameworkContext.TaskManager.ExecuteInContext(
                        TaskTriggers.Hive.Revisions.PostAddOrUpdateOnUnitComplete,
                        this,
                        new TaskEventArgs(FrameworkContext, new HiveRevisionPostActionEventArgs(typedEntity, UnitScopedCache)));
                }

                //Fourth, raise entity deletion triggers
                foreach (var id in providerUnit.EntityRepository.EntitiesDeleted)
                {
                    FrameworkContext.TaskManager.ExecuteInContext(
                        TaskTriggers.Hive.PostDeleteOnUnitComplete,
                        this,
                        new TaskEventArgs(FrameworkContext, new HiveEntityPostDeletionEventArgs(id, UnitScopedCache)));
                }
            }
        }

        public bool WasAbandoned { get; protected set; }

        public void Abandon()
        {
            try
            {
                foreach (var providerUnit in ProviderUnits)
                {
                    providerUnit.Abandon();
                }
            }
            finally
            {
                WasAbandoned = true;
            }
        }

        protected override void DisposeResources()
        {
            //if (_entityRepository.IsValueCreated)
                _entityRepository.Dispose();

            if (UnitScopedCache != null && _canDisposeCache)
            {
                UnitScopedCache.ScopeComplete();
                UnitScopedCache.Dispose();
            }
        }

        #region Implementation of IRequiresFrameworkContext

        /// <summary>
        /// Gets the framework context.
        /// </summary>
        /// <remarks></remarks>
        public IFrameworkContext FrameworkContext { get; protected set; }

        #endregion
    }
}