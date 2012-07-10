using System;
using System.Web;
using System.Web.Mvc;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.IO;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Packaging;
using Rebel.Cms.Web.Routing;
using Rebel.Framework;

namespace Rebel.Cms.Web.Context
{
    /// <summary>
    /// Encapsulates information specific to a request handled by the Rebel back-office
    /// </summary>
    public class DefaultBackOfficeRequestContext : RoutableRequestContext, IBackOfficeRequestContext
    {

        public DefaultBackOfficeRequestContext(IRebelApplicationContext applicationContext,
            HttpContextBase httpContext,
            ComponentRegistrations components,
            IPackageContext packageContext,
            IRoutingEngine routingEngine)
            : base(applicationContext, components, routingEngine)
        {

            //create the IO resolvers
            var urlHelper = new UrlHelper(httpContext.Request.RequestContext);
            DocumentTypeIconResolver = new DocumentTypeIconFileResolver(httpContext.Server, applicationContext.Settings, urlHelper);
            DocumentTypeThumbnailResolver = new DocumentTypeThumbnailFileResolver(httpContext.Server, applicationContext.Settings, urlHelper);
            ApplicationIconResolver = new ApplicationIconFileResolver(httpContext.Server, applicationContext.Settings, urlHelper);

            PackageContext = packageContext;
        }

        /// <summary>
        /// Gets the package context.
        /// </summary>
        public IPackageContext PackageContext { get; private set; }

        public SpriteIconFileResolver DocumentTypeIconResolver { get; protected set; }

        public SpriteIconFileResolver ApplicationIconResolver { get; protected set; }

        public IResolver<Icon> DocumentTypeThumbnailResolver { get; protected set; }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            Application.FrameworkContext.ScopedFinalizer.FinalizeScope();
        }


    }
}
