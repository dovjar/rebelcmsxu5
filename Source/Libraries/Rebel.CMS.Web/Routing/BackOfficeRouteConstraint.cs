using System.Web;
using System.Web.Routing;
using Rebel.Cms.Web.Context;

namespace Rebel.Cms.Web.Routing
{
    /// <summary>
    /// This constraint must pass to route anything to any back office controllers
    /// </summary>
    public class BackOfficeRouteConstraint : IRouteConstraint
    {
        private readonly IRebelApplicationContext _applicationContext;

        public BackOfficeRouteConstraint(IRebelApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
          
            return true;
        }
    }
}