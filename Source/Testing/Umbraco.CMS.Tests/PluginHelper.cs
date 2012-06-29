using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc.Controllers;
using Umbraco.Cms.Web.Surface;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Tests.Cms.Stubs.Surface;
using ApplicationTreeController = Umbraco.Tests.Cms.Stubs.Trees.ApplicationTreeController;

namespace Umbraco.Tests.Cms
{
    /// <summary>
    /// Helper utility for returning collection of plugin metadata
    /// </summary>
    public static class PluginHelper
    {
        public static IEnumerable<Lazy<SurfaceController, SurfaceMetadata>> GetSurfaceMetadata(DirectoryInfo packageFolder)
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
                                ControllerName = UmbracoController.GetControllerName(t),
                                PluginDefinition = new PluginDefinition(
                                    new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")),
                                    packageFolder.FullName,
                                    null, false)
                            })).ToList();
        }

        public static IEnumerable<Lazy<AbstractEditorController, EditorMetadata>> GetEditorMetadata(DirectoryInfo packageFolder)
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
                    let a = t.GetCustomAttributes(typeof(EditorAttribute), false).Cast<EditorAttribute>().First()
                    let defaultEditor = t.GetCustomAttributes(typeof(UmbracoEditorAttribute), false).Any()
                    select new Lazy<AbstractEditorController, EditorMetadata>(
                        new EditorMetadata(new Dictionary<string, object>())
                            {
                                Id = a.Id,
                                ComponentType = t,
                                IsInternalUmbracoEditor = defaultEditor,
                                ControllerName = UmbracoController.GetControllerName(t),
                                PluginDefinition = new PluginDefinition(
                                    new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")),
                                    packageFolder.FullName,
                                    null, false)
                            })).ToList();
        }

        public static IEnumerable<Lazy<TreeController, TreeMetadata>> GetTreeMetadata(DirectoryInfo packageFolder)
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
                    let a = t.GetCustomAttributes(typeof(TreeAttribute), false).Cast<TreeAttribute>().First()
                    let defaultTree = t.GetCustomAttributes(typeof(UmbracoTreeAttribute), false).Any()
                    select new Lazy<TreeController, TreeMetadata>(
                        new TreeMetadata(new Dictionary<string, object>())
                            {
                                Id = a.Id,
                                TreeTitle = a.TreeTitle,
                                ComponentType = t,
                                IsInternalUmbracoTree = defaultTree,
                                ControllerName = UmbracoController.GetControllerName(t),
                                PluginDefinition = new PluginDefinition(
                                    new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")),
                                    packageFolder.FullName,
                                    null, false)
                            })).ToList();
        }
    }
}