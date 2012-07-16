using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rebel.Cms.Web.Configuration.Languages;
using Rebel.Framework;
using Rebel.Framework.Caching;
using Rebel.Framework.Data;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Associations;
using Rebel.Framework.Persistence.Model.Associations._Revised;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.Versioning;
using Rebel.Framework.Serialization;
using Rebel.Hive;
using Rebel.Hive.Caching;
using Rebel.Tests.Extensions;
using Rebel.Cms.Web;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Hive.Configuration;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Tests.CoreAndFramework.Serialization
{
    public class JsonNetSerializationFixture : AbstractSerializationFixture
    {
        public override void SetUp()
        {
            var serializer = new JsonNetSerializer();
            SerializationService = new SerializationService(serializer);
        }
    }

    public class ServiceStackSerializationFixture : AbstractSerializationFixture
    {
        public override void SetUp()
        {
            var serializer = new ServiceStackSerialiser();
            SerializationService = new SerializationService(serializer);
        }
    }

    [TestFixture]
    public abstract class AbstractSerializationFixture
    {
        #region Properties

        protected SerializationService SerializationService { get; set; }

        #endregion

        [Test]
        public void CanSerializeThenDeserializeLanguageElement()
        {
            var languageElement = new LanguageElement
                                      {
                                          IsoCode = "en-GB", Name = "English (United Kingdom)", Fallbacks = new FallbackCollection
                                                                                                                {
                                                                                                                    new FallbackElement {IsoCode = "en-US"}
                                                                                                                }
                                      };

            var result = SerializationService.ToStream(languageElement);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            var resultJson = result.ResultStream.ToJsonString();

            var reHydrated = SerializationService.FromJson<LanguageElement>(resultJson);

            Assert.That(reHydrated, Is.Not.Null);
        }

        [Test]
        public void CompositeSchemaSurvivesRoundtripOnTypedEntity()
        {
            var hive = GetHiveForTest();

            SetupContentTest(hive);

            hive.Cms().NewContentType("base")
                .Define("inheritedfield", "system-string-type", "tab1")
                .Commit();

            hive.Cms().NewContentType("inherited")
                .InheritFrom("base")
                .Define("realfield", "system-string-type", "tab2")
                .Commit();

            var content = hive.Cms().NewRevision("some-content", "some-content-url", "inherited")
                .SetValue("inheritedfield", "a value")
                .SetValue("realfield", "a real value")
                .Publish()
                .Commit();

            Assert.That(content.Item.EntitySchema, Is.TypeOf<CompositeEntitySchema>());
            var entityResult = SerializationService.ToJson(content.Item);

            var reLoad = SerializationService.FromJson<TypedEntity>(entityResult);
            Assert.That(reLoad.EntitySchema, Is.TypeOf<CompositeEntitySchema>());

        }

        private void SetupContentTest(IHiveManager hive)
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent schema and content root exists for this test
            var contentVirtualRoot = FixedEntities.ContentVirtualRoot;
            var systemRoot = new SystemRoot();
            var contentRootSchema = new ContentRootSchema();
            hive.AutoCommitTo<IContentStore>(
                x =>
                {
                    x.Repositories.AddOrUpdate(systemRoot);
                    x.Repositories.AddOrUpdate(contentVirtualRoot);
                    x.Repositories.Schemas.AddOrUpdate(contentRootSchema);
                });
        }

        private static HiveManager GetHiveForTest()
        {
            var nhibernateTestSetup = new NhibernateTestSetupHelper();

            var storageProvider = new IoHiveTestSetupHelper(nhibernateTestSetup.FakeFrameworkContext);


            var hive =
                new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test",
                                new WildcardUriMatch("content://"),
                                nhibernateTestSetup.ReadonlyProviderSetup,
                                nhibernateTestSetup.ProviderSetup,
                                nhibernateTestSetup.FakeFrameworkContext),
                            storageProvider.CreateGroup("uploader", "storage://")
                        },
                    nhibernateTestSetup.FakeFrameworkContext);

            return hive;
        }

        [Test]
        public void CanSerializeLocalizedString()
        {
            var testString = new LocalizedString("myvalue");

            var result = SerializationService.ToJson(testString);

            Assert.IsNotNullOrEmpty(result);

            var reHydrate = SerializationService.FromJson<LocalizedString>(result);

            Assert.NotNull(reHydrate);

            Assert.That(reHydrate.Value, Is.EqualTo(testString.Value));
            CollectionAssert.AreEquivalent(testString.Values, reHydrate.Values);
        }

        [Test]
        public void CanSerializeTypedEntityToJson()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);

            var result = SerializationService.ToStream(entity);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string json = result.ResultStream.ToJsonString();
            Assert.That(json, Is.Not.Empty);
            Assert.That(json.StartsWith("{"), Is.True);
            Assert.That(json.EndsWith("}"));
        }

        [Test]
        public void CanSerializeEntitySchemaToJson()
        {
            EntitySchema schema = HiveModelCreationHelper.MockEntitySchema("jsonSchemcaTest", "JsonSchemcaTest");

            var result = SerializationService.ToStream(schema);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string json = result.ResultStream.ToJsonString();
            Assert.That(json, Is.Not.Empty);
            Assert.That(json.StartsWith("{"), Is.True);
            Assert.That(json.EndsWith("}"));
        }

        [Test]
        public void CanSerializeAttributeDefinitionToJson()
        {
            AttributeType typeDef = HiveModelCreationHelper.CreateAttributeType("attrTypeTest", "AttrTypeTest", "Attribute Type Test", true);
            AttributeGroup groupDef = HiveModelCreationHelper.CreateAttributeGroup("attrGroupTest", "AttrGropTest", 0, true);
            AttributeDefinition definition = HiveModelCreationHelper.CreateAttributeDefinition("attrDefTest", "AttrDefTest", "Attribute Definition Test", typeDef, groupDef, true);

            var result = SerializationService.ToStream(definition);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string json = result.ResultStream.ToJsonString();
            Assert.That(json, Is.Not.Empty);
            Assert.That(json.StartsWith("{"), Is.True);
            Assert.That(json.EndsWith("}"));
        }

        [Test]
        public void CanSerializeAttributeGroupToJson()
        {
            AttributeGroup groupDef = HiveModelCreationHelper.CreateAttributeGroup("attrGroupTest", "AttrGropTest", 0, true);

            var result = SerializationService.ToStream(groupDef);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string json = result.ResultStream.ToJsonString();
            Assert.That(json, Is.Not.Empty);
            Assert.That(json.StartsWith("{"), Is.True);
            Assert.That(json.EndsWith("}"));
        }

        [Test]
        public void CanSerializeAttributeTypeToJson()
        {
            AttributeType typeDef = HiveModelCreationHelper.CreateAttributeType("attrTypeTest", "AttrTypeTest", "Attribute Type Test", true);

            var result = SerializationService.ToStream(typeDef);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string json = result.ResultStream.ToJsonString();
            Assert.That(json, Is.Not.Empty);
            Assert.That(json.StartsWith("{"), Is.True);
            Assert.That(json.EndsWith("}"));
        }

        [Test]
        public void CanSerializeThenDeserializeTypedAttribute()
        {
            var att = new TypedAttribute(new NodeNameAttributeDefinition(new AttributeGroup("tab1", "My Group", 5)),
                                         "some value");

            var result = SerializationService.ToStream(att);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            var resultJson = result.ResultStream.ToJsonString();

            var reHydrated = SerializationService.FromJson<TypedAttribute>(resultJson);

            Assert.That(reHydrated, Is.Not.Null);
        }

        [Test]
        public void CanSerializeRelationToJson()
        {
            var sourceId = new HiveId(Guid.NewGuid());
            var destinationId = new HiveId(Guid.NewGuid());
            var relationType = new RelationType("DefaultRelation");
            var relation = new RelationById(sourceId, destinationId, relationType, 0);

            var result = SerializationService.ToStream(relation);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string json = result.ResultStream.ToJsonString();
            Assert.That(json, Is.Not.Empty);
            Assert.That(json.StartsWith("{"), Is.True);
            Assert.That(json.EndsWith("}"));
        }

        [Test]
        public void CanSerializeVerionedTypedEntityToJson()
        {
            Revision<TypedEntity> revision = HiveModelCreationHelper.MockVersionedTypedEntity(true);

            var result = SerializationService.ToStream(revision);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string json = result.ResultStream.ToJsonString();
            Assert.That(json, Is.Not.Empty);
            Assert.That(json.StartsWith("{"), Is.True);
            Assert.That(json.EndsWith("}"));
        }

        [Test]
        public void CanSerializeEntitiesWithRelations()
        {
            TypedEntity entity1 = HiveModelCreationHelper.MockTypedEntity(true);
            TypedEntity entity2 = HiveModelCreationHelper.MockTypedEntity(true);
            //NOTE relation proxies are currently ignored
            entity1.RelationProxies.EnlistChild(entity2, FixedRelationTypes.DefaultRelationType);

            var result1 = SerializationService.ToStream(entity1);
            var result2 = SerializationService.ToStream(entity2);

            Assert.That(result1.Success, Is.True);
            Assert.That(result1.ResultStream, Is.Not.Null);

            Assert.That(result2.Success, Is.True);
            Assert.That(result2.ResultStream, Is.Not.Null);

            //TODO deserialize to verify relations in entities
        }

        [Test]
        public void CanSerializeTypedEntityToJsonAndVerifyResultIsJson()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);

            var result = SerializationService.ToStream(entity);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string data = result.ResultStream.ToJsonString();

            Assert.That(data, Is.Not.Empty);
            Assert.That(data.StartsWith("{"), Is.True);
            Assert.That(data.EndsWith("}"));
        }

        [Test]
        public void CanSerializeThenDeserializeAttributeTypeAndVerifyAttributeTypes()
        {
            IEnumerable<AttributeType> types = GetCoreSystemAttributeTypes();

            var result = SerializationService.ToStream(types);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string data = result.ResultStream.ToJsonString();
            Assert.That(data, Is.Not.Empty);

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            MemoryStream stream = new MemoryStream(byteArray);
            var obj = SerializationService.FromStream(stream, typeof(IEnumerable<AttributeType>));

            var attributeTypes = obj as IEnumerable<AttributeType>;

            Assert.That(attributeTypes, Is.Not.Null);
            Assert.IsTrue(attributeTypes.Any(x => x.Alias == StringAttributeType.AliasValue));
            Assert.IsTrue(attributeTypes.Any(x => x.Alias == BoolAttributeType.AliasValue));
            Assert.IsTrue(attributeTypes.Any(x => x.Alias == DateTimeAttributeType.AliasValue));
            Assert.IsTrue(attributeTypes.Any(x => x.Alias == IntegerAttributeType.AliasValue));
            Assert.IsTrue(attributeTypes.Any(x => x.Alias == TextAttributeType.AliasValue));
            Assert.IsTrue(attributeTypes.Any(x => x.Alias == NodeNameAttributeType.AliasValue));
            Assert.IsTrue(attributeTypes.Any(x => x.Alias == SelectedTemplateAttributeType.AliasValue));
        }

        [Test]
        public void CanSerializeThenDeserializeAttributeType()
        {
            AttributeType typeDef = HiveModelCreationHelper.CreateAttributeType("attrTypeTest", "AttrTypeTest", "Attribute Type Test", true);

            var result = SerializationService.ToStream(typeDef);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            var json = result.ResultStream.ToJsonString();
            Assert.That(json, Is.Not.Empty);

            var obj = SerializationService.FromJson<AttributeType>(json);

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.GetType(), Is.EqualTo(typeof(AttributeType)));
            Assert.That(typeDef, Is.EqualTo(obj));
        }

        [Test]
        public void CanSerializeThenDeserializeAttributeGroup()
        {
            AttributeGroup groupDef = HiveModelCreationHelper.CreateAttributeGroup("attrGroupTest", "AttrGropTest", 0, true);

            var result = SerializationService.ToStream(groupDef);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            object obj = SerializationService.FromStream(result.ResultStream, typeof(AttributeGroup));

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.GetType(), Is.EqualTo(typeof(AttributeGroup)));
        }

        [Test]
        public void CanSerializeThenDeserializeAttributeDefinition()
        {
            AttributeType typeDef = HiveModelCreationHelper.CreateAttributeType("attrTypeTest", "AttrTypeTest", "Attribute Type Test", true);
            AttributeGroup groupDef = HiveModelCreationHelper.CreateAttributeGroup("attrGroupTest", "AttrGropTest", 0, true);
            AttributeDefinition definition = HiveModelCreationHelper.CreateAttributeDefinition("attrDefTest", "AttrDefTest", "Attribute Definition Test", typeDef, groupDef, true);

            var result = SerializationService.ToStream(definition);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            object obj = SerializationService.FromStream(result.ResultStream, typeof(AttributeDefinition));

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.GetType(), Is.EqualTo(typeof(AttributeDefinition)));
        }

        [Test]
        public void CanSerializeThenDeserializeAttributeDefinitionArray()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);
            AttributeDefinition[] definitions = entity.EntitySchema.AttributeDefinitions.ToArray();

            var result = SerializationService.ToStream(definitions);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            object obj = SerializationService.FromStream(result.ResultStream, typeof(AttributeDefinition[]));

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.GetType(), Is.EqualTo(typeof(AttributeDefinition[])));
            Assert.That(((AttributeDefinition[])obj).Length, Is.EqualTo(definitions.Length));
        }

        [Test]
        public void CanSerializeThenDeserializeEntitySchema()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);
            var schema = entity.EntitySchema;

            var result = SerializationService.ToStream(schema);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            var resultJson = result.ResultStream.ToJsonString();

            var entitySchema = SerializationService.FromJson<EntitySchema>(resultJson);

            Assert.That(entitySchema, Is.Not.Null);
            Assert.That(entitySchema.Alias, Is.EqualTo(schema.Alias));
            Assert.That(entitySchema.AttributeDefinitions.Count, Is.EqualTo(schema.AttributeDefinitions.Count));
            Assert.That(entitySchema.AttributeGroups.Count, Is.EqualTo(schema.AttributeGroups.Count));
            Assert.That(entitySchema.AttributeTypes.Count(), Is.EqualTo(schema.AttributeTypes.Count()));
        }

        [Test]
        public void CanSerializeThenDeserializeTypedEntity()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);

            var graph = entity.GetAllIdentifiableItems().Reverse().ToArray();

            var testTypes = new List<object>();
            foreach (var referenceByHiveId in graph.WhereNotNull())
            {
                object test;
                try
                {
                    test = SerializeDeserialize(referenceByHiveId);
                    Assert.NotNull(test);
                    testTypes.Add(test);
                }
                catch (Exception ex )
                {
                    throw;
                }
            }

            var obj = SerializeDeserialize(entity);

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.GetType(), Is.EqualTo(typeof(TypedEntity)));
            Assert.That(((TypedEntity)obj).Id, Is.EqualTo(entity.Id));
            Assert.That(((TypedEntity)obj).Attributes.Count, Is.EqualTo(entity.Attributes.Count));
            Assert.That(((TypedEntity)obj).AttributeGroups.Count(), Is.EqualTo(entity.AttributeGroups.Count()));
        }

        private object SerializeDeserialize(object entity)
        {
            var result = SerializationService.ToStream(entity);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string data = result.ResultStream.ToJsonString();
            Assert.That(data, Is.Not.Empty);

            byte[] byteArray = Encoding.UTF8.GetBytes(data);
            MemoryStream stream = new MemoryStream(byteArray);
            var obj = SerializationService.FromStream(stream, entity.GetType());
            return obj;
        }

        [Test]
        public void CanRoundtripDictionary()
        {
            var dict = new Dictionary<string, object>()
                {
                    {"BlahKey", "Blah"},
                    {"FoodKey", "Foo"}
                };
            var json = SerializationService.ToJson(dict);
            var rehydrated = SerializationService.FromJson<IDictionary<string, object>>(json);

            Assert.That(rehydrated, Is.EqualTo(dict));
        }

        [Test]
        public void CanRoundtripGenericLazyDictionary()
        {
            var dict = new Dictionary<string, object>()
                {
                    {"BlahKey", "Blah"},
                    {"FoodKey", "Foo"}
                };
            var json = SerializationService.ToJson(dict);
            var rehydrated = SerializationService.FromJson<Dictionary<string, object>>(json);

            Assert.That(dict.Count, Is.EqualTo(rehydrated.Count));
            Assert.That(dict.First().Key, Is.EqualTo(rehydrated.First().Key));
            Assert.That(dict.First().Value, Is.EqualTo(rehydrated.First().Value));
        }

        [Test]
        public void CanRoundtripGenericNotifyingDictionary()
        {
            var dict = new NotifyingDictionary<string, object>()
                {
                    {"BlahKey", "Blah"},
                    {"FoodKey", "Foo"}
                };
            var json = SerializationService.ToJson(dict);
            var rehydrated = SerializationService.FromJson<NotifyingDictionary<string, object>>(json);

            CollectionAssert.AreEquivalent(dict, rehydrated);
        }

        [Test]
        public void CanRoundtripTypedAttributeValueCollection()
        {
            var dict = new TypedAttributeValueCollection()
                {
                    {"BlahKey", "Blah"},
                    {"FoodKey", "Foo"}
                };
            var json = SerializationService.ToJson(dict);
            var rehydrated = SerializationService.FromJson<TypedAttributeValueCollection>(json);

            Assert.That(rehydrated, Is.EqualTo(dict));
        }

        [Test] //TODO Fix the issue with deserialization of XmlConfiguration with null value, as its causing tests to fail although not a major issue.
        public void CanSerializeThenDeserializeThenSerializeToVerifyResults()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);

            var entitySerialized = SerializationService.ToStream(entity);
            Assert.That(entitySerialized.Success, Is.True);
            Assert.That(entitySerialized.ResultStream, Is.Not.Null);

            var entityJson = entitySerialized.ResultStream.ToJsonString();
            var entityObject = SerializationService.FromStream(entitySerialized.ResultStream, typeof(TypedEntity));

            Assert.That(entityObject, Is.Not.Null);
            Assert.That(entityObject.GetType(), Is.EqualTo(typeof(TypedEntity)));

            var entitySerializedAgain = SerializationService.ToStream(entityObject);

            Assert.That(entitySerializedAgain.Success, Is.True);
            Assert.That(entitySerializedAgain.ResultStream, Is.Not.Null);

            var entityJsonTwice = entitySerializedAgain.ResultStream.ToJsonString();

            Assert.That(entityJson, Is.EqualTo(entityJsonTwice));
        }

        [Test]
        public void CanSerializeThenDeserializeVerionedTypedEntity()
        {
            Revision<TypedEntity> revision = HiveModelCreationHelper.MockVersionedTypedEntity(true);

            var result = SerializationService.ToStream(revision);
            var item = SerializationService.ToStream(revision.Item);
            var metadata = SerializationService.ToStream(revision.MetaData);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);
            Assert.That(item.Success, Is.True);
            Assert.That(item.ResultStream, Is.Not.Null);
            Assert.That(metadata.Success, Is.True);
            Assert.That(metadata.ResultStream, Is.Not.Null);

            var objTE = SerializationService.FromStream(item.ResultStream, typeof(TypedEntity));
            var objRD = SerializationService.FromStream(metadata.ResultStream, typeof(RevisionData));
            var objR = SerializationService.FromStream(result.ResultStream, typeof(Revision<TypedEntity>));

            Assert.That(objTE, Is.Not.Null);
            Assert.That(objRD, Is.Not.Null);
            Assert.That(objR, Is.Not.Null);
            Assert.That(objR.GetType(), Is.EqualTo(typeof(Revision<TypedEntity>)));
            Assert.That(((Revision<TypedEntity>)objR).Item, Is.Not.Null);
            Assert.That(((Revision<TypedEntity>)objR).MetaData, Is.Not.Null);
            Assert.That(((Revision<TypedEntity>)objR).Item.Attributes.Count, Is.EqualTo(revision.Item.Attributes.Count));
        }

        [Test]
        public void CanDeserializeJsonStringToTypedEntity()
        {
            string jsonObject = "{}";
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonObject);
            MemoryStream stream = new MemoryStream(byteArray);

            var obj = SerializationService.FromStream(stream, typeof(TypedEntity));

            Assert.That(obj, Is.Not.Null);
            Assert.That(obj.GetType(), Is.EqualTo(typeof(TypedEntity)));
        }

        [Test]
        public void SerializedCacheKeyEquals()
        {
            var one = CacheKey.Create<string>("this");
            var two = CacheKey.Create<string>("this");

            //Serialize
            var result = SerializationService.ToStream(two);
            //Deserialize
            var obj = SerializationService.FromStream(result.ResultStream, typeof(CacheKey<string>));
            var three = (CacheKey)obj;

            Assert.That(one, Is.EqualTo(two));
            Assert.That(two, Is.EqualTo(three));
            Assert.That(two.GetHashCode(), Is.EqualTo(three.GetHashCode()));
        }

        [Test]
        public void CacheKeySerializesToJson()
        {
            var key = CacheKey.Create<string>("my-1");
            var result = SerializationService.ToStream(key);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string keyJson = result.ResultStream.ToJsonString();

            Assert.That(keyJson, Is.Not.Empty);
            Assert.That(keyJson.StartsWith("{"), Is.True);
            Assert.That(keyJson.EndsWith("}"));
        }

        [Test]
        public void HiveRelationCacheKeySerializeToFromJson()
        {
            var guid = Guid.NewGuid();
            var key =
                CacheKey.Create(new HiveRelationCacheKey(HiveRelationCacheKey.RepositoryTypes.Entity, new HiveId(guid),
                                                         Direction.Siblings, FixedRelationTypes.ApplicationRelationType));
            //Serialize
            var serializedKey = SerializationService.ToStream(key);
            var keyJson = serializedKey.ResultStream.ToJsonString();
            //Deserialize
            var keyBack = SerializationService.FromStream(serializedKey.ResultStream, typeof(CacheKey<HiveRelationCacheKey>));
            //Serialize again
            var serializedKeyBack = SerializationService.ToStream(keyBack);
            var keyJsonTwice = serializedKeyBack.ResultStream.ToJsonString();

            Assert.That(keyBack, Is.Not.Null);
            Assert.That(keyJson, Is.EqualTo(keyJsonTwice));
        }

        [Test]
        public void CanSerializeTypedEntityCacheKey()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);

            var key = CacheKey.Create<TypedEntity>(entity);
            var result = SerializationService.ToStream(key);

            Assert.That(result.Success, Is.True);
            Assert.That(result.ResultStream, Is.Not.Null);

            string keyJson = result.ResultStream.ToJsonString();

            Assert.That(keyJson, Is.Not.Empty);
            Assert.That(keyJson.StartsWith("{"), Is.True);
            Assert.That(keyJson.EndsWith("}"));
        }

        [Test] //TODO: Fix this test, which currently fails because XmlConfiguration changes upon deserialization
        public void TypedEntityCacheKeySerializeToFromJson()
        {
            TypedEntity entity = HiveModelCreationHelper.MockTypedEntity(true);
            var key = CacheKey.Create<TypedEntity>(entity);

            //Serialize
            var result = SerializationService.ToStream(key);
            var keyJson = result.ResultStream.ToJsonString();

            //Deserialize
            var keyBack = SerializationService.FromStream(result.ResultStream, typeof(CacheKey<TypedEntity>));

            //Serialize again
            var serializedKeyBack = SerializationService.ToStream(keyBack);
            var keyJsonTwice = serializedKeyBack.ResultStream.ToJsonString();

            Assert.That(keyBack, Is.Not.Null);
            Assert.That(keyJson, Is.EqualTo(keyJsonTwice));
        }

        [Test]
        public void CanRoundtripDateTimeOffsetWithTimezone()
        {
            var date = new DateTimeOffset(2000, 1, 1, 2, 3, 4, 5, TimeSpan.FromHours(5));
            var json = SerializationService.ToJson(date);
            var rehydrate = SerializationService.FromJson<DateTimeOffset>(json);
            Assert.That(rehydrate, Is.EqualTo(date));
            Assert.True(rehydrate.EqualsExact(date));
        }

        [Test]
        public void CanSerializeCacheValueOfIRelationById()
        {
            var relation1 = new RelationById(new HiveId(Guid.NewGuid()), new HiveId(Guid.NewGuid()),
                                             FixedRelationTypes.DefaultRelationType, 0);
            var cacheValue = new CacheValueOf<IRelationById>(relation1);

            var json = SerializationService.ToJson(cacheValue);
            Assert.That(json, Is.Not.Empty);

            var rehydrate = SerializationService.FromJson<CacheValueOf<IRelationById>>(json);
            Assert.That(rehydrate, Is.Not.Null);

            Assert.That(rehydrate, Is.EqualTo(cacheValue));
        }

        [Test]
        public void CanSerializeCacheValueOfListOfRelations()
        {
            var relation1 = new RelationById(new HiveId(Guid.NewGuid()), new HiveId(Guid.NewGuid()),
                                             FixedRelationTypes.DefaultRelationType, 0);
            var relation2 = new RelationById(new HiveId(Guid.NewGuid()), new HiveId(Guid.NewGuid()),
                                             FixedRelationTypes.DefaultRelationType, 0);
            var relation3 = new RelationById(new HiveId(Guid.NewGuid()), new HiveId(Guid.NewGuid()),
                                             FixedRelationTypes.DefaultRelationType, 0);
            
            var list = new List<IRelationById> {relation1, relation2, relation3};
            var cacheValueOf = new CacheValueOf<List<IRelationById>>(list);

            //Serialize
            var serializedKey = SerializationService.ToStream(cacheValueOf);
            var keyJson = serializedKey.ResultStream.ToJsonString();
            //Deserialize
            var keyBack = SerializationService.FromStream(serializedKey.ResultStream, typeof(CacheValueOf<List<IRelationById>>));
            //Serialize again
            var serializedKeyBack = SerializationService.ToStream(keyBack);
            var keyJsonTwice = serializedKeyBack.ResultStream.ToJsonString();

            Assert.That(keyBack, Is.Not.Null);
            Assert.That(keyJson, Is.EqualTo(keyJsonTwice));
        }

        [Test]
        public void DoesNotHaveAccessToPrivateSetter()
        {
            var json = @"{""Name"":""Morten""}";
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(bytes);

            //Deserialize
            var person = (Person)SerializationService.FromStream(stream, typeof(Person));

            Assert.That(person.Name, Is.Not.EqualTo("Morten"));
            Assert.That(person.Name, Is.Null);
        }

        [TestFixtureSetUp]
        public abstract void SetUp();

        [TestFixtureTearDown]
        public virtual void TearDown()
        {
        }

        private static IEnumerable<AttributeType> GetCoreSystemAttributeTypes()
        {
            return new[]
                {
                    AttributeTypeRegistry.Current.GetAttributeType(StringAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(TextAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(IntegerAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(DateTimeAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(BoolAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(ReadOnlyAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(ContentPickerAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(MediaPickerAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(ApplicationsListPickerAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(NodeNameAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(SelectedTemplateAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(UserGroupUsersAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(FileUploadAttributeType.AliasValue),
                    AttributeTypeRegistry.Current.GetAttributeType(DictionaryItemTranslationsAttributeType.AliasValue)
                };
        }
    }

    public class Person
    {
        public string Name { get; private set; }

        public void SetName(string name)
        {
            Name = name;
        }
    }
}
