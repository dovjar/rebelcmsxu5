using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Rebel.Cms.Web.Configuration;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Cms.Web.Mvc.Controllers.BackOffice;
using System.Linq;
using Rebel.Cms.Web.Routing;
using Rebel.Cms.Web.Surface;
using Rebel.Cms.Web.Trees;
using Rebel.Cms.Web.Mvc.Metadata;
using Rebel.Framework;


namespace Rebel.Cms.Web.Mvc.Areas
{
    /// <summary>
    /// Responsible for registering the Rebel Area and all of it's affiliated routes
    /// </summary>
    public class RebelAreaRegistration : AreaRegistration
    {

        public const string DashboardRouteName = "rebel-dashboard";
        public const string DefaultRouteName = "rebel-default";
        public const string ApplicationRouteName = "rebel-app";
        public const string ApplicationTreeRouteName = "rebel-app-tree";

        /// <summary>
        /// Constructor using a specific RebelSettings object
        /// </summary>
        /// <param name="applicationContext"></param>
        /// <param name="componentRegistrar"></param>
        public RebelAreaRegistration(
            IRebelApplicationContext applicationContext,
            ComponentRegistrations componentRegistrar)
        {
            _applicationContext = applicationContext;
            _rebelSettings = _applicationContext.Settings;
            _componentRegistrar = componentRegistrar;
            _treeControllers = _componentRegistrar.TreeControllers;
            _editorControllers = _componentRegistrar.EditorControllers;
            _surfaceControllers = _componentRegistrar.SurfaceControllers;
        }

        private readonly IRebelApplicationContext _applicationContext;
        private readonly RebelSettings _rebelSettings;
        private readonly ComponentRegistrations _componentRegistrar;
        private readonly IEnumerable<Lazy<TreeController, TreeMetadata>> _treeControllers;
        private readonly IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> _editorControllers;
        private readonly IEnumerable<Lazy<SurfaceController, SurfaceMetadata>> _surfaceControllers;

        public override string AreaName
        {
            get { return _rebelSettings.RebelPaths.BackOfficePath; }
        }

        /// <summary>
        /// Creates the routes for the back office area
        /// </summary>
        /// <param name="context"></param>
        public override void RegisterArea(AreaRegistrationContext context)
        {

            MapRouteEditors(context.Routes, _editorControllers.Select(x => x.Metadata));
            MapRouteTrees(context.Routes, _treeControllers.Select(x => x.Metadata));            
            MapRouteSurfaceControllers(context.Routes, _surfaceControllers.Select(x => x.Metadata));

            MapRouteBackOffice(context.Routes);
        }

        /// <summary>
        /// This maps locally declared (non-package) surface controllers so that they are routing through the Rebel back office path
        /// /Rebel (or what is defined in config)
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="surfaceControllers"></param>
        private void MapRouteSurfaceControllers(RouteCollection routes, IEnumerable<SurfaceMetadata> surfaceControllers)
        {
            foreach (var s in surfaceControllers.Where(x => x.PluginDefinition == null))
            {
                var route = routes.MapRoute(
                    string.Format("rebel-{0}-{1}", "surface", s.ControllerName),
                    AreaName + "/Surface/" + s.ControllerName + "/{action}/{id}",//url to match
                    new { controller = s.ControllerName, action = "Index", id = UrlParameter.Optional },
                    new[] { s.ComponentType.Namespace }); //only match this namespace
                    route.DataTokens.Add("area", AreaName); //only match this area
                    route.DataTokens.Add("rebel", "surface"); //ensure the rebel token is set
            }
        }

        /// <summary>
        /// Creates the routing rules for the editors
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="editorControllers"></param>
        private void MapRouteEditors(RouteCollection routes, IEnumerable<EditorMetadata> editorControllers)
        {
            //first, register the internal/default Rebel editor routes (so they work without the IDs)
            //but the constraint will ONLY allow built in Rebel editors to work like this, 3rd party 
            //editors will only be accessible by using an ID.
            var defaultEditorMetadata = editorControllers.Where(x => x.IsInternalRebelEditor);                

            //first register the special DashboardEditorController as the default
            var dashboardControllerName = RebelController.GetControllerName(typeof(DashboardEditorController));
            var dashboardControllerId = RebelController.GetControllerId<EditorAttribute>(typeof(DashboardEditorController));
            var route = routes.MapRoute(
                DashboardRouteName,//name
                AreaName + "/Editors/" + dashboardControllerName + "/{action}/{appAlias}",//url to match
                new { controller = dashboardControllerName, action = "Dashboard", appAlias = "content", editorId = dashboardControllerId.ToString("N") },
                new { backOffice = new BackOfficeRouteConstraint(_applicationContext) },
                new[] { typeof(DashboardEditorController).Namespace }); //only match this namespace
            route.DataTokens.Add("area", AreaName); //only match this area
            route.DataTokens.Add("rebel", "backoffice"); //ensure the rebel token is set

            //register the default (built-in) editors
            foreach (var t in defaultEditorMetadata)
            {
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "editorId", "Editor", "Editors", "Dashboard", UrlParameter.Optional, true, _applicationContext);
            }

