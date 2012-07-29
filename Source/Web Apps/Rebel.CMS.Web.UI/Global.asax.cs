using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.Caching;
using Rebel.Cms.Web.Mvc;
using Rebel.Cms.Web.System;
using Rebel.Framework;
using log4net.Config;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.DependencyManagement.DemandBuilders;
using Rebel.Cms.Web.Mvc.Areas;
using Rebel.Cms.Web.Mvc.ControllerFactories;
using Rebel.Cms.Web.System.Boot;
using Rebel.Cms.Web.Tasks;
using Rebel.Cms.Web.Trees;
using Rebel.Framework.DependencyManagement;
using Rebel.Framework.DependencyManagement.Autofac;
using System.Collections.Generic;
using Rebel.Framework.Diagnostics;
using IDependencyResolver = Rebel.Framework.DependencyManagement.IDependencyResolver;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Framework.Tasks;

namespace Rebel.Cms.Web.UI 
{
    using global::System.Threading;

    public class MvcApplication : HttpApplication
    {
        private static bool _isInitialised = false;
        private static bool _isFirstRequest = true;

        private static readonly object _mutex = new Object();
        private static readonly ReaderWriterLockSlim InitialiserLocker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        static MvcApplication()
        {
            LogHelper.TraceIfEnabled<MvcApplication>("Http application warmup");
        }

        protected static RebelWebApplication RebelWebApplication { get; set; }

        /// <summary>
        /// Initializes the application
        /// </summary>
        protected virtual void Application_Start()
        {
            using (new WriteLockDisposable(InitialiserLocker))
            {
                if (_isInitialised) return;
                RebelWebApplication = CreateRebelApplication();
                RebelWebApplication.Start();
                _isInitialised = true;
            }
        }
        
        protected virtual void Application_End()
        {
            var reason = HostingEnvironment.ShutdownReason;

            LogHelper.TraceIfEnabled<MvcApplication>("Shutting down due to: " + reason.ToString());
            LogHelper.TraceIfEnabled<MvcApplication>("Stack on shutdown: " + TryGetShutdownStack());
        }

        private static string TryGetShutdownStack()
        {
            try
            {
                var runtime = (HttpRuntime) typeof (HttpRuntime)
                                                .InvokeMember("_theRuntime",
                                                              BindingFlags.NonPublic | BindingFlags.Static |
                                                              BindingFlags.GetField,
                                                              null,
                                                              null,
                                                              null);

                if (runtime != null)
                {
                    return (string) runtime
                                        .GetType()
                                        .InvokeMember("_shutDownStack",
                                                      BindingFlags.NonPublic
                                                      | BindingFlags.Instance
                                                      | BindingFlags.GetField,
                                                      null,
                                                      runtime,
                                                      null);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<MvcApplication>("Couldn't get shutdown stack: " + ex.Message, ex);
            }
            return string.Empty;
        }

        protected virtual void Application_BeginRequest(object sender, EventArgs e)
        {
            if (_isFirstRequest) // lets warm up that cache the first time app starts up
            {
                lock(_mutex){
                    _isFirstRequest = false;
                }
                var host = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
                var thread = new Thread(() =>
                {
                    var cacheWarmer = new CacheWarmer(host);
                    cacheWarmer.TraverseFrom("/");
                }) { IsBackground = true };
                thread.Start();
            }
        }

        // RebelWebApplication cannot hook this event programatically in the Start event due to a known
        // bug in ASP.NET causing a NullRef in PipelineRequestManager if events are hooked in the application init (http://forums.asp.net/t/1327102.aspx/1)
        protected virtual void Application_EndRequest(object sender, EventArgs e)
        {
            RebelWebApplication.IfNotNull(x => x.OnEndRequest(sender, e));
        }

        /// <summary>
        /// Creates the container builder. Override this if you'd like to use an alternative <see cref="AbstractContainerBuilder"/> to the default.
        /// </summary>
        /// <returns></returns>
        protected virtual AbstractContainerBuilder CreateContainerBuilder()
        {
            return new AutofacContainerBuilder();
        }

        protected virtual Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> MvcResolverFactory()
        {
            return x => new AutofacMvcResolver(x);
        }

        /// <summary>
        /// Creates the Rebel application. Override this if you'd like to use your own extended version of <see cref="RebelWebApplication"/>.
        /// </summary>
        /// <returns></returns>
        protected virtual RebelWebApplication CreateRebelApplication()
        {
            return new RebelWebApplication(
                this, 
                CreateContainerBuilder(), 
                MvcResolverFactory(),
                RegisterMvcAreas,
                () => RegisterMvcGlobalFilters(GlobalFilters.Filters),
                () => RegisterCustomMvcRoutes(RouteTable.Routes),
                () => RegisterDefaultMvcRoutes(RouteTable.Routes));
        }

        /// <summary>
        /// Registers MVC Areas
        /// </summary>
        protected virtual void RegisterMvcAreas()
        {
            RebelWebApplication.RegisterAllDefaultMvcAreas();
        }

        /// <summary>
        /// Registers custom MVC Routes which occur before the Rebel catch all route
        /// </summary>
        /// <param name="routes"></param>
        protected virtual void RegisterCustomMvcRoutes(RouteCollection routes)
        {
            
        }

        /// <summary>
        /// Registers default MVC Routes which occur after the Rebel catch all route
        /// </summary>
        /// <param name="routes"></param>
        protected virtual void RegisterDefaultMvcRoutes(RouteCollection routes)
        {
            RebelWebApplication.RegisterDefaultRoutes(routes);
        }

        /// <summary>
        /// Registers MVC Global Filters
        /// </summary>
        /// <param name="filters"></param>
        protected virtual void RegisterMvcGlobalFilters(GlobalFilterCollection filters)
        {
            RebelWebApplication.RegisterDefaultGlobalFilters(filters);  
        }

    }
}