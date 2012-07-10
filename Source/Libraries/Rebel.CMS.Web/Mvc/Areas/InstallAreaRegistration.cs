using System;
using System.Web.Mvc;
using Rebel.Cms.Web.Configuration;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;

namespace Rebel.Cms.Web.Mvc.Areas
{
    public class InstallAreaRegistration : AreaRegistration
    {
        public const string RouteName = "rebel-install";

        private readonly RebelSettings _rebelSettings;

        public InstallAreaRegistration(RebelSettings rebelSettings)
        {
            _rebelSettings = rebelSettings;            
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            //map installer routes, only one controller
            context.Routes.MapRoute(
                RouteName,
                AreaName + "/{controller}/{action}/{id}",
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