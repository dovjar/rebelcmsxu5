using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;
using Rebel.Cms.Web.Mvc.ViewEngines;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;

namespace Rebel.Cms.Web
{
    public static class ControllerExtensions
    {

        /// <summary>
        /// Return the ID of an controller plugin
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="controllerPluginType"></param>
        /// <returns></returns>
        public static Guid GetControllerId<T>(Type controllerPluginType)
            where T : PluginAttribute
        {
            //Locate the editor attribute
            var editorAttribute = controllerPluginType
                .GetCustomAttributes(typeof (T), false)
                .OfType<T>()
                .ToArray();
            if (!editorAttribute.Any()) throw new InvalidOperationException("The controller plugin type is missing the " + typeof(T).FullName + " attribute");
            var attr = editorAttribute.First();
            return attr.Id;
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <param name="controllerType"></param>
        /// <returns></returns>
        public static string GetControllerName(Type controllerType)
        {
            return controllerType.Name.Substring(0, controllerType.Name.LastIndexOf("Controller"));
        }

        /// <summary>
        /// Return the controller name from the controller type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string GetControllerName<T>()
        {
            return GetControllerName(typeof(T));
        }

        /// <summary>
        /// Executes the secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <returns></returns>
        public static void ExecuteSecuredMethod<TControllerType>(this TControllerType controller,
            Expression<Action<TControllerType>> methodSelector)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var result = controller.TryExecuteSecuredMethod(methodSelector);
            if (result.ExecutionError != null) throw result.ExecutionError;
        }

