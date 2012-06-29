using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using NuGet;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.IO;
using Umbraco.Cms.Web.Model.BackOffice;
using Umbraco.Cms.Web.Model.BackOffice.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Mvc;
using Umbraco.Cms.Web.Mvc.ActionFilters;
using Umbraco.Cms.Web.Packaging;
using Umbraco.Cms.Web.Security;
using Umbraco.Cms.Web.Trees;
using Umbraco.Framework;
using Umbraco.Framework.Diagnostics;
using Umbraco.Framework.Localization;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Associations;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Framework.Security;
using Umbraco.Framework.Security.Model.Entities;
using Umbraco.Framework.Tasks;
using Umbraco.Hive;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Hive.RepositoryTypes;

//[assembly: WebResource("Umbraco.Cms.Packaging.Editors.Resources.Packaging.css", "text/css", PerformSubstitution = true)]
//[assembly: WebResource("Umbraco.Cms.Packaging.Editors.Resources.PackagingInstaller.js", "application/x-javascript")]
//[assembly: WebResource("Umbraco.Cms.Packaging.Editors.Resources.AppRestarter.js", "application/x-javascript")]

namespace Umbraco.Cms.Web.Editors
{
    [Editor(CorePluginConstants.PackagingEditorControllerId)]
    [UmbracoEditor]
    [SupportClientNotifications]
    public class PackagingEditorController : DashboardEditorController
    {
        private readonly IGroupUnitFactory<IFileStore> _hive;
        private readonly PackageInstallUtility _packageInstallUtility;

        public PackagingEditorController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        {
            _hive = BackOfficeRequestContext.Application.Hive.GetWriter<IFileStore>(new Uri("storage://created-packages"));
            _packageInstallUtility = new PackageInstallUtility(requestContext);
        }

        /// <summary>
        /// Creates the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Create(HiveId id)
        {
            var model = new PackageDefinitionEditorModel {ParentId = id};

            PopulateCollections(model);

            return View("Edit", model);
        }

        /// <summary>
        /// Creates the form.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        [ActionName("Create")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        [Save("submit.Save", "submit.Publish")]
        public ActionResult CreateForm(PackageDefinitionEditorModel model)
        {
            PopulateCollections(model);

            return ProcessSubmit(model, Request.Form.ContainsKey("submit.Publish"));
        }

        /// <summary>
        /// Action to render the editor
        /// </summary>
        /// <returns></returns>     
        [HttpGet]   
        public ActionResult Edit(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = _hive.Create())
            {
                var def = PackageBuilderHelper.GetPackageDefinitionById(uow, id.Value);
                if(def == null)
                    return HttpNotFound();

                var pkg = PackageBuilderHelper.GetPackageFileById(uow, id.Value);

                var model = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers
                    .Map<PackageDefinition, PackageDefinitionEditorModel>(def);
                model.Id = id.Value;
                model.IsPublished = pkg != null;

                PopulateCollections(model);

                return View(model);
            }
        }

        /// <summary>
        /// Handles the editor post back
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [ActionName("Edit")]
        [HttpPost]
        [ValidateInput(false)]
        [SupportsPathGeneration]
        [PersistTabIndexOnRedirect]
        [Save("submit.Save", "submit.Publish")]
        public ActionResult EditForm(PackageDefinitionEditorModel model)
        {
            Mandate.ParameterNotNull(model, "model");

            PopulateCollections(model);

            return ProcessSubmit(model, Request.Form.ContainsKey("submit.Publish"));
        }

        /// <summary>
        /// Action to render the editor
        /// </summary>
        /// <returns></returns>     
        [HttpGet]
        public ActionResult Download(HiveId? id)
        {
            if (id.IsNullValueOrEmpty()) return HttpNotFound();

            using (var uow = _hive.Create())
            {
                var def = PackageBuilderHelper.GetPackageDefinitionById(uow, id.Value);
                if(def == null)
                    return HttpNotFound();

                var pkg = PackageBuilderHelper.GetPackageFileById(uow, id.Value);
                if (pkg == null)
                    return HttpNotFound();

                return File(pkg.ContentBytes, "application/zip", def.Alias +"."+ def.Version + ".nupkg");
            }
        }

