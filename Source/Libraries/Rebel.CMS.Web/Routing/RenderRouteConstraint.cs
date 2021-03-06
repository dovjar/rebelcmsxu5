﻿using System.Web;
using System.Web.Routing;
using Rebel.Cms.Web.Context;

namespace Rebel.Cms.Web.Routing
{
    /// <summary>
    /// The rules of this route constraint must pass in order to be able to route Rebel requests to the front end
    /// </summary>
    public class RenderRouteConstraint : IncludeExcludeRouteConstraint
    {
        private readonly IRebelApplicationContext _applicationContext;
        private readonly IRenderModelFactory _modelFactory;

        public RenderRouteConstraint(IRebelApplicationContext applicationContext, IRenderModelFactory modelFactory)
            : base(applicationContext.Settings.RouteMatches)
        {
            _applicationContext = applicationContext;
            _modelFactory = modelFactory;
        }

        public override bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var includeExcludeMatch = base.Match(httpContext, route, parameterName, values, routeDirection);

            if (includeExcludeMatch)
            {

                //TODO: We need to add a local cache to this class for every route that comes in that doesn't match a current node we need to cache 
                // that somewhere so we don't have lookup the node again, otherwise for normal MVC requests this will always attempt to lookup
                // an item.

                //need to ensure the application is installed, if not then don't route this request
                if (!_applicationContext.AllProvidersInstalled())
                    return false;

                //ensure theres content to route to
                var renderModel = _modelFactory.Create(httpContext, httpContext.Request.RawUrl);
                if (renderModel == null || renderModel.CurrentNode == null)
                    return false;

            }
            return includeExcludeMatch;
        }
    }
}