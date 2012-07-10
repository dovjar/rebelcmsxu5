namespace Rebel.Cms.Web.Tasks
{
    using Rebel.Cms.Web.Macros;
    using Rebel.Framework;
    using Rebel.Framework.Context;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Persistence.Model.Constants;
    using Rebel.Framework.Tasks;
    using Rebel.Hive;
    using Rebel.Hive.ProviderGrouping;
    using Rebel.Hive.RepositoryTypes;
    using global::System.Linq;

    /// <summary>
    /// Calls <see cref="MacroRenderer.ClearCacheByPrefix"/> should any new changes be made to relations or [un]published items.
    /// </summary>
    /// <remarks>Note: This is a simple version 1 as per requirements, and currently clears all caches rather than just those particular to the data that is changing. 
    /// For future modifications, look into using the IFrameworkContext.Caches.ExtendedLifetime cache system which can invalidate a cache granularly based on a delegate.</remarks>
    [Task("87566CB8-6E4D-4E29-BD0B-E5354C05D83C", TaskTriggers.Hive.Revisions.PostAddOrUpdateOnUnitComplete, ContinueOnFailure = true)]
    [Task("C3912669-BC61-405D-8BC6-C10830D50A16", TaskTriggers.Hive.Relations.PostRelationAdded, ContinueOnFailure = true)]
    [Task("809AC208-380E-4C59-8C1E-A6675B8D9AC5", TaskTriggers.Hive.Relations.PostRelationRemoved, ContinueOnFailure = true)]
    public class ClearMacroCacheTask : AbstractTask
    {
        public ClearMacroCacheTask(IFrameworkContext frameworkContext)
            : base(frameworkContext)
        {
            
        }

        public override void Execute(TaskExecutionContext context)
        {
            if (context.TriggerName == TaskTriggers.Hive.Revisions.PostAddOrUpdateOnUnitComplete)
            {
                var uow = context.EventSource as IGroupUnit<IProviderTypeFilter>;
                var args = context.EventArgs.CallerEventArgs as HiveRevisionPostActionEventArgs;

                if (uow != null && args != null && args.Entity != null)
                {
                    var shouldInvalidate = new[] {FixedStatusTypes.Published.Alias, FixedStatusTypes.Unpublished.Alias};
                    if (shouldInvalidate.Contains(args.Entity.MetaData.StatusType.Alias))
                    {
                        Clear();
                    }
                }
            }
            if (context.TriggerName == TaskTriggers.Hive.Relations.PostRelationAdded || context.TriggerName == TaskTriggers.Hive.Relations.PostRelationRemoved)
            {
                Clear();
            }
        }

        private void Clear()
        {
            var itemsCleared = MacroRenderer.ClearCacheByPrefix(this.Context);
            LogHelper.TraceIfEnabled<ClearMacroCacheTask>("Cleared {0} macro caches due to an item being published or unpublished", () => itemsCleared);
        }
    }
}