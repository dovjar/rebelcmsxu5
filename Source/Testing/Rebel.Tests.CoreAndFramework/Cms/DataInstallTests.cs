using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rebel.Cms.Packages.DevDataset;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Context;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Mapping;
using Rebel.Cms.Web.Security;
using Rebel.Cms.Web.Security.Permissions;
using Rebel.Cms.Web.System;
using Rebel.Cms.Web.Tasks;
using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.NHibernate;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Configuration;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Framework.Security.Model.Schemas;
using Rebel.Framework.TypeMapping;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.RepositoryTypes;
using Rebel.Framework.Testing.PartialTrust;

using Rebel.Tests.Extensions;
using Rebel.Tests.Extensions.Stubs;

namespace Rebel.Tests.CoreAndFramework.Cms
{
    using System.Web.Security;

    [TestFixture]
    public class DataInstallTests //: AbstractPartialTrustFixture<DataInstallTests>
    {
        [TestFixtureSetUp]
        public void SetupLog4net()
        {
            TestHelper.SetupLog4NetForTests();
        }

        [TestFixtureTearDown]
        public void ClearFrameworkScope()
        {
            if (FrameworkContext != null)
                FrameworkContext.ScopedCache.ScopeComplete();
        }

        public IFrameworkContext FrameworkContext { get; set; }

        //[Test]
        //public void DataInstallTest_CoreData_Examine()
        //{
        //    //Arrange

        //    var examineTestSetup = new ExamineTestSetupHelper();

        //    var storageProvider = new IoHiveTestSetupHelper(examineTestSetup.FrameworkContext);

        //    FrameworkContext = storageProvider.FrameworkContext;

        //    var hiveManager =
        //        new HiveManager(
        //            new[]
        //                {
        //                    new ProviderMappingGroup(
        //                        "test",
        //                        new WildcardUriMatch("content://"),
        //                        examineTestSetup.ReadonlyProviderSetup,
        //                        examineTestSetup.ProviderSetup,
        //                        examineTestSetup.FrameworkContext),
        //                    storageProvider.CreateGroup("uploader", "storage://file-uploader"),

        //                    new ProviderMappingGroup(
        //                        "security",
        //                        new WildcardUriMatch("security://"),
        //                        examineTestSetup.ReadonlyProviderSetup,
        //                        examineTestSetup.ProviderSetup,
        //                        examineTestSetup.FrameworkContext)

        //                },
        //            examineTestSetup.FrameworkContext);

        //    RunTest(hiveManager, examineTestSetup.FrameworkContext, () =>
        //    {
        //        hiveManager.Context.GenerationScopedCache.Clear();
        //    });

        //    hiveManager.Dispose();
        //}

        [Test]
        public void DataInstallTest_CoreData_NHibernate()
        {
            //Arrange

            var nhibernateTestSetup = new NhibernateTestSetupHelper();

            var storageProvider = new IoHiveTestSetupHelper(nhibernateTestSetup.FakeFrameworkContext);

            FrameworkContext = nhibernateTestSetup.FakeFrameworkContext;

            var hiveManager =
                new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                nhibernateTestSetup.ReadonlyProviderSetup,
                                nhibernateTestSetup.ProviderSetup,
                                nhibernateTestSetup.FakeFrameworkContext),
                            storageProvider.CreateGroup("uploader", "storage://file-uploader"),

