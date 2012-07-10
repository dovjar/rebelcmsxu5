using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Rebel.Cms.Web;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.FluentExtensions;
using Rebel.Cms.Web.Mapping;
using Rebel.Cms.Web.Security;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Persistence.Model.Constants.SerializationTypes;
using Rebel.Framework.Persistence.Model.IO;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Configuration;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Framework.TypeMapping;
using Rebel.Hive.Configuration;
using Rebel.Tests.Extensions;

namespace Rebel.Tests.CoreAndFramework.Hive
{
    using System.Threading;
    using NUnit.Framework;
    using Rebel.Cms.Web.Model;
    using Rebel.Framework;
    using Rebel.Framework.Persistence.Model;
    using Rebel.Framework.Persistence.Model.Attribution.MetaData;
    using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
    using Rebel.Framework.Persistence.Model.Constants.Entities;
    using Rebel.Hive;
    using Rebel.Hive.RepositoryTypes;

    [TestFixture]
    public class HighLevelApiFixture
    {
        protected IHiveManager Hive;
        private NhibernateTestSetupHelper _nhibernateTestSetup;

        [SetUp]
        public void Setup()
        {
            _nhibernateTestSetup = new NhibernateTestSetupHelper();

            var storageProvider = new IoHiveTestSetupHelper(_nhibernateTestSetup.FakeFrameworkContext);


            Hive =
                new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                _nhibernateTestSetup.ReadonlyProviderSetup,
                                _nhibernateTestSetup.ProviderSetup,
                                _nhibernateTestSetup.FakeFrameworkContext),
                                storageProvider.CreateGroup("uploader", "storage://")
                        },
                    _nhibernateTestSetup.FakeFrameworkContext);
        }

        [TearDown]
        public void TearDown()
        {
            ClearCaches();
            _nhibernateTestSetup.Dispose();
            Hive.Dispose();
        }

        public void ClearCaches()
        {
            _nhibernateTestSetup.SessionForTest.Clear();
            Hive.Context.GenerationScopedCache.IfNotNull(x => x.Clear());
        }

        [Test]
        public void CreateNormalSchema()
        {
            var schema = Hive.CreateSchema<EntitySchema, IContentStore>("mySchema");
            Assert.NotNull(schema);
        }

        [Test]
        public void SchemaPartBuilder_CanCreateAttributeTypes_UsingCurrentRegistry()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());
            var innerBuilder = new EntityBuilderStarter<AttributeType, IContentStore>(Hive);
            var type = innerBuilder.UseExistingType("richTextEditor");
            Assert.NotNull(type);
            Assert.That(type.Item, Is.Not.Null);
        }

        [Test]
        public void SchemaPartBuilder_CanCreateAttributeTypes_UsingSpecificRegistry()
        {
            var registry = new CmsAttributeTypeRegistry();
            string key = "richTextEditor";
            var theRealType = registry.GetAttributeType(key);
            var innerBuilder = new EntityBuilderStarter<AttributeType, IContentStore>(Hive);
            var type = innerBuilder.UseExistingType(registry, key);
            Assert.NotNull(type);
            Assert.That(type.Item, Is.Not.Null);
            Assert.That(type.Item.Alias, Is.EqualTo(theRealType.Alias));
            Assert.That(type.Item.Id, Is.EqualTo(theRealType.Id));
        }

        [Test]
        public void SchemaBuilder_CanCreateEntitySchema_WithTypeAliasAndGroupDefinition()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            var schema = Hive
                .NewSchema<EntitySchema, IContentStore>("mySchema")
                .Define("title", "singleLineTextBox", new AttributeGroup("tab1", "tab1", 0))
                .Commit();

            if (schema.Errors.Any())
            {
                Assert.Fail(schema.Errors.FirstOrDefault().ToString());
            }

            Assert.True(schema.Success);
            Assert.NotNull(schema.Item);

            ClearCaches();
            var schemaReloaded = AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, schema.Item.Id);
            Assert.That(schemaReloaded.AttributeDefinitions.Any());
            Assert.That(schemaReloaded.AttributeDefinitions[0].Alias, Is.EqualTo("title"));
        }

        [Test]
        public void SchemaBuilders_CanBatchInOneCommit()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            var parentSchema = Hive
                .UsingStore<IContentStore>()
                .NewSchema("parentSchema")
                .Define("title", "singleLineTextBox", "tab1");

            var anotherParentSchema = Hive
                .UsingStore<IContentStore>()
                .NewSchema("anotherParentSchema")
                .Define("seoTitle", "singleLineTextBox", "tab1");


            var child = Hive
                .UsingStore<IContentStore>()
                .NewSchema("childSchema")
                .InheritFrom(parentSchema)
                .InheritFrom(anotherParentSchema)
                .Define("my-different-property", "singleLineTextBox", "tab1");

            using (var writer = Hive.OpenWriter<IContentStore>())
            {
                writer.Repositories.Schemas.AddOrUpdate(parentSchema.Item);
                writer.Repositories.Schemas.AddOrUpdate(anotherParentSchema.Item);
                writer.Repositories.Schemas.AddOrUpdate(child.Item);
                writer.Complete();
            }

            AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, parentSchema.Item.Id);
            AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, anotherParentSchema.Item.Id);
            AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, child.Item.Id);
        }

        [Test]
        public void SchemaBuilders_CanBatchInOneCommit_Inline()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            HiveId parent, another, child = HiveId.Empty;

            using (var writer = Hive.OpenWriter<IContentStore>())
            {
                var parentSchema = Hive
                    .UsingStore<IContentStore>()
                    .NewSchema("parentSchema")
                    .Define("title", "singleLineTextBox", "tab1")
                    .SaveTo(writer);

                var anotherParentSchema = Hive
                    .UsingStore<IContentStore>()
                    .NewSchema("anotherParentSchema")
                    .Define("seoTitle", "singleLineTextBox", "tab1")
                    .SaveTo(writer);


                var childSchema = Hive
                    .UsingStore<IContentStore>()
                    .NewSchema("childSchema")
                    .InheritFrom(parentSchema)
                    .InheritFrom(anotherParentSchema)
                    .Define("my-different-property", "singleLineTextBox", "tab1")
                    .SaveTo(writer);

                writer.Complete();

                parent = parentSchema.Item.Id;
                another = anotherParentSchema.Item.Id;
                child = childSchema.Item.Id;
            }

            AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, parent);
            AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, another);
            AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, child);
        }

        [Test]
        public void SchemaBuilder_CanSetToInheritFromOthers()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Make a new schema that we'll then inherit from
            var parentSchema = Hive
                .UsingStore<IContentStore>()
                .NewSchema("parentSchema")
                .Define("title", "singleLineTextBox", "tab1")
                .Commit();

            var anotherParentSchema = Hive
                .UsingStore<IContentStore>()
                .NewSchema("anotherParentSchema")
                .Define("seoTitle", "singleLineTextBox", "tab1")
                .Commit();

            Assert.True(parentSchema.Success);
            Assert.True(anotherParentSchema.Success);

            var child = Hive
                .UsingStore<IContentStore>()
                .NewSchema("childSchema")
                .InheritFrom("parentSchema")
                .InheritFrom("anotherParentSchema")
                .Define("my-different-property", "singleLineTextBox", "tab1")
                .Commit();

            if (child.Errors.Any())
            {
                Assert.Fail(child.Errors.FirstOrDefault().ToString());
            }

            Assert.True(child.Success);
            Assert.NotNull(child.Item);

            ClearCaches();
            var schemaReloaded = AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, parentSchema.Item.Id);
            var anotherParentSchemaReloaded = AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, anotherParentSchema.Item.Id);
            var childReloaded = AssertCompositeSchemaExists<IContentStore>(Hive, child.Item.Id);
            Assert.That(childReloaded, Is.TypeOf<CompositeEntitySchema>());
            Assert.True(childReloaded.InheritedAttributeDefinitions.Any());
            Assert.NotNull(childReloaded.InheritedAttributeDefinitions["title"]);
            Assert.NotNull(childReloaded.InheritedAttributeDefinitions["seoTitle"]);
            Assert.Null(childReloaded.InheritedAttributeDefinitions["my-different-property"]);
            Assert.NotNull(childReloaded.AttributeDefinitions["my-different-property"]);
        }

        [Test]
        public void SchemaBuilder_CanCreateEntitySchema_WithTypeAliasAndGroupAlias_GroupIsCreatedWithAlias()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            var schema = Hive
                .NewSchema<EntitySchema, IContentStore>("mySchema")
                .Define("title", "singleLineTextBox", "tab1")
                .Define("bodyText", "richTextEditor", "tab1")
                .Define("somethingOnAnotherTab", "richTextEditor", "another-tab")
                .Commit();

            if (schema.Errors.Any())
            {
                Assert.Fail(schema.Errors.FirstOrDefault().ToString());
            }

            Assert.True(schema.Success);
            Assert.NotNull(schema.Item);

            ClearCaches();
            var schemaReloaded = AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, schema.Item.Id);
            Assert.That(schemaReloaded.AttributeDefinitions.Any());
            Assert.That(schemaReloaded.AttributeDefinitions[0].Alias, Is.EqualTo("title"));
            Assert.That(schemaReloaded.AttributeDefinitions[0].AttributeGroup.Alias, Is.EqualTo("tab1"));
            Assert.That(schemaReloaded.AttributeDefinitions[0].AttributeGroup, Is.SameAs(schemaReloaded.AttributeDefinitions[1].AttributeGroup));
            Assert.That(schemaReloaded.AttributeDefinitions[0].AttributeGroup, Is.Not.SameAs(schemaReloaded.AttributeDefinitions[2].AttributeGroup));
            Assert.That(schemaReloaded.AttributeDefinitions[2].AttributeGroup.Alias, Is.EqualTo("another-tab"));
        }

        [Test]
        public void SchemaBuilder_CanCreateEntitySchema_WithLongTypeAndGroupDefinition()
        {
            var schema = Hive
                .NewSchema<EntitySchema, IContentStore>("mySchema")
                .Define("title", new AttributeType("textbox", "Text box", "who cares", new StringSerializationType()), new AttributeGroup("tab1", "tab1", 0))
                .Commit();

            if (schema.Errors.Any())
            {
                Assert.Fail(schema.Errors.FirstOrDefault().ToString());
            }

            Assert.True(schema.Success);
            Assert.NotNull(schema.Item);

            ClearCaches();
            var schemaReloaded = AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, schema.Item.Id);
            Assert.That(schemaReloaded.AttributeDefinitions.Any());
            Assert.That(schemaReloaded.AttributeDefinitions[0].Alias, Is.EqualTo("title"));
        }

        [Test]
        public void FileStore_AddFile()
        {
            var newFile = new File {Name = "MyNewFile.txt"};
            const string thisIsMyNewFile = "This is my new file";
            newFile.ContentBytes = Encoding.UTF32.GetBytes(thisIsMyNewFile);
            var fileSaveResult = Hive.FileStore().SaveFile(newFile);

            AssertFileSaved(thisIsMyNewFile, fileSaveResult);
        }

        [Test]
        public void FileStore_AddFile_UsingFileCtor()
        {
            const string thisIsMyNewFile = "This is my new file";
            var newFile = new File("MyNewFile.txt", thisIsMyNewFile);
            var fileSaveResult = Hive.FileStore().SaveFile(newFile);

            AssertFileSaved(thisIsMyNewFile, fileSaveResult);
        }

        private void AssertFileSaved(string thisIsMyNewFile, IModelSaveResult<File> fileSaveResult)
        {
            Assert.That(fileSaveResult.Success);
            Assert.That(fileSaveResult.Item, Is.Not.Null);

            ClearCaches();
            using (var unit = Hive.OpenReader<IFileStore>())
            {
                var theFile = unit.Repositories.Get<File>(HiveId.Parse("MyNewFile.txt"));
                Assert.NotNull(theFile);
                var bytes = theFile.ContentBytes;
                var unencoded = Encoding.UTF32.GetString(bytes);
                Assert.That(unencoded, Is.EqualTo(thisIsMyNewFile));
            }
        }

        [Test]
        public void SchemaBuilder_CanCreateEntitySchema_WithAttributeTypeAndGroupSubBuilders()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            var schema = Hive
                .NewSchema<EntitySchema, IContentStore>("mySchema")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("bodyText", type => type.UseExistingType("richTextEditor"), FixedGroupDefinitions.GeneralGroup)
                .Define("extraBodyText", type => type.UseExistingType("richTextEditor"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            if (schema.Errors.Any())
            {
                Assert.Fail(schema.Errors.FirstOrDefault().ToString());
            }

            Assert.True(schema.Success);
            Assert.NotNull(schema.Item);

            ClearCaches();
            var schemaReloaded = AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, schema.Item.Id);
            Assert.That(schemaReloaded.AttributeDefinitions.Any());
            Assert.That(schemaReloaded.AttributeDefinitions.Count(), Is.EqualTo(3));
            Assert.That(schemaReloaded.AttributeDefinitions[0].Alias, Is.EqualTo("title"));
            Assert.NotNull(schemaReloaded.AttributeDefinitions["title"]);
            Assert.NotNull(schemaReloaded.AttributeDefinitions[0].AttributeType);
            Assert.That(schemaReloaded.AttributeDefinitions[0].AttributeType.Alias, Is.EqualTo("singleLineTextBox"));
            Assert.That(schemaReloaded.AttributeDefinitions[1].AttributeType.Alias, Is.EqualTo("richTextEditor"));
            Assert.That(schemaReloaded.AttributeDefinitions[2].AttributeType.Alias, Is.EqualTo("richTextEditor"));
        }

        public static T AssertSchemaPartExists<T, TProviderFilter>(IHiveManager hiveManager, HiveId id)
            where TProviderFilter : class, IProviderTypeFilter
            where T : AbstractSchemaPart
        {
            using (var uow = hiveManager.OpenReader<TProviderFilter>())
            {
                var item = uow.Repositories.Schemas.Get<T>(id);
                Assert.NotNull(item);
                return item;
            }
        }

        public static CompositeEntitySchema AssertCompositeSchemaExists<TProviderFilter>(IHiveManager hiveManager, HiveId id)
            where TProviderFilter : class, IProviderTypeFilter
        {
            using (var uow = hiveManager.OpenReader<TProviderFilter>())
            {
                var item = uow.Repositories.Schemas.GetComposite<EntitySchema>(id);
                Assert.NotNull(item);
                return item;
            }
        }
    }

    [TestFixture]
    public class HighLevelApiCmsExtensionsFixture
    {
        protected IHiveManager Hive;
        protected IMembershipService<Member> MembershipService;
        protected IPublicAccessService PublicAccessService;
        private NhibernateTestSetupHelper _nhibernateTestSetup;

        [SetUp]
        public void Setup()
        {
            _nhibernateTestSetup = new NhibernateTestSetupHelper();

            var storageProvider = new IoHiveTestSetupHelper(_nhibernateTestSetup.FakeFrameworkContext);

            Hive = new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                _nhibernateTestSetup.ReadonlyProviderSetup,
                                _nhibernateTestSetup.ProviderSetup,
                                _nhibernateTestSetup.FakeFrameworkContext),
                            storageProvider.CreateGroup("uploader", "storage://file-uploader"),
                        },
                    _nhibernateTestSetup.FakeFrameworkContext);

            var appContext = new FakeRebelApplicationContext(Hive, false);

            var resolverContext = new MockedMapResolverContext(_nhibernateTestSetup.FakeFrameworkContext, Hive, new MockedPropertyEditorFactory(appContext), new MockedParameterEditorFactory());
            var webmModelMapper = new CmsModelMapper(resolverContext);
            var renderModelMapper = new RenderTypesModelMapper(resolverContext);

            _nhibernateTestSetup.FakeFrameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { webmModelMapper, renderModelMapper, new FrameworkModelMapper(_nhibernateTestSetup.FakeFrameworkContext) }));

            var membersMembershipProvider = new MembersMembershipProvider {AppContext = appContext};
            membersMembershipProvider.Initialize("MembersMembershipProvider", new NameValueCollection());
            MembershipService = new MembershipService<Member, MemberProfile>(appContext.FrameworkContext, Hive, 
                "security://member-profiles", "security://member-groups", Framework.Security.Model.FixedHiveIds.MemberProfileVirtualRoot,
                membersMembershipProvider, Enumerable.Empty<MembershipProviderElement>());

            PublicAccessService = new PublicAccessService(Hive, MembershipService, appContext.FrameworkContext);
        }

        [TearDown]
        public void TearDown()
        {
            ClearCaches();
            _nhibernateTestSetup.Dispose();
            Hive.Dispose();
        }

        public void ClearCaches()
        {
            _nhibernateTestSetup.SessionForTest.Clear();
            Hive.Context.GenerationScopedCache.IfNotNull(x => x.Clear());
        }

        [Test]
        public void FileStore_AddFile_UsingFileCtor()
        {
            const string thisIsMyNewFile = "This is my new file";
            var newFile = new File("MyNewFile.txt", thisIsMyNewFile);
            var fileSaveResult = Hive.Cms().UploadFileStore().SaveFile(newFile);

            AssertFileSaved(thisIsMyNewFile, fileSaveResult);
        }

        [Test]
        public void FileStore_AddFile_UsingSaveOverloadWithNameAndTextContents()
        {
            const string thisIsMyNewFile = "This is my new file";
            var fileSaveResult = Hive.Cms().UploadFileStore().SaveFile("MyNewFile.txt", thisIsMyNewFile);

            AssertFileSaved(thisIsMyNewFile, fileSaveResult);
        }

        [Test]
        public void FileStore_GetFile()
        {
            const string thisIsMyNewFile = "This is my new file";
            var fileSaveResult = Hive.Cms().UploadFileStore().SaveFile("MyNewFile.txt", thisIsMyNewFile);
            var file2SaveResult = Hive.Cms().UploadFileStore().SaveFile("MyNewFile2.txt", thisIsMyNewFile);

            var getFile = Hive.Cms().UploadFileStore().GetFile("MyNewFile.txt");
            var getFile2 = Hive.Cms().UploadFileStore().GetFile("MyNewFile2.txt");
            var getFile3 = Hive.Cms().UploadFileStore().GetFile("NonExistent.txt");

            Assert.NotNull(getFile);
            Assert.NotNull(getFile2);
            Assert.Null(getFile3);
        }

        [Test]
        public void FileStore_GetFilesForContainer()
        {
            const string thisIsMyNewFile = "This is my new file";
            var folderSaveResult = Hive.Cms().UploadFileStore().CreateDirectory("MyFolder");
            var folderSaveResult2 = Hive.Cms().UploadFileStore().CreateDirectory(@"MyFolder\MySubFolder");

            // Use a dll extension because the test IO helper sets the SupportedExtensions to that only
            var file1SaveResult = Hive.Cms().UploadFileStore().SaveFile(@"MyFolder\MyNewFile.dll", thisIsMyNewFile);
            var file2SaveResult = Hive.Cms().UploadFileStore().SaveFile(@"MyFolder\MyNewFile2.dll", thisIsMyNewFile);
            var file3SaveResult = Hive.Cms().UploadFileStore().SaveFile(@"MyFolder\MySubFolder\MyNewFile3.dll", thisIsMyNewFile);
            var file4SaveResult = Hive.Cms().UploadFileStore().SaveFile(@"MyFolder\MySubFolder\MyNewFile4.dll", thisIsMyNewFile);

            Assert.True(folderSaveResult.Success);
            Assert.True(folderSaveResult2.Success);
            Assert.True(file1SaveResult.Success);
            Assert.True(file2SaveResult.Success);
            Assert.True(file3SaveResult.Success);
            Assert.True(file4SaveResult.Success);

            var getFile = Hive.Cms().UploadFileStore().GetFiles("MyFolder");
            
            Assert.NotNull(getFile);
            Assert.That(getFile.Count(), Is.EqualTo(2));

            // Now get the files again telling it to recurse into subdirectories
            var getFilesRecurse = Hive.Cms().UploadFileStore().GetFiles("MyFolder", true);

            Assert.NotNull(getFile);
            Assert.That(getFilesRecurse.Count(), Is.EqualTo(4));
        }

        private void AssertFileSaved(string thisIsMyNewFile, IModelSaveResult<File> fileSaveResult)
        {
            Assert.That(fileSaveResult.Success);
            Assert.That(fileSaveResult.Item, Is.Not.Null);

            ClearCaches();
            using (var unit = Hive.OpenReader<ICmsUploadFileStore>())
            {
                var theFile = unit.Repositories.Get<File>(HiveId.Parse("MyNewFile.txt"));
                Assert.NotNull(theFile);
                var bytes = theFile.ContentBytes;
                var unencoded = Encoding.UTF32.GetString(bytes);
                Assert.That(unencoded, Is.EqualTo(thisIsMyNewFile));
            }
        }

        #region Create content
        [Test]
        public void CreateNewContent_WithoutSettingValues_UseDynamicModelToGetValue()
        {
            SetupContentTest();

            // Make a new content type with two properties
            var doctype = Hive.Cms<IContentStore>()
                .NewContentType("newsItem")
                .Define("title", "singleLineTextBox", "tab1")
                .Define("somethingEndingInS", "integer", "tab1")
                .Commit();

            // Make a new revision without specifying properties
            var firstRev = Hive.Cms().NewRevision("article", "article", "newsItem")
                .SetValue("title", "This is my title")
                .Publish()
                .Commit();

            var bent = new Content(firstRev.Item).Bend(Hive, MembershipService, PublicAccessService);

            Assert.NotNull(bent);

            var checkTitle = bent.Title;

            Assert.That(checkTitle, Is.EqualTo("This is my title"));

            // When a property exists on the doctype, ends in S, but is not set, need to make sure the doctype pluralisation doesn't kick in
            // First assert that pluralisation of a genuine doctype alias is still kicking in
            Assert.That(bent.NewsItems, Is.TypeOf<DynamicContentList>());
            // Now assert that a property that ends in s, but was not set, does not incurr doctype pluralization
            Assert.NotNull(bent.SomethingEndingInS);
            Assert.That(bent.SomethingEndingInS, Is.Not.TypeOf<DynamicContentList>());
            Assert.That(bent.SomethingEndingInS, Is.EqualTo(0));
        }

        [Test]
        public void CreateNewContent_NewRevision_ClearsChildCaches()
        {
            SetupContentTest();

            // Make a new content type with two properties
            var doctype = Hive.Cms<IContentStore>()
                .NewContentType("newsItem")
                .Define("title", "singleLineTextBox", "tab1")
                .Define("somethingEndingInS", "integer", "tab1")
                .Commit();

            // Make a new revision without specifying properties
            IContentRevisionSaveResult<TypedEntity> firstRev = Hive.Cms().NewRevision("article", "article", "newsItem")
                .SetValue("title", "This is my title")
                .Publish()
                .Commit();

            // Make a few children for later querying
            IContentRevisionSaveResult<TypedEntity> lastResult = null;
            for (int i = 0; i < 15; i++)
            {
                var buildit = Hive.Cms().NewRevision("child" + i, "child" + i, "newsItem")
                    .SetValue("title", "Title " + i)
                    .SetParent(firstRev.Item)
                    .Publish();

                // Set the id for easier debugging
                buildit.Item.Id = new HiveId(i.ToString().EncodeAsGuid());

                lastResult = buildit.Commit();
            }

            var allChildRelations = firstRev.Item.RelationProxies.AllChildRelations();

            Assert.That(allChildRelations.Any(), Is.True);

            var bent = new Content(firstRev.Item).Bend(Hive, MembershipService, PublicAccessService);

            Assert.NotNull(bent);

            IEnumerable<dynamic> children = bent.Children.Where("NodeTypeAlias == @0", "newsItem").ToList();

            // Check that the last child created is returned from the query
            dynamic foundItem = null;
            foreach (var child in children)
            {
                if (child.Id == lastResult.Item.Id)
                {
                    foundItem = child;
                    break;
                }
            }
            Assert.That(foundItem, Is.Not.Null);
            Assert.That(foundItem.Title, Is.EqualTo("Title 14"));

            // Now update the title, save it, run the same query, and ensure the updated one is returned
            var resaveResult = Hive.Cms().NewRevisionOf(lastResult.Item)
                .SetValue("title", "Resaved title")
                .Publish()
                .Commit();

            // First check that querying for that item by id (a new type of query to cache) returns the updated title
            dynamic directGetResult = bent.Children.Where("Id == @0", lastResult.Item.Id).FirstOrDefault();
            Assert.That(directGetResult, Is.Not.Null);
            Assert.That(directGetResult.Title, Is.EqualTo("Resaved title"));

            // Now check the same query as might previously have been cached
            IEnumerable<dynamic> reloadChildren = bent.Children.Where("NodeTypeAlias == @0", "newsItem").ToList();

            // Check that the last child created is returned from the query
            foundItem = null;
            foreach (var child in reloadChildren)
            {
                if (child.Id == lastResult.Item.Id)
                {
                    foundItem = child;
                    break;
                }
            }
            Assert.That(foundItem, Is.Not.Null);
            Assert.That(foundItem.Title, Is.EqualTo("Resaved title"));
        }

        [Test]
        public void CreateNewContent_SecondRevision_WithoutSettingValues_UsesPreviousValues()
        {
            SetupContentTest();

            // Make a new content type with two properties
            var doctype = Hive.Cms<IContentStore>()
                .NewContentType("newsItem")
                .Define("title", "singleLineTextBox", "tab1")
                .Define("title2", "singleLineTextBox", "tab1")
                .Commit();

            // Make a new revision specifying both properties
            var firstRev = Hive.Cms().NewRevision("article", "article", "newsItem")
                .SetValue("title", "This is my title")
                .SetValue("title2", "This is my 2nd title")
                .Publish()
                .Commit();

            Assert.True(firstRev.Success);

            var secondRev = Hive.Cms().NewRevisionOf(firstRev.Item)
                .SetValue("title", "This is my modified title")
                .Commit(); // Don't set title2

            Assert.True(secondRev.Success);

            // Now get the latest revision and check if title2 has a value
            using (var unit = Hive.OpenReader<IContentStore>())
            {
                var rev = unit.Repositories.Revisions.GetLatestRevision<TypedEntity>(secondRev.Item.Id);
                Assert.That(rev.Item.Attribute<string>("title"), Is.EqualTo("This is my modified title"));
                Assert.That(rev.Item.Attribute<string>("title2"), Is.EqualTo("This is my 2nd title"));
            }
        }

        [Test]
        public void CreateNewContent_BatchedInOneCommit_Inline_WithInheritedTypes()
        {
            SetupContentTest();

            IContentRevisionBuilderStep<TypedEntity, IContentStore> someContent;
            using (var writer = Hive.OpenWriter<IContentStore>())
            {
                var parent = Hive
                    .Cms()
                    .NewContentType("newsItem")
                    .Define("title", "singleLineTextBox", "tab1")
                    .SaveTo(writer);

                var child = Hive.Cms()
                    .NewContentType("innerItem")
                    .InheritFrom(parent)
                    .Define("another_title", "integer", "tab1")
                    .SaveTo(writer);

                someContent = Hive.Cms<IContentStore>()
                    .NewRevision("something", "somethingUrl", child)
                    .SetValue("another_title", 5)
                    .SetValue("title", "Hello")
                    .Publish()
                    .SaveTo(writer);

                writer.Complete();
            }

            ClearCaches();

            AssertContentWithTitle(someContent.Item, "something", "somethingUrl", "Hello");
        }

        [Test]
        public void CreateNewContent_SetParent()
        {
            SetupContentTest();

            // Make a new content type, don't save yet so we can do it in one transaction
            var doctype = Hive.Cms<IContentStore>()
                .NewContentType("newsItem")
                .Define("title", "singleLineTextBox", "tab1")
                .Commit();

            var parent = Hive.Cms<IContentStore>()
                .NewRevision("Home Page", "homePage", "newsItem")
                .SetValue("title", "this is my title")
                .Commit();

            var child = Hive.Cms<IContentStore>()
                .NewRevision("Child Page", "childPage", "newsItem", false)
                .SetParent(parent.Content.Id)
                .SetValue("title", "this is my child page")
                .Publish()
                .Commit();

            if (child.Errors.Any())
            {
                Assert.Fail(child.Errors.FirstOrDefault().ToString());
            }

            Assert.True(child.Success);
            Assert.NotNull(child.Item);

            ClearCaches();

            AssertContentWithOneParent(child, "Child Page", "childPage", "this is my child page");
        }

        private void AssertContentWithOneParent(IContentRevisionSaveResult<TypedEntity> child, string itemName, string itemUrlName, string itemTitle)
        {
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var item = uow.Repositories.Get<Content>(child.Item.Id);
                Assert.NotNull(item);
                var parentRelations = item.RelationProxies.AllParentRelations(FixedRelationTypes.DefaultRelationType);
                // Ensure there is only one parent, since we only added one and we left setRootAsParent as false in the previous creation call
                Assert.That(parentRelations.Count(), Is.EqualTo(1));
                var getParent = uow.Repositories.Get<TypedEntity>(parentRelations.First().Item.SourceId);
                Assert.NotNull(getParent);
                Assert.That(item.Name, Is.EqualTo(itemName));
                Assert.That(item.UrlName, Is.EqualTo(itemUrlName));
                Assert.That(item.Field("title"), Is.EqualTo(itemTitle));
            }
        }

        private void AssertContentWithTitle(IContentRevisionSaveResult<TypedEntity> child, string itemName, string itemUrlName, string itemTitle)
        {
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var item = uow.Repositories.Get<Content>(child.Item.Id);
                Assert.NotNull(item);
                Assert.That(item.Name, Is.EqualTo(itemName));
                Assert.That(item.UrlName, Is.EqualTo(itemUrlName));
                Assert.That(item.Field("title"), Is.EqualTo(itemTitle));
            }
        }

        private void AssertContentWithTitle(TypedEntity child, string itemName, string itemUrlName, string itemTitle)
        {
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var item = uow.Repositories.Get<Content>(child.Id);
                Assert.NotNull(item);
                Assert.That(item.Name, Is.EqualTo(itemName));
                Assert.That(item.UrlName, Is.EqualTo(itemUrlName));
                Assert.That(item.Field("title"), Is.EqualTo(itemTitle));
            }
        }

        [Test]
        public void CreateNewContent_SetParent_WithRootDefaultSet_OverridesRootDefault()
        {
            SetupContentTest();

            // Make a new content type, don't save yet so we can do it in one transaction
            var doctype = Hive.Cms<IContentStore>()
                .NewContentType("newsItem")
                .Define("title", "singleLineTextBox", "tab1")
                .Commit();

            var parent = Hive.Cms<IContentStore>()
                .NewRevision("Home Page", "homePage", "newsItem")
                .SetValue("title", "this is my title")
                .Commit();

            var child = Hive.Cms<IContentStore>()
                .NewRevision("Child Page", "childPage", "newsItem") // Here the param "setRootAsParent" defaults to true
                .SetParent(parent.Content.Id) // This should override the fact that "setRootAsParent" defaulted to true
                .SetValue("title", "this is my child page")
                .Publish()
                .Commit();

            if (child.Errors.Any())
            {
                Assert.Fail(child.Errors.FirstOrDefault().ToString());
            }

            Assert.True(child.Success);
            Assert.NotNull(child.Item);

            ClearCaches();

            AssertContentWithOneParent(child, "Child Page", "childPage", "this is my child page");
        }

        [Test]
        public void CreateNewContent_WithNewContentType_ContentAndTypeAreCreatedInOneCall()
        {
            SetupContentTest();

            // Make a new content type, don't save yet so we can do it in one transaction
            var doctype = Hive.Cms<IContentStore>()
                .NewContentType("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup);

            var content = Hive.Cms<IContentStore>()
                .NewRevision("Home Page", "homePage", doctype)
                .SetValue("title", "this is my title")
                .Commit();

            if (content.Errors.Any())
            {
                Assert.Fail(content.Errors.FirstOrDefault().ToString());
            }

            Assert.True(content.Success);
            Assert.NotNull(content.Item);

            ClearCaches();
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var item = uow.Repositories.Get<Content>(content.Item.Id);
                Assert.NotNull(item);
                var parentRelations = item.RelationProxies.AllParentRelations(FixedRelationTypes.DefaultRelationType);
                var getParent = uow.Repositories.Get<TypedEntity>(parentRelations.First().Item.SourceId);
                Assert.NotNull(getParent);
                Assert.That(item.Name, Is.EqualTo("Home Page"));
                Assert.That(item.UrlName, Is.EqualTo("homePage"));
                Assert.That(item.Field("title"), Is.EqualTo("this is my title"));
            }
            AssertContentTypeValid(content.Content.EntitySchema);
        }

        [Test]
        public void CreateNewContent_WhenContentTypeHasDefaultTemplate_TemplateIsSelectedAutomatically()
        {
            SetupContentTest();

            // Make a new content type, don't save yet so we can do it in one transaction
            var doctype = Hive.Cms<IContentStore>()
                .NewContentType("newsItem")
                .AddPermittedTemplate("Homepage.cshtml")
                .SetDefaultTemplate("Homepage.cshtml")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup);

            var content = Hive.Cms<IContentStore>()
                .NewRevision("Home Page", "homePage", doctype)
                .SetValue("title", "this is my title")
                .Commit();

            if (content.Errors.Any())
            {
                Assert.Fail(content.Errors.FirstOrDefault().ToString());
            }

            Assert.True(content.Success);
            Assert.NotNull(content.Item);

            ClearCaches();
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var item = uow.Repositories.Get<Content>(content.Item.Id);
                Assert.NotNull(item);

                Assert.That(item.Attribute<HiveId>(SelectedTemplateAttributeDefinition.AliasValue), Is.EqualTo(HiveId.Parse("Homepage.cshtml")));
            }
        }

        private void SetupContentTest()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent schema and content root exists for this test
            var contentVirtualRoot = FixedEntities.ContentVirtualRoot;
            var systemRoot = new SystemRoot();
            var contentRootSchema = new ContentRootSchema();
            Hive.AutoCommitTo<IContentStore>(
                x =>
                    {
                        x.Repositories.AddOrUpdate(systemRoot);
                        x.Repositories.AddOrUpdate(contentVirtualRoot);
                        x.Repositories.Schemas.AddOrUpdate(contentRootSchema);
                    });
        }

        [Test]
        public void CreateNewContent_WithExistingContentType_Publish()
        {
            SetupContentTest();

            // Make a new document type
            Hive.Cms().NewContentType("newsArticle")
                .Define("title", "singleLineTextBox", "tab1")
                .Define("bodyText", "richTextEditor", "tab2")
                .Commit();

            // Make a new piece of content, not published
            var contentResult = Hive.Cms().NewRevision("An article", "an-article", "newsArticle")
                .SetValue("title", "This is an article")
                .SetValue("bodyText", "<p>This is an article!</p>")
                .Commit();

            // Assert that "get latest published revision" returns null
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var latest = uow.Repositories.Revisions.GetLatestRevision<Content>(contentResult.Item.Id, FixedStatusTypes.Published);
                Assert.Null(latest);
            }

            // Assert that querying for a published revision returns null
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var latest = uow.Repositories.OfRevisionType(FixedStatusTypes.Published).InIds(contentResult.Item.Id).FirstOrDefault();
                Assert.Null(latest);
            }

            Thread.Sleep(200);

            // Make some changes and publish the item (Name, Selected Template, Title)
            var saveResult = Hive.Cms().NewRevisionOf(contentResult.Item)
                .SetValue("title", "This is a modified article")
                .SetValue("bodyText", "<p>This is a modified article!</p>")
                .SetName("Modified Name")
                .SetUrlName("modified-name")
                .SetSelectedTemplate("homepage.cshtml")
                .Publish()
                .Commit();

            if (saveResult.Errors.Any())
            {
                Assert.Fail(saveResult.Errors.FirstOrDefault().ToString());
            }

            Assert.True(saveResult.Success);
            Assert.NotNull(saveResult.Item);

            // Assert that "get latest published revision" returns the changes
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var checkExists = uow.Repositories.Get(contentResult.Item.Id);
                Assert.NotNull(checkExists);

                var latest = uow.Repositories.Revisions.GetLatestRevision<TypedEntity>(contentResult.Item.Id, FixedStatusTypes.Published);
                Assert.NotNull(latest);
                Assert.That(latest.Item.Attribute<string>("title"), Is.EqualTo("This is a modified article"));
                Assert.That(latest.Item.Attribute<string>("bodyText"), Is.EqualTo("<p>This is a modified article!</p>"));
                Assert.That(latest.Item.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, valueKey: "Name"), Is.EqualTo("Modified Name"));
                Assert.That(latest.Item.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, valueKey: "UrlName"), Is.EqualTo("modified-name"));
                Assert.That(latest.Item.Attribute<HiveId>(SelectedTemplateAttributeDefinition.AliasValue), Is.EqualTo(HiveId.Parse("homepage.cshtml")));
            }

            // Assert that querying for a published revision returns the same changes
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var latest = uow.Repositories.OfRevisionType(FixedStatusTypes.Published).InIds(contentResult.Item.Id).FirstOrDefault();
                Assert.NotNull(latest);
                Assert.That(latest.Attribute<string>("title"), Is.EqualTo("This is a modified article"));
                Assert.That(latest.Attribute<string>("bodyText"), Is.EqualTo("<p>This is a modified article!</p>"));
                Assert.That(latest.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, valueKey: "Name"), Is.EqualTo("Modified Name"));
                Assert.That(latest.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, valueKey: "UrlName"), Is.EqualTo("modified-name"));
                Assert.That(latest.Attribute<HiveId>(SelectedTemplateAttributeDefinition.AliasValue), Is.EqualTo(HiveId.Parse("homepage.cshtml")));
            }
        }

        #endregion

        #region Content Revisions
        [Test]
        public void GetExistingContentRevision_ThenPublish()
        {
            
        }
        #endregion

        #region Get content types
        [Test]
        public void GetContentTypeByAlias_IfExists_IsReturned()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Create a content type for this test
            Hive
                .Cms<IContentStore>()
                .NewContentType<EntitySchema, IContentStore>("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            ClearCaches();

            // Get the content type
            var contentType = Hive.Cms<IContentStore>().GetContentTypeByAlias("newsItem");
            Assert.NotNull(contentType);
            Assert.That(contentType, Is.TypeOf<CompositeEntitySchema>());
            Assert.That(contentType.Alias, Is.EqualTo("newsItem"));
            Assert.True(contentType.AttributeDefinitions.Any(x => x.Alias == "title"));
        }

        [Test]
        public void GetContentTypeById_IfExists_IsReturned()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            // Create a content type for this test
            var contentType = Hive
                .Cms<IContentStore>()
                .NewContentType<EntitySchema, IContentStore>("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            ClearCaches();

            // Get the content type
            var reloadContentType = Hive.Cms<IContentStore>().GetContentTypeById(contentType.Item.Id);
            Assert.NotNull(contentType);
            Assert.That(reloadContentType.Alias, Is.EqualTo("newsItem"));
            Assert.True(reloadContentType.AttributeDefinitions.Any(x => x.Alias == "title"));
        }

        [Test]
        public void GetContentTypeByAlias_IfDoesNotExist_ReturnsNull()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            // Create a content type for this test but we won't query for it, it's just to ensure it's not returned later
            Hive
                .Cms<IContentStore>()
                .NewContentType<EntitySchema, IContentStore>("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            ClearCaches();

            // Get the content type
            var contentType = Hive.Cms<IContentStore>().GetContentTypeByAlias("nothing exists with this alias");
            Assert.Null(contentType);
        }

        [Test]
        public void GetContentTypeById_IfDoesNotExist_ReturnsNull()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            // Create a content type for this test but we won't query for it, it's just to ensure it's not returned later
            Hive
                .Cms<IContentStore>()
                .NewContentType<EntitySchema, IContentStore>("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            ClearCaches();

            // Get the content type
            var contentType = Hive.Cms<IContentStore>().GetContentTypeById(new HiveId(Guid.NewGuid()));
            Assert.Null(contentType);
        }
        #endregion

        #region Create content types

        [Test]
        public void CreateContentType_UsingGenericCmsBuildStarter()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var doctype = Hive
                .Cms<IContentStore>()
                .NewContentType<EntitySchema, IContentStore>("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            AssertContentTypeCreated(doctype);
        }

        [Test]
        public void CreateContentType_WithAvailableTemplate()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var doctype = Hive.
                Cms<IContentStore>()
                .NewContentType("newsItem")
                .AddPermittedTemplate("Homepage.cshtml")
                .Commit();

            AssertContentTypeCreated(doctype);

            var allowedTemplates = doctype.Item.GetXmlPropertyAsList<HiveId>("allowed-templates");
            Assert.That(allowedTemplates, Contains.Item(HiveId.Parse("Homepage.cshtml")));
        }

        [Test]
        public void CreateContentType_WithAvailableTemplates_AllAreAdded()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var doctype = Hive.
                Cms<IContentStore>()
                .NewContentType("newsItem")
                .AddPermittedTemplate("Homepage.cshtml")
                .AddPermittedTemplate("Another.cshtml")
                .Commit();

            AssertContentTypeCreated(doctype);

            var allowedTemplates = doctype.Item.GetXmlPropertyAsList<HiveId>("allowed-templates");
            Assert.That(allowedTemplates, Contains.Item(HiveId.Parse("Homepage.cshtml")));
            Assert.That(allowedTemplates, Contains.Item(HiveId.Parse("Another.cshtml")));
        }

        [Test]
        public void CreateContentType_ClearAvailableTemplates()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var doctype = Hive.
                Cms<IContentStore>()
                .NewContentType("newsItem")
                .AddPermittedTemplate("Homepage.cshtml")
                .AddPermittedTemplate("Another.cshtml")
                .ClearPermittedTemplates()
                .Commit();

            AssertContentTypeCreated(doctype);

            var allowedTemplates = doctype.Item.GetXmlPropertyAsList<HiveId>("allowed-templates");
            Assert.That(allowedTemplates.Count(), Is.EqualTo(0));
        }

        [Test]
        public void CreateContentType_SetDefaultTemplate()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var doctype = Hive.
                Cms<IContentStore>()
                .NewContentType("newsItem")
                .AddPermittedTemplate("Homepage.cshtml")
                .AddPermittedTemplate("Another.cshtml")
                .SetDefaultTemplate("Homepage.cshtml")
                .ClearPermittedTemplates()
                .Commit();

            AssertContentTypeCreated(doctype);

            var defaultTemplate = doctype.Item.GetXmlConfigProperty<HiveId>("default-template");
            Assert.That(defaultTemplate, Is.EqualTo(HiveId.Parse("Homepage.cshtml")));
        }

        [Test]
        public void CreateContentType_WhenSettingDefaultTemplate_IfTemplateNotAdded_Fails()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var doctype = Hive.
                        Cms<IContentStore>()
                        .NewContentType("newsItem")
                        .AddPermittedTemplate("Homepage.cshtml")
                        .AddPermittedTemplate("Another.cshtml")
                        .SetDefaultTemplate("NotAdded.cshtml")
                        .ClearPermittedTemplates()
                        .Commit();
                });
        }

        [Test]
        public void CreateContentType_UsingGenericCmsBuildStarter_AndChainedStoreType()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var doctype = Hive
                .Cms<IContentStore>()
                .NewContentType("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            AssertContentTypeCreated(doctype);
        }

        private void AssertContentTypeCreated(ISchemaSaveResult<EntitySchema> doctype)
        {
            if (doctype.Errors.Any())
            {
                Assert.Fail(doctype.Errors.FirstOrDefault().ToString());
            }

            Assert.True(doctype.Success);
            AssertContentTypeValid(doctype.Item);
        }

        private void AssertContentTypeValid(EntitySchema entitySchema)
        {
            Assert.NotNull(entitySchema);

            ClearCaches();
            var schemaReloaded = HighLevelApiFixture.AssertSchemaPartExists<EntitySchema, IContentStore>(Hive, entitySchema.Id);

            // Check that the schema was created with the minimum properties for content, taken from SchemaEditorModelToEntitySchema<TEditorModel>
            //to.TryAddAttributeDefinition(new NodeNameAttributeDefinition(existingOrNewGeneralGroup));
            //to.TryAddAttributeDefinition(new SelectedTemplateAttributeDefinition(existingOrNewGeneralGroup));
            Assert.That(schemaReloaded.AttributeDefinitions.Any(x => x.Alias == NodeNameAttributeDefinition.AliasValue));
            Assert.That(schemaReloaded.AttributeDefinitions.Any(x => x.Alias == SelectedTemplateAttributeDefinition.AliasValue));
        }

        [Test]
        public void CreateContentType_UsingGenericStoreBuildStarter_AndChainedStoreType()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var doctype = Hive
                .UsingStore<IContentStore>()
                .Cms()
                .NewContentType("newsItem")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            AssertContentTypeCreated(doctype);
        }

        [Test]
        public void CreateContentType_AndInheritingContentType_WithContent()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            var parent = Hive
                .Cms()
                .NewContentType("newsItem")
                .Define("title", "singleLineTextBox", "tab1")
                .Commit();

            ClearCaches();

            AssertContentTypeCreated(parent);

            var child = Hive.Cms()
                .NewContentType("innerItem")
                .InheritFrom("newsItem")
                .Define("another_title", "integer", "tab1")
                .Commit();

            ClearCaches();

            AssertContentTypeCreated(child);

            var someContent = Hive.Cms()
                .NewRevision("something", "somethingUrl", "innerItem")
                .SetValue("another_title", 5)
                .SetValue("title", "Hello")
                .Publish()
                .Commit();

            ClearCaches();

            AssertContentWithTitle(someContent, "something", "somethingUrl", "Hello");
        }

        #endregion
    }
}