        /// <summary>
        /// JSON action to delete a node
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        [UmbracoAuthorize(Permissions = new[] { FixedPermissionIds.Delete })]
        public virtual JsonResult Delete(HiveId? id)
        {
            Mandate.ParameterNotEmpty(id, "id");

            using (var uow = _hive.Create())
            {
                var oldFolder = uow.Repositories.Get<Umbraco.Framework.Persistence.Model.IO.File>(id.Value);
                if (oldFolder != null)
                {
                    uow.Repositories.Delete<Umbraco.Framework.Persistence.Model.IO.File>(oldFolder.Id);
                }
            }

            //return a successful JSON response
            return Json(new { message = "Success" });
        }

        /// <summary>
        /// Browses the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        [HttpGet]
        public virtual ActionResult Browse(string path)
        {
            var nodes = new TreeNodeCollection();

            //get the request app name, by default it is content (based on default routes)
            if (string.IsNullOrEmpty(path))
            {
                var nodeId = new HiveId(-1);
                var rootNode = new TreeNode(nodeId, BackOfficeRequestContext.RegisteredComponents.MenuItems, "")
                {
                    HasChildren = true,
                    JsonUrl = Url.Action("Browse", "PackagingEditor", new { path = "~/" }),
                    Title = "Site",
                    Icon = "tree-folder"
                };
                rootNode.Style.AddCustom("root-wrapper");
                nodes.Add(rootNode);
            }
            else
            {
                var absolutePath = Server.MapPath(path);

                var folders = Directory.GetDirectories(absolutePath);
                foreach (var folder in folders)
                {
                    var folderInfo = new DirectoryInfo(folder);
                    var folderPath = string.Format("{0}{1}/", path, folderInfo.Name);

                    var nodeId = new HiveId(folderPath);
                    var rootNode = new TreeNode(nodeId, BackOfficeRequestContext.RegisteredComponents.MenuItems, "")
                    {
                        HasChildren = true,
                        EditorUrl = "Umbraco.Editors.PackageEditor.nodeClickHandler",
                        JsonUrl = Url.Action("Browse", "PackagingEditor", new { path = folderPath }),
                        Title = folderInfo.Name,
                        Icon = "tree-folder"
                    };
                    nodes.Add(rootNode);
                }

                var files = Directory.GetFiles(absolutePath);
                foreach (var file in files)
                {
                    var fileInfo = new DirectoryInfo(file);
                    var filePath = string.Format("{0}{1}", path, fileInfo.Name);

                    var nodeId = new HiveId(filePath);
                    var rootNode = new TreeNode(nodeId, BackOfficeRequestContext.RegisteredComponents.MenuItems, "")
                    {
                        HasChildren = false,
                        EditorUrl = "Umbraco.Editors.PackageEditor.nodeClickHandler",
                        Title = fileInfo.Name,
                        Icon = "tree-doc"
                    };
                    nodes.Add(rootNode);
                }
            }

            return new UmbracoTreeResult(nodes, ControllerContext);

        }

