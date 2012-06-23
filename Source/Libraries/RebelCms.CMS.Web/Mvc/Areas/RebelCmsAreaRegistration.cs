using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using RebelCms.Cms.Web.Configuration;
using RebelCms.Cms.Web.Context;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Editors;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Mvc.Controllers;
using RebelCms.Cms.Web.Mvc.Controllers.BackOffice;
using System.Linq;
using RebelCms.Cms.Web.Routing;
using RebelCms.Cms.Web.Surface;
using RebelCms.Cms.Web.Trees;
using RebelCms.Cms.Web.Mvc.Metadata;
using RebelCms.Framework;


namespace RebelCms.Cms.Web.Mvc.Areas
{
    /// <summary>
    /// Responsible for registering the RebelCms Area and all of it's affiliated routes
    /// </summary>
    public class RebelCmsAreaRegistration : AreaRegistration
    {

        public const string DashboardRouteName = "RebelCms-dashboard";
        public const string DefaultRouteName = "RebelCms-default";
        public const string ApplicationRouteName = "RebelCms-app";
        public const string ApplicationTreeRouteName = "RebelCms-app-tree";

        /// <summary>
        /// Constructor using a specific RebelCmsSettings object
        /// </summary>
        /// <param name="applicationContext"></param>
        /// <param name="componentRegistrar"></param>
        public RebelCmsAreaRegistration(
            IRebelCmsApplicationContext applicationContext,
            ComponentRegistrations componentRegistrar)
        {
            _applicationContext = applicationContext;
            _RebelCmsSettings = _applicationContext.Settings;
            _componentRegistrar = componentRegistrar;
            _treeControllers = _componentRegistrar.TreeControllers;
            _editorControllers = _componentRegistrar.EditorControllers;
            _surfaceControllers = _componentRegistrar.SurfaceControllers;
        }

        private readonly IRebelCmsApplicationContext _applicationContext;
        private readonly RebelCmsSettings _RebelCmsSettings;
        private readonly ComponentRegistrations _componentRegistrar;
        private readonly IEnumerable<Lazy<TreeController, TreeMetadata>> _treeControllers;
        private readonly IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> _editorControllers;
        private readonly IEnumerable<Lazy<SurfaceController, SurfaceMetadata>> _surfaceControllers;

