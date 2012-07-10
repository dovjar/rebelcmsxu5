using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Trees;

namespace Rebel.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// Filters out the tree nodes Menu Items based on permissions
    /// </summary>
    public class MenuItemsPermissionFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called by the ASP.NET MVC framework after the action method executes.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            if (filterContext.Result is RebelTreeResult)
            {
                var treeResult = (RebelTreeResult)filterContext.Result;

                var nodeCollection = new TreeNodeCollection();
                nodeCollection.AddRange(treeResult.NodeCollection);

                foreach (var node in nodeCollection)
                {
                    var menuActions = node.MenuActions.ToList();
                    foreach (var menuAction in menuActions)
                    {
                        var attributes = menuAction.Metadata.ComponentType.GetCustomAttributes(typeof(RebelAuthorizeAttribute), true);
                        if (attributes.Length > 0)
                        {
                            var authorized = attributes.Aggregate(false, (current, attribute) => current || ((RebelAuthorizeAttribute)attribute).IsAuthorized(filterContext.HttpContext, node.HiveId));
                            if (!authorized)
                                node.MenuActions.Remove(menuAction);
                        }
                    }
                }

                filterContext.Result = new RebelTreeResult(nodeCollection, filterContext.Controller.ControllerContext);
            }
        }
    }
}
