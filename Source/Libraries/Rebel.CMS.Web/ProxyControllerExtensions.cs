using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ActionInvokers;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Framework;

namespace Rebel.Cms.Web
{
    /// <summary>
    /// Extension methods to proxy a request to another controller's action.
    /// </summary>
    internal static class ProxyControllerExtensions
    {
        /// <summary>
        /// Validates the controller to be proxied to, this requires that ProxyableResultAttribute is added to GlobalFilters.
        /// </summary>
        /// <param name="throwException">true to throw an exception if validation fails</param>
        /// <returns></returns>
        private static bool ValidateProxyableRequest(bool throwException = false)
        {
            var valid = true;
            if (!GlobalFilters.Filters.ContainsFilter<ProxyableResultAttribute>())
            {
                valid = false;
            }

            if (!valid && throwException)
            {
                throw new InvalidOperationException("Cannot proxy a request to a controller without " + typeof(ProxyableResultAttribute).FullName + " added to the GlobalFilters.");
            }

            return true;
        }

        /// <summary>
        /// Executes the action as a ChildAction but maintains the action's result so that it can be retreived
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TControllerType"></typeparam>
        /// <param name="controller"></param>
        /// <param name="methodSelector"></param>        
        /// <param name="area"></param>
        /// <param name="routeVals"> </param>
        /// <returns></returns>
        public static ProxyRequestResult<TResult> ProxyRequestToController<TResult, TControllerType>(
            this ControllerBase controller,
            Expression<Func<TControllerType, TResult>> methodSelector,
            string area,
            IEnumerable<KeyValuePair<string, object>> routeVals)            
            where TResult : ActionResult
            where TControllerType : class
        {
            Mandate.ParameterNotNull(methodSelector, "methodSelector");
            var proxyController = DependencyResolver.Current.GetService<TControllerType>();
            Mandate.That(TypeFinder.IsTypeAssignableFrom<ControllerBase>(proxyController), x => new InvalidOperationException("TControllerType must be of type " + typeof(ControllerBase).FullName));            
            //validate that proxying is possible, if not an exception is thrown
            ValidateProxyableRequest(true);

            var methodInfo = Rebel.Framework.ExpressionHelper.GetMethodInfo(methodSelector);
            var methodParams = Rebel.Framework.ExpressionHelper.GetMethodParams(methodSelector);

            //merge values
            foreach (var i in routeVals.Where(i => !methodParams.ContainsKey(i.Key)))
            {
                methodParams.Add(i.Key, i.Value);
            }

            return controller.ExecuteProxiedControllerAction<TResult>(
                ControllerExtensions.GetControllerName(typeof(TControllerType)), methodInfo.Name, area, methodParams);

        }

        /// <summary>
        /// Executes the action as a ChildAction but maintains the action's result so that it can be retreived
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TControllerType"></typeparam>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="methodSelector"></param>
        /// <param name="area"></param>
        /// <param name="routeVals"> </param>
        /// <returns></returns>
        public static ProxyRequestResult<TResult> ProxyRequestToController<TResult, TControllerType>(
            this ControllerBase controller,
            TControllerType proxyController,
            Expression<Func<TControllerType, TResult>> methodSelector,
            string area,
            IEnumerable<KeyValuePair<string, object>> routeVals)
            where TResult : ActionResult
            where TControllerType : class
        {
            Mandate.ParameterNotNull(proxyController, "proxyController");
            Mandate.ParameterNotNull(methodSelector, "methodSelector");
            Mandate.That(TypeFinder.IsTypeAssignableFrom<ControllerBase>(proxyController), x => new InvalidOperationException("TControllerType must be of type " + typeof(ControllerBase).FullName));
            //validate that proxying is possible, if not an exception is thrown
            ValidateProxyableRequest(true);

            var methodInfo = Rebel.Framework.ExpressionHelper.GetMethodInfo(methodSelector);
            var methodParams = Rebel.Framework.ExpressionHelper.GetMethodParams(methodSelector);

            //merge values
            foreach (var i in routeVals.Where(i => !methodParams.ContainsKey(i.Key)))
            {
                methodParams.Add(i.Key, i.Value);
            }

            return controller.ExecuteProxiedControllerAction<TResult>(
                ControllerExtensions.GetControllerName(proxyController.GetType()), methodInfo.Name, area, methodParams);

        }