        /// <summary>
        /// Executes the secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public static void ExecuteSecuredMethod<TControllerType>(this TControllerType controller,
            Expression<Action<TControllerType>> methodSelector, HiveId entityId)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var result = controller.TryExecuteSecuredMethod(methodSelector, entityId);
            if (result.ExecutionError != null) throw result.ExecutionError;
        }

        /// <summary>
        /// Executes the secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <typeparam name="TResultType">The type of the result type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <returns></returns>
        public static void ExecuteSecuredMethod<TControllerType, TResultType>(this TControllerType controller,
            Expression<Func<TControllerType, TResultType>> methodSelector)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var result = controller.TryExecuteSecuredMethod(methodSelector);
            if (result.ExecutionError != null) throw result.ExecutionError;
        }

        /// <summary>
        /// Executes the secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <typeparam name="TResultType">The type of the result type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public static void ExecuteSecuredMethod<TControllerType, TResultType>(this TControllerType controller,
            Expression<Func<TControllerType, TResultType>> methodSelector, HiveId entityId)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var result = controller.TryExecuteSecuredMethod(methodSelector, entityId);
            if (result.ExecutionError != null) throw result.ExecutionError;
        }

        /// <summary>
        /// Tries to execute a secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <returns></returns>
        public static SecuredMethodResult TryExecuteSecuredMethod<TControllerType>(this TControllerType controller,
            Expression<Action<TControllerType>> methodSelector)
            where TControllerType : ControllerBase
        {
            return controller.TryExecuteSecuredMethod(methodSelector, HiveId.Empty);
        }

        /// <summary>
        /// Tries to execute a secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public static SecuredMethodResult TryExecuteSecuredMethod<TControllerType>(this TControllerType controller,
            Expression<Action<TControllerType>> methodSelector, HiveId entityId)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var authorised = controller.IsMethodAuthorized((MethodCallExpression)methodSelector.Body, entityId);

            if (!authorised)
                return SecuredMethodResult.False;

            try
            {
                methodSelector.Compile().Invoke(controller);
                return new SecuredMethodResult(true, true);
            }
            catch (Exception ex)
            {
                return new SecuredMethodResult(ex);
            }
        }

        /// <summary>
        /// Tries to execute a secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller.</typeparam>
        /// <typeparam name="TResultType">The type of the result.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <returns></returns>
        public static SecuredMethodResult<TResultType> TryExecuteSecuredMethod<TControllerType, TResultType>(this TControllerType controller,
            Expression<Func<TControllerType, TResultType>> methodSelector)
            where TControllerType : ControllerBase
        {
            return controller.TryExecuteSecuredMethod(methodSelector, HiveId.Empty);
        }

        /// <summary>
        /// Tries to execute a secured method.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <typeparam name="TResultType">The type of the result type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="methodSelector">The method selector.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns></returns>
        public static SecuredMethodResult<TResultType> TryExecuteSecuredMethod<TControllerType, TResultType>(this TControllerType controller,
            Expression<Func<TControllerType, TResultType>> methodSelector, HiveId entityId)
            where TControllerType : ControllerBase
        {
            Mandate.That<NullReferenceException>(controller.ControllerContext != null);
            Mandate.That<NullReferenceException>(controller.ControllerContext.HttpContext != null);

            var authorised = controller.IsMethodAuthorized((MethodCallExpression)methodSelector.Body, entityId);

            if (!authorised)
                return SecuredMethodResult<TResultType>.False;

            try
            {
                return new SecuredMethodResult<TResultType>(true, true, methodSelector.Compile().Invoke(controller));
            }
            catch (Exception ex)
            {
                return new SecuredMethodResult<TResultType>(ex);
            }
        }

        /// <summary>
        /// Determines whether the method call expression is authorized on the specified controller.
        /// </summary>
        /// <typeparam name="TControllerType">The type of the controller type.</typeparam>
        /// <param name="controller">The controller.</param>
        /// <param name="mce">The mce.</param>
        /// <param name="entityId">The entity id.</param>
        /// <returns>
        ///   <c>true</c> if the is method authorized on the specified controller; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsMethodAuthorized<TControllerType>(this TControllerType controller, MethodCallExpression mce, HiveId entityId)
            where TControllerType : ControllerBase
        {
            var methodInfo = mce.Method;
            var attributes = methodInfo.GetCustomAttributes(typeof(RebelAuthorizeAttribute), true);
            var success = true; // If no authorize attributes, authorize by default

            // Validate attributes
            if (attributes.Length > 0)
            {
                success = false; // We have authorize attributes so set to false unless we are authorized

                foreach (RebelAuthorizeAttribute attribute in attributes)
                {
                    if (controller is BackOfficeController)
                        attribute.RoutableRequestContext = (controller as BackOfficeController).BackOfficeRequestContext;

                    var id = entityId;

                    if (id.IsNullValueOrEmpty())
                    {
                        // Try to get id from method info
                        var parameters = methodInfo.GetParameters();
                        var idParameter = parameters.FirstOrDefault(x => x.Name == attribute.IdParameterName && x.ParameterType == typeof(HiveId));
                        if (idParameter != null)
                        {
                            var arg = mce.Arguments[idParameter.Position];
                            id = (HiveId)Expression.Lambda(Expression.Convert(arg, arg.Type)).Compile().DynamicInvoke();
                        }

                        // Try to get id from route data
                        else if (controller.ControllerContext.RouteData != null
                            && controller.ControllerContext.RouteData.Values != null
                            && controller.ControllerContext.RouteData.Values.ContainsKey(attribute.IdParameterName))
                            id = new HiveId(controller.ControllerContext.RouteData.Values[attribute.IdParameterName].ToString());

                        // Try to get id from request collection
                        else if (controller.ControllerContext.HttpContext.Request != null
                            && !string.IsNullOrWhiteSpace(controller.ControllerContext.HttpContext.Request[attribute.IdParameterName]))
                            id = new HiveId(controller.ControllerContext.HttpContext.Request[attribute.IdParameterName]);

                        else
                            id = FixedHiveIds.SystemRoot;
                    }

                    success = success || attribute.IsAuthorized(controller.ControllerContext.HttpContext, id);
                }
            }

            return success;
        }
    }

    
}
