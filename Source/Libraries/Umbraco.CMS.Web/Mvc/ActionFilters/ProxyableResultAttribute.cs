using System.Web.Mvc;

namespace Umbraco.Cms.Web.Mvc.ActionFilters
{
    /// <summary>
    /// When using the ProxyRequestToController methods and you wish to receive the typed 'ActionResult' (TResult) result 
    /// from the proxy, then this filter must be used. 
    /// </summary>
    /// <remarks>
    /// This filter is added to the Global filters so will always execute.
    /// </remarks>
    public class ProxyableResultAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            StoreResult(filterContext.Controller.ControllerContext, filterContext.Result);
            base.OnActionExecuted(filterContext);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            StoreResult(filterContext.Controller.ControllerContext, filterContext.Result);
            base.OnResultExecuted(filterContext);
        }

        /// <summary>
        /// Checks if the DataTokens key exists which flags that we should store the result for retreival after execution
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="result"></param>
        private static void StoreResult(ControllerContext controllerContext, ActionResult result)
        {
            if (controllerContext.IsChildAction)
            {
                if (controllerContext.ParentActionViewContext.RouteData.DataTokens.ContainsKey("ProxyResult"))
                {
                    controllerContext.ParentActionViewContext.RouteData.DataTokens["ProxyResult"] = result;
                }
            }
        }
    }
}