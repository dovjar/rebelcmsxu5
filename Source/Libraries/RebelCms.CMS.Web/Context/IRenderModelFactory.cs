using System.Web;
using System.Web.Routing;
using RebelCms.Cms.Web.Model;

namespace RebelCms.Cms.Web.Context
{
    public interface IRenderModelFactory
    {
        IRebelCmsRenderModel Create(HttpContextBase httpContext, string rawUrl);
    }
}