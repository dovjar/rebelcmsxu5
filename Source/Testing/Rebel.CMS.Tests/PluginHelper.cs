using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rebel.Cms.Web.Editors;
using Rebel.Cms.Web.Model.BackOffice.Trees;
using Rebel.Cms.Web.Mvc.Controllers;
using Rebel.Cms.Web.Surface;
using Rebel.Cms.Web.Trees;
using Rebel.Framework;
using Rebel.Tests.Cms.Stubs.Surface;
using ApplicationTreeController = Rebel.Tests.Cms.Stubs.Trees.ApplicationTreeController;

namespace Rebel.Tests.Cms
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
                                ControllerName = RebelController.GetControllerName(t),
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
                    let defaultEditor = t.GetCustomAttributes(typeof(RebelEditorAttribute), false).Any()
                    select new Lazy<AbstractEditorController, EditorMetadata>(
                        new EditorMetadata(new Dictionary<string, object>())
                            {
                                Id = a.Id,
                                ComponentType = t,
                                IsInternalRebelEditor = defaultEditor,
                                ControllerName = RebelController.GetControllerName(t),
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
                    let defaultTree = t.GetCustomAttributes(typeof(RebelTreeAttribute), false).Any()
                    select new Lazy<TreeController, TreeMetadata>(
                        new TreeMetadata(new Dictionary<string, object>())
                            {
                                Id = a.Id,
                                TreeTitle = a.TreeTitle,
                                ComponentType = t,
                                IsInternalRebelTree = defaultTree,
                                ControllerName = RebelController.GetControllerName(t),
                                PluginDefinition = new PluginDefinition(
                                    new FileInfo(Path.Combine(packageFolder.FullName, "lib", "hello.dll")),
                                    packageFolder.FullName,
                                    null, false)
                            })).ToList();
        }
    }
}