        public override string AreaName
        {
            get { return _RebelCmsSettings.RebelCmsPaths.BackOfficePath; }
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
        /// This maps locally declared (non-package) surface controllers so that they are routing through the RebelCms back office path
        /// /RebelCms (or what is defined in config)
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="surfaceControllers"></param>
        private void MapRouteSurfaceControllers(RouteCollection routes, IEnumerable<SurfaceMetadata> surfaceControllers)
        {
            foreach (var s in surfaceControllers.Where(x => x.PluginDefinition == null))
            {
                var route = routes.MapRoute(
                    string.Format("RebelCms-{0}-{1}", "surface", s.ControllerName),
                    AreaName + "/Surface/" + s.ControllerName + "/{action}/{id}",//url to match
                    new { controller = s.ControllerName, action = "Index", id = UrlParameter.Optional },
                    new[] { s.ComponentType.Namespace }); //only match this namespace
                    route.DataTokens.Add("area", AreaName); //only match this area
                    route.DataTokens.Add("RebelCms", "surface"); //ensure the RebelCms token is set
            }
        }

        /// <summary>
        /// Creates the routing rules for the editors
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="editorControllers"></param>
        private void MapRouteEditors(RouteCollection routes, IEnumerable<EditorMetadata> editorControllers)
        {
            //first, register the internal/default RebelCms editor routes (so they work without the IDs)
            //but the constraint will ONLY allow built in RebelCms editors to work like this, 3rd party 
            //editors will only be accessible by using an ID.
            var defaultEditorMetadata = editorControllers.Where(x => x.IsInternalRebelCmsEditor);                

            //first register the special DashboardEditorController as the default
            var dashboardControllerName = RebelCmsController.GetControllerName(typeof(DashboardEditorController));
            var dashboardControllerId = RebelCmsController.GetControllerId<EditorAttribute>(typeof(DashboardEditorController));
            var route = routes.MapRoute(
                DashboardRouteName,//name
                AreaName + "/Editors/" + dashboardControllerName + "/{action}/{appAlias}",//url to match
                new { controller = dashboardControllerName, action = "Dashboard", appAlias = "content", editorId = dashboardControllerId.ToString("N") },
                new { backOffice = new BackOfficeRouteConstraint(_applicationContext) },
                new[] { typeof(DashboardEditorController).Namespace }); //only match this namespace
            route.DataTokens.Add("area", AreaName); //only match this area
            route.DataTokens.Add("RebelCms", "backoffice"); //ensure the RebelCms token is set

            //register the default (built-in) editors
            foreach (var t in defaultEditorMetadata)
            {
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "editorId", "Editor", "Editors", "Dashboard", UrlParameter.Optional, true, _applicationContext);
            }

            //now, we need to get the 'internal' editors, these could be not part of a package and just exist in the 'bin' if someone just developed their
            //trees in VS in their local RebelCms project
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
            var defaultTreeTypes = treeControllers.Where(x => x.IsInternalRebelCmsTree);

            //First, register the special default route for the ApplicationTreeController, this is required
            //because this special tree doesn't take a HiveId as a parameter but an application name (string)
            //in order for it to render all trees in the application routed
            var applicationTreeControllerName = RebelCmsController.GetControllerName(typeof(ApplicationTreeController));
            var applicationTreeControllerId = RebelCmsController.GetControllerId<TreeAttribute>(typeof(ApplicationTreeController));
            var route = routes.MapRoute(
                ApplicationTreeRouteName,//name
                AreaName + "/Trees/" + applicationTreeControllerName + "/{action}/{appAlias}",//url to match
                new { controller = applicationTreeControllerName, action = "Index", appAlias = "content", treeId = applicationTreeControllerId.ToString("N") },
                new { backOffice = new BackOfficeRouteConstraint(_applicationContext) },
                new[] { typeof(ApplicationTreeController).Namespace }); //only match this namespace            
            route.DataTokens.Add("area", AreaName); //only match this area
            route.DataTokens.Add("RebelCms", "backoffice"); //ensure the RebelCms token is set

            //Register routes for the default trees
            foreach (var t in defaultTreeTypes)
            {
                this.RouteControllerPlugin(t.Id, t.ControllerName, t.ComponentType, routes, "treeId", "Tree", "Trees", "Index", HiveId.Empty, true, _applicationContext);
            }

            //now, we need to get the 'internal' trees, these could be not part of a package and just exist in the 'bin' if someone just developed their
            //trees in VS in their local RebelCms project
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
            //url to match deep linked apps (i.e. /RebelCms or /RebelCms/Media)
            var defaultControllerName = RebelCmsController.GetControllerName(typeof(DefaultController));
            var appRoute = routes.MapRoute(
                ApplicationRouteName,
                AreaName + "/{appAlias}",
                new { controller = defaultControllerName, action = "App", appAlias = "content" },
                new { backOffice = new BackOfficeRouteConstraint(_applicationContext) },
                new[] { typeof(DefaultController).Namespace });//match controllers in these namespaces                
            appRoute.DataTokens.Add("area", AreaName);//only match this area
            appRoute.DataTokens.Add("RebelCms", "backoffice"); //ensure the RebelCms token is set

            //url to match normal controller routes for the back office
            var standardRoute = routes.MapRoute(
                DefaultRouteName,
                AreaName + "/{controller}/{action}/{id}",
                new { controller = defaultControllerName, action = "Index", id = UrlParameter.Optional },
                new { backOffice = new BackOfficeRouteConstraint(_applicationContext) },
                new[] { typeof(DefaultController).Namespace });//match controllers in these namespaces                
            standardRoute.DataTokens.Add("area", AreaName);//only match this area
            standardRoute.DataTokens.Add("RebelCms", "backoffice"); //ensure the RebelCms token is set
        }


        
    }
}
