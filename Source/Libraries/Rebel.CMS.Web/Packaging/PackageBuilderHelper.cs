using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using NuGet;
using Rebel.Cms.Web.Configuration.Languages;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Macros;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Packaging
{
    public static class PackageBuilderHelper
    {
        public static CreatePackageResult CreatePackage(string id, PackageDefinition packageDef)
        {
            return CreatePackage(id, packageDef, DependencyResolver.Current.GetService<IRebelApplicationContext>(),
                new HttpContextWrapper(HttpContext.Current));
        }

        public static CreatePackageResult CreatePackage(string id, PackageDefinition packageDef, IRebelApplicationContext appContext, 
            HttpContextWrapper httpContext)
        {
            var result = new CreatePackageResult();

            try
            {
                var packagePath = string.Format("~/App_Data/Rebel/CreatedPackages/{0}", id);
                var packageDir = new DirectoryInfo(httpContext.Server.MapPath(packagePath));
                var packageFile = new FileInfo(Path.Combine(packageDir.FullName, "package.nupkg"));

                // Build package
                var builder = new PackageBuilder
                {
                    Id = packageDef.Alias,
                    Title = packageDef.Name,
                    Version = new Version(packageDef.Version),
                    Description = packageDef.Description,
                    ProjectUrl = new Uri(packageDef.ProjectUrl)
                };
                builder.Authors.Add(packageDef.Author);

                if(!string.IsNullOrWhiteSpace(packageDef.Tags))
                    builder.Tags.AddRange(packageDef.Tags.Split(' '));

                if(!string.IsNullOrWhiteSpace(packageDef.LicenseUrl))
                    builder.LicenseUrl = new Uri(packageDef.LicenseUrl);

                using (var uow = appContext.Hive.OpenReader<IContentStore>())
                {
                    var uploadedFilesToPackage = new List<HiveId>();

                    // Content
                    if(!packageDef.ContentNodeId.IsNullValueOrEmpty())
                    {
                        var relationsToSerialize = new List<IRelationById>();
                        var nodesToSerialize = new List<TypedEntity>();

                        var contentNode = uow.Repositories.Get<TypedEntity>(packageDef.ContentNodeId);
                        nodesToSerialize.Add(contentNode);
                        var parentRelations = uow.Repositories.GetParentRelations(contentNode.Id, FixedRelationTypes.DefaultRelationType);
                        relationsToSerialize.AddRange(parentRelations);

                        if(packageDef.IncludeChildContentNodes)
                        {
                            var childrenRelations = uow.Repositories.GetDescendentRelations(contentNode.Id, FixedRelationTypes.DefaultRelationType);
                            nodesToSerialize.AddRange(uow.Repositories.Get<TypedEntity>(true, childrenRelations.Select(x => x.DestinationId).ToArray()));
                            relationsToSerialize.AddRange(childrenRelations);
                        }

                        foreach (var node in nodesToSerialize.Where(x => x.Attributes.Any(y => y.AttributeDefinition.AttributeType.RenderTypeProvider.InvariantEquals(CorePluginConstants.FileUploadPropertyEditorId))))
                        {
                            var attributes = node.Attributes.Where(x => x.AttributeDefinition.AttributeType.RenderTypeProvider.InvariantEquals(CorePluginConstants.FileUploadPropertyEditorId));
                            uploadedFilesToPackage.AddRange(attributes.Where(x => x.Values["Value"] != null).Select(attribute => HiveId.Parse(attribute.Values["Value"].ToString())));
                        }

                        CopySerializedObjectsToPackage(builder, appContext, nodesToSerialize, 
                            node => "Data/Content/" + node.Id.ToString().ToMd5() + ".json");

                        // Relations
                        CopySerializedObjectsToPackage(builder, appContext, relationsToSerialize,
                            relation => "Data/Content/Relations/" + relation.SourceId.ToString().ToMd5() + "-" + relation.DestinationId.ToString().ToMd5() + ".json");
                    }

                    // Media
                    if (!packageDef.MediaNodeId.IsNullValueOrEmpty())
                    {
                        var relationsToSerialize = new List<IRelationById>();
                        var nodesToSerialize = new List<TypedEntity>();

                        var mediaNode = uow.Repositories.Get<TypedEntity>(packageDef.MediaNodeId);
                        if (mediaNode != null)
                        {
                            nodesToSerialize.Add(mediaNode);
                            var parentRelations = uow.Repositories.GetParentRelations(mediaNode.Id, FixedRelationTypes.DefaultRelationType);
                            relationsToSerialize.AddRange(parentRelations);

                            if (packageDef.IncludeChildContentNodes)
                            {
                                var childrenRelations = uow.Repositories.GetDescendentRelations(mediaNode.Id, FixedRelationTypes.DefaultRelationType);
                                nodesToSerialize.AddRange(uow.Repositories.Get<TypedEntity>(true, childrenRelations.Select(x => x.DestinationId).ToArray()));
                                relationsToSerialize.AddRange(childrenRelations);
                            }
                        }

                        foreach (var node in nodesToSerialize.Where(x => x.Attributes.Any(y => y.AttributeDefinition.AttributeType.RenderTypeProvider.InvariantEquals(CorePluginConstants.FileUploadPropertyEditorId))))
                        {
                            var attributes = node.Attributes.Where(x => x.AttributeDefinition.AttributeType.RenderTypeProvider.InvariantEquals(CorePluginConstants.FileUploadPropertyEditorId));
                            uploadedFilesToPackage.AddRange(attributes.Where(x => x.Values["Value"] != null).Select(attribute => HiveId.Parse(attribute.Values["Value"].ToString())));
                        }

                        CopySerializedObjectsToPackage(builder, appContext, nodesToSerialize,
                            node => "Data/Media/" + node.Id.ToString().ToMd5() + ".json");

                        // Relations
                        CopySerializedObjectsToPackage(builder, appContext, relationsToSerialize,
                            relation => "Data/Media/Relations/" + relation.SourceId.ToString().ToMd5() + "-" + relation.DestinationId.ToString().ToMd5() + ".json");
                    }

                    // Files
                    CopyFilesToPackage(builder, appContext, "storage://file-uploader", uploadedFilesToPackage, "Content/Content/Media/");

                    // Dictionary Items
                    var dictionaryRelationsToSerialize = new List<IRelationById>();
                    var dictionaryItems = uow.Repositories.Get<TypedEntity>(true, packageDef.DictionaryItemIds.ToArray());
                    CopySerializedObjectsToPackage(builder, appContext, dictionaryItems,
                        dictionaryItem => "Data/DictionaryItems/" + dictionaryItem.Id.ToString().ToMd5() + ".json");

                    foreach (var parentRelations in dictionaryItems.Select(dictionaryItem => uow.Repositories.GetParentRelations(dictionaryItem.Id)))
                    {
                        dictionaryRelationsToSerialize.AddRange(parentRelations);
                    }

                    CopySerializedObjectsToPackage(builder, appContext, dictionaryRelationsToSerialize,
                            relation => "Data/DictionaryItems/Relations/" + relation.SourceId.ToString().ToMd5() + "-" + relation.DestinationId.ToString().ToMd5() + ".json");

                    // Doc Types
                    var docTypeRelationsToSerialize = new List<IRelationById>();
                    var docTypes = uow.Repositories.Schemas.Get<EntitySchema>(true, packageDef.DocumentTypeIds.ToArray());
                    CopySerializedObjectsToPackage(builder, appContext, docTypes, docType => "Data/DocumentTypes/" + docType.Alias + ".json");

                    foreach (var parentRelations in docTypes.Select(docType => uow.Repositories.Schemas.GetParentRelations(docType.Id)))
                    {
                        docTypeRelationsToSerialize.AddRange(parentRelations);
                    }

                    CopySerializedObjectsToPackage(builder, appContext, docTypeRelationsToSerialize,
                            relation => "Data/DocumentTypes/Relations/" + relation.SourceId.ToString().ToMd5() + "-" + relation.DestinationId.ToString().ToMd5() + ".json");

                    // Media Types
                    var mediaTypeRelationsToSerialize = new List<IRelationById>();
                    var mediaTypes = uow.Repositories.Schemas.Get<EntitySchema>(true, packageDef.MediaTypeIds.ToArray());
                    CopySerializedObjectsToPackage(builder, appContext, mediaTypes, mediaType => "Data/MediaTypes/" + mediaType.Alias + ".json");

                    foreach (var parentRelations in mediaTypes.Select(mediaType => uow.Repositories.Schemas.GetParentRelations(mediaType.Id)))
                    {
                        mediaTypeRelationsToSerialize.AddRange(parentRelations);
                    }

                    CopySerializedObjectsToPackage(builder, appContext, mediaTypeRelationsToSerialize,
                            relation => "Data/MediaTypes/Relations/" + relation.SourceId.ToString().ToMd5() + "-" + relation.DestinationId.ToString().ToMd5() + ".json");

                    // Data Types
                    var dataTypes = uow.Repositories.Schemas.Get<AttributeType>(true, packageDef.DataTypeIds.ToArray());
                    CopySerializedObjectsToPackage(builder, appContext, dataTypes, dataType => "Data/DataTypes/" + dataType.Alias + ".json");
                }

                // Templates
                CopyFilesToPackage(builder, appContext, "storage://templates", packageDef.TemplateIds, "Content/Views/");

                // Partials
                CopyFilesToPackage(builder, appContext, "storage://partials", packageDef.PartialIds, "Content/Views/Partials/");

                // Stylesheets
                CopyFilesToPackage(builder, appContext, "storage://stylesheets", packageDef.StylesheetIds, "Content/Content/Styles/");

                // Scripts
                CopyFilesToPackage(builder, appContext, "storage://scripts", packageDef.ScriptIds, "Content/Scripts/");

                // Macros
                CopyFilesToPackage(builder, appContext, "storage://macros", packageDef.MacroIds, "Content/App_Data/Rebel/Macros/",
                    (file, packageBuilder) =>
                {
                    var macro = MacroSerializer.FromFile(file);
                    if (macro.MacroType == "PartialView")
                    {
                        var macroParts = macro.SelectedItem.Split('-');
                        var areaName = macroParts.Length > 1 ? macroParts[0] : "";

                        var macroName = (areaName.IsNullOrWhiteSpace())
                            ? string.Join("", macroParts)
                            : macroParts[1];

                        var relativePath = (areaName.IsNullOrWhiteSpace())
                            ? "~/Views/MacroPartials/" + macroName + ".cshtml"
                            : "~/App_Plugins/Packages/" + areaName + "/Views/MacroPartials/" + macroName + ".cshtml";

                        var path = httpContext.Server.MapPath(relativePath);

                        packageBuilder.Files.Add(new PhysicalPackageFile
                        {
                            SourcePath = path,
                            TargetPath = "Content/Views/MacroPartials/" + macroName +".cshtml"
                        });
                    }
                });

                // Languages
                var languages = appContext.Settings.Languages.Where(x => packageDef.LanguageIds.Contains(x.IsoCode));
                CopySerializedObjectsToPackage(builder, appContext, languages, lang => "Data/Languages/" + lang.IsoCode + ".json");

                // Misc files
                foreach (var file in packageDef.AdditionalFiles)
                {
                    var cleanFile = "~/" + file.Replace('\\', '/').TrimStart('~','/');
                    var cleanFilePath = httpContext.Server.MapPath(cleanFile);

                    if (!File.Exists(cleanFilePath) && !Directory.Exists(cleanFilePath))
                        continue;

                    var fileInfo = File.GetAttributes(cleanFilePath);
                    var isDirectory = (fileInfo & FileAttributes.Directory) == FileAttributes.Directory;

                    if (cleanFile.StartsWith("~/App_Plugins/Packages/" + packageDef.Alias + "/", true, CultureInfo.InvariantCulture))
                    {
                        if (isDirectory)
                        {
                            CopyFolderToPackage(builder, appContext, httpContext.Server.MapPath("~/App_Plugins/Packages/" + packageDef.Alias + "/"), 
                                cleanFilePath, (rootPath, path) => path.TrimStart(rootPath).Replace('\\', '/'));
                        }
                        else
                        {
                            builder.Files.Add(new PhysicalPackageFile
                            {
                                SourcePath = httpContext.Server.MapPath(cleanFile),
                                TargetPath = Regex.Replace(cleanFile, "^~/App_Plugins/Packages/" + packageDef.Alias + "/", "", RegexOptions.IgnoreCase)
                            });
                        }
                    }
                    else if (cleanFile.StartsWith("~/Bin/", true, CultureInfo.InvariantCulture))
                    {
                        if (isDirectory)
                        {
                            CopyFolderToPackage(builder, appContext, httpContext.Server.MapPath("~/Bin/"),
                                cleanFilePath, (rootPath, path) => "lib/" + path.TrimStart(rootPath).Replace('\\', '/'));
                        }
                        else
                        {
                            builder.Files.Add(new PhysicalPackageFile
                            {
                                SourcePath = httpContext.Server.MapPath(cleanFile),
                                TargetPath = "lib/" + Regex.Replace(cleanFile, "^~/Bin/", "", RegexOptions.IgnoreCase)
                            });
                        }
                    }
                    else
                    {
                        if (isDirectory)
                        {
                            CopyFolderToPackage(builder, appContext, httpContext.Server.MapPath("~/"),
                                cleanFilePath, (rootPath, path) => "Content/" + path.TrimStart(rootPath).Replace('\\', '/'));
                        }
                        else
                        {
                            builder.Files.Add(new PhysicalPackageFile
                            {
                                SourcePath = httpContext.Server.MapPath(cleanFile),
                                TargetPath = "Content/" + Regex.Replace(cleanFile, "^~/", "", RegexOptions.IgnoreCase)
                            });
                        }
                    }
                }

                // Web.config
                if(!string.IsNullOrWhiteSpace(packageDef.Config))
                {
                    builder.Files.Add(new ByteArrayPackageFile
                    {
                        Contents = Encoding.UTF8.GetBytes(packageDef.Config),
                        TargetPath = "Web.config"
                    });
                }

                // Write package to disc
                using (Stream stream = File.Create(packageFile.FullName))
                {
                    builder.Save(stream);
                }

                // If we've gotten this far, everything must have gone ok
                result.Success = true;
            }
            catch (global::System.Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        private static void CopySerializedObjectsToPackage<T>(IPackageBuilder builder, IRebelApplicationContext appContext, 
            IEnumerable<T> objects, Func<T, string> targetPathCallback)
        {
            foreach (var obj in objects)
            {
                var serializeResult = appContext.FrameworkContext.Serialization.ToStream(obj);
                if (serializeResult.Success)
                {
                    builder.Files.Add(new StreamPackageFile
                    {
                        Contents = serializeResult.ResultStream,
                        TargetPath = targetPathCallback.Invoke(obj)
                    });
                }
            }
        }

        private static void CopyFilesToPackage(IPackageBuilder builder, IRebelApplicationContext appContext, 
            string storeUri, IEnumerable<HiveId> fileIds, string targetRootPath,
            Action<Rebel.Framework.Persistence.Model.IO.File, IPackageBuilder> callback = null)
        {
            using (var uow = appContext.Hive.OpenReader<IFileStore>(new Uri(storeUri)))
            {
                var root = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(new HiveId("/"));
                var files = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(true, fileIds.ToArray())
                    .Where(x => !x.IsContainer);

                foreach (var file in files)
                {
                    builder.Files.Add(new ByteArrayPackageFile
                    {
                        Contents = file.ContentBytes,
                        TargetPath = targetRootPath + file.RootRelativePath.TrimStart(root.RootRelativePath).TrimStart("/")
                    });
                
                    if(callback != null)
                        callback.Invoke(file, builder);
                }
            }
        }

        private static void CopyFolderToPackage(IPackageBuilder builder, IRebelApplicationContext appContext,
            string rootPath, string path, Func<string, string, string> targetPathCallback)
        {
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                builder.Files.Add(new PhysicalPackageFile
                {
                    SourcePath = file,
                    TargetPath = targetPathCallback.Invoke(rootPath, file)
                });
            }

            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                CopyFolderToPackage(builder, appContext, rootPath, dir, targetPathCallback);
            }
        }

        public static PackageDefinition GetPackageDefinitionById(IGroupUnit<IFileStore> uow, 
            HiveId  id)
        {
            var folder = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(id);
            if (folder == null)
                return null;

            var fileIds = uow.Repositories.GetChildRelations(folder.Id).Select(x => x.DestinationId).ToArray();
            var files = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(true, fileIds);
            var definitionFile = files.SingleOrDefault(x => x.Name.EndsWith(".definition"));

            if (definitionFile == null)
                return null;

            var contents = Encoding.UTF8.GetString(definitionFile.ContentBytes);
            if (string.IsNullOrWhiteSpace(contents))
                return null;
            try
            {
                var def = contents.DeserializeJson<PackageDefinition>();
                def.Id = id;

                return def;
            }
            catch(global::System.Exception)
            {
                return null;
            }
        }

        public static PackageDefinition GetPackageDefinitionById(IReadonlyGroupUnit<IFileStore> uow,
            HiveId id)
        {
            var folder = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(id);
            if (folder == null)
                return null;

            var fileIds = uow.Repositories.GetChildRelations(folder.Id).Select(x => x.DestinationId).ToArray();
            var files = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(true, fileIds);
            var definitionFile = files.SingleOrDefault(x => x.Name.EndsWith(".definition"));

            if (definitionFile == null)
                return null;

            var contents = Encoding.UTF8.GetString(definitionFile.ContentBytes);
            if (string.IsNullOrWhiteSpace(contents))
                return null;

            try
            {
                var def = contents.DeserializeJson<PackageDefinition>();
                def.Id = id;

                return def;
            }
            catch (global::System.Exception)
            {
                return null;
            }
        }

        public static Rebel.Framework.Persistence.Model.IO.File GetPackageFileById(IGroupUnit<IFileStore> uow,
            HiveId id)
        {
            var folder = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(id);
            if (folder == null)
                return null;

            var fileIds = uow.Repositories.GetChildRelations(folder.Id).Select(x => x.DestinationId).ToArray();
            var files = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(true, fileIds);
            var packageFile = files.SingleOrDefault(x => x.Name.EndsWith(".nupkg"));

            return packageFile;
        }

        public static Rebel.Framework.Persistence.Model.IO.File GetPackageFileById(IReadonlyGroupUnit<IFileStore> uow,
            HiveId id)
        {
            var folder = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(id);
            if (folder == null)
                return null;

            var fileIds = uow.Repositories.GetChildRelations(folder.Id).Select(x => x.DestinationId).ToArray();
            var files = uow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(true, fileIds);
            var packageFile = files.SingleOrDefault(x => x.Name.EndsWith(".nupkg"));

            return packageFile;
        }
    }

    internal sealed class ByteArrayPackageFile : IPackageFile
    {
        public byte[] Contents { get; set; }
        public string TargetPath { get; set; }

        public override string ToString()
        {
            return TargetPath;
        }

        public Stream GetStream()
        {
            var stream = new MemoryStream();
            stream.Write(Contents, 0, Contents.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        string IPackageFile.Path
        {
            get { return TargetPath; }
        }
    }

    internal sealed class StreamPackageFile : IPackageFile
    {
        public Stream Contents { get; set; }
        public string TargetPath { get; set; }

        public override string ToString()
        {
            return TargetPath;
        }

        public Stream GetStream()
        {
            if (Contents.CanSeek)
                Contents.Seek(0, 0);

            return Contents;
        }

        string IPackageFile.Path
        {
            get { return TargetPath; }
        }
    }
}
