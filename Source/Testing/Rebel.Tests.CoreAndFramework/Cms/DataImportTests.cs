using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rebel.Cms.Web.Mapping;
using Rebel.Cms.Web.Security;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Configuration;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Framework.Serialization;
using Rebel.Framework.TypeMapping;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.RepositoryTypes;
using Rebel.Tests.Extensions;

namespace Rebel.Tests.CoreAndFramework.Cms
{
    [TestFixture]
    public class DataImportTests
    {
        protected IHiveManager Hive;
        protected IMembershipService<Member> MembershipService;
        protected IPublicAccessService PublicAccessService;
        private NhibernateTestSetupHelper _nhibernateTestSetup;
        protected SerializationService SerializationService { get; set; }

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

            var membersMembershipProvider = new MembersMembershipProvider { AppContext = appContext };
            membersMembershipProvider.Initialize("MembersMembershipProvider", new NameValueCollection());
            MembershipService = new MembershipService<Member, MemberProfile>(appContext.FrameworkContext, Hive,
                "security://member-profiles", "security://member-groups", Framework.Security.Model.FixedHiveIds.MemberProfileVirtualRoot,
                membersMembershipProvider, Enumerable.Empty<MembershipProviderElement>());

            PublicAccessService = new PublicAccessService(Hive, MembershipService, appContext.FrameworkContext);

            var serializer = new ServiceStackSerialiser();
            SerializationService = new SerializationService(serializer);
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
        public void ImportTypedEntity_WhereAttributeGroupExistsWithOtherId()
        {
            // Arrange
            //Create entity with system General Group
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);
            var definition = new NodeNameAttributeDefinition(FixedGroupDefinitions.GeneralGroup);
            entity.EntitySchema.AttributeDefinitions.Add(definition);

            //Create new entity with same group, but different Id
            TypedEntity entityToImport = HiveModelCreationHelper.MockTypedEntity(true);
            var definition2 = new NodeNameAttributeDefinition(FixedGroupDefinitions.GeneralGroup);
            definition2.AttributeGroup.Id = new HiveId(Guid.NewGuid());
            entityToImport.EntitySchema.AttributeDefinitions.Add(definition2);

            //Serialize and deserialize
            IStreamedResult result = SerializationService.ToStream(entityToImport);
            object obj = SerializationService.FromStream(result.ResultStream, typeof(TypedEntity));
            TypedEntity typedEntity = obj as TypedEntity;

            // Act
            //Save entity with NodeNameAttributeDefinition to repository
            //Id for Group should be set 'automatically'
            using (var uow = Hive.OpenWriter<IContentStore>())
            {
                uow.Repositories.AddOrUpdate(entity);
                uow.Complete();
            }

            //Try to save/import deserialized entity
            using (var uow = Hive.OpenWriter<IContentStore>())
            {
                uow.Repositories.AddOrUpdate(typedEntity);
                uow.Complete();
            }

            // Assert
            Assert.That(entity.AttributeGroups.Any(x => x.Alias == "rebel-internal-general-properties"), Is.True);
            Assert.That(typedEntity.AttributeGroups.Any(x => x.Alias == "rebel-internal-general-properties"), Is.True);

            Assert.That(entity.AttributeGroups.Any(x => x.Id == HiveId.Empty), Is.False);
            Assert.That(typedEntity.AttributeGroups.Any(x => x.Id == HiveId.Empty), Is.False);
        }
    }
}
