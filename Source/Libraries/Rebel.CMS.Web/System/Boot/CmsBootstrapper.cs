using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using DataAnnotationsExtensions.ClientValidation;
using Rebel.Cms.Web.BuildManagerCodeDelegates;
using Rebel.Cms.Web.Configuration;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.EmbeddedViewEngine;
using Rebel.Cms.Web.Mapping;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Mvc.ActionFilters;
using Rebel.Cms.Web.Mvc.Areas;
using Rebel.Cms.Web.Mvc.Validation;
using Rebel.Framework.Context;
using Rebel.Framework.Localization.Web;
using Rebel.Framework.Persistence;
using Rebel.Framework.Tasks;

namespace Rebel.Cms.Web.System.Boot
{
    public class CmsBootstrapper : AbstractBootstrapper
    {
        private readonly RebelSettings _settings;
        private readonly RebelAreaRegistration _areaRegistration;
        private readonly InstallAreaRegistration _installRegistration;
        private readonly IEnumerable<PackageAreaRegistration> _componentAreas;
        private readonly IAttributeTypeRegistry _attributeTypeRegistry;

        public CmsBootstrapper(RebelSettings settings,
            RebelAreaRegistration areaRegistration,
            InstallAreaRegistration installRegistration,
            IEnumerable<PackageAreaRegistration> componentAreas)
        {
            _areaRegistration = areaRegistration;
            _installRegistration = installRegistration;
            _componentAreas = componentAreas;
            _settings = settings;
            _attributeTypeRegistry = new DependencyResolverAttributeTypeRegistry();
        }

        public CmsBootstrapper(RebelSettings settings,
            RebelAreaRegistration areaRegistration,
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

            //we requrie that a custom GlobalFilter is added so see if it is there:
            if (!GlobalFilters.Filters.ContainsFilter<ProxyableResultAttribute>())
            {
                GlobalFilters.Filters.Add(new ProxyableResultAttribute());
            }

            routes.RegisterArea(_installRegistration);
            //register all component areas
            foreach (var c in _componentAreas)
            {
                routes.RegisterArea(c);
            }

            //IMPORTANT: We need to register the Rebel area after the components because routes overlap.
            // For example, a surface controller might have a url of:
            //   /Rebel/MyPackage/Surface/MySurface
            // and because the default action is 'Index' its not required to be there, however this same route
            // matches the default Rebel back office route of Rebel/{controller}/{action}/{id}
            // so we want to make sure that the plugin routes are matched first
            routes.RegisterArea(_areaRegistration);

            //ensure that the IAttributeTypeRegistry is set
            AttributeTypeRegistry.SetCurrent(_attributeTypeRegistry);

            //register validation extensions
            DataAnnotationsModelValidatorProviderExtensions.RegisterValidationExtensions();

            LocalizationWebConfig.RegisterRoutes(routes, _settings.RebelPaths.LocalizationPath);
            
            //If this is outside of an ASP.Net application (i.e. Unit test) and RegisterVirtualPathProvider is called then an exception is thrown.
            if (HostingEnvironment.IsHosted)
            {
                HostingEnvironment.RegisterVirtualPathProvider(new EmbeddedViewVirtualPathProvider());
                HostingEnvironment.RegisterVirtualPathProvider(new CodeDelegateVirtualPathProvider());
            }
            
            //register custom validation adapters
            DataAnnotationsModelValidatorProvider.RegisterAdapterFactory(
                typeof (HiveIdRequiredAttribute),
                (metadata, controllerContext, attribute) =>
                new RequiredHiveIdAttributeAdapter(metadata, controllerContext, (HiveIdRequiredAttribute) attribute));

        }

    }
}
