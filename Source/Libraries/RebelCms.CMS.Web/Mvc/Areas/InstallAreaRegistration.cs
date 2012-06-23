using System;
using System.Web.Mvc;
using RebelCms.Cms.Web.Configuration;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Mvc.Controllers;
using RebelCms.Cms.Web.Mvc.Controllers.BackOffice;

namespace RebelCms.Cms.Web.Mvc.Areas
{
    public class InstallAreaRegistration : AreaRegistration
    {
        public const string RouteName = "RebelCms-install";

        private readonly RebelCmsSettings _RebelCmsSettings;

        public InstallAreaRegistration(RebelCmsSettings RebelCmsSettings)
        {
            _RebelCmsSettings = RebelCmsSettings;            
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //map installer routes, only one controller
            context.Routes.MapRoute(
                RouteName,
                AreaName + "/{action}/{id}",
                new { controller = "Install", action = "Index", id = UrlParameter.Optional },
                null,
                new[] { typeof(InstallController).Namespace })//match controllers in these namespaces                
                .DataTokens.Add("area", AreaName);//only match this area
        }

        public override string AreaName
        {
            get { return "Install"; }
        }
    }
}