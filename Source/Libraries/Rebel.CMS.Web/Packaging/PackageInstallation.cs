using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using NuGet;
using Rebel.Cms.Web.Configuration.Languages;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.Model.BackOffice.Editors;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Cms.Web.Packaging
{
    public class PackageInstallation
    {
        private readonly IBackOfficeRequestContext _context;
        private readonly IPackage _package;
        private readonly HttpContextBase _httpContext;
        private readonly string _absoluteRootedPath;
        private readonly string _absolutePackagePath;

        public PackageInstallation(IBackOfficeRequestContext context, HttpContextBase httpContext, IPackage package)
        {
            _context = context;
            _package = package;
            _httpContext = httpContext;

            var packageFolderName = _context.PackageContext.LocalPathResolver.GetPackageDirectory(_package);
            var packageFolderPath = Path.Combine(_context.Application.Settings.PluginConfig.PluginsPath, "Packages", packageFolderName);
            _absolutePackagePath = _httpContext.Server.MapPath(packageFolderPath);
            _absoluteRootedPath = _httpContext.Server.MapPath("~/");

            Mandate.That(Directory.Exists(_absolutePackagePath), x => new FileNotFoundException("The package directory " + _absolutePackagePath + " could not be found"));
        }

        /// <summary>
        /// Verifies that all files from Package source can be copied to destination,
        /// and then goes on to copy the files that can be copied.
        /// </summary>
        public List<CustomFileSystemInfo> CopyPackageFiles()
        {
            //Get folders in Content directory
            var absoluteContentFolderPath = Path.Combine(_absolutePackagePath, "Content");
            if(!Directory.Exists(absoluteContentFolderPath))
                return new List<CustomFileSystemInfo>();

            var directories = new DirectoryInfo(absoluteContentFolderPath).GetDirectories("*", SearchOption.TopDirectoryOnly);
            var files = new List<CustomFileSystemInfo>();

            //Loop through files in Content directory and compare to destination
            foreach (var directory in directories)
            {
                var fileInfos = directory.GetFiles("*", SearchOption.AllDirectories);
                files.AddRange(from fileInfo in fileInfos
                               let destinationFilePath =
                                   fileInfo.FullName.Replace(absoluteContentFolderPath.TrimEnd('\\'), _absoluteRootedPath.TrimEnd('\\'))
                               select
                                   new CustomFileSystemInfo(fileInfo.FullName, destinationFilePath, fileInfo.Name, false)
                                       {IsCopiable = !File.Exists(destinationFilePath)});

                var directoryInfos = directory.GetDirectories("*", SearchOption.AllDirectories);
                files.AddRange(from directoryInfo in directoryInfos
                               let destinationFilePath =
                                   directoryInfo.FullName.Replace(absoluteContentFolderPath.TrimEnd('\\'), _absoluteRootedPath.TrimEnd('\\'))
                               select
                                   new CustomFileSystemInfo(directoryInfo.FullName, destinationFilePath, directoryInfo.Name, true)
                                       {IsCopiable = !Directory.Exists(destinationFilePath)});
            }

            //Copy all files to destination except those that already exist or are read only
            //TODO change to use hive provider for copying files to the right location.
            foreach (var file in files.Where(file => file.IsCopiable && file.IsDirectory))
            {
                Directory.CreateDirectory(file.DestinationPath);
            }

            foreach (var file in files.Where(file => file.IsCopiable && !file.IsDirectory))
            {
                File.Copy(file.SourcePath, file.DestinationPath);
            }

            return files;
        }

        /// <summary>
        /// Imports serialized data from a package by checking if its valid
        /// and already exists.
        /// </summary>
        public SerializedDataImport ImportData()
        {
            //Check if data folder exists
            var absoluteDataFolderPath = Path.Combine(_absolutePackagePath, "Data");
            if(!Directory.Exists(absoluteDataFolderPath))
                return new SerializedDataImport();

            //Load serialized data and deserialize to objects for import
            var serialization = _context.Application.FrameworkContext.Serialization;
            SerializedDataImport dataImport = new SerializedDataImport();

            var directories = new DirectoryInfo(absoluteDataFolderPath).GetDirectories("*", SearchOption.TopDirectoryOnly);
            foreach (var directory in directories)
            {
                var fileInfos = directory.GetFiles("*", SearchOption.AllDirectories);
                foreach (var fileInfo in fileInfos)
                {
                    if (fileInfo.DirectoryName == null) continue;

                    //Deserialize content
                    if (fileInfo.DirectoryName.EndsWith("data\\content", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(TypedEntity));
                        dataImport.Entities.Add(new DeseriazliedDataResult(o, typeof(TypedEntity)));
                    }
                    else if (fileInfo.DirectoryName.EndsWith("data\\content\\relations", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(Relation));
                        dataImport.EntityRelations.Add(new DeseriazliedDataResult(o, typeof(Relation)));
                    }
                    //Deserialize DataTypes
                    if (fileInfo.DirectoryName.EndsWith("data\\datatypes", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(AttributeType));
                        dataImport.AttributeTypes.Add(new DeseriazliedDataResult(o, typeof(AttributeType)));
                    }
                    //Deserialize Dictionary Items
                    if (fileInfo.DirectoryName.EndsWith("data\\dictionaryitems", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(TypedEntity));
                        dataImport.Entities.Add(new DeseriazliedDataResult(o, typeof(TypedEntity)));
                    }
                    else if (fileInfo.DirectoryName.EndsWith("data\\dictionaryitems\\relations", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(Relation));
                        dataImport.EntityRelations.Add(new DeseriazliedDataResult(o, typeof(Relation)));
                    }
                    //Deserialize DocumentTypes
                    if (fileInfo.DirectoryName.EndsWith("data\\documenttypes", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(EntitySchema));
                        dataImport.Schemas.Add(new DeseriazliedDataResult(o, typeof(EntitySchema)));
                    }
                    else if (fileInfo.DirectoryName.EndsWith("data\\documenttypes\\relations", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(Relation));
                        dataImport.SchemaRelations.Add(new DeseriazliedDataResult(o, typeof(Relation)));
                    }
                    //Deserialize Media
                    if (fileInfo.DirectoryName.EndsWith("data\\media", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(TypedEntity));
                        dataImport.Entities.Add(new DeseriazliedDataResult(o, typeof(TypedEntity)));
                    }
                    else if (fileInfo.DirectoryName.EndsWith("data\\media\\relations", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(Relation));
                        dataImport.EntityRelations.Add(new DeseriazliedDataResult(o, typeof(Relation)));
                    }
                    //Deserialize MediaTypes
                    if (fileInfo.DirectoryName.EndsWith("data\\mediatypes", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(EntitySchema));
                        dataImport.Schemas.Add(new DeseriazliedDataResult(o, typeof(EntitySchema)));
                    }
                    else if (fileInfo.DirectoryName.EndsWith("data\\mediatypes\\relations", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(Relation));
                        dataImport.SchemaRelations.Add(new DeseriazliedDataResult(o, typeof(Relation)));
                    }
                    //Deserialize Languages
                    if (fileInfo.DirectoryName.EndsWith("data\\languages", StringComparison.InvariantCultureIgnoreCase))
                    {
                        object o = serialization.FromStream(fileInfo.OpenRead(), typeof(LanguageElement));
                        dataImport.Languages.Add(new DeseriazliedDataResult(o, typeof(LanguageElement)));
                    }
                }
            }

            //Check entities and relations
            CheckImportableEntities(dataImport);
            CheckImportableRelations(dataImport);

            //Open writer and save deserialized attribute types, schemas, entities and relations
            using (var uow = _context.Application.Hive.OpenWriter<IContentStore>())
            {
                foreach (var attributeType in dataImport.AttributeTypes.Where(attributeType => attributeType.IsImportable))
                {
                    uow.Repositories.Schemas.AddOrUpdate(attributeType.DeserializedObject as AttributeType);
                }

                foreach (var schema in dataImport.Schemas.Where(schema => schema.IsImportable))
                {
                    uow.Repositories.Schemas.AddOrUpdate(schema.DeserializedObject as EntitySchema);
                }

                foreach (var schemaRelation in dataImport.SchemaRelations.Where(schema => schema.IsImportable))
                {
                    var relation = schemaRelation.DeserializedObject as IRelationById;
                    uow.Repositories.Schemas.AddRelation(relation.SourceId, relation.DestinationId, relation.Type, relation.Ordinal, relation.MetaData.ToArray());
                }

                foreach (var entity in dataImport.Entities.Where(entity => entity.IsImportable))
                {
                    uow.Repositories.AddOrUpdate(entity.DeserializedObject as TypedEntity);
                }

                foreach (var entityRelation in dataImport.EntityRelations.Where(schema => schema.IsImportable))
                {
                    var relation = entityRelation.DeserializedObject as IRelationById;
                    uow.Repositories.AddRelation(relation.SourceId, relation.DestinationId, relation.Type, relation.Ordinal, relation.MetaData.ToArray());
                }

                uow.Complete();
            }

            // Regenerate any image thubnails
            var entitiesWithFiles = dataImport.Entities.Where(x => x.IsImportable)
                .Select(x => x.DeserializedObject as TypedEntity)
                .Where(x => x.Attributes
                    .Any(y => y.AttributeDefinition.AttributeType.RenderTypeProvider.InvariantEquals(CorePluginConstants.FileUploadPropertyEditorId)));

            if (entitiesWithFiles.Any())
            {
                // Get a list of attribute type ids that use the file uploader property editor
                var attributeTypeIds = entitiesWithFiles
                    .SelectMany(x => x.Attributes
                        .Where(y => y.AttributeDefinition.AttributeType.RenderTypeProvider.InvariantEquals(CorePluginConstants.FileUploadPropertyEditorId))
                        .Select(y => y.AttributeDefinition.AttributeType.Id))
                        .Distinct().ToArray();

                using (var contentUow = _context.Application.Hive.OpenWriter<IContentStore>())
                using (var fileUow = _context.Application.Hive.OpenWriter<IFileStore>(new Uri("storage://file-uploader/")))
                {
                    // Load attribute types from hive incase prevalues are different to those serialized (hive take presidence)
                    var attributeTypes = contentUow.Repositories.Schemas.Get<AttributeType>(true, attributeTypeIds);

                    foreach (var entity in entitiesWithFiles)
                    {
                        var uploadAttributes = entity.Attributes
                            .Where(x => x.AttributeDefinition.AttributeType.RenderTypeProvider.InvariantEquals(CorePluginConstants.FileUploadPropertyEditorId))
                            .ToList();

                        foreach (var uploadAttribute in uploadAttributes)
                        {
                            var mediaId = uploadAttribute.Values["MediaId"].ToString();
                            var fileId = HiveId.Parse(uploadAttribute.Values["Value"].ToString());
                            if (!string.IsNullOrWhiteSpace(mediaId) && !fileId.IsNullValueOrEmpty())
                            {
                                // Refetch the attribute type
                                var attributeType = attributeTypes.SingleOrDefault(x => x.Id == uploadAttribute.AttributeDefinition.AttributeType.Id);
                                var dataType = _context.Application.FrameworkContext.TypeMappers.Map<DataType>(attributeType);

                                var preValue = dataType.GetPreValueModel() as dynamic;
                                var sizes = preValue.Sizes;
                                var file = fileUow.Repositories.Get<Rebel.Framework.Persistence.Model.IO.File>(fileId);
                                if (file.IsImage())
                                    ContentExtensions.CreateThumbnails(fileUow, file, mediaId, sizes);
                            }
                        }
                    }

                    fileUow.Complete();
                }
            }

            if(dataImport.Languages.Any())
            {
                // Persist the language entity
                var configFile = Path.Combine(_httpContext.Server.MapPath("~/App_Data/Rebel/Config"), "rebel.cms.languages.config");
                var configXml = XDocument.Load(configFile);

                foreach (var language in dataImport.Languages)
                {
                    var lang = language.DeserializedObject as LanguageElement;
                    if(lang == null) continue;

                    // Remove previous entry
                    configXml.Descendants("language").Where(x => x.Attribute("isoCode").Value == lang.IsoCode).Remove();

                    // Add new entry
                    configXml.Element("languages").Add(XElement.Parse(lang.ToXmlString()));

                    language.IsImportable = true;
                    language.ObjectId = new HiveId(lang.IsoCode.EncodeAsGuid());
                }

                configXml.Save(configFile);
            }

            return dataImport;
        }

        /// <summary>
        /// Check Entities can be imported.
        /// Updates 'IsImportable' property on <see cref="DeseriazliedDataResult"/>
        /// </summary>
        /// <param name="dataImport"></param>
        private void CheckImportableEntities(SerializedDataImport dataImport)
        {
            using (var uow = _context.Application.Hive.OpenReader<IContentStore>())
            {
                foreach (var attributeType in dataImport.AttributeTypes)
                {
                    var attributeTypeObj = attributeType.DeserializedObject as AttributeType;
                    if(attributeTypeObj == null) continue;

                    if (!uow.Repositories.Schemas.Exists<AttributeType>(attributeTypeObj.Id))
                    {
                        attributeType.IsImportable = true;
                        attributeType.IsUpdatingEntity = false;
                        attributeType.ObjectId = attributeTypeObj.Id;
                    }
                }

                foreach (var entity in dataImport.Entities)
                {
                    var typedEntity = entity.DeserializedObject as TypedEntity;
                    if (typedEntity == null) continue;

                    entity.IsImportable = true;
                    entity.IsUpdatingEntity = uow.Repositories.Exists<TypedEntity>(typedEntity.Id);
                    entity.ObjectId = typedEntity.Id;
                    //Note this might be a dirty hack - removing Ids from source AttributeGroups to avoid conflicts
                    foreach (var attributeGroup in typedEntity.EntitySchema.AttributeGroups)
                    {
                        attributeGroup.Id = HiveId.Empty;
                    }
                }

                foreach (var schema in dataImport.Schemas)
                {
                    var entitySchema = schema.DeserializedObject as EntitySchema;
                    if(entitySchema == null) continue;

                    schema.IsImportable = true;
                    schema.IsUpdatingEntity = uow.Repositories.Schemas.Exists<EntitySchema>(entitySchema.Id);
                    schema.ObjectId = entitySchema.Id;
                    //Note this might be a dirty hack - removing Ids from source AttributeGroups to avoid conflicts
                    foreach (var attributeGroup in entitySchema.AttributeGroups)
                    {
                        attributeGroup.Id = HiveId.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Check relations to verify source and destination exists
        /// </summary>
        /// <param name="dataImport"></param>
        private void CheckImportableRelations(SerializedDataImport dataImport)
        {
            //NOTE Should the checks include both TypedEntities and EntitySchemas for both lists of relations?
            using (var uow = _context.Application.Hive.OpenReader<IContentStore>())
            {
                foreach (var entity in dataImport.EntityRelations)
                {
                    var relation = entity.DeserializedObject as IRelationById;
                    if(relation == null) continue;

                    bool sourceExists = uow.Repositories.Exists<TypedEntity>(relation.SourceId);
                    bool packagedSourceExists = dataImport.Entities.Any(x => x.ObjectId == relation.SourceId);

                    bool destinationExists = uow.Repositories.Exists<TypedEntity>(relation.DestinationId);
                    bool packagedDestinationExists = dataImport.Entities.Any(x => x.ObjectId == relation.DestinationId);

                    if((sourceExists || packagedSourceExists) && (destinationExists || packagedDestinationExists))
                    {
                        entity.IsImportable = true;
                    }
                }

                foreach (var schema in dataImport.SchemaRelations)
                {
                    var relation = schema.DeserializedObject as IRelationById;
                    if (relation == null) continue;

                    bool sourceExists = uow.Repositories.Schemas.Exists<EntitySchema>(relation.SourceId);
                    bool packagedSourceExists = dataImport.Schemas.Any(x => x.ObjectId == relation.SourceId);

                    bool destinationExists = uow.Repositories.Schemas.Exists<EntitySchema>(relation.DestinationId);
                    bool packagedDestinationExists = dataImport.Schemas.Any(x => x.ObjectId == relation.DestinationId);

                    if ((sourceExists || packagedSourceExists) && (destinationExists || packagedDestinationExists))
                    {
                        schema.IsImportable = true;
                    }
                }
            }
        }
    }

    public class CustomFileSystemInfo
    {
        public CustomFileSystemInfo(string sourcePath, string destinationPath, string fileName, bool isDirectory)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            FileName = fileName;
            IsDirectory = isDirectory;
        }

        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string FileName { get; set; }
        public bool IsDirectory { get; set; }
        public bool IsCopiable { get; set; }

        public string Message()
        {
            string message = IsDirectory ? "Directory: " : "File: ";

            if (IsCopiable)
            {
                message += FileName + " was copied";
            }
            else
            {
                message += FileName + " was not copied";
            }
            return message;
        }
    }

    public class SerializedDataImport
    {
        public SerializedDataImport()
        {
            Entities = new List<DeseriazliedDataResult>();
            EntityRelations = new List<DeseriazliedDataResult>();
            Schemas = new List<DeseriazliedDataResult>();
            SchemaRelations = new List<DeseriazliedDataResult>();
            AttributeTypes = new List<DeseriazliedDataResult>();
            Languages = new List<DeseriazliedDataResult>();
        }

        public List<DeseriazliedDataResult> Entities { get; set; }
        public List<DeseriazliedDataResult> EntityRelations { get; set; }

        public List<DeseriazliedDataResult> Schemas { get; set; }
        public List<DeseriazliedDataResult> SchemaRelations { get; set; }

        public List<DeseriazliedDataResult> AttributeTypes { get; set; }

        public List<DeseriazliedDataResult> Languages { get; set; }
    }

    public class DeseriazliedDataResult
    {
        public DeseriazliedDataResult(object deserialziedObject, Type type)
        {
            DeserializedObject = deserialziedObject;
            Type = type;
        }

        public object DeserializedObject { get; private set; }
        public HiveId ObjectId { get; set; }
        public bool IsImportable { get; set; }
        public bool IsUpdatingEntity { get; set; }
        public Type Type { get; private set; }
    }
}