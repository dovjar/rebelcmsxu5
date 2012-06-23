using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.Mapping;
using RebelCms.Cms.Web.Mvc.ViewEngines;
using RebelCms.Cms.Web.Routing;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.System.Boot
{
    public class RenderBootstrapper : AbstractBootstrapper
    {
        private readonly IRouteHandler _routeHandler;
        private readonly IRenderModelFactory _renderModelFactory;
        private readonly IRebelCmsApplicationContext _applicationContext;

        public RenderBootstrapper(
            IRebelCmsApplicationContext applicationContext, 
            IRouteHandler routeHandler, 
            IRenderModelFactory renderModelFactory)
        {
            _routeHandler = routeHandler;
            _renderModelFactory = renderModelFactory;
            _applicationContext = applicationContext;
        }

        public override void Boot(RouteCollection routes)
        {
            base.Boot(routes);

            SetupRoutes(routes);

            //we're going to add a custom viewlocationcache to the normal built in razor view engine
            ViewEngines.Engines.RemoveAll(x => x.GetType() == typeof (RazorViewEngine));
            var ve = new RazorViewEngine();
            ve.ViewLocationCache = new TwoLevelViewCache(ve.ViewLocationCache);
            ViewEngines.Engines.Add(ve);
        }

        private void SetupRoutes(RouteCollection routes)
        {
            // Declare media  routes
            // NOTE: We have to declare each combination as you can't mix optional / mandatory parameters
            routes.MapRoute("Media1", "Media/{mediaId}/{fileName}",
                new { controller = "MediaProxy", action = "Proxy", propertyAlias = "RebelCmsFile", size = 0 },
                new { mediaFile = new MediaRouteConstraint() });

            routes.MapRoute("Media2", "Media/{mediaId}/{size}/{fileName}",
                new { controller = "MediaProxy", action = "Proxy", propertyAlias = "RebelCmsFile" },
                new { mediaFile = new MediaRouteConstraint() });

            routes.MapRoute("Media3", "Media/{propertyAlias}/{mediaId}/{fileName}",
                new { controller = "MediaProxy", action = "Proxy", size = 0 },
                new { mediaFile = new MediaRouteConstraint() });

            routes.MapRoute("Media4", "Media/{propertyAlias}/{mediaId}/{size}/{fileName}",
                new { controller = "MediaProxy", action = "Proxy" },
                new { mediaFile = new MediaRouteConstraint() });

            routes.MapRoute(
                //name
                "RebelCms",
                //url to match (match all requests)
                "{*allpages}",
                //default options
                new { controller = "RebelCms", action = "Index" },
                //constraints
                new { umbPages = new RenderRouteConstraint(_applicationContext, _renderModelFactory) })
                //set the route handler
                .RouteHandler = _routeHandler;
        }
    }
}