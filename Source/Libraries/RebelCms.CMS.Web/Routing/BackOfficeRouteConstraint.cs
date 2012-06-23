using System.Web;
using System.Web.Routing;
using RebelCms.Cms.Web.Context;

namespace RebelCms.Cms.Web.Routing
{
    /// <summary>
    /// This constraint must pass to route anything to any back office controllers
    /// </summary>
    public class BackOfficeRouteConstraint : IRouteConstraint
    {
        private readonly IRebelCmsApplicationContext _applicationContext;

        public BackOfficeRouteConstraint(IRebelCmsApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
          
            return true;
        }
    }
}