using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Mapping;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.ViewEngines;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;

namespace Rebel.Cms.Web.System.Boot
{
    public class RenderBootstrapper : AbstractBootstrapper
    {
        private readonly IRouteHandler _routeHandler;
        private readonly IRenderModelFactory _renderModelFactory;
        private readonly IRebelApplicationContext _applicationContext;

        public RenderBootstrapper(
            IRebelApplicationContext applicationContext, 
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

            //we requrie that a custom GlobalFilter is added so see if it is there:
            if (!GlobalFilters.Filters.ContainsFilter<ProxyableResultAttribute>())
            {
                GlobalFilters.Filters.Add(new ProxyableResultAttribute());
            }

            SetupRoutes(routes);

            //TODO: Once the issues are resolved with TwoLevelViewCache, put this back in.
            ////add custom view cache to the standard view engine
            //ViewEngines.Engines.RemoveAll(x => x.GetType() == typeof (RazorViewEngine));
            //var ve = new RazorViewEngine();
            //ve.ViewLocationCache = new TwoLevelViewCache(ve.ViewLocationCache);
            //ViewEngines.Engines.Add(ve);
        }

        private void SetupRoutes(RouteCollection routes)
        {
            // Declare media  routes
            // NOTE: We have to declare each combination as you can't mix optional / mandatory parameters
            routes.MapRoute("Media1", "Media/{mediaId}/{fileName}",
                new { controller = "MediaProxy", action = "Proxy", propertyAlias = "rebelFile", size = 0 },
                new { mediaFile = new MediaRouteConstraint() });

            routes.MapRoute("Media2", "Media/{mediaId}/{size}/{fileName}",
                new { controller = "MediaProxy", action = "Proxy", propertyAlias = "rebelFile" },
                new { mediaFile = new MediaRouteConstraint() });

            routes.MapRoute("Media3", "Media/{propertyAlias}/{mediaId}/{fileName}",
                new { controller = "MediaProxy", action = "Proxy", size = 0 },
                new { mediaFile = new MediaRouteConstraint() });

            routes.MapRoute("Media4", "Media/{propertyAlias}/{mediaId}/{size}/{fileName}",
                new { controller = "MediaProxy", action = "Proxy" },
                new { mediaFile = new MediaRouteConstraint() });

            routes.MapRoute(
                //name
                "Rebel",
                //url to match (match all requests)
                "{*allpages}",
                //default options
                new { controller = "Rebel", action = "Index" },
                //constraints
                new { umbPages = new RenderRouteConstraint(_applicationContext, _renderModelFactory) })
                //set the route handler
                .RouteHandler = _routeHandler;
        }
    }
}