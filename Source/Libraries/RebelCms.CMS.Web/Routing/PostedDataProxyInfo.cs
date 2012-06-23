using System;
using System.Web.Mvc;
using RebelCms.Cms.Web.Model;

namespace RebelCms.Cms.Web.Routing
{

    /// <summary>
    /// Represents the data required to route to a specific controller/action during an RebelCms request
    /// </summary>
    public class RouteDefinition
    {
        public string ControllerName { get; set; }
        public string ActionName { get; set; }

        /// <summary>
        /// The Controller instance found for routing to
        /// </summary>
        public ControllerBase Controller { get; set; }

        /// <summary>
        /// The current RenderModel found for the request
        /// </summary>
        public IRebelCmsRenderModel RenderModel { get; set; }
    }

    /// <summary>
    /// Represents the data required to proxy a request to a surface controller for posted data
    /// </summary>
    public class PostedDataProxyInfo : RouteDefinition
    {
        public string Area { get; set; }
        public Guid SurfaceId { get; set; }
    }
}