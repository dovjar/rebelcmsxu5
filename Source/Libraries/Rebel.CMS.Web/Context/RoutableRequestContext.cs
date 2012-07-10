using System;
using System.Web.Mvc;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.IO;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Context
{
    /// <summary>
    /// Default implementation of <see cref="IRoutableRequestContext"/>. Encapsulates information specific to a request handled by Rebel.
    /// </summary>
    public class RoutableRequestContext : DisposableObject, IRoutableRequestContext
    {
        private Guid? _requestId = null;

        public RoutableRequestContext(IRebelApplicationContext applicationContext, ComponentRegistrations components, IRoutingEngine routingEngine)
        {
            Mandate.ParameterNotNull(applicationContext, "applicationContext");
            Mandate.ParameterNotNull(components, "components");
            Mandate.ParameterNotNull(routingEngine, "routingEngine");

            Application = applicationContext;
            RegisteredComponents = components;
            RoutingEngine = routingEngine;
        }

        /// <summary>
        /// Gets the request id, useful for debugging or tracing.
        /// </summary>
        /// <value>The request id.</value>
        public Guid RequestId
        {
            get
            {
                if (_requestId == null)
                    _requestId = Guid.NewGuid();
                return _requestId.Value;
            }
        }

        /// <summary>
        /// Gets the Rebel application context which contains services which last for the lifetime of the application.
        /// </summary>
        /// <value>The application.</value>
        public IRebelApplicationContext Application { get; protected set; }

        /// <summary>
        /// Lists all plugin components registered
        /// </summary>
        public ComponentRegistrations RegisteredComponents { get; private set; }

        /// <summary>
        /// Gets the URL utility.
        /// </summary>
        public IRoutingEngine RoutingEngine { get; private set; }


        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        /// <remarks></remarks>
        protected override void DisposeResources()
        {
            // Nothing managed by this instance
            return;
        }
    }


}
