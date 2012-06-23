using System;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.Routing;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.RepositoryTypes;

namespace RebelCms.Cms.Web.Context
{
    /// <summary>
    /// Encapsulates information specific to a request handled by RebelCms
    /// </summary>
    public interface IRoutableRequestContext
    {
        /// <summary>
        /// Gets the request id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        Guid RequestId { get; }

        /// <summary>
        /// Gets the RebelCms application context which contains services which last for the lifetime of the application.
        /// </summary>
        /// <value>The application.</value>
        IRebelCmsApplicationContext Application { get; }

        /// <summary>
        /// Lists all plugin components registered
        /// </summary>
        ComponentRegistrations RegisteredComponents { get; }

        /// <summary>
        /// Gets the URL utility.
        /// </summary>
        IRoutingEngine RoutingEngine { get; }

    }
}
