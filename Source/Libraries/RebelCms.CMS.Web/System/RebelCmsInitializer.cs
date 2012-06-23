using System.Web.Hosting;
using System.Web.Routing;
using RebelCms.CMS.Web.EmbeddedViewEngine;
using RebelCms.CMS.Web.Mvc.Areas;
using RebelCms.CMS.Web.Mvc.RoutableRequest;
using RebelCms.CMS.Web.Mvc.RouteHandlers;
using RebelCms.Framework.DependencyManagement;

namespace RebelCms.CMS.Web.RebelCmsSystem
{
    /// <summary>
    /// Used to setup all of the RebelCms specific MVC routes, AutoMapper and IoC
    /// </summary>
    internal class RebelCmsInitializer
    {
        private static bool _isInitialized;
        private static readonly object Locker = new object();
        private readonly IDependencyResolver _container;

        /// <summary>
        /// Creates a new RebelCmsInitializer
        /// </summary>
        /// <param name="container">The IoC container to use for manual resolution of objects</param>
        internal RebelCmsInitializer(IDependencyResolver container)
        {
            _container = container;
        }

        /// <summary>
        /// Performs the initialization
        /// </summary>
        /// <param name="routes"></param>
        /// <remarks>
        /// Initialization will only happen once
        /// </remarks>
        internal void Initialize(RouteCollection routes)
        {
            if (!_isInitialized)
            {
                lock (Locker)
                {
                    if (!_isInitialized)
                    {
                        //register the RebelCms area, this requires manually interventino because we have cosntructor dependencies on the RebelCmsArea
                        routes.RegisterArea<RebelCmsAreaRegistration>(_container);

                        //setup automapper
                        var autoMapperInit = _container.Resolve<AutoMapperInitializer>();
                        autoMapperInit.EnsureInitialised();

                        var routableRequestProvider = _container.Resolve<IRoutableRequestProvider>();

                        //setup routes and front-end route handler
                        //TODO: Remove magic strings
                        var frontEndRouteHandler = _container.Resolve<IRouteHandler>("FrontEndRouteHandler");

                        ConfigureFrontEndRoutes(routes, frontEndRouteHandler, routableRequestProvider);

                        //adds custom virtual path provider
                        HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedViewVirtualPathProvider());

                        //set as init
                        _isInitialized = true;
                    }
                }
            }
        }

        /// <summary>
        /// Creates the RebelCms routes for the front-end
        /// </summary>
        /// <param name="routes">The routes.</param>
        /// <param name="routeHandler">The route handler.</param>
        /// <param name="routableRequestProvider">The routable request provider.</param>
        internal static void ConfigureFrontEndRoutes(RouteCollection routes, IRouteHandler routeHandler,
                                                     IRoutableRequestProvider routableRequestProvider)
        {
            // Ignore standard stuff...
            System.Web.Mvc.RouteCollectionExtensions.IgnoreRoute(routes, "{resource}.axd/{*pathInfo}");
            System.Web.Mvc.RouteCollectionExtensions.IgnoreRoute(routes, "{*allaxd}",
                                                                 new {allaxd = @".*\.axd(/.*|\?.*)?"});
            System.Web.Mvc.RouteCollectionExtensions.IgnoreRoute(routes, "{*favicon}",
                                                                 new {favicon = @"(.*/)?favicon.ico(/.*)?"});

            System.Web.Mvc.RouteCollectionExtensions.MapRoute(
                //name
                routes, "RebelCms",
                //url to match (match all requests)
                "{*allpages}",
                //default options
                new {controller = "RebelCms", action = "Index"},
                //constraints
                new {umbPages = new RebelCmsRouteConstraint(routableRequestProvider)})
                //set the route handler
                .RouteHandler = routeHandler;
        }
    }
}