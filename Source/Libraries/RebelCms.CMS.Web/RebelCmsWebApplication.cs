using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RebelCms.Cms.Web
{
    using RebelCms.Cms.Web.Context;
    using RebelCms.Cms.Web.DependencyManagement.DemandBuilders;
    using RebelCms.Cms.Web.System.Boot;
    using RebelCms.Framework;
    using RebelCms.Framework.DependencyManagement;
    using RebelCms.Framework.Diagnostics;
    using RebelCms.Framework.Tasks;
    using global::System.Reflection;
    using global::System.Web;
    using global::System.Web.Mvc;
    using global::System.Web.Routing;
    using IDependencyResolver = RebelCms.Framework.DependencyManagement.IDependencyResolver;

    public class RebelCmsWebApplication : DisposableObject
    {
        private readonly HttpApplication _httpApplication;
        private IRebelCmsApplicationContext _appContext;
        private readonly AbstractContainerBuilder _containerBuilder;
        private readonly Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> _mvcResolverFactory;

        public RebelCmsWebApplication(HttpApplication httpApplication, AbstractContainerBuilder containerBuilder, Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> mvcResolverFactory)
        {
            _mvcResolverFactory = mvcResolverFactory;
            _httpApplication = httpApplication;
            _containerBuilder = containerBuilder;
        }

        public static string ProductVersionInfo
        {
            get
            {
                var RebelCmsWebAssembly = typeof(RebelCmsWebApplication).Assembly;
                var att = RebelCmsWebAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
                var attName = att != null ? att.InformationalVersion : "(Version not found)";
                return attName;
            }
        }

        public static string ProductVersion
        {
            get
            {
                var RebelCmsWebAssembly = typeof(RebelCmsWebApplication).Assembly;
                return RebelCmsWebAssembly.GetName().Version.ToString();
            }
        }

        public virtual void OnEndRequest(object sender, EventArgs e)
        {
            DisposeScope();
        }

        protected virtual void DisposeScope()
        {
            //TODO: This is temporary until the render context registration has been refactored to properly dispose when out of http request scope
            //in which case the below is handled by the disposal of RoutableRequestContext
            if (_appContext != null && _appContext.FrameworkContext != null
                && _appContext.FrameworkContext.ScopedFinalizer != null)
            {
                _appContext.FrameworkContext.ScopedFinalizer.FinalizeScope();
            }
        }

        /// <summary>
        /// Gets the application.
        /// </summary>
        /// <value>The application.</value>
        public HttpApplication Application
        {
            get
            {
                return _httpApplication;
            }
        }

        public virtual void Start()
        {
            LogHelper.TraceIfEnabled<RebelCmsWebApplication>("Start called. Product info: {0} build {1}", () => RebelCmsWebApplication.ProductVersionInfo, () => RebelCmsWebApplication.ProductVersion);
            using (var bootManager = CreateBootManager())
            {
                bootManager
                    .AddAppErrorHandler(OnApplicationError)
                    .InitializeContainer(() => _containerBuilder)
                    .AddContainerBuildingHandler(OnContainerBuilding)
                    .AddContainerBuildingCompleteHandler(OnContainerBuildingComplete)
                    .SetMvcResolverFactory(_mvcResolverFactory);

                _appContext = Boot(bootManager);

                //standard MVC registration:
                AreaRegistration.RegisterAllAreas();
                RegisterGlobalFilters(GlobalFilters.Filters);
                RegisterRoutes(RouteTable.Routes);
            }
        }

        /// <summary>
        /// Boots the specified boot manager. This method can be overriden if implementors wish to modify the environment prior to RebelCms booting and building its dependency containers.
        /// </summary>
        /// <param name="bootManager">The boot manager.</param>
        protected virtual IRebelCmsApplicationContext Boot(BootManager bootManager)
        {
            return bootManager.Boot();
        }

        /// <summary>
        /// Event handler allowing developers to register their own types in the container
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnContainerBuilding(object sender, ContainerBuilderEventArgs e)
        {
            LogHelper.TraceIfEnabled<RebelCmsWebApplication>("Container is building");
        }

        protected virtual void OnContainerBuildingComplete(object sender, ContainerBuilderEventArgs e)
        {
            LogHelper.TraceIfEnabled<RebelCmsWebApplication>("Container building is complete");
        }

        protected virtual void OnApplicationError(object sender, EventArgs e)
        {
            var ex = HttpContext.Current.Error;
            LogHelper.Error<RebelCmsWebApplication>(ex.Message, ex);
            _appContext.FrameworkContext.TaskManager.ExecuteInContext(TaskTriggers.GlobalError, new TaskExecutionContext(sender, new TaskEventArgs(_appContext.FrameworkContext, e)));
            DisposeScope();
        }

        public virtual void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());            
        }

        public virtual void RegisterRoutes(RouteCollection routes)
        {
            // Ignore standard stuff...
            routes.IgnoreStandardExclusions();

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                );
        }

        /// <summary>
        /// Creates the boot manager.
        /// </summary>
        /// <returns></returns>
        public virtual BootManager CreateBootManager()
        {
            return new BootManager(Application);
        }

        protected override void DisposeResources()
        {
            _appContext.IfNotNull(x => x.DisposeIfDisposable());
        }
    }
}