        /// <summary>
        /// Executes to the Plugin controller's action as a ChildAction but maintains the action's result so that it can be retreived
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="TControllerType"></typeparam>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="methodSelector"></param>
        /// <param name="pluginDef"></param>
        /// <param name="backOfficePath"></param>
        /// <param name="routeIdParameterName">Plugin controllers routes are constructed with a route Id parameter such as editorId or surfaceId </param>
        /// <returns></returns>
        public static ProxyRequestResult<TResult> ProxyRequestToController<TResult, TControllerType>(
            this ControllerBase controller,
            TControllerType proxyController,
            Expression<Func<TControllerType, TResult>> methodSelector,
            PluginMetadataComposition pluginDef,
            string backOfficePath,
            string routeIdParameterName)
            where TResult : ActionResult
            where TControllerType : class
        {
            Mandate.ParameterNotNull(pluginDef, "pluginDef");
            Mandate.ParameterNotNullOrEmpty(backOfficePath, "backOfficePath");

            IDictionary<string, object> routeDictionary = new Dictionary<string, object>();

            var proxyArea = backOfficePath;
            if (pluginDef.PluginDefinition.HasRoutablePackageArea())
            {
                proxyArea = pluginDef.PluginDefinition.PackageName;                
            }

            if (pluginDef.Id != Guid.Empty)
            {
                //need to add the routing id to it
                routeDictionary.Add(routeIdParameterName, pluginDef.Id.ToString("N"));
            }

            return controller.ProxyRequestToController(proxyController, methodSelector, proxyArea, routeDictionary);

        }

        /// <summary>
        /// Executes the action as a ChildAction but maintains the action's result so that it can be retreived
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="childAction"></param>
        /// <param name="routeVals"></param>
        /// <param name="area"> </param>
        /// <returns></returns>
        public static ProxyRequestResult<ActionResult> ProxyRequestToController(
            this ControllerBase controller,
            ControllerBase proxyController,
            MethodInfo childAction,
            IEnumerable<KeyValuePair<string, object>> routeVals,
            string area)
        {
            return controller.ProxyRequestToController(
                ControllerExtensions.GetControllerName(proxyController.GetType()),
                childAction.Name,
                routeVals,
                area);
        }
        
        /// <summary>
        /// Executes the action as a ChildAction but maintains the action's result so that it can be retreived
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="childAction"></param>
        /// <param name="area"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static ProxyRequestResult<ActionResult> ProxyRequestToController(
            this ControllerBase controller,
            string proxyController,
            string childAction,            
            IEnumerable<KeyValuePair<string, object>> routeVals,
            string area)
        {            
            return controller.ExecuteProxiedControllerAction<ActionResult>(
                proxyController, 
                childAction, 
                area,
                routeVals);
        }

