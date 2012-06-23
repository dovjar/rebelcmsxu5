using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.DependencyManagement.DemandBuilders;

using RebelCms.Framework;
using RebelCms.Framework.DependencyManagement;
using RebelCms.Framework.Diagnostics;
using RebelCms.Framework.Tasks;
using IDependencyResolver = RebelCms.Framework.DependencyManagement.IDependencyResolver;

namespace RebelCms.Cms.Web.System.Boot
{
    using RebelCms.Cms.Web.DependencyManagement;

    public class BootManager : DisposableObject
    {
        private readonly HttpApplication _app;
        private readonly DisposableTimer _timer;
        private readonly RebelCmsContainerBuilder<HttpApplication> _RebelCmsWireup;
        private Func<AbstractContainerBuilder> _containerDelegate;
        private Type _dependencyResolverType;
        private Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> _mvcResolverFactory;
        private ApplicationTaskManager _taskManager;

        public BootManager(HttpApplication app)
        {
            _timer = DisposableTimer.Start(timer => LogHelper.TraceIfEnabled<BootManager>("Application start took {0}ms", () => timer));
            _app = app;
            LogHelper.TraceIfEnabled<BootManager>("Created");
            _RebelCmsWireup = new RebelCmsContainerBuilder<HttpApplication>(_app);
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
            _RebelCmsWireup.ContainerBuilding += handler;
            return this;
        }

        public BootManager AddContainerBuildingCompleteHandler(EventHandler<ContainerBuilderEventArgs> handler)
        {
            _RebelCmsWireup.ContainerBuildingComplete += handler;
            return this;
        }

        public BootManager AddAppErrorHandler(EventHandler handler)
        {
            _app.Error += handler;
            return this;
        }

        /// <summary>
        ///  Wires everything up
        /// </summary>
        /// <returns></returns>
        public virtual IRebelCmsApplicationContext Boot()
        {
            LogHelper.TraceIfEnabled<BootManager>("Booting");
            var container = _containerDelegate();
            container.AddDependencyDemandBuilder(_RebelCmsWireup);

            var built = container.Build();
            //var resolver = (IDependencyResolver)Activator.CreateInstance(_dependencyResolverType, new[] { built });
            var mvcResolver = _mvcResolverFactory.Invoke(built);
            DependencyResolver.SetResolver(mvcResolver);

            _taskManager = built.Resolve<ApplicationTaskManager>();

            //launch pre-app startup tasks
            _taskManager.ExecuteInContext(TaskTriggers.PreAppStartupComplete, new TaskExecutionContext(this, null));

            // Initialise the RebelCms system (v2)
            // If this is a Cms app:
            LogHelper.TraceIfEnabled<BootManager>("Booting CmsBootstrapper");
            built.Resolve<CmsBootstrapper>().Boot(RouteTable.Routes);

            // If this is a front-end app:
            LogHelper.TraceIfEnabled<BootManager>("Booting RenderBootstrapper");
            built.Resolve<RenderBootstrapper>().Boot(RouteTable.Routes);

            //launch post app startup tasks
            _taskManager.ExecuteInContext(TaskTriggers.PostAppStartup, new TaskExecutionContext(this, null));

            return built.Resolve<IRebelCmsApplicationContext>();
        }

        protected override void DisposeResources()
        {           

            _timer.Dispose();
        }
    }
}
