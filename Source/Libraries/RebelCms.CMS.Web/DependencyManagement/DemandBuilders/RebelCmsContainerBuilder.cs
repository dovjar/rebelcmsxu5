using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using RebelCms.Cms.Web.Configuration;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.IO;
using RebelCms.Cms.Web.Mapping;
using RebelCms.Cms.Web.Mvc.Areas;
using RebelCms.Cms.Web.Mvc.ControllerFactories;
using RebelCms.Cms.Web.Packaging;
using RebelCms.Cms.Web.Routing;
using RebelCms.Cms.Web.Security;
using RebelCms.Cms.Web.System;
using RebelCms.Cms.Web.System.Boot;

using RebelCms.Framework;
using RebelCms.Framework.Context;
using RebelCms.Framework.DependencyManagement;
using System.Linq;

using RebelCms.Framework.Localization.Configuration;
using RebelCms.Framework.Localization.Web;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Security;
using RebelCms.Framework.Tasks;
using RebelCms.Framework.TypeMapping;
using RebelCms.Hive;

namespace RebelCms.Cms.Web.DependencyManagement.DemandBuilders
{
    /// <summary>
    /// A module that creates and build the IoC container that powers the RebelCms core
    /// </summary>
    /// <typeparam name="TWebApp">The type of the web application defined in the web application's assembly</typeparam>
    public class RebelCmsContainerBuilder<TWebApp> : IDependencyDemandBuilder
        where TWebApp : HttpApplication
    {
        #region Constructors

        /// <summary>
        /// Default constructor that uses the standard RebelCmsComponentRegistrar as the IComponentRegistrar
        /// </summary>
        /// <param name="httpApp"></param>
        public RebelCmsContainerBuilder(TWebApp httpApp)
        {
            _httpApp = httpApp;
            _settings = RebelCmsSettings.GetSettings();
            _componentRegistrar = new RebelCmsComponentRegistrar();
        }

        /// <summary>
        /// Custom constructor that can be used to assign a custom IComponentRegistrar
        /// </summary>
        /// <param name="httpApp"></param>
        /// <param name="componentRegistrar"></param>
        /// <param name="settings"></param>        
        public RebelCmsContainerBuilder(TWebApp httpApp, IComponentRegistrar componentRegistrar, RebelCmsSettings settings)
        {
            _httpApp = httpApp;
            _componentRegistrar = componentRegistrar;
            _settings = settings;
        }

        #endregion

        #region Private fields

        private readonly IComponentRegistrar _componentRegistrar;
        private readonly RebelCmsSettings _settings;
        private readonly HttpApplication _httpApp;
        private readonly object _locker = new object();

        #endregion

        #region Events

        /// <summary>
        /// Event raised just before the container is built allow developers to register their own 
        /// objects in the container
        /// </summary>
        public event EventHandler<ContainerBuilderEventArgs> ContainerBuilding;

        /// <summary>
        /// Event raised after the container is built allow developers to register their own 
        /// objects in the container
        /// </summary>
        public event EventHandler<ContainerBuilderEventArgs> ContainerBuildingComplete;

        #endregion

        #region Protected event raising methods

        protected void OnContainerBuilding(ContainerBuilderEventArgs args)
        {
            if (ContainerBuilding != null)
            {
                ContainerBuilding(this, args);
            }
        }

        protected void OnContainerBuildingComplete(ContainerBuilderEventArgs args)
        {
            if (ContainerBuildingComplete != null)
            {
                ContainerBuildingComplete(this, args);
            }
        }

        #endregion

        #region IDependencyDemandBuilder Members

        /// <summary>Builds the dependency demands required by this implementation. </summary>
        /// <param name="builder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="builderContext"></param>
        public void Build(IContainerBuilder builder, IBuilderContext builderContext)
        {
            //raise the building event
            OnContainerBuilding(new ContainerBuilderEventArgs(builder));
            
            //register all of the abstract web types
            builder.AddDependencyDemandBuilder(new WebTypesDemandBuilder(_httpApp));
            
            var typeFinder = new TypeFinder();
            builder.ForInstanceOfType(typeFinder)
                .ScopedAs.Singleton();

            //register the RebelCms settings
            builder.ForFactory(x => RebelCmsSettings.GetSettings())
                .KnownAsSelf()
                .ScopedAs.Singleton(); //only have one instance ever

            //register our MVC types
            builder.AddDependencyDemandBuilder(new MvcTypesDemandBuilder(typeFinder));

            // Register the IRoutableRequestContext
            builder.For<HttpRequestScopedCache>().KnownAs<AbstractScopedCache>().ScopedAs.Singleton();
            builder.For<HttpRuntimeApplicationCache>().KnownAs<AbstractApplicationCache>().ScopedAs.Singleton();
            builder.For<HttpRequestScopedFinalizer>().KnownAs<AbstractFinalizer>().ScopedAs.Singleton();
            builder.For<DefaultFrameworkContext>().KnownAs<IFrameworkContext>().ScopedAs.Singleton();
            builder.For<RebelCmsApplicationContext>().KnownAs<IRebelCmsApplicationContext>().ScopedAs.Singleton();
            builder.For<RoutableRequestContext>().KnownAs<IRoutableRequestContext>().ScopedAs.HttpRequest();
            builder.For<DefaultBackOfficeRequestContext>().KnownAs<IBackOfficeRequestContext>().ScopedAs.HttpRequest();

            // TODO: Ensure this isn't created manually anywhere but tests, only via a factory: builder.ForType<IRebelCmsRenderModel, RebelCmsRenderContext>().Register().ScopedPerHttpRequest();
            builder.For<DefaultRenderModelFactory>().KnownAs<IRenderModelFactory>().
                ScopedAs.Singleton();

            // Register Hive provider
            //builder.AddDependencyDemandBuilder(new HiveDemandBuilder());
            builder.AddDependencyDemandBuilder(new Hive.DemandBuilders.HiveDemandBuilder());

            // Register Persistence provider loader
            //builder.AddDependencyDemandBuilder(new Framework.Persistence.DependencyManagement.DemandBuilders.LoadFromPersistenceConfig());
            builder.AddDependencyDemandBuilder(new Hive.DemandBuilders.LoadFromPersistenceConfig());

            // Register Cms bootstrapper
            // TODO: Split RebelCmsContainerBuilder between Cms and Frontend variants / needs
            builder.For<CmsBootstrapper>().KnownAsSelf();
            // Register Frontend bootstrapper
            builder.ForFactory(
                x =>
                new RenderBootstrapper(
                    x.Resolve<IRebelCmsApplicationContext>(),
                    x.Resolve<IRouteHandler>(RenderRouteHandler.SingletonServiceName),
                    x.Resolve<IRenderModelFactory>())
                )
                .KnownAsSelf();
            
            //register all component areas, loop through all found package folders
            //TODO: All other places querying for packages use the NuGet IO FileManager stuff, not the standard .Net IO classes
            var pluginFolder = new DirectoryInfo(_httpApp.Server.MapPath(_settings.PluginConfig.PluginsPath));
            foreach (var package in pluginFolder.GetDirectories(PluginManager.PackagesFolderName)
                .SelectMany(x => x.GetDirectories()
                    .Where(PluginManager.IsPackagePluginFolder)))
            {
                //register an area for this package
                builder.For<PackageAreaRegistration>()
                    .KnownAsSelf()
                    .WithNamedParam("packageFolder", package);
            }

            //register the RoutingEngine
            builder
                .For<DefaultRoutingEngine>()
                .KnownAs<IRoutingEngine>()
                .ScopedAs.HttpRequest();

            //register the package context
            builder
                .ForFactory(x => new DefaultPackageContext(x.Resolve<RebelCmsSettings>(), HostingEnvironment.MapPath))
                //.For<DefaultPackageContext>()
                .KnownAs<IPackageContext>()
                .ScopedAs.Singleton();

            //register the PropertyEditorFactory
            builder.For<PropertyEditorFactory>()
                .KnownAs<IPropertyEditorFactory>()
                .ScopedAs.Singleton();

            //register the ParameterEditorFactory
            builder.For<ParameterEditorFactory>()
                .KnownAs<IParameterEditorFactory>()
                .ScopedAs.Singleton();

            //register the SecurityService
            builder.For<SecurityService>()
                .KnownAs<ISecurityService>();

            //register the CmsAttributeTypeRegistry
            builder.For<CmsAttributeTypeRegistry>()
                .KnownAs<IAttributeTypeRegistry>()
                .ScopedAs.Singleton();

            //component registration
            _componentRegistrar.RegisterTasks(builder, typeFinder);
            _componentRegistrar.RegisterTreeControllers(builder, typeFinder);
            _componentRegistrar.RegisterPropertyEditors(builder, typeFinder);
            _componentRegistrar.RegisterParameterEditors(builder, typeFinder);
            _componentRegistrar.RegisterEditorControllers(builder, typeFinder);
            _componentRegistrar.RegisterMenuItems(builder, typeFinder);
            _componentRegistrar.RegisterSurfaceControllers(builder, typeFinder);
            _componentRegistrar.RegisterDashboardFilters(builder, typeFinder);
            _componentRegistrar.RegisterDashboardMatchRules(builder, typeFinder);
            _componentRegistrar.RegisterPermissions(builder, typeFinder);
            _componentRegistrar.RegisterMacroEngines(builder, typeFinder);

            //register the registrations
            builder.For<ComponentRegistrations>().KnownAsSelf();

            //register task manager
            builder.For<ApplicationTaskManager>().KnownAsSelf().ScopedAs.Singleton();
          
            //register our model mappings and resolvers
            builder.AddDependencyDemandBuilder(new ModelMappingsDemandBuilder());

            //TODO: More stuff should happen with the TextManager here (e.g. db access and whatnot)
            //The user may later override settings, most importantly the LocalizationConfig.CurrentTextManager delegate to implement different environments
            //The text manager is assumed to be set up by the framework
            var textManager = LocalizationConfig.TextManager;
            LocalizationWebConfig.ApplyDefaults<TWebApp>(textManager, overridesPath: "~/App_Data/RebelCms/LocalizationEntries.xml");
            LocalizationWebConfig.SetupMvcDefaults(setupMetadata: false);

            //The name of the assembly that contains common texts
            textManager.FallbackNamespaces.Add("RebelCms.Cms.Web");

            OnContainerBuildingComplete(new ContainerBuilderEventArgs(builder));
        }

        #endregion
    }
}