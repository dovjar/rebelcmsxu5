using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rebel.Cms.Web.Caching;
using Rebel.Cms.Web.Mvc;
using Rebel.Cms.Web.Mvc.ActionFilters;

namespace Rebel.Cms.Web
{
    using Rebel.Cms.Web.Context;
    using Rebel.Cms.Web.DependencyManagement.DemandBuilders;
    using Rebel.Cms.Web.System.Boot;
    using Rebel.Framework;
    using Rebel.Framework.DependencyManagement;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Tasks;
    using global::System.Reflection;
    using global::System.Web;
    using global::System.Web.Mvc;
    using global::System.Web.Routing;
    using IDependencyResolver = Rebel.Framework.DependencyManagement.IDependencyResolver;

    public class RebelWebApplication : DisposableObject
    {
        private readonly HttpApplication _httpApplication;
        private IRebelApplicationContext _appContext;
        private readonly AbstractContainerBuilder _containerBuilder;
        private readonly Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> _mvcResolverFactory;
        private readonly Action _registerAreas;
        private readonly Action _registerFilters;
        private readonly Action _registerCustomRoutes;
        private readonly Action _registerDefaultRoutes;
        private static bool _isFirstRequest = true;
        private static readonly object Mutex = new Object();


        /// <summary>
        /// Constructor which sets up custom logic to register Areas, Global Filters and standard MVC routes
        /// </summary>
        /// <param name="httpApplication"></param>
        /// <param name="containerBuilder"></param>
        /// <param name="mvcResolverFactory"></param>
        /// <param name="registerAreas"></param>
        /// <param name="registerFilters"></param>
        /// <param name="registerCustomRoutes"></param>
        /// <param name="registerDefaultRoutes"> </param>
        public RebelWebApplication(
            HttpApplication httpApplication, 
            AbstractContainerBuilder containerBuilder, 
            Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> mvcResolverFactory,
            Action registerAreas,
            Action registerFilters,
            Action registerCustomRoutes,
            Action registerDefaultRoutes)
        {
            _mvcResolverFactory = mvcResolverFactory;
            _registerAreas = registerAreas;
            _registerFilters = registerFilters;
            _registerCustomRoutes = registerCustomRoutes;
            _registerDefaultRoutes = registerDefaultRoutes;
            _httpApplication = httpApplication;
            _containerBuilder = containerBuilder;
        }

        /// <summary>
        /// Constructor which sets up the default logic to register Areas, Global Filters and standard MVC routes
        /// </summary>
        /// <param name="httpApplication"></param>
        /// <param name="containerBuilder"></param>
        /// <param name="mvcResolverFactory"></param>
        public RebelWebApplication(
            HttpApplication httpApplication,
            AbstractContainerBuilder containerBuilder,
            Func<IDependencyResolver, global::System.Web.Mvc.IDependencyResolver> mvcResolverFactory)
        {
            _mvcResolverFactory = mvcResolverFactory;
            _registerAreas = RegisterAllDefaultMvcAreas;
            _registerFilters = () => RegisterDefaultGlobalFilters(GlobalFilters.Filters);
            _registerDefaultRoutes = () => RegisterDefaultRoutes(RouteTable.Routes);
            _httpApplication = httpApplication;
            _containerBuilder = containerBuilder;
        }

        public static string ProductVersionInfo
        {
            get
            {
                var rebelWebAssembly = typeof(RebelWebApplication).Assembly;
                var att = rebelWebAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), true).OfType<AssemblyInformationalVersionAttribute>().FirstOrDefault();
                var attName = att != null ? att.InformationalVersion : "(Version not found)";
                return attName;
            }
        }

        public static string ProductVersion
        {
            get
            {
                var rebelWebAssembly = typeof(RebelWebApplication).Assembly;
                return rebelWebAssembly.GetName().Version.ToString();
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
            if (AppContext != null && AppContext.FrameworkContext != null
                && AppContext.FrameworkContext.ScopedFinalizer != null)
            {
                AppContext.FrameworkContext.ScopedFinalizer.FinalizeScope();
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

        public IRebelApplicationContext AppContext
        {
            get
            {
                if (!_isStarted)
                    throw new InvalidOperationException("AppContext property can't be accessed until Start is called, as this is what boots up the applicatio and initialises the context itself");
                return _appContext;
            }
        }

        private bool _isStarted = false;
        public virtual IRebelApplicationContext Start()
        {
            LogHelper.TraceIfEnabled<RebelWebApplication>("Start called. Product info: {0} build {1}", () => RebelWebApplication.ProductVersionInfo, () => RebelWebApplication.ProductVersion);
            using (var bootManager = CreateBootManager())
            {
                bootManager
                    .AddAppErrorHandler(OnApplicationError)
                    .InitializeContainer(() => _containerBuilder)
                    .AddContainerBuildingHandler(OnContainerBuilding)
                    .AddContainerBuildingCompleteHandler(OnContainerBuildingComplete)
                    .SetMvcResolverFactory(_mvcResolverFactory)
                    .MvcAreaRegistration(_registerAreas)
                    .MvcCustomRouteRegistration(_registerCustomRoutes)
                    .MvcDefaultRouteRegistration(_registerDefaultRoutes)
                    .MvcGlobalFilterRegistration(_registerFilters);

                _appContext = Boot(bootManager);
                _isStarted = true;
            }
            return AppContext;
        }

        /// <summary>
        /// Boots the specified boot manager. This method can be overriden if implementors wish to modify the environment prior to Rebel booting and building its dependency containers.
        /// </summary>
        /// <param name="bootManager">The boot manager.</param>
        protected virtual IRebelApplicationContext Boot(BootManager bootManager)
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
            LogHelper.TraceIfEnabled<RebelWebApplication>("Container is building");
        }

        protected virtual void OnContainerBuildingComplete(object sender, ContainerBuilderEventArgs e)
        {
            LogHelper.TraceIfEnabled<RebelWebApplication>("Container building is complete");
        }

        protected virtual void OnApplicationError(object sender, EventArgs e)
        {
            var ex = HttpContext.Current.Error;
            LogHelper.Error<RebelWebApplication>(ex.Message, ex);
            AppContext.FrameworkContext.TaskManager.ExecuteInContext(TaskTriggers.GlobalError, new TaskExecutionContext(sender, new TaskEventArgs(AppContext.FrameworkContext, e)));
            DisposeScope();
        }
        
        public static void RegisterAllDefaultMvcAreas()
        {
            AreaRegistration.RegisterAllAreas();
        }

        public static void RegisterDefaultGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterDefaultRoutes(RouteCollection routes)
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
            AppContext.IfNotNull(x => x.DisposeIfDisposable());
        }
    }
}
