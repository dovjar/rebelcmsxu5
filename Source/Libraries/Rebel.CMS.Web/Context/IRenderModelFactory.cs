using System.Web;
using System.Web.Routing;
using Rebel.Cms.Web.Model;

namespace Rebel.Cms.Web.Context
{
    public interface IRenderModelFactory
    {
        IRebelRenderModel Create(HttpContextBase httpContext, string rawUrl);
    }
}