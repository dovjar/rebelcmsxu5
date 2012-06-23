using System;
using System.Web;
using System.Web.Mvc;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.IO;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.Packaging;
using RebelCms.Cms.Web.Routing;
using RebelCms.Framework;

namespace RebelCms.Cms.Web.Context
{
    /// <summary>
    /// Encapsulates information specific to a request handled by the RebelCms back-office
    /// </summary>
    public class DefaultBackOfficeRequestContext : RoutableRequestContext, IBackOfficeRequestContext
    {

        public DefaultBackOfficeRequestContext(IRebelCmsApplicationContext applicationContext,
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
