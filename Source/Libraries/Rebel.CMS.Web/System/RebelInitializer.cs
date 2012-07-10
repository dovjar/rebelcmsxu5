using System.Web.Hosting;
using System.Web.Routing;
using Rebel.CMS.Web.EmbeddedViewEngine;
using Rebel.CMS.Web.Mvc.Areas;
using Rebel.CMS.Web.Mvc.RoutableRequest;
using Rebel.CMS.Web.Mvc.RouteHandlers;
using Rebel.Framework.DependencyManagement;

namespace Rebel.CMS.Web.RebelSystem
{
    /// <summary>
    /// Used to setup all of the Rebel specific MVC routes, AutoMapper and IoC
    /// </summary>
    internal class RebelInitializer
    {
        private static bool _isInitialized;
        private static readonly object Locker = new object();
        private readonly IDependencyResolver _container;

        /// <summary>
        /// Creates a new RebelInitializer
        /// </summary>
        /// <param name="container">The IoC container to use for manual resolution of objects</param>
        internal RebelInitializer(IDependencyResolver container)
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
                        //register the Rebel area, this requires manually interventino because we have cosntructor dependencies on the RebelArea
                        routes.RegisterArea<RebelAreaRegistration>(_container);

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
        /// Creates the Rebel routes for the front-end
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
                routes, "Rebel",
                //url to match (match all requests)
                "{*allpages}",
                //default options
                new {controller = "Rebel", action = "Index"},
                //constraints
                new {umbPages = new RebelRouteConstraint(routableRequestProvider)})
                //set the route handler
                .RouteHandler = routeHandler;
        }
    }
}