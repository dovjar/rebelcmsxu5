using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Tasks;

namespace Umbraco.Cms.Web.PropertyEditors.NodeSelector
{
    public static class NodeSelectorDataSourceExtensions
    {
        /// <summary>
        /// Returns the TooltipContents after proxying through the task system to allow developers to modify the output
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="sender"></param>
        /// <param name="entity"> </param>
        /// <param name="htmlContent"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static TooltipContents CreateTooltipContentsViaTask(this INodeSelectorDataSource ds, 
                                                                   object sender, 
                                                                   TypedEntity entity,
                                                                   string htmlContent, 
                                                                   int width = -1, 
                                                                   int height = -1)
        {
            var args = new NodeSelectorTooltipEventArgs(entity, htmlContent)
                {
                    Height = height,
                    Width = width
                };

            //launch task to modify the contents
            ds.FrameworkContext.TaskManager
                .ExecuteInContext(
                    NodeSelectorTaskTriggers.GetTooltipContents,
                    sender,
                    new TaskEventArgs(ds.FrameworkContext, args));

            return new TooltipContents(args.HtmlContents)
                {
                    Height = args.Height,
                    Width = args.Width
                };
        }
    }
}