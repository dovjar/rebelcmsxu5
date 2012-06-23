using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using RebelCms.Cms.Web.Dashboards;
using RebelCms.Cms.Web.Dashboards.Filters;
using RebelCms.Cms.Web.Dashboards.Rules;
using RebelCms.Cms.Web.Macros;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Model.BackOffice.ParameterEditors;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;
using RebelCms.Cms.Web.Model.BackOffice.Trees;
using RebelCms.Cms.Web.Routing;
using RebelCms.Cms.Web.System;
using RebelCms.Cms.Web.System.Boot;
using RebelCms.Cms.Web.UI;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Security;
using RebelCms.Framework.Testing;
using RebelCms.Tests.Cms.Stubs;
using RebelCms.Cms.Web.DependencyManagement;
using RebelCms.Cms.Web.Editors;
using RebelCms.Cms.Web.Mvc.Areas;
using RebelCms.Cms.Web.Mvc.Controllers;
using RebelCms.Cms.Web.Surface;
using RebelCms.Cms.Web.Trees;
using System.IO;
using System.Web.Routing;
using System.Web.Mvc;

using RebelCms.Framework;
using RebelCms.Framework.Tasks;
using RebelCms.Tests.Cms.Stubs.Surface;
using RebelCms.Tests.Extensions;
using RebelCms.Tests.Extensions.Stubs;
using ApplicationTreeController = RebelCms.Tests.Cms.Stubs.Trees.ApplicationTreeController;

namespace RebelCms.Tests.Cms.Mvc.Routing
{
    using RebelCms.Cms.Web;

    [TestFixture]
    public abstract class RoutingTest : StandardWebTest
    {

        protected ComponentRegistrations Components { get; private set; }

        #region Initialize

        /// <summary>
        /// Initialize test
        /// </summary>
        [SetUp]
        public void InitTest()
        {

            Init();


            RenderModelFactory = FakeRenderModelFactory.CreateWithApp();
            var frontEndRouteHandler = new RenderRouteHandler(new TestControllerFactory(), RebelCmsApplicationContext, RenderModelFactory);
            
            //register areas/routes...            

            RouteTable.Routes.Clear();

            var packageFolder = new DirectoryInfo(Path.Combine(Common.CurrentAssemblyDirectory, "App_Plugins", PluginManager.PackagesFolderName, "TestPackage"));

            Components = new ComponentRegistrations(new List<Lazy<MenuItem, MenuItemMetadata>>(),
                                                        GetEditorMetadata(packageFolder), 
                                                        GetTreeMetadata(packageFolder),
                                                        GetSurfaceMetadata(packageFolder),
                                                        new List<Lazy<AbstractTask, TaskMetadata>>(),
                                                        new List<Lazy<PropertyEditor, PropertyEditorMetadata>>(),
                                                        new List<Lazy<AbstractParameterEditor, ParameterEditorMetadata>>(),
                                                        new List<Lazy<DashboardMatchRule, DashboardRuleMetadata>>(),
                                                        new List<Lazy<DashboardFilter, DashboardRuleMetadata>>(),
                                                        new List<Lazy<Permission, PermissionMetadata>>(),
                                                        new List<Lazy<AbstractMacroEngine, MacroEngineMetadata>>());

            var componentRegistration = new PackageAreaRegistration(packageFolder, RebelCmsApplicationContext, Components);
            var areaRegistration = new RebelCmsAreaRegistration(RebelCmsApplicationContext, Components);
            var installRegistration = new InstallAreaRegistration(RebelCmsApplicationContext.Settings);
            
            var cmsBootstrapper = new CmsBootstrapper(RebelCmsApplicationContext.Settings, areaRegistration, installRegistration, new[] {componentRegistration}, new DefaultAttributeTypeRegistry());
            var renderBootstrapper = new RenderBootstrapper(RebelCmsApplicationContext, frontEndRouteHandler, RenderModelFactory);

            //bootstrappers setup the routes
            cmsBootstrapper.Boot(RouteTable.Routes);
            renderBootstrapper.Boot(RouteTable.Routes);

            new RebelCmsWebApplication(null, null, null).RegisterRoutes(RouteTable.Routes);
        }

