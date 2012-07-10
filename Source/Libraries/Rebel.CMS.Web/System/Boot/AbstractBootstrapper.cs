using System.Web.Mvc;
using System.Web.Routing;

namespace Rebel.Cms.Web.System.Boot
{
    public abstract class AbstractBootstrapper
    {
        public virtual void Boot(RouteCollection routes)
        {
            routes.IgnoreStandardExclusions();

        }
    }
}