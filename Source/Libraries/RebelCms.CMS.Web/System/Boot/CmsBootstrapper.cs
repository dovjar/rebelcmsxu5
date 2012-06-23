using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using DataAnnotationsExtensions.ClientValidation;
using RebelCms.Cms.Web.Configuration;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.EmbeddedViewEngine;
using RebelCms.Cms.Web.Mapping;
using RebelCms.Cms.Web.Model;
using RebelCms.Cms.Web.Mvc.Areas;
using RebelCms.Cms.Web.Mvc.Validation;
using RebelCms.Framework.Context;
using RebelCms.Framework.Localization.Web;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Tasks;

namespace RebelCms.Cms.Web.System.Boot
{
    public class CmsBootstrapper : AbstractBootstrapper
    {
        private readonly RebelCmsSettings _settings;
        private readonly RebelCmsAreaRegistration _areaRegistration;
        private readonly InstallAreaRegistration _installRegistration;
        private readonly IEnumerable<PackageAreaRegistration> _componentAreas;
        private readonly IAttributeTypeRegistry _attributeTypeRegistry;

        public CmsBootstrapper(RebelCmsSettings settings,
            RebelCmsAreaRegistration areaRegistration,
            InstallAreaRegistration installRegistration,
            IEnumerable<PackageAreaRegistration> componentAreas)
        {
            _areaRegistration = areaRegistration;
            _installRegistration = installRegistration;
            _componentAreas = componentAreas;
            _settings = settings;
            _attributeTypeRegistry = new DependencyResolverAttributeTypeRegistry();
        }

        public CmsBootstrapper(RebelCmsSettings settings,
            RebelCmsAreaRegistration areaRegistration,
            InstallAreaRegistration installRegistration,
            IEnumerable<PackageAreaRegistration> componentAreas,
            IAttributeTypeRegistry attributeTypeRegistry)
        {
            _areaRegistration = areaRegistration;
            _installRegistration = installRegistration;
            _componentAreas = componentAreas;
            _attributeTypeRegistry = attributeTypeRegistry;
            _settings = settings;
        }

        public override void Boot(RouteCollection routes)
        {
            base.Boot(routes);

            routes.RegisterArea(_installRegistration);
            //register all component areas
            foreach (var c in _componentAreas)
            {
                routes.RegisterArea(c);
            }

            //IMPORTANT: We need to register the RebelCms area after the components because routes overlap.
            // For example, a surface controller might have a url of:
            //   /RebelCms/MyPackage/Surface/MySurface
            // and because the default action is 'Index' its not required to be there, however this same route
            // matches the default RebelCms back office route of RebelCms/{controller}/{action}/{id}
            // so we want to make sure that the plugin routes are matched first
            routes.RegisterArea(_areaRegistration);

            //ensure that the IAttributeTypeRegistry is set
            AttributeTypeRegistry.SetCurrent(_attributeTypeRegistry);

            //register validation extensions
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();

            LocalizationWebConfig.RegisterRoutes(routes, _settings.RebelCmsPaths.LocalizationPath);
            
            //If this is outside of an ASP.Net application (i.e. Unit test) and RegisterVirtualPathProvider is called then an exception is thrown.
            if (HostingEnvironment.IsHosted)
            {
                HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedViewVirtualPathProvider());    
            }
            

            //register custom validation adapters
            DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(
                typeof (HiveIdRequiredAttribute),
                (metadata, controllerContext, attribute) =>
                new RequiredHiveIdAttributeAdapter(metadata, controllerContext, (HiveIdRequiredAttribute) attribute));

        }

    }
}