            //now, we need to get the 'internal' editors, these could be not part of a package and just exist in the 'bin' if someone just developed their
            //trees in VS in their local Rebel project
            var localEditors = editorControllers.Where(x => x.PluginDefinition == null && x.Id != dashboardControllerId);
            foreach (var t in localEditors)
            {
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "editorId", "Editor", "Editors", "Dashboard", UrlParameter.Optional, true, _applicationContext);
            }

        }

        /// <summary>
        /// Create the routes to handle tree requests
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="treeControllers"></param>
        private void MapRouteTrees(RouteCollection routes, IEnumerable<TreeMetadata> treeControllers)
        {
            //get the core ubmraco trees
            var defaultTreeTypes = treeControllers.Where(x => x.IsInternalRebelTree);

            //First, register the special default route for the ApplicationTreeController, this is required
            //because this special tree doesn't take a HiveId as a parameter but an application name (string)
            //in order for it to render all trees in the application routed
            var applicationTreeControllerName = RebelController.GetControllerName(typeof(ApplicationTreeController));
            var applicationTreeControllerId = RebelController.GetControllerId<TreeAttribute>(typeof(ApplicationTreeController));
            var route = routes.MapRoute(
                ApplicationTreeRouteName,//name
                AreaName + "/Trees/" + applicationTreeControllerName + "/{action}/{appAlias}",//url to match
                new { controller = applicationTreeControllerName, action = "Index", appAlias = "content", treeId = applicationTreeControllerId.ToString("N") },
                new { backOffice = new BackOfficeRouteConstraint(_applicationContext) },
                new[] { typeof(ApplicationTreeController).Namespace }); //only match this namespace            
            route.DataTokens.Add("area", AreaName); //only match this area
            route.DataTokens.Add("rebel", "backoffice"); //ensure the rebel token is set

            //Register routes for the default trees
            foreach (var t in defaultTreeTypes)
            {
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "treeId", "Tree", "Trees", "Index", HiveId.Empty, true, _applicationContext);
            }

            //now, we need to get the 'internal' trees, these could be not part of a package and just exist in the 'bin' if someone just developed their
            //trees in VS in their local Rebel project
            var localTrees = treeControllers.Where(x => x.PluginDefinition == null && x.Id != applicationTreeControllerId);
            foreach (var t in localTrees)
            {
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "treeId", "Tree", "Trees", "Index", HiveId.Empty, true, _applicationContext);
            }
        }

        /// <summary>
        /// The standard routes for the back office main pages
        /// </summary>
        /// <param name="routes"></param>
        private void MapRouteBackOffice(RouteCollection routes)
        {
            //url to match deep linked apps (i.e. /Rebel or /Rebel/Media)
            var defaultControllerName = RebelController.GetControllerName(typeof(DefaultController));
            var appRoute = routes.MapRoute(
                ApplicationRouteName,
                AreaName + "/{appAlias}",
                new { controller = defaultControllerName, action = "App", appAlias = "content" },
                new { backOffice = new BackOfficeRouteConstraint(_applicationContext) },
                new[] { typeof(DefaultController).Namespace });//match controllers in these namespaces                
            appRoute.DataTokens.Add("area", AreaName);//only match this area
            appRoute.DataTokens.Add("rebel", "backoffice"); //ensure the rebel token is set

            //url to match normal controller routes for the back office
            var standardRoute = routes.MapRoute(
                DefaultRouteName,
                AreaName + "/{controller}/{action}/{id}",
                new { controller = defaultControllerName, action = "Index", id = UrlParameter.Optional },
                new { backOffice = new BackOfficeRouteConstraint(_applicationContext) },
                new[] { typeof(DefaultController).Namespace });//match controllers in these namespaces                
            standardRoute.DataTokens.Add("area", AreaName);//only match this area
            standardRoute.DataTokens.Add("rebel", "backoffice"); //ensure the rebel token is set
        }


        
    }
}
