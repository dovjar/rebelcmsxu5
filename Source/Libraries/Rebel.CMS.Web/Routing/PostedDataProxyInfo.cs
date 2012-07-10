using System;
using System.Web.Mvc;
using Rebel.Cms.Web.Model;

namespace Rebel.Cms.Web.Routing
{

    /// <summary>
    /// Represents the data required to route to a specific controller/action during an Rebel request
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
        public IRebelRenderModel RenderModel { get; set; }
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