        protected ActionResult ProcessSubmit(PackageDefinitionEditorModel model, bool publish)
        {
            Mandate.ParameterNotNull(model, "model");

            //if there's model errors, return the view
            if (!ModelState.IsValid)
            {
                AddValidationErrorsNotification();
                return View("Edit", model);
            }

            //persist the data
            using (var uow = _hive.Create())
            {
                var entity = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map
                            <PackageDefinitionEditorModel, PackageDefinition>(model);

                var randomId = Guid.NewGuid().ToString();

                if(!model.Id.IsNullValueOrEmpty())
                {
                    var oldFolder = uow.Repositories.Get<Umbraco.Framework.Persistence.Model.IO.File>(model.Id);
                    if (oldFolder != null)
                    {
                        uow.Repositories.Delete<Umbraco.Framework.Persistence.Model.IO.File>(oldFolder.Id);
                        randomId = oldFolder.Id.Value.ToString();
                    }
                }

                // Create folder
                var folder = new Umbraco.Framework.Persistence.Model.IO.File(
                    randomId, "") { IsContainer = true };

                uow.Repositories.AddOrUpdate(folder);

                // Create definition file
                var file = new Umbraco.Framework.Persistence.Model.IO.File(
                    randomId + "/package.definition",
                    Encoding.UTF8.GetBytes(entity.ToJsonString()));

                uow.Repositories.AddOrUpdate(file);

                model.Id = folder.Id;

                uow.Complete();

                //add path for entity for SupportsPathGeneration (tree syncing) to work
                GeneratePathsForCurrentEntity(new EntityPathCollection(model.Id, new[]{ new EntityPath(new[]
                    {
                        new HiveId(FixedSchemaTypes.SystemRoot, null, new HiveIdValue(new Guid("802282F2-134E-4165-82B5-6DB2AFDDD135"))),
                        new HiveId(3), 
                        model.Id, 
                    })
                }));

                if(publish)
                {
                    // Generate the package file
                    var result = PackageBuilderHelper.CreatePackage(randomId, entity);
                    if(!result.Success)
                    {
                        ModelState.AddModelError("", "An error occured whilst trying to create the package file:\n"+ result.ErrorMessage);
                        return View("Edit", model);
                    }
                }

                Notifications.Add(new NotificationMessage(
                    "Package.Save.Message".Localize(this),
                    "Package.Save.Title".Localize(this),
                    NotificationType.Success));

                return RedirectToAction("Edit", new { id = model.Id });
            }
        }

