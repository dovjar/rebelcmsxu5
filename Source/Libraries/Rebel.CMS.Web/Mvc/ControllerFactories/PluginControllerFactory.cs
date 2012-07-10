using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.Context;
using Rebel.Framework;
using Rebel.Framework.Configuration;
using Rebel.Framework.Diagnostics;

namespace Rebel.Cms.Web.Mvc.ControllerFactories
{
    /// <summary>
    /// Controller factory used for finding and instantiating plugin controllers
    /// </summary>
    /// <remarks>
    /// Since the DefaultControllerFactory in MVC performs some very aggressive caching of controllers in a cache
    /// file which persists app pool restarts. The subsystem queries the bin folder for changes made to the controllers
    /// but since plugin controllers aren't in the bin folder if they are loaded in the CodeGen folder they don't get updated, 
    /// therefore this factory is required in order to ensure that plugin controllers don't get 'stale'
    /// </remarks>
    internal class PluginControllerFactory : DefaultControllerFactory, IFilteredControllerFactory
    {
        public PluginControllerFactory(IRebelApplicationContext applicationContext)
        {
            Mandate.ParameterNotNull(applicationContext, "applicationContext");
            ApplicationContext = applicationContext;
        }

        public IRebelApplicationContext ApplicationContext { get; private set; }

        private readonly ConcurrentDictionary<string, Type> _controllerCache = new ConcurrentDictionary<string, Type>(); 

        /// <summary>
        /// This factory will attempt to return the controller only if the PluginManager is using the CodeGen folder 
        /// andthe  route values contains both the 'Namespaces' and 'rebel' data tokens in its route values.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanHandle(RequestContext request)
        {
            if (ApplicationContext.Settings.PluginConfig.ShadowCopyType != PluginManagerShadowCopyType.UseDynamicFolder)
                return false;

            object namespaceToken;
            object rebelToken;
            return request.RouteData.DataTokens.TryGetValue("Namespaces", out namespaceToken)
                   && request.RouteData.DataTokens.TryGetValue("rebel", out rebelToken);
        }
        
        /// <summary>
        /// Returns the controller type for the specified controllerName based on the RouteData specified.
        /// </summary>
        /// <param name="requestContext"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        /// <remarks>
        /// Since this factory should only fire for plugin controllers, we just check for the Namespace token and look it up in our
        /// cache, if it doesn't exist there, we'll check the BuildManager since we know we've registered it with that on pre app init.
        /// If nothing is found, then we'll just revert to using the base classes implementation.
        /// </remarks>
        protected override Type GetControllerType(RequestContext requestContext, string controllerName)
        {
            //first lets check the cache
            var key = requestContext.RouteData.GetControllerHash();
            var type = _controllerCache.GetValue(key, null);
            if (type != null)
            {
                //check if it has already been flagged that we cannot find it, if so then return the base method
                return type == typeof(NullController) 
                    ? base.GetControllerType(requestContext, controllerName) 
                    : type;                
            }

            //only do this if one namespace is specified
            object namespaceToken;
            if (requestContext.RouteData.DataTokens.TryGetValue("Namespaces", out namespaceToken))
            {
                LogHelper.TraceIfEnabled<PluginControllerFactory>("Querying for controller '{0}' from the BuildManager", () => controllerName);

                try
                {
                    var namespaces = (string[])namespaceToken;
                    if (namespaces.Length == 1)
                    {
                        var typeName = namespaces[0] + "." + controllerName + "Controller";                        
                        //ensure it is cached
                        type = _controllerCache.GetOrAdd(key, x => global::System.Web.Compilation.BuildManager.GetType(typeName, false));
                        if (type == null)
                        {
                            LogHelper.Warn<PluginControllerFactory>("Could not find controller '{0}' from the BuildManager", controllerName);
                            //ensure it is cached that we haven't found it so we don't try to find it again
                            _controllerCache.AddOrUpdate(key, typeof(NullController), (s, t) => typeof(NullController));
                        }
                    }
                    else
                    {
                        LogHelper.Warn<PluginControllerFactory>("Could not find controller '{0}' from the BuildManager because a Namespace DataToken was not found in the routes for this request or the number of namespaces in the Token was not equal to 1", controllerName);
                        //ensure it is cached that we haven't found it so we don't try to find it again
                        _controllerCache.AddOrUpdate(key, typeof (NullController), (s, t) => typeof (NullController));
                    }
                }
                catch (Exception)
                {
                    LogHelper.Warn<PluginControllerFactory>("Could not find controller '{0}' from the BuildManager because a Namespace DataToken was not formatted properly as a string array", controllerName);
                    //ensure it is cached that we haven't found it so we don't try to find it again
                    _controllerCache.AddOrUpdate(key, typeof(NullController), (s, t) => typeof(NullController));
                }
            }            
            else
            {
                LogHelper.Warn<PluginControllerFactory>("Could not find controller '{0}' from the BuildManager because a Namespace DataToken was not set", controllerName);                
            }

            if (type != null)
            {
                LogHelper.TraceIfEnabled<PluginControllerFactory>("Found controller '{0}' from the BuildManager", () => controllerName);
                return type;
            }

            //its null , so use base implementation
            return base.GetControllerType(requestContext, controllerName);    

        }

        /// <summary>
        /// Just a type used to be stored in our cache for when we definitely cannot resolve a type
        /// </summary>
        private class NullController
        {
            
        }

    }
}