using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Dashboards.Filters;
using Umbraco.Cms.Web.Dashboards.Rules;
using Umbraco.Cms.Web.DependencyManagement;
using Umbraco.Cms.Web.Macros;
using Umbraco.Cms.Web.Model.BackOffice.ParameterEditors;
using Umbraco.Cms.Web.Model.BackOffice.PropertyEditors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.Areas;
using Umbraco.Cms.Web.Routing;
using Umbraco.Cms.Web.System;
using Umbraco.Cms.Web.System.Boot;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Security;
using Umbraco.Framework.Tasks;
using Umbraco.Framework.Testing;
using Umbraco.Tests.Cms.Stubs;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.Cms
{
    /// <summary>
    /// Test that can be used for any route testing
    /// </summary>
    public abstract class AbstractRoutingTest : StandardWebTest
    {
        protected ComponentRegistrations Components { get; private set; }
        protected FakeRenderModelFactory RenderModelFactory { get; private set; }
        protected FakeHttpContextFactory HttpContextFactory { get; private set; }

        #region Initialize

        /// <summary>
        /// Initialize test
        /// </summary>
        [SetUp]
        public void InitTest()
        {

            Init();

            RenderModelFactory = FakeRenderModelFactory.CreateWithApp();
            var frontEndRouteHandler = new RenderRouteHandler(new TestControllerFactory(), UmbracoApplicationContext, RenderModelFactory);

            //register areas/routes...            

            RouteTable.Routes.Clear();

            var packageFolder = new DirectoryInfo(Path.Combine(Common.CurrentAssemblyDirectory, "App_Plugins", PluginManager.PackagesFolderName, "TestPackage"));

            Components = new ComponentRegistrations(new List<Lazy<MenuItem, MenuItemMetadata>>(),
                                                    PluginHelper.GetEditorMetadata(packageFolder),
                                                    PluginHelper.GetTreeMetadata(packageFolder),
                                                    PluginHelper.GetSurfaceMetadata(packageFolder),
                                                    new List<Lazy<AbstractTask, TaskMetadata>>(),
                                                    new List<Lazy<PropertyEditor, PropertyEditorMetadata>>(),
                                                    new List<Lazy<AbstractParameterEditor, ParameterEditorMetadata>>(),
                                                    new List<Lazy<DashboardMatchRule, DashboardRuleMetadata>>(),
                                                    new List<Lazy<DashboardFilter, DashboardRuleMetadata>>(),
                                                    new List<Lazy<Permission, PermissionMetadata>>(),
                                                    new List<Lazy<AbstractMacroEngine, MacroEngineMetadata>>());

            var componentRegistration = new PackageAreaRegistration(packageFolder, UmbracoApplicationContext, Components);
            var areaRegistration = new UmbracoAreaRegistration(UmbracoApplicationContext, Components);
            var installRegistration = new InstallAreaRegistration(UmbracoApplicationContext.Settings);

            var cmsBootstrapper = new CmsBootstrapper(UmbracoApplicationContext.Settings, areaRegistration, installRegistration, new[] { componentRegistration }, new DefaultAttributeTypeRegistry());
            var renderBootstrapper = new RenderBootstrapper(UmbracoApplicationContext, frontEndRouteHandler, RenderModelFactory);

            //bootstrappers setup the routes
            cmsBootstrapper.Boot(RouteTable.Routes);
            renderBootstrapper.Boot(RouteTable.Routes);

            UmbracoWebApplication.RegisterDefaultRoutes(RouteTable.Routes);
        }

        #endregion
    }
}