        private void PopulateCollections(PackageDefinitionEditorModel model)
        {
            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IContentStore>())
            {
                // Doc types
                var docTypeIds = uow.Repositories.Schemas.GetDescendentRelations(FixedHiveIds.ContentRootSchema, FixedRelationTypes.DefaultRelationType)
                    .Where(x => !x.DestinationId.IsSystem())
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();
                var docTypes = uow.Repositories.Schemas.Get<EntitySchema>(true, docTypeIds)
                    .OrderBy(x => x.Name.Value);

                model.AvailableDocumentTypes = docTypes.Select(x => new HierarchicalSelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    ParentValues = x.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType)
                        .Where(y => y.Item.SourceId.Value != FixedHiveIds.ContentRootSchema.Value)
                        .Select(y => y.Item.SourceId.ToString()).ToArray(),
                    Selected = model.DocumentTypeIds != null && model.DocumentTypeIds.Contains(x.Id, new HiveIdComparer(true))
                }).ToArray();

                // Media types
                var mediaTypeIds = uow.Repositories.Schemas.GetDescendentRelations(FixedHiveIds.MediaRootSchema, FixedRelationTypes.DefaultRelationType)
                    .Where(x => !x.DestinationId.IsSystem())
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();
                var mediaTypes = uow.Repositories.Schemas.Get<EntitySchema>(true, mediaTypeIds)
                    .OrderBy(x => x.Name.Value);

                model.AvailableMediaTypes = mediaTypes.Select(x => new HierarchicalSelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    ParentValues = x.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType)
                        .Where(y => y.Item.SourceId.Value != FixedHiveIds.MediaRootSchema.Value)
                        .Select(y => y.Item.SourceId.ToString()).ToArray(),
                    Selected = model.MediaTypeIds != null && model.MediaTypeIds.Contains(x.Id, new HiveIdComparer(true))
                }).ToArray();

                // Data Types
                var dataTypes = uow.Repositories.Schemas.GetAll<AttributeType>()
                        .Where(x => !x.Id.IsSystem())
                        .OrderBy(x => x.Name.Value);

                model.AvailableDataTypes = dataTypes.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                    Selected = model.DataTypeIds != null && model.DataTypeIds.Contains(x.Id, new HiveIdComparer(true))
                });

                // Dictionary items
                var dictionaryItemIds = uow.Repositories.GetDescendentRelations(FixedHiveIds.DictionaryVirtualRoot, FixedRelationTypes.DefaultRelationType)
                    .Where(x => !x.DestinationId.IsSystem())
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();
                var dictionaryItems = uow.Repositories.Get<TypedEntity>(true, dictionaryItemIds)
                    .OrderBy(x => x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, "Name"));

                model.AvailableDictionaryItems = dictionaryItems.Select(x => new HierarchicalSelectListItem
                {
                    Text = x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, "Name"),
                    Value = x.Id.ToString(),
                    ParentValues = x.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType)
                        .Where(y => y.Item.SourceId.Value != FixedHiveIds.DictionaryVirtualRoot.Value)
                        .Select(y => y.Item.SourceId.ToString()).ToArray(),
                    Selected = model.DictionaryItemIds != null && model.DictionaryItemIds.Contains(x.Id, new HiveIdComparer(true))
                }).ToArray();
            }

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://templates/")))
            {
                // Templates
                var templates = uow.Repositories.GetAllNonContainerFiles()
                    .OrderBy(x => x.Name);

                model.AvailableTemplates = templates.Select(x => new SelectListItem
                {
                    Text = x.GetFileNameForDisplay(), 
                    Value = x.Id.ToString(),
                    Selected = model.TemplateIds != null && model.TemplateIds.Contains(x.Id, new HiveIdComparer(true))
                });
            }

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://partials/")))
            {
                // Partials
                var partialsRoot = uow.Repositories.Get(new HiveId("/"));
                var partialIds = uow.Repositories.GetDescendentRelations(partialsRoot.Id, FixedRelationTypes.DefaultRelationType)
                    .Where(x => !x.DestinationId.IsSystem())
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();
                var partials = uow.Repositories.Get<Umbraco.Framework.Persistence.Model.IO.File>(true, partialIds)
                    .OrderBy(x => x.Name);

                model.AvailablePartials = partials.Select(x => new HierarchicalSelectListItem
                {
                    Text = x.GetFileNameForDisplay(),
                    Value = x.Id.ToString(),
                    ParentValues = x.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType)
                        .Where(y => y.Item.SourceId.Value != partialsRoot.Id.Value)
                        .Select(y => y.Item.SourceId.ToString()).ToArray(),
                    Selected = model.PartialIds != null && model.PartialIds.Contains(x.Id, new HiveIdComparer(true))
                });
            }

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://stylesheets/")))
            {
                // Stylesheets
                var stylesheetsRoot = uow.Repositories.Get(new HiveId("/"));
                var stylesheetIds = uow.Repositories.GetDescendentRelations(stylesheetsRoot.Id, FixedRelationTypes.DefaultRelationType)
                    .Where(x => !x.DestinationId.IsSystem())
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();
                var stylesheets = uow.Repositories.Get<Umbraco.Framework.Persistence.Model.IO.File>(true, stylesheetIds)
                    .OrderBy(x => x.Name);

                model.AvailableStylesheets = stylesheets.Select(x => new HierarchicalSelectListItem
                {
                    Text = x.GetFileNameForDisplay(),
                    Value = x.Id.ToString(),
                    ParentValues = x.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType)
                        .Where(y => y.Item.SourceId.Value != stylesheetsRoot.Id.Value)
                        .Select(y => y.Item.SourceId.ToString()).ToArray(),
                    Selected = model.StylesheetIds != null && model.StylesheetIds.Contains(x.Id, new HiveIdComparer(true))
                });
            }

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://scripts/")))
            {
                // Scripts
                var scriptsRoot = uow.Repositories.Get(new HiveId("/"));
                var scriptIds = uow.Repositories.GetDescendentRelations(scriptsRoot.Id, FixedRelationTypes.DefaultRelationType)
                    .Where(x => !x.DestinationId.IsSystem())
                    .DistinctBy(x => x.DestinationId)
                    .Select(x => x.DestinationId).ToArray();
                var scripts = uow.Repositories.Get<Umbraco.Framework.Persistence.Model.IO.File>(true, scriptIds)
                    .OrderBy(x => x.Name);

                model.AvailableScripts = scripts.Select(x => new HierarchicalSelectListItem
                {
                    Text = x.GetFileNameForDisplay(),
                    Value = x.Id.ToString(),
                    ParentValues = x.RelationProxies.GetParentRelations(FixedRelationTypes.DefaultRelationType)
                        .Where(y => y.Item.SourceId.Value != scriptsRoot.Id.Value)
                        .Select(y => y.Item.SourceId.ToString()).ToArray(),
                    Selected = model.ScriptIds != null && model.ScriptIds.Contains(x.Id, new HiveIdComparer(true))
                });
            }

            using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<IFileStore>(new Uri("storage://macros/")))
            {
                // Macros
                var partials = uow.Repositories.GetAllNonContainerFiles()
                    .OrderBy(x => x.Name);

                model.AvailableMacros = partials.Select(x => new SelectListItem
                {
                    Text = x.GetFileNameForDisplay(),
                    Value = x.Id.ToString(),
                    Selected = model.MacroIds != null && model.MacroIds.Contains(x.Id, new HiveIdComparer(true))
                });
            }

            // Languages
            var languages = BackOfficeRequestContext.Application.Settings.Languages
                .OrderBy(x => x.Name);

            model.AvailableLanguages = languages.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.IsoCode,
                Selected = model.LanguageIds != null && model.LanguageIds.Contains(x.IsoCode)
            });
        }

        /// <summary>
        /// Displays the public repository
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult PublicRepository()
        {
            return View();
        }

        /// <summary>
        /// Displays the local repository
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult LocalRepository()
        {
            return View("LocalRepository", GetLocalPackages());
        }

        /// <summary>
        /// An action to recycle app domain
        /// </summary>
        /// <param name="id">The Hive id of the package</param>
        /// <param name="state">If it is installing or uninstalling</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult RecycleApplication(string id, PackageInstallationState state)
        {
            if (id == null) throw new ArgumentNullException("id");
            return View(new PackageInstallerStateModel {PackageId = id, State = state});
        }

        /// <summary>
        /// Actually does the shutting down of the app which is called by ajax, if onlyCheck is true, this will just
        /// return the status
        /// </summary>
        /// <param name="onlyCheck"></param> 
        /// <returns></returns>
        [HttpPost]
        public JsonResult PerformRecycleApplication(bool onlyCheck)
        {
            if (!onlyCheck)
            {
                UmbracoApplicationContext.RestartApplicationPool(HttpContext);
                return Json(new { status = "restarting" });
            }

            return Json(new { status = "restarted" });
        }

        /// <summary>
        /// Handles the post back for installing/removing/deleting local packages
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [SuccessfulOnRedirect]
        [Save("install", "uninstall", "remove")]
        public ActionResult ManagePackage()
        {
            var toInstallVal = ValueProvider.GetValue("install") == null 
                ? new object[] { } 
                : ValueProvider.GetValue("install").AttemptedValue.Split('-');
            var toInstall = toInstallVal.Length > 0 ? toInstallVal[0] : null;

            var toUninstallVal = ValueProvider.GetValue("uninstall") == null
                ? new object[] { }
                : ValueProvider.GetValue("uninstall").AttemptedValue.Split('-');
            var toUninstall = toUninstallVal.Length > 0 ? toUninstallVal[0] : null;

            var toRemoveVal = ValueProvider.GetValue("remove") == null
                ? new object[] { }
                : ValueProvider.GetValue("remove").AttemptedValue.Split('-');
            var toRemove = toRemoveVal.Length > 0 ? toRemoveVal[0] : null;

            if (toInstall != null)
            {
                //get the package from the source
                var package = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(toInstall.ToString());
                var version = Version.Parse(toInstallVal[1].ToString());
                BackOfficeRequestContext.PackageContext.LocalPackageManager.InstallPackage(package.Id, version, false);

                var logger = new PackageLogger(BackOfficeRequestContext, HttpContext, package);
                var installation = new PackageInstallation(BackOfficeRequestContext, HttpContext, package);

                //Copy files from package folder to destination and log results
                var fileResults = installation.CopyPackageFiles();
                foreach (var info in fileResults)
                {
                    logger.Log(info.IsCopiable, info.Message());
                }
                //Import data and log results
                var dataResults = installation.ImportData();
                foreach (var attributeType in dataResults.AttributeTypes)
                {
                    logger.Log(attributeType.IsImportable, string.Format("AttributeType {0}", attributeType.ObjectId.Value.ToString()));
                }
                foreach (var schema in dataResults.Schemas)
                {
                    logger.Log(schema.IsImportable, string.Format("Schema {0}", schema.ObjectId.Value.ToString()));
                }
                foreach (var schemaRelation in dataResults.SchemaRelations)
                {
                    logger.Log(schemaRelation.IsImportable, string.Format("Schema Relation {0}", schemaRelation.ObjectId.Value.ToString()));
                }
                foreach (var entity in dataResults.Entities)
                {
                    logger.Log(entity.IsImportable, string.Format("Entity {0}", entity.ObjectId.Value.ToString()));
                }
                foreach (var entityRelation in dataResults.EntityRelations)
                {
                    logger.Log(entityRelation.IsImportable, string.Format("Entity Relation {0}", entityRelation.ObjectId.Value.ToString()));
                }
                foreach (var language in dataResults.Languages)
                {
                    logger.Log(language.IsImportable, string.Format("Language {0}", language.ObjectId.Value.ToString()));
                }

                //Notifications.Add(new NotificationMessage(package.Title + " has been installed", "Package installed", NotificationType.Success));
                SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);

                logger.Persist();

                return RedirectToAction("RecycleApplication", new { id = package.Id, state = PackageInstallationState.Installing });
            }

            if (toUninstall != null)
            {
                //get the package from the installed location
                var nugetPackage = BackOfficeRequestContext.PackageContext.LocalPackageManager.LocalRepository.FindPackage(toUninstall.ToString());
                var packageFolderName = BackOfficeRequestContext.PackageContext.LocalPathResolver.GetPackageDirectory(nugetPackage);
                
                //execute some tasks
                var taskExeContext = _packageInstallUtility.GetTaskExecutionContext(nugetPackage, packageFolderName, PackageInstallationState.Uninstalling, this);
                _packageInstallUtility.RunPrePackageUninstallActions(taskExeContext, packageFolderName);

                BackOfficeRequestContext.PackageContext.LocalPackageManager.UninstallPackage(nugetPackage, false, false);

                //Notifications.Add(new NotificationMessage(nugetPackage.Title + " has been uninstalled", "Package uninstalled", NotificationType.Success));
                SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", nugetPackage.Id);

                return RedirectToAction("RecycleApplication", new { id = nugetPackage.Id, state = PackageInstallationState.Uninstalling });
            }
            
            if (toRemove != null)
            {

                var package = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(toRemove.ToString());
                var packageFile = BackOfficeRequestContext.PackageContext.LocalPathResolver.GetPackageFileName(package);


                //delete the package folder... this will check if the file exists by just the package name or also with the version
                if (BackOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.FileExists(packageFile))
                {
                    BackOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.DeleteFile(
                        Path.Combine(BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.Source, packageFile));    
                }
                else
                {
                    var fileNameWithVersion = packageFile.Substring(0, packageFile.IndexOf(".nupkg")) + "." + package.Version + ".nupkg";
                    BackOfficeRequestContext.PackageContext.LocalPackageManager.FileSystem.DeleteFile(
                        Path.Combine(BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.Source, fileNameWithVersion));    
                }
                
                Notifications.Add(new NotificationMessage(package.Title + " has been removed from the local repository", "Package removed", NotificationType.Success));
                SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);

                return RedirectToAction("LocalRepository");

            }


            return HttpNotFound();
        }

        /// <summary>
        /// Handles uploading a new local package
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [SuccessfulOnRedirect(IsRequired = false)]
        [Save("upload")]
        public ActionResult AddLocalPackage(HttpPostedFileBase file)
        {
            if(file == null)
            {
                ModelState.AddModelError("PackageFileValidation", "No file selected. Please select a package file to upload.");
                return LocalRepository();
            }

            if (!Path.GetExtension(file.FileName).EndsWith("nupkg"))
            {
                ModelState.AddModelError("PackageFileValidation", "The file uploaded is not a valid package file, only Nuget packages are supported");
                return LocalRepository();
            }

            IPackage package;
            try
            {
                package = new ZipPackage(file.InputStream);
            }
            catch (Exception ex)
            {
                LogHelper.Error<PackagingEditorController>("Package could not be unziped.", ex);

                ModelState.AddModelError("PackageFileValidation", "The Nuget package file uploaded could not be read");
                return LocalRepository();
            }

            try
            {
                var fileName = Path.Combine(BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.Source, file.FileName);
                file.SaveAs(fileName);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("PackageFileValidation", "The package file could not be saved. " + ex.Message);
                return LocalRepository();
            }

            if(!string.IsNullOrWhiteSpace(Request.Form["autoinstall"]))
            {
                BackOfficeRequestContext.PackageContext.LocalPackageManager.InstallPackage(package, false);

                var logger = new PackageLogger(BackOfficeRequestContext, HttpContext, package);
                var installation = new PackageInstallation(BackOfficeRequestContext, HttpContext, package);

                //Copy files from package folder to destination and log results
                var fileResults = installation.CopyPackageFiles();
                foreach (var info in fileResults)
                {
                    logger.Log(info.IsCopiable, info.Message());
                }
                //Import data and log results
                var dataResults = installation.ImportData();
                foreach (var attributeType in dataResults.AttributeTypes)
                {
                    logger.Log(attributeType.IsImportable, string.Format("AttributeType {0}", attributeType.ObjectId.Value.ToString()));
                }
                foreach (var schema in dataResults.Schemas)
                {
                    logger.Log(schema.IsImportable, string.Format("Schema {0}", schema.ObjectId.Value.ToString()));
                }
                foreach (var schemaRelation in dataResults.SchemaRelations)
                {
                    logger.Log(schemaRelation.IsImportable, string.Format("Schema Relation {0}", schemaRelation.ObjectId.Value.ToString()));
                }
                foreach (var entity in dataResults.Entities)
                {
                    logger.Log(entity.IsImportable, string.Format("Entity {0}", entity.ObjectId.Value.ToString()));
                }
                foreach (var entityRelation in dataResults.EntityRelations)
                {
                    logger.Log(entityRelation.IsImportable, string.Format("Entity Relation {0}", entityRelation.ObjectId.Value.ToString()));
                }
                foreach (var language in dataResults.Languages)
                {
                    logger.Log(language.IsImportable, string.Format("Language {0}", language.ObjectId.Value.ToString()));
                }

                //Notifications.Add(new NotificationMessage(package.Title + " has been installed", "Package installed", NotificationType.Success));
                //SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);

                return RedirectToAction("RecycleApplication", new { id = package.Id, state = PackageInstallationState.Installing });
            }

            Notifications.Add(new NotificationMessage(package.Title + " added to local repository", "Package added", NotificationType.Success));
            SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", package.Id);

            return RedirectToAction("LocalRepository");
        }

        /// <summary>
        /// Runs the actions for a package, this is generally run after the app pool is recycled
        /// </summary>
        /// <param name="id">The nuget package id</param>
        /// <param name="state">If its installing or uninstalling</param>
        /// <returns></returns>
        [HttpGet]
        [SuccessfulOnRedirect]
        public ActionResult RunPackageActions(string id, PackageInstallationState state)
        {
            var nugetPackage = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(id);
            
            //Run the configuration tasks
            //get the package with the name and ensure it exists
            
            if (nugetPackage != null)
            {
                //get the package folder name, create the task execution context and then execute the package tasks using the utility class
                var packageFolderName = BackOfficeRequestContext.PackageContext.LocalPathResolver.GetPackageDirectory(nugetPackage);
                var taskExeContext = _packageInstallUtility.GetTaskExecutionContext(nugetPackage, packageFolderName, state, this);
                _packageInstallUtility.RunPostPackageInstallActions(state, taskExeContext, packageFolderName);    
            
                //re-issue authentication token incase any permissions have changed so that a re-login is not required.
                if (User.Identity is UmbracoBackOfficeIdentity)
                {
                    var userId = ((UmbracoBackOfficeIdentity)User.Identity).Id;
                    var user = BackOfficeRequestContext.Application.Security.Users.GetById(userId, true);
                    var userData = BackOfficeRequestContext.Application.FrameworkContext.TypeMappers.Map<UserData>(user);  
                    HttpContext.CreateUmbracoAuthTicket(userData);
                }

                switch (state)
                {
                    case PackageInstallationState.Installing:

                        Notifications.Add(new NotificationMessage(nugetPackage.Title + " has been installed", "Package installed", NotificationType.Success));
                        SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", nugetPackage.Id);

                        return RedirectToAction("Installed", new { id = nugetPackage.Id });
                    case PackageInstallationState.Uninstalling:

                        Notifications.Add(new NotificationMessage(nugetPackage.Title + " has been uninstalled", "Package uninstalled", NotificationType.Success));
                        SuccessfulOnRedirectAttribute.EnsureRouteData(this, "id", nugetPackage.Id);

                        return RedirectToAction("Uninstalled");
                }
            }

            //if the task did not redirect, then perform default redirects
            return RedirectToAction("LocalRepository");
        }

        /// <summary>
        /// Displayed after a package is successfully installed
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Installed(string id)
        {
            //TODO: Load install log and report any errors

            var package = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository.FindPackage(id);
            var logger = new PackageLogger(BackOfficeRequestContext, HttpContext, package, true);
            var model = new PackageLog
                            {
                                PackageId = package.Id,
                                Title = package.Title, 
                                Version = package.Version,
                                ProjectUrl = package.ProjectUrl,
                                LogEntries = logger.GetLogEntries()
                            };

            return View("Installed", model);
        }

        /// <summary>
        /// Displayed after a package is successfully installed
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Uninstalled()
        {
            //TODO: Load uninstall log and report any errors

            return View("Uninstalled", GetLocalPackages());
        }

        private IEnumerable<PackageModel> GetLocalPackages()
        {           
            var packages = BackOfficeRequestContext.PackageContext.LocalPackageManager.SourceRepository
                .GetPackages()
                .ToArray();

            Func<IPackage, bool> checkLatest = (p) =>
                {
                    var packagesForId = packages.Where(x => x.Id == p.Id).ToArray();
                    //need to check major/minor version first, then need to go by DatePublished.
                    var biggestMajor = packagesForId.Max(x => x.Version.Major);
                    if (p.Version.Major < biggestMajor) return false;
                    //this means the major versions are the same, so now check minor
                    var biggestMinor = packagesForId
                        .Where(x => x.Version.Major == biggestMajor)
                        .Max(x => x.Version.Minor);
                    if (p.Version.Minor < biggestMinor) return false;
                    //this means the minor version are the same, so now check published date
                    var published = packagesForId
                        .Where(x => x.Version.Major == biggestMajor
                            && x.Version.Minor == biggestMinor
                            && x.Published.HasValue).ToArray();
                    if (!p.Published.HasValue && published.Count(x => x.Published.HasValue) > 0)
                        return false;
                    var maxPublish = published.Max(x => x.Published.Value);
                    if (p.Published.Value < maxPublish) return false;
                    
                    return true;
                };

            var models =  packages.Select(x => new PackageModel
                {
                    IsLatestVersion = checkLatest(x),
                    IsVersionInstalled = BackOfficeRequestContext.PackageContext.LocalPackageManager.LocalRepository.FindPackage(x.Id, x.Version) != null,
                    Metadata = x,
                    IsPackageInstalled = x.IsInstalled(BackOfficeRequestContext.PackageContext.LocalPackageManager.LocalRepository)
                }).ToArray();

            return models;
        }
    }
}