        #endregion

        protected FakeRenderModelFactory RenderModelFactory { get; private set; }

        private static IEnumerable<Lazy<SurfaceController, SurfaceMetadata>> GetSurfaceMetadata(DirectoryInfo packageFolder)
        {
            var surfaceTypes = new List<Type>
                                  {
                                      //standard editors
                                      typeof(CustomSurfaceController),
                                      typeof(BlahSurfaceController)                                     
                                  };
            //now we need to create the meta data for each
            return (from t in surfaceTypes
                    let a = t.GetCustomAttributes(typeof(SurfaceAttribute), false).Cast<SurfaceAttribute>().First()
                    select new Lazy<SurfaceController, SurfaceMetadata>(
                        new SurfaceMetadata(new Dictionary<string, object>())
                        {
                            Id = a.Id,
                            ComponentType = t,                                                       
                            ControllerName = RebelCmsController.GetControllerName(t),
                            PluginDefinition = new PluginDefinition(
                                new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")), 
                                packageFolder.FullName,
                                null,false)
                        })).ToList();
        }

        private static IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> GetEditorMetadata(DirectoryInfo packageFolder)
        {
            var editorTypes = new List<Type>
                                  {
                                      //standard editors
                                      typeof(ContentEditorController),
                                      typeof(DataTypeEditorController),
                                      typeof(DocumentTypeEditorController),
                                      //duplicate named editors
                                      typeof(Stubs.Editors.ContentEditorController),
                                      typeof(Stubs.Editors.MediaEditorController)
                                  };
            //now we need to create the meta data for each
            return (from t in editorTypes
                    let a = t.GetCustomAttributes(typeof (EditorAttribute), false).Cast<EditorAttribute>().First()
                    let defaultEditor = t.GetCustomAttributes(typeof (RebelCmsEditorAttribute), false).Any()
                    select new Lazy<AbstractEditorController, EditorMetadata>(
                        new EditorMetadata(new Dictionary<string, object>())
                            {
                                Id = a.Id,
                                ComponentType = t,
                                IsInternalRebelCmsEditor = defaultEditor,
                                ControllerName = RebelCmsController.GetControllerName(t),
                                PluginDefinition = new PluginDefinition(
                                    new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")),
                                    packageFolder.FullName, 
                                    null, false)
                            })).ToList();
        }

        private static IEnumerable<Lazy<TreeController, TreeMetadata>> GetTreeMetadata(DirectoryInfo packageFolder)
        {
            //create the list of trees
            var treeControllerTypes = new List<Type>
                                          {
                                              //standard trees
                                              typeof (ContentTreeController),
                                              typeof (MediaTreeController),
                                              typeof (DataTypeTreeController),
                                              typeof (DocumentTypeTreeController),
                                              //duplicate named controllers
                                              typeof (ApplicationTreeController),
                                              typeof (Stubs.Trees.ContentTreeController),
                                              typeof (Stubs.Trees.MediaTreeController)
                                          };
            //now we need to create the meta data for each
            return (from t in treeControllerTypes
                    let a = t.GetCustomAttributes(typeof (TreeAttribute), false).Cast<TreeAttribute>().First()
                    let defaultTree = t.GetCustomAttributes(typeof (RebelCmsTreeAttribute), false).Any()
                    select new Lazy<TreeController, TreeMetadata>(
                        new TreeMetadata(new Dictionary<string, object>())
                            {
                                Id = a.Id,
                                TreeTitle = a.TreeTitle,
                                ComponentType = t,
                                IsInternalRebelCmsTree = defaultTree,
                                ControllerName = RebelCmsController.GetControllerName(t),
                                PluginDefinition = new PluginDefinition(
                                    new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")),
                                    packageFolder.FullName, 
                                    null, false)
                            })).ToList();
        }



    }
}
