using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Routing;
using NUnit.Framework;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Dashboards.Filters;
using Rebel.Cms.Web.Dashboards.Rules;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Macros;
using Rebel.Cms.Web.Model.BackOffice.ParameterEditors;
using Rebel.Cms.Web.Model.BackOffice.PropertyEditors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.Areas;
using Rebel.Cms.Web.Routing;
using Rebel.Cms.Web.System;
using Rebel.Cms.Web.System.Boot;
using Rebel.Framework.Persistence;
using Rebel.Framework.Security;
using Rebel.Framework.Tasks;
using Rebel.Framework.Testing;
using Rebel.Tests.Cms.Stubs;
using Rebel.Tests.Extensions;

namespace Rebel.Tests.Cms
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
            var frontEndRouteHandler = new RenderRouteHandler(new TestControllerFactory(), RebelApplicationContext, RenderModelFactory);

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

            var componentRegistration = new PackageAreaRegistration(packageFolder, RebelApplicationContext, Components);
            var areaRegistration = new RebelAreaRegistration(RebelApplicationContext, Components);
            var installRegistration = new InstallAreaRegistration(RebelApplicationContext.Settings);

            var cmsBootstrapper = new CmsBootstrapper(RebelApplicationContext.Settings, areaRegistration, installRegistration, new[] { componentRegistration }, new DefaultAttributeTypeRegistry());
            var renderBootstrapper = new RenderBootstrapper(RebelApplicationContext, frontEndRouteHandler, RenderModelFactory);

            //bootstrappers setup the routes
            cmsBootstrapper.Boot(RouteTable.Routes);
            renderBootstrapper.Boot(RouteTable.Routes);

            RebelWebApplication.RegisterDefaultRoutes(RouteTable.Routes);
        }

        #endregion
    }
}