        /// <summary>
        /// Executes to the Plugin controller's action as a ChildAction but maintains the action's result so that it can be retreived
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyController"></param>
        /// <param name="childAction"></param>
        /// <param name="pluginDef"></param>
        /// <param name="backOfficePath"></param>
        /// <param name="routeIdParameterName">Plugin controllers routes are constructed with a route Id parameter such as editorId or surfaceId </param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        public static ProxyRequestResult<ActionResult> ProxyRequestToController(
            this ControllerBase controller,
            string proxyController,
            string childAction,
            PluginMetadataComposition pluginDef,
            string backOfficePath,
            string routeIdParameterName,
            IEnumerable<KeyValuePair<string, object>> routeVals)
        {
            Mandate.ParameterNotNullOrEmpty(backOfficePath, "backOfficePath");
            Mandate.ParameterNotNullOrEmpty(routeIdParameterName, "routeIdParameterName");
            IDictionary<string, object> routeDictionary = new Dictionary<string, object>();

            if (routeVals != null)
            {
                foreach(var i in routeVals)
                {
                    routeDictionary.Add(i.Key, i.Value);
                }
            }
            
            var proxyArea = backOfficePath;
            if (pluginDef.PluginDefinition.HasRoutablePackageArea())
            {
                proxyArea = pluginDef.PluginDefinition.PackageName;             
            }

            if (pluginDef.Id != Guid.Empty)
            {
                //need to add the routing id to it
                routeDictionary.Add(routeIdParameterName, pluginDef.Id.ToString("N"));
            }

            return controller.ProxyRequestToController(proxyController, childAction, routeDictionary, proxyArea);
        }

        /// <summary>
        /// Returns a ProxyRequestResult which gets the string output response for the executed action by executing the controller on a different context and reading
        /// the response stream. This ensures that the entire MVC process is executed including ActionFilters.
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="proxyControllerName"></param>
        /// <param name="action"></param>
        /// <param name="area"></param>
        /// <param name="routeVals"></param>
        /// <returns></returns>
        private static ProxyRequestResult<TResult> ExecuteProxiedControllerAction<TResult>(
            this ControllerBase controller,
            string proxyControllerName,
            string action,
            string area,
            IEnumerable<KeyValuePair<string, object>> routeVals)
            where TResult : ActionResult
        {

            Mandate.ParameterNotNull(controller, "controller");
            Mandate.ParameterNotNull(proxyControllerName, "proxyControllerName");
            Mandate.ParameterNotNull(action, "action");
            if (routeVals == null)
            {
                routeVals = new RouteValueDictionary();
            }

            //check if proxying the actual TResult is possible
            var isProxyableResult = ValidateProxyableRequest();
            
            var routeData = controller.ControllerContext.RouteData.Clone();

            if (isProxyableResult)
            {
                //IMPORTANT! Because we are executing the controller, there is not real response, so we need a way to get
                //that response as well as executing the controller. This is why we validate the controller to proxy to ensure
                //that it has an ActionInvoker of type ProxyControllerActionInvoker. In this ActionInvoker, we check for a DataToken which
                //we now must set, this informs the ProxyableResult Filter to store a reference to the ActionResult just after the action is executed
                //in this DataToken. After execution, we will still have a reference to the ActionResult for us to return.
                routeData.DataTokens.Add("ProxyResult", null);
            }

            //create a request context for use in executing the controller
            var requestCtx = new RequestContext(controller.ControllerContext.HttpContext, routeData);
            
            //create an HtmlHelper with new view context based on our custom RequestContext
            var html = controller.GetHtmlHelper(requestCtx);

            //for the Html.Action method to work, we need a route value dictionary, so we'll merge
            //what we've got an then add the special 'area' value.
            var routeValsDictionary = new RouteValueDictionary();
            foreach (var i in routeVals)
            {
                routeValsDictionary.Add(i.Key, i.Value);
            }
            if (!area.IsNullOrWhiteSpace())
            {
                routeValsDictionary.Add("area", area);
            }

            //execute the proxied action using the Html.Action method.
            var stringResult = html.Action(action,
                                           proxyControllerName,
                                           routeValsDictionary).ToString();

            //check if we can strongly type it
            ActionResult result;
            if (isProxyableResult)
            {
                result = routeData.DataTokens["ProxyResult"] as TResult;
            }
            else
            {
                result = new ContentResult() { Content = stringResult };
            }

            return new ProxyRequestResult<TResult>(stringResult, result as TResult);


        }

        /// <summary>
        /// Create an HtmlHelper for the controller
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="requestContext"></param>
        /// <returns></returns>
        public static HtmlHelper GetHtmlHelper(this ControllerBase controller, RequestContext requestContext)
        {
            //create an HtmlHelper with new view context based on our custom RequestContext
            return new HtmlHelper(new ViewContext()
            {
                Controller = controller,
                RequestContext = requestContext,
                TempData = controller.TempData,
                ViewData = controller.ViewData
            }, new ProxyViewDataContainer());
        }

        private class ProxyViewDataContainer : IViewDataContainer
        {
            public ProxyViewDataContainer()
            {
                ViewData = new ViewDataDictionary();
            }

            public ViewDataDictionary ViewData { get; set; }
        }

    }
}