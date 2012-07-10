using System;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Routing;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Context
{
    /// <summary>
    /// Encapsulates information specific to a request handled by Rebel
    /// </summary>
    public interface IRoutableRequestContext
    {
        /// <summary>
        /// Gets the request id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        Guid RequestId { get; }

        /// <summary>
        /// Gets the Rebel application context which contains services which last for the lifetime of the application.
        /// </summary>
        /// <value>The application.</value>
        IRebelApplicationContext Application { get; }

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
