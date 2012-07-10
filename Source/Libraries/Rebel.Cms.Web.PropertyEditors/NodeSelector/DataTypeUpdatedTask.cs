using System;
using System.Linq;
using Rebel.Cms.Web.BuildManagerCodeDelegates;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Tasks;
using Rebel.Hive;

namespace Rebel.Cms.Web.PropertyEditors.NodeSelector
{
    /// <summary>
    /// Whenever an attribute type is updated, we need to invalidate the CodeDelegate hash code so the filters recompile.
    /// </summary>
    [Task("8B56D1DD-F9D1-41C3-A0CC-8D04E133FE50", TaskTriggers.Hive.Schemas.PostAddOrUpdateOnUnitComplete)]
    public class DataTypeUpdatedTask : AbstractTask
    {
        public DataTypeUpdatedTask(IFrameworkContext context) : base(context)
        {
        }

        public override void Execute(TaskExecutionContext context)
        {
            var args = context.EventArgs.CallerEventArgs as HiveSchemaPostActionEventArgs;
            if (args != null)
            {
                var attType = args.SchemaPart as AttributeType;
                if (attType != null)
                {
                    //first refresh the code for the filter
                    RefreshCodeBlocks(attType.Id, CSharpCodeBlockType.NodeFilter);
                    //second refresh the code for the start node query
                    RefreshCodeBlocks(attType.Id, CSharpCodeBlockType.StartNodeQuery);

                }
            }
        }

        private void RefreshCodeBlocks(HiveId attrTypeId, CSharpCodeBlockType cSharpCodeType)
        {
            var delegateParameter = new Tuple<HiveId, CSharpCodeBlockType>(attrTypeId, cSharpCodeType);

            //check if we have this registered (multiple ids may exist for the same delegate, but in thsi case it will never be true)
            var ids = CodeDelegateVirtualPath.GetVirtualPathIdsForDelegate(delegateParameter).ToArray();
            if (ids.Any())
            {
                var codeDelegateId = ids.First();
                CodeDelegateVirtualPath.TryUpdate(codeDelegateId, delegateParameter);
            }

        }
    }
}