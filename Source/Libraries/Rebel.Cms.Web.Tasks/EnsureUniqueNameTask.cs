using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rebel.Cms.Web.Context;
using Rebel.Framework.DataManagement;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.Tasks;

namespace Rebel.Cms.Web.Tasks
{
    [Task("DAFCA271-1874-4F67-B339-51E9A41F8C9C", TriggerTypes.HivePreAddOrUpdateRevision, ContinueOnFailure = false)]
    public class EnsureUniqueNameTask : AbstractWebTask
    {
        public EnsureUniqueNameTask(IRebelApplicationContext applicationContext) 
            : base(applicationContext)
        { }

        public override void Execute(TaskExecutionContext context)
        {
            if(context.EventArgs.CallerEventArgs is RepositoryEventArgs)
            {
                //var eventArgs = context.EventArgs.CallerEventArgs as RepositoryEventArgs;
                //var item = eventArgs.EntityRevision.Item;
                //var parent = item.Relations.Get<TypedEntity>(FixedRelationTypes.ContentTreeRelationType, HierarchyScope.Parent);
                //var parent = item.Relations.GetRelations(HierarchyScope.Parent)
            }
        }
    }
}
