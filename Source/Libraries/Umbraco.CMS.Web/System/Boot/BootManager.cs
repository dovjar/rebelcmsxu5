using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.DependencyManagement.DemandBuilders;

using Umbraco.Framework;
using Umbraco.Framework.DependencyManagement;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Tasks;
using IDependencyResolver = Umbraco.Framework.DependencyManagement.IDependencyResolver;

namespace Umbraco.Cms.Web.System.Boot
{
    using Umbraco.Cms.Web.DependencyManagement;

    public class BootManager : DisposableObject
    {
        private readonly HttpApplication _app;
        private readonly DisposableTimer _timer;
        private readonly UmbracoContainerBuilder<HttpApplication> _umbracoWireup;
        private Func<AbstractContainerBuilder> _containerDelegate;
        private Type _dependencyResolverType;
        private Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> _mvcResolverFactory;
        private ApplicationTaskManager _taskManager;
        private Action _mvcAreaRegistration;
        private Action _mvcCustomRouteRegistration;
        private Action _mvcDefaultRouteRegistration;
        private Action _mvcGlobalFilterRegistration;

        public BootManager(HttpApplication app)
        {
            _timer = DisposableTimer.Start(timer => LogHelper.TraceIfEnabled<BootManager>("Application start took {0}ms", () => timer));
            _app = app;
            LogHelper.TraceIfEnabled<BootManager>("Created");
            _umbracoWireup = new UmbracoContainerBuilder<HttpApplication>(_app);
        }
       
        public BootManager InitializeContainer(Func<AbstractContainerBuilder> container)
        {
            LogHelper.TraceIfEnabled<BootManager>("Initialising container");
            _containerDelegate = container;
            return this;
        }

        public BootManager SetMvcResolverFactory(Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> mvcResolverFactory)
        {
            _mvcResolverFactory = mvcResolverFactory;
            return this;
        }

        public BootManager AddContainerBuildingHandler(EventHandler<ContainerBuilderEventArgs> handler)
        {
            _umbracoWireup.ContainerBuilding += handler;
            return this;
        }

        public BootManager AddContainerBuildingCompleteHandler(EventHandler<ContainerBuilderEventArgs> handler)
        {
            _umbracoWireup.ContainerBuildingComplete += handler;
            return this;
        }

        public BootManager AddAppErrorHandler(EventHandler handler)
        {
            _app.Error += handler;
            return this;
        }

        /// <summary>
        /// Assigns the method to register all MVC Areas
        /// </summary>
        /// <remarks>
        /// the registrationMethod will be called just before the RenderBootstrapper is executed, but after the CmsBootstrapper.
        /// </remarks>
        public BootManager MvcAreaRegistration(Action registrationMethod)
        {
            _mvcAreaRegistration = registrationMethod;
            return this;
        }

        /// <summary>
        /// Registers the MVC Global filters
        /// </summary>
        /// <param name="registrationMethod"></param>
        /// <returns></returns>
        public BootManager MvcGlobalFilterRegistration(Action registrationMethod)
        {
            _mvcGlobalFilterRegistration = registrationMethod;   
            return this;
        }

        /// <summary>
        /// Creates custom MVC routes
        /// </summary>
        /// <param name="registrationMethod"></param>
        /// <returns></returns>
        /// <remarks>
        /// This method will be called before the Umbraco catch all route is registered
        /// </remarks>
        public BootManager MvcCustomRouteRegistration(Action registrationMethod)
        {
            _mvcCustomRouteRegistration = registrationMethod;
            return this;
        }

        /// <summary>
        /// Creates the default MVC routes
        /// </summary>
        /// <param name="registrationMethod"></param>
        /// <returns></returns>
        /// <remarks>
        /// the registrationMethod will be called after the Umbraco catch all route is registered
        /// </remarks>
        public BootManager MvcDefaultRouteRegistration(Action registrationMethod)
        {
            _mvcDefaultRouteRegistration = registrationMethod;
            return this;
        }

        /// <summary>
        ///  Wires everything up
        /// </summary>
        /// <returns></returns>
        public virtual IUmbracoApplicationContext Boot()
        {
            Mandate.That<NullReferenceException>(_mvcDefaultRouteRegistration != null);
            Mandate.That<NullReferenceException>(_mvcAreaRegistration != null);
            Mandate.That<NullReferenceException>(_mvcCustomRouteRegistration != null);
            Mandate.That<NullReferenceException>(_mvcGlobalFilterRegistration != null);

            LogHelper.TraceIfEnabled<BootManager>("Booting");
            var container = _containerDelegate();
            container.AddDependencyDemandBuilder(_umbracoWireup);

            var built = container.Build();
            //var resolver = (IDependencyResolver)Activator.CreateInstance(_dependencyResolverType, new[] { built });
            var mvcResolver = _mvcResolverFactory.Invoke(built);
            DependencyResolver.SetResolver(mvcResolver);

            _taskManager = built.Resolve<ApplicationTaskManager>();

            //launch pre-app startup tasks
            _taskManager.ExecuteInContext(TaskTriggers.PreAppStartupComplete, new TaskExecutionContext(this, null));

            // Initialise the Umbraco system (v2)
            // If this is a Cms app:
            LogHelper.TraceIfEnabled<BootManager>("Booting CmsBootstrapper");
            built.Resolve<CmsBootstrapper>().Boot(RouteTable.Routes);

            //register all standard MVC components after the CmsBootstrapper (which registers our custom areas)
            LogHelper.TraceIfEnabled<BootManager>("Registering MVC areas");
            _mvcAreaRegistration();

            LogHelper.TraceIfEnabled<BootManager>("Registering custom MVC routes");
            _mvcCustomRouteRegistration();

            // If this is a front-end app:
            LogHelper.TraceIfEnabled<BootManager>("Booting RenderBootstrapper");
            built.Resolve<RenderBootstrapper>().Boot(RouteTable.Routes);

            LogHelper.TraceIfEnabled<BootManager>("Registering MVC global filters");
            _mvcGlobalFilterRegistration();

            LogHelper.TraceIfEnabled<BootManager>("Registering MVC default routes");
            _mvcDefaultRouteRegistration();

            //launch post app startup tasks
            _taskManager.ExecuteInContext(TaskTriggers.PostAppStartup, new TaskExecutionContext(this, null));

            return built.Resolve<IUmbracoApplicationContext>();
        }

        protected override void DisposeResources()
        {           

            _timer.Dispose();
        }
    }
}