                            new ProviderMappingGroup(
                                "security",
                                new WildcardUriMatch("security://"),
                                nhibernateTestSetup.ReadonlyProviderSetup,
                                nhibernateTestSetup.ProviderSetup,
                                nhibernateTestSetup.FakeFrameworkContext)
                        },
                    nhibernateTestSetup.FakeFrameworkContext);

            RunTest(hiveManager, nhibernateTestSetup.FakeFrameworkContext, () =>
                {
                    nhibernateTestSetup.SessionForTest.Clear();
                    hiveManager.Context.GenerationScopedCache.Clear();
                });

            hiveManager.Dispose();
        }

        private void RunTest(
            HiveManager hiveManager,
            FakeFrameworkContext frameworkContext,
            Action installCallback = null)
        {
            var attributeTypeRegistry = new CmsAttributeTypeRegistry();
            AttributeTypeRegistry.SetCurrent(attributeTypeRegistry);
            var appContext = new FakeRebelApplicationContext(hiveManager, false);
            var mockedPropertyEditorFactory = new MockedPropertyEditorFactory(appContext);
            var resolverContext = new MockedMapResolverContext(frameworkContext, hiveManager, mockedPropertyEditorFactory, new MockedParameterEditorFactory());
            var webmModelMapper = new CmsModelMapper(resolverContext);
            frameworkContext.SetTypeMappers(new FakeTypeMapperCollection(new AbstractMappingEngine[] { webmModelMapper, new FrameworkModelMapper(frameworkContext) }));

            var devDataset = DemoDataHelper.GetDemoData(appContext, attributeTypeRegistry);

            //Setup permissions
            var permissions = new Permission[] { new SavePermission(), new PublishPermission(), new HostnamesPermission(), new CopyPermission(), new MovePermission() }
                .Select(x => new Lazy<Permission, PermissionMetadata>(() => x, new PermissionMetadata(new Dictionary<string, object>
                    {
                        {"Id", x.Id},
                        {"Name", x.Name},
                        {"Type", x.Type},
                        {"UserType", x.UserType}
                    }))).ToArray();

            //Setup security service
            var usersMembershipProvider = new UsersMembershipProvider { AppContext = appContext };
            usersMembershipProvider.Initialize("UsersMembershipProvider", new NameValueCollection());
            var usersMembershipService = new MembershipService<User, UserProfile>(frameworkContext, hiveManager, 
                "security://user-profiles", "security://user-groups", Framework.Security.Model.FixedHiveIds.UserProfileVirtualRoot,
                usersMembershipProvider, Enumerable.Empty<MembershipProviderElement>());

            ResetMembershipProvider(usersMembershipService.MembershipProvider);

            var membersMembershipProvider = new MembersMembershipProvider { AppContext = appContext };
            membersMembershipProvider.Initialize("MembersMembershipProvider", new NameValueCollection());
            var membersMembershipService = new MembershipService<Member, MemberProfile>(frameworkContext, hiveManager,
                "security://member-profiles", "security://member-groups", Framework.Security.Model.FixedHiveIds.MemberProfileVirtualRoot,
                membersMembershipProvider, Enumerable.Empty<MembershipProviderElement>());

            ResetMembershipProvider(membersMembershipService.MembershipProvider);

            var permissionService = new PermissionsService(hiveManager, permissions, usersMembershipService);
            var publicAccessService = new PublicAccessService(hiveManager, membersMembershipService, appContext.FrameworkContext);
            var securityService = new SecurityService(usersMembershipService, membersMembershipService, permissionService, publicAccessService);

            var coreDataInstallTask = new EnsureCoreDataTask(frameworkContext, hiveManager, permissions, securityService);
            //var devDatasetInstallTask = new DevDatasetInstallTask(frameworkContext, mockedPropertyEditorFactory, hiveManager, attributeTypeRegistry);

            //Act

            coreDataInstallTask.InstallOrUpgrade();
            if (installCallback != null) installCallback();
            //devDatasetInstallTask.InstallOrUpgrade();
            //if (installCallback != null) installCallback();

            //Assert

            var totalSchemaCount = CoreCmsData.RequiredCoreSchemas().Count() + devDataset.DocTypes.Count() + 1; // +1 for SystemRoot schema
            var totalEntityCount =
                CoreCmsData.RequiredCoreUserGroups(permissions).Count() +
                CoreCmsData.RequiredCoreRootNodes().Count() +
                devDataset.ContentData.Count();
            var totalAttributeTypeCount = CoreCmsData.RequiredCoreSystemAttributeTypes().Count() + CoreCmsData.RequiredCoreUserAttributeTypes().Count();
            DoCoreAssertions(hiveManager, totalSchemaCount, totalEntityCount, totalAttributeTypeCount, 2, permissions, securityService);

            //CoreCmsData.RequiredCoreUsers().ForEach(
            //    x =>
            //    {
            //        securityService.UsersMembershipService.DeleteUser(x.Username, true);
            //        securityService.MembersMembershipService.DeleteUser(x.Username, true);
            //    });

            ResetMembershipProvider(securityService.Users.MembershipProvider);
            ResetMembershipProvider(securityService.Members.MembershipProvider);
        }

        private static void ResetMembershipProvider(MembershipProvider membershipProvider)
        {
            var hiveMembershipProvider = membershipProvider as AbstractRebelMembershipProvider;
            if (hiveMembershipProvider != null)
            {
                hiveMembershipProvider.ResetHiveReference();
            }
        }

        private void DoCoreAssertions(IHiveManager hiveManager, int totalSchemaCount, int totalEntityCount, int totalAttributeTypeCount, int mediaSchemaCount, IEnumerable<Lazy<Permission, PermissionMetadata>> permissions, ISecurityService securityService)
        {
            //Assert

            using (var uow = hiveManager.OpenReader<IContentStore>())
            {
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedHiveIds.SystemRoot));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.ContentVirtualRoot.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.ContentRecycleBin.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.MediaVirtualRoot.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedEntities.MediaRecycleBin.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(Framework.Security.Model.FixedEntities.UserGroupVirtualRoot.Id));
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(Framework.Security.Model.FixedEntities.UserVirtualRoot.Id));

                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.ContentRootSchema.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.MediaRootSchema.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(Rebel.Framework.Security.Model.FixedSchemas.MembershipUserSchema.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(Rebel.Framework.Security.Model.FixedSchemas.UserGroup.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.MediaFolderSchema.Id));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.MediaImageSchema.Id));

                //MB: Removed the below as creating of Admin is no longer part of the DataInstallTask
                // Get the admin user and check roles
                //var adminMemberUser = securityService.Users.GetByUsername("admin", false);
                //Assert.NotNull(adminMemberUser);
                //Assert.True(adminMemberUser.Groups.Any());


                var schemas = uow.Repositories.Schemas.GetAll<EntitySchema>().ToArray();
                
                Assert.IsTrue(schemas.SelectMany(x => x.AttributeGroups).Any());
                Assert.IsTrue(schemas.SelectMany(x => x.AttributeGroups).All(x => x != null));

                //ensure that schemas have relations on them
                var mediaImage = schemas.Where(x => x.Alias == "mediaImage").Single();
                Assert.True(mediaImage.RelationProxies.IsConnected);
                Assert.AreEqual(FixedHiveIds.MediaRootSchema.Value, mediaImage.RelationProxies.Single().Item.SourceId.Value);

                var mediaSchemas = uow.Repositories.Schemas.GetChildren<EntitySchema>(FixedRelationTypes.DefaultRelationType, FixedHiveIds.MediaRootSchema);
                Assert.AreEqual(mediaSchemaCount, mediaSchemas.Count());


                //ensure that the built in attribute types are there and set correctly
                var attributeTypes = uow.Repositories.Schemas.GetAll<AttributeType>().ToArray();
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == StringAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == BoolAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == DateTimeAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == IntegerAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == TextAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == NodeNameAttributeType.AliasValue));
                Assert.IsTrue(attributeTypes.Any(x => x.Alias == SelectedTemplateAttributeType.AliasValue));
                // TODO: Add other in built attribute types

                //now make sure that the render types are set
                var inbuiltString = attributeTypes.Single(x => x.Alias == StringAttributeType.AliasValue);
                Assert.AreEqual(CorePluginConstants.TextBoxPropertyEditorId, inbuiltString.RenderTypeProvider);
                var inbuiltText = attributeTypes.Single(x => x.Alias == TextAttributeType.AliasValue);
                Assert.AreEqual(CorePluginConstants.TextBoxPropertyEditorId, inbuiltText.RenderTypeProvider);
                var inbuiltDateTime = attributeTypes.Single(x => x.Alias == DateTimeAttributeType.AliasValue);
                Assert.AreEqual(CorePluginConstants.DateTimePickerPropertyEditorId, inbuiltDateTime.RenderTypeProvider);
                var inbuiltBool = attributeTypes.Single(x => x.Alias == BoolAttributeType.AliasValue);
                Assert.AreEqual(CorePluginConstants.TrueFalsePropertyEditorId, inbuiltBool.RenderTypeProvider);
                // TODO: Add other in built attribute types

                var entities = uow.Repositories.GetAll<TypedEntity>().ToArray();

                AssertItems(entities, CoreCmsData.RequiredCoreUserGroups(permissions), (x, y) => x.Name == y.Name, x => x.Name);

                var rootCount = entities.Count(x => x.EntitySchema.Alias == CoreCmsData.RequiredCoreRootNodes().First().EntitySchema.Alias);
                Assert.That(rootCount, Is.EqualTo(CoreCmsData.RequiredCoreRootNodes().Count()), "Root count different");

                //var userCount = entities.Where(x => x.EntitySchema.Alias == CoreCmsData.RequiredCoreUsers().First().EntitySchema.Alias).ToList();
                //Assert.That(userCount.Count, Is.EqualTo(CoreCmsData.RequiredCoreUsers().Count()), "User count different");

                var userGroupCount = entities.Count(x => x.EntitySchema.Alias == CoreCmsData.RequiredCoreUserGroups(permissions).First().EntitySchema.Alias);
                Assert.That(userGroupCount, Is.EqualTo(CoreCmsData.RequiredCoreUserGroups(permissions).Count()), "User group count different");

                //MB: Removed the below as administrator setup is no longer part of the DataInstallTask
                /*
                var adminUser = entities.SingleOrDefault(x => x.EntitySchema.Id.Value == Framework.Security.Model.FixedHiveIds.MembershipUserSchema.Value);
                Assert.IsNotNull(adminUser);
                Assert.IsTrue(adminUser.AttributeGroups.All(x => x != null));
                Assert.IsTrue(adminUser.AttributeGroups.Any());
                Assert.IsTrue(adminUser.EntitySchema.AttributeGroups.All(x => x != null));
                Assert.IsTrue(adminUser.EntitySchema.AttributeGroups.Any());
                var attributeGroups = adminUser.EntitySchema.AttributeDefinitions.Select(x => new {Def = x, x.AttributeGroup}).ToArray();
                var nullGroupsMsg = attributeGroups.Where(x => x.AttributeGroup == null).Select(x => x.Def.Alias).ToArray();
                Assert.That(nullGroupsMsg.Length, Is.EqualTo(0), "Group is null for attrib defs: " + string.Join(", ", nullGroupsMsg));
                 */

                //ensure they're all published
                foreach (var e in entities.Where(x => x.IsContent(uow) || x.IsMedia((uow))))
                {
                    var snapshot = uow.Repositories.Revisions.GetLatestSnapshot<TypedEntity>(e.Id);
                    Assert.AreEqual(FixedStatusTypes.Published.Alias, snapshot.Revision.MetaData.StatusType.Alias);
                }

                // Admin user is not longer created as part of the data install task
                //var adminUser = entities.SingleOrDefault(x => x.EntitySchema.Id.Value == FixedHiveIds.UserSchema.Value);
                //Assert.IsNotNull(adminUser);
                //Assert.IsTrue(adminUser.AttributeGroups.All(x => x != null));
                //Assert.IsTrue(adminUser.AttributeGroups.Any());
                //Assert.IsTrue(adminUser.EntitySchema.AttributeGroups.All(x => x != null));
                //Assert.IsTrue(adminUser.EntitySchema.AttributeGroups.Any());
                //Assert.IsTrue(adminUser.EntitySchema.AttributeDefinitions.Select(x => x.AttributeGroup).All(x => x != null));

                var distinctTypesByAlias = attributeTypes.DistinctBy(x => x.Alias).ToArray();
                var actualCount = attributeTypes.Count();
                var distinctCount = distinctTypesByAlias.Count();
                var actualCountById = attributeTypes.DistinctBy(x => x.Id).Count();
                var allWithAliasAndId = string.Join("\n", attributeTypes.OrderBy(x => x.Alias).Select(x => x.Alias + ": " + x.Id.Value));
                Assert.That(actualCount, Is.EqualTo(distinctCount),
                    "Duplicate AttributeTypes were created: {0} distinct by alias, {1} total loaded from provider, {2} distinct by id. All:{3}".InvariantFormat(distinctCount, actualCount, actualCountById, allWithAliasAndId));

                Assert.AreEqual(totalAttributeTypeCount, actualCount);

                //ensure the default templates are set
                var contentRoot = new Uri("content://");
                var homePage = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1048)));
                var templateRoot = new Uri("storage://templates");
                var templateProvider = "templates";

                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Homepage.cshtml")).ToString(),
                    homePage.Attributes[SelectedTemplateAttributeDefinition.AliasValue].DynamicValue.ToString());

                var installingModules = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1049)));
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Textpage.cshtml")).ToString(),
                    installingModules.Attributes[SelectedTemplateAttributeDefinition.AliasValue].DynamicValue.ToString());

                var faq = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1059)));
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Faq.cshtml")).ToString(),
                    faq.Attributes[SelectedTemplateAttributeDefinition.AliasValue].DynamicValue.ToString());

                //ensure the allowed templates are set
                Assert.AreEqual(1, homePage.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Count());
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Homepage.cshtml")).ToString(),
                    homePage.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Single().ToString());
                Assert.AreEqual(1, installingModules.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Count());
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Textpage.cshtml")).ToString(),
                    installingModules.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Single().ToString());
                Assert.AreEqual(1, faq.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Count());
                Assert.AreEqual(new HiveId(templateRoot, templateProvider, new HiveIdValue("Faq.cshtml")).ToString(),
                    faq.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-templates").Single().ToString());

                //ensure the allowed children are set
                var allowedChildrenOfHomepage = homePage.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").ToArray();
                Assert.AreEqual(2, allowedChildrenOfHomepage.Count());

                // Check installing-modules is an allowed child of homepage
                Assert.That(allowedChildrenOfHomepage.Select(x => x.Value), Has.Some.EqualTo(installingModules.EntitySchema.Id.Value));

                var faqCat = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1060)));
                Assert.AreEqual(1, faq.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").Count());
                Assert.IsTrue(faqCat.EntitySchema.Id.EqualsIgnoringProviderId(faq.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").Single()));

                var faqQuestion = entities.Single(x => x.Id.EqualsIgnoringProviderId(HiveId.ConvertIntToGuid(contentRoot, null, 1067)));
                Assert.AreEqual(1, faqCat.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").Count());
                Assert.IsTrue(faqQuestion.EntitySchema.Id.EqualsIgnoringProviderId(faqCat.EntitySchema.GetXmlPropertyAsList<HiveId>("allowed-children").Single()));

                var userGroups = uow.Repositories.GetAll<UserGroup>()
                    .Where(x => x.EntitySchema.Alias == UserGroupSchema.SchemaAlias);
                Assert.AreEqual(CoreCmsData.RequiredCoreUserGroups(permissions).Count(), userGroups.Count());
                var adminUserGroup = userGroups.First();
                Assert.AreEqual(1, adminUserGroup.RelationProxies.GetChildRelations(FixedRelationTypes.PermissionRelationType).Count());
                Assert.AreEqual(permissions.Count(),
                    adminUserGroup.RelationProxies.GetChildRelations(FixedRelationTypes.PermissionRelationType).Single().Item.MetaData.Count());

            }

            // Check same method coverage on GroupUnit<T>
            using (var uow = hiveManager.OpenWriter<IContentStore>())
            {
                Assert.IsTrue(uow.Repositories.Exists<TypedEntity>(FixedHiveIds.SystemRoot));
                Assert.IsTrue(uow.Repositories.Schemas.Exists<EntitySchema>(FixedSchemas.ContentRootSchema.Id));
                var schemas = uow.Repositories.Schemas.GetAll<EntitySchema>().ToArray();
                Assert.AreEqual(totalSchemaCount, schemas.Count());
                var mediaImage = schemas.Where(x => x.Alias == "mediaImage").Single();
                Assert.True(mediaImage.RelationProxies.IsConnected);
                Assert.AreEqual(FixedHiveIds.MediaRootSchema.Value, mediaImage.RelationProxies.Single().Item.SourceId.Value);

                var entities = uow.Repositories.GetAll<TypedEntity>().ToArray();
                Assert.AreEqual(totalEntityCount, entities.Count());
                var attributeTypes = uow.Repositories.Schemas.GetAll<AttributeType>().ToArray();
                Assert.AreEqual(totalAttributeTypeCount, attributeTypes.Count());
            }
        }

        private void AssertItems<T>(IEnumerable<TypedEntity> entities, IEnumerable<T> compareTo, Func<T, T, bool> comparisonDelegate, Func<T, string> assertionDelegate)
            where T : TypedEntity, new()
        {
            var found = new List<T>();
            var all = new List<TypedEntity>();
            entities.ForEach(
                entity =>
                {
                    foreach (var comparison in compareTo)
                    {
                        try
                        {
                            var recreate = new T();

                            if (recreate.EntitySchema.Alias == entity.EntitySchema.Alias)
                            {
                                all.Add(entity);
                                recreate.SetupFromEntity(entity);
                                if (comparisonDelegate.Invoke(recreate, comparison))
                                {
                                    found.Add(recreate);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            /* Nothing */
                        }
                    }
                });
            CollectionAssert.AreEquivalent(compareTo.Select(assertionDelegate).ToArray(), found.Select(assertionDelegate).ToArray());
        }

        public void TestSetup()
        {
            return;
        }

        public void TestTearDown()
        {
            return;
        }
    }
}
