using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rebel.Cms.Web;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Security.Permissions;
using Rebel.Cms.Web.System;
using Rebel.Framework;
using Rebel.Framework.Dynamics.Expressions;
using Rebel.Framework.Expressions.Remotion;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Framework.Security.Model.Schemas;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.ProviderGrouping;
using Rebel.Hive.ProviderSupport;
using Rebel.Hive.RepositoryTypes;
using Rebel.Hive.Tasks;
using Rebel.Tests.Extensions;
using FixedEntities = Rebel.Framework.Security.Model.FixedEntities;
using FixedHiveIds = Rebel.Framework.Security.Model.FixedHiveIds;

namespace Rebel.Tests.CoreAndFramework.Hive.DefaultProviders
{
    using System.Threading;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Linq;

    using Rebel.Framework.Linq.QueryModel;

    using Rebel.Framework.Persistence.Model.Versioning;
    using FixedEntities = Rebel.Framework.Persistence.Model.Constants.FixedEntities;

    public abstract class AbstractProviderQueryTests
    {
        [TestFixtureSetUp]
        public void SetupFixture()
        {
            DataHelper.SetupLog4NetForTests();
        }


        protected virtual string DynamicAttributeAliasForQuerying
        {
            get { return "aliasForQuerying"; }
        }

        protected virtual string AttributeAlias2ForQuerying
        {
            get { return HiveModelCreationHelper.DefAlias2WithType1; }
        }

        protected virtual string AttributeAlias1ForQuerying
        {
            get { return HiveModelCreationHelper.DefAlias1WithType1; }
        }

        protected virtual string SchemaAliasForQuerying
        {
            get { return "schema-alias1"; }
        }

        [Test]
        public void Create_Dynamic_Search_With_Results()
        {

            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            // Create schema
            var schema1 = Hive.Cms().NewContentType<EntitySchema, IContentStore>("homePage")
                .Define("pageTitle", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("pageContent", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();
            var schema2 = Hive.Cms().NewContentType<EntitySchema, IContentStore>("contentPage")
                .Define("pageTitle", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("pageContent", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            Assert.True(schema1.Success);
            Assert.True(schema2.Success);

            var item1 = new Content();
            item1.SetupFromSchema(schema1.Item);
            item1.Id = new HiveId(Guid.NewGuid());
            item1["pageTitle"] = "Hello There";
            item1["pageContent"] = "some page content";

            var item2 = new Content();
            item2.SetupFromSchema(schema2.Item);
            item2.Id = new HiveId(Guid.NewGuid());
            item2["pageTitle"] = "Title 1";
            item2["pageContent"] = "this is some page content. hi, hello, goodbye.";

            var item3 = new Content();
            item3.SetupFromSchema(schema1.Item);
            item3.Id = new HiveId(Guid.NewGuid());
            item3["pageTitle"] = "Another page";
            item3["pageContent"] = "Say hello to my little friend.";

            var writerResult = Hive.AutoCommitTo<IContentStore>(x =>
            {
                x.Repositories.AddOrUpdate(item1);
                x.Repositories.AddOrUpdate(item2);
                x.Repositories.AddOrUpdate(item3);
            });
            Assert.True(writerResult.WasCommitted);
            AddRevision(item1, FixedStatusTypes.Published);
            AddRevision(item2, FixedStatusTypes.Published);
            AddRevision(item3, FixedStatusTypes.Published);

            // Check can get the items normally
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                Assert.True(uow.Repositories.Exists<Content>(item1.Id));
                Assert.True(uow.Repositories.Exists<Content>(item2.Id));
                Assert.True(uow.Repositories.Exists<Content>(item3.Id));
            }

            //Create a dynamic search

            var docTypes = new[] { "homePage", "contentPage" };
            var fields = new[] { "pageTitle", "pageContent" };
            var searchTerm = "hello";
            
            using (var uow = GroupUnitFactory.Create())
            {
                var predicate = ExpressionExtensions.False<TypedEntity>();
                var query = uow.Repositories.Query();
                foreach (var d in docTypes)
                {
                    //this will add an 'AND' clause, not an 'OR' clause
                    //query = query.Where(x => x.EntitySchema.Alias == d);
                    var d1 = d;
                    predicate = predicate.Or(x => x.EntitySchema.Alias == d1);
                }
                foreach (var f in fields)
                {
                    //this will add an 'AND' clause, not an 'OR' clause
                    //query = query.Where(x => x.Attribute<string>(f) == searchTerm);
                    var f1 = f;
                    predicate = predicate.Or(x => x.Attribute<string>(f1) == searchTerm);
                }

                var result = query.Where(predicate).ToArray();

                Assert.AreEqual(3, result.Count());
            }

            
        }

        [Test]
        public void CountUsesSpecifiedRevision()
        {
            // Arrange
            LogHelper.TraceIfEnabled(typeof(QueryExtensions), "In CountUsesSpecifiedRevision");
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var twoRevisionsOfOneEntity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);
            AddRevision(twoRevisionsOfOneEntity, FixedStatusTypes.Published);
            AddRevision(twoRevisionsOfOneEntity, FixedStatusTypes.Published);

            var revisionStatusType = new RevisionStatusType("custom", "custom for test");
            var oneRevisionOfAnEntity = CreateEntityForTest(Guid.NewGuid(), Guid.NewGuid(), ProviderSetup);
            AddRevision(oneRevisionOfAnEntity, revisionStatusType);

            var anotherRevisionOfSameType = CreateEntityForTest(Guid.NewGuid(), Guid.NewGuid(), ProviderSetup);
            AddRevision(anotherRevisionOfSameType, revisionStatusType);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var countOfCustomType = uow.Repositories.OfRevisionType(revisionStatusType.Alias).Count();
                var countOfPublished = uow.Repositories.OfRevisionType(FixedStatusTypes.Published).Count();

                // Assert
                Assert.That(countOfCustomType, Is.EqualTo(2));
                Assert.That(countOfPublished, Is.EqualTo(1));

                Thread.Sleep(500); // Let profiler catch up
            }
        }

        [Test]
        public void WhenExecutingCount_WithoutRevisionSpecified_OnlyPublishedResultsAreIncluded()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            AddRevision(entity, FixedStatusTypes.Published);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var result = uow.Repositories.Count(x => x.Id == (HiveId)newGuid);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }
        }

        private void AddRevision(TypedEntity entity, RevisionStatusType revisionStatusType)
        {
            using (var uow = GroupUnitFactory.Create())
            {
                // Make a new revision that is published
                var revision = new Revision<TypedEntity>(entity);
                revision.MetaData.StatusType = revisionStatusType;
                uow.Repositories.Revisions.AddOrUpdate(revision);
                uow.Complete();
            }
        }

        [Test]
        public void Temp_TypedEntity_WithDynamicQuery()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var dynQuery = DynamicMemberMetadata.GetAsPredicate(DynamicAttributeAliasForQuerying + " == @0", "my-new-value");
                var query = uow.Repositories.OfRevisionType("created").Where(dynQuery).Cast<TypedEntity>();

                // Assert
                var item = query.FirstOrDefault();


                Assert.IsNotNull(item);
                Assert.That(item.Id.ProviderId, Is.Not.Null);
                Assert.That(item.Id.ProviderGroupRoot, Is.Not.Null);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void UserGroup_ByName_EqualsOperator()
        {
            // Arrange
            var permission = new Lazy<Permission, PermissionMetadata>(() => new ViewPermission(), new PermissionMetadata(new Dictionary<string, object>()));
            var userGroup = CoreCmsData.RequiredCoreUserGroups(Enumerable.Repeat(permission, 1)).FirstOrDefault(x => x.Name == "Administrator");
            Assert.NotNull(userGroup);

            using (var uow = GroupUnitFactory.Create())
            {
                uow.Repositories.AddOrUpdate(new SystemRoot());
                uow.Repositories.AddOrUpdate(Framework.Security.Model.FixedEntities.UserGroupVirtualRoot);
                uow.Repositories.AddOrUpdate(userGroup);
                uow.Complete();
            }

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var getAdminByName = uow.Repositories.Query<UserGroup>().FirstOrDefault(x => x.Name == "Administrator");
                Assert.NotNull(getAdminByName);
                Assert.That(userGroup.Id.Value, Is.EqualTo(getAdminByName.Id.Value));
            }
        }

        [Test]
        public void WhenEntitiesAreQueried_ResultsArePutInScopedCache()
        {
            // Arrange
            var permission = new Lazy<Permission, PermissionMetadata>(() => new ViewPermission(), new PermissionMetadata(new Dictionary<string, object>()));
            var userGroup = CoreCmsData.RequiredCoreUserGroups(Enumerable.Repeat(permission, 1)).FirstOrDefault(x => x.Name == "Administrator");
            Assert.NotNull(userGroup);
            using (var uow = GroupUnitFactory.Create())
            {
                uow.Repositories.AddOrUpdate(new SystemRoot());
                uow.Repositories.AddOrUpdate(Framework.Security.Model.FixedEntities.UserGroupVirtualRoot);
                uow.Repositories.AddOrUpdate(userGroup);
                uow.Complete();
            }

            // Assert - check single result
            using (var uow = GroupUnitFactory.Create())
            {
                // Cause the task to be fired
                Expression<Func<UserGroup, bool>> expression = x => x.Name == "Administrator";
                var getAdminByName = uow.Repositories.Query<UserGroup>().FirstOrDefault(expression);
                Assert.NotNull(getAdminByName);

                // Generate what should be an exact-same QueryDescription for the above query, to check the cache
                var executor = new Executor(uow.Repositories.QueryableDataSource, Queryable<UserGroup>.GetBinderFromAssembly());
                var queryable = new Queryable<UserGroup>(executor);
                queryable.FirstOrDefault(expression);
                var description = executor.LastGeneratedDescription;

                // Assert the task has been fired
                Assert.That(uow.UnitScopedCache.GetOrCreate(new QueryDescriptionCacheKey(description), () => null), Is.Not.Null);
            }

            // Assert - check many results
            using (var uow = GroupUnitFactory.Create())
            {
                // Cause the task to be fired
                Expression<Func<UserGroup, bool>> expression = x => x.Name == "Administrator";
                var getAdminByName = uow.Repositories.Query<UserGroup>().Where(expression).ToList();
                Assert.NotNull(getAdminByName.FirstOrDefault());

                // Generate what should be an exact-same QueryDescription for the above query, to check the cache
                var executor = new Executor(uow.Repositories.QueryableDataSource, Queryable<UserGroup>.GetBinderFromAssembly());
                var queryable = new Queryable<UserGroup>(executor);
                queryable.Where(expression).ToList();
                var description = executor.LastGeneratedDescription;

                // Assert the task has been fired
                Assert.That(uow.UnitScopedCache.GetOrCreate(new QueryDescriptionCacheKey(description), () => null), Is.Not.Null);
            }
        }

        [Test]
        public void TypedEntity_Published_ByEntitSchemaAlias_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateContentForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().OfRevisionType(FixedStatusTypes.Published).Where(x => x.EntitySchema.Alias == SchemaAliasForQuerying);

                // Assert
                Assert.AreEqual(1, query.Count());
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
                Assert.AreEqual("schema-alias1", item.EntitySchema.Alias);
            }
        }

        [Test]
        public void TypedEntity_ByEntitSchemaAlias_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.EntitySchema.Alias == SchemaAliasForQuerying);

                // Assert
                Assert.AreEqual(1, query.Count());
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
                Assert.AreEqual("schema-alias1", item.EntitySchema.Alias);
            }
        }

        [Test]
        public void UserGroup_ByName_IncludingInferredSchemaType_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            var userGroup = new UserGroup() {Name = "Anonymous", Id = new HiveId(newGuid)};

            using (var uow = ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(new SystemRoot());
                uow.EntityRepository.AddOrUpdate(Framework.Security.Model.FixedEntities.UserGroupVirtualRoot);
                uow.EntityRepository.AddOrUpdate(userGroup);
                uow.Complete();
            }

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var checkSchemaExists = uow.Repositories.Schemas.Get<EntitySchema>(new UserGroupSchema().Id);
                Assert.NotNull(checkSchemaExists);

                var genericQuery = uow.Repositories.QueryContext.Query<TypedEntity>()
                    .Where(x => x.Attribute<string>(NodeNameAttributeDefinition.AliasValue) == "Anonymous")
                    .ToList()
                    .Select(x => x.Id);

                // Assert
                Assert.AreEqual(1, genericQuery.Count());

                var queryAll = uow.Repositories.QueryContext.Query<UserGroup>().ToList();
                Assert.That(queryAll.Count(), Is.EqualTo(1));

                var queryAll2 = uow.Repositories.Query<UserGroup>().ToList();
                Assert.That(queryAll2.Count(), Is.EqualTo(1));

                var query = uow.Repositories.QueryContext.Query<UserGroup>()
                    .Where(x => x.Name == "Anonymous").ToList();

                // Assert
                Assert.AreEqual(1, query.Count());
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
                Assert.AreEqual(new UserGroupSchema().Alias, item.EntitySchema.Alias);
            }
        }

        [Test]
        public void TypedEntity_ById_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Id == (HiveId)newGuid);

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void TypedEntity_ById_EqualsOperator_UsingLinq()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = from x in uow.Repositories.QueryContext.Query()
                            where x.Id == (HiveId)newGuid
                            select x;

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void TypedEntity_ByAttributeValue_NotEqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring");

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuidRedHerring, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void WhenCountQueryHasSkipOrTake_ItemsAreSkipped()
        {
            const int totalExpected = 15;
            for (int i = 0; i < totalExpected; i++)
            {
                var newGuid = Guid.NewGuid();
                var newGuidRedHerring = Guid.NewGuid();

                var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);
            }

            using (var uow = GroupUnitFactory.Create())
            {
                var count = uow.Repositories.Query().Count(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring");
                Assert.That(count, Is.EqualTo(totalExpected));

                count = uow.Repositories.Query().Skip(5).Count(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring");
                Assert.That(count, Is.EqualTo(totalExpected - 5));

                count = uow.Repositories.Query().Skip(5).Take(6).Count(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring");
                Assert.That(count, Is.EqualTo(6));

                count = uow.Repositories.Query().OrderByDescending(x => x.UtcCreated).Take(5).Count();
                Assert.That(count, Is.EqualTo(5));
            }
        }

        

        [Test]
        public void WhenListQueryHasOrder_ResultsAreOrdered()
        {
            const int totalExpected = 15;
            for (int i = 0; i < totalExpected; i++)
            {
                var newGuid = Guid.NewGuid();
                var newGuidRedHerring = Guid.NewGuid();

                var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);
            }

            using (var uow = GroupUnitFactory.Create())
            {
                var dbResults = uow.Repositories
                    .Query()
                    .Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring")
                    .OrderByDescending(x => x.UtcModified)
                    .Take(5)
                    .ToList();

                var inMemResults = uow.Repositories
                    .Query()
                    .Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring")
                    .ToList()
                    .OrderByDescending(x => x.UtcModified)
                    .Take(5)
                    .ToList();

                for (int i = 0; i < dbResults.Count; i++)
                {
                    var item = dbResults[i];
                    var inMemItem = inMemResults[i];
                    Assert.That(inMemItem.Id, Is.EqualTo(item.Id));
                }

                //Ascending
                dbResults = uow.Repositories
                    .Query()
                    .Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring")
                    .OrderBy(x => x.UtcModified)
                    .Take(5)
                    .ToList();

                inMemResults = uow.Repositories
                    .Query()
                    .Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring")
                    .ToList()
                    .OrderBy(x => x.UtcModified)
                    .Take(5)
                    .ToList();

                for (int i = 0; i < dbResults.Count; i++)
                {
                    var item = dbResults[i];
                    var inMemItem = inMemResults[i];
                    Assert.That(inMemItem.Id, Is.EqualTo(item.Id));
                }
            }
        }

        [Test]
        public void WhenListQueryHasSkipOrTake_ItemsAreSkipped()
        {
            const int totalExpected = 15;
            for (int i = 0; i < totalExpected; i++)
            {
                var newGuid = Guid.NewGuid();
                var newGuidRedHerring = Guid.NewGuid();

                var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);
            }

            using (var uow = GroupUnitFactory.Create())
            {
                var count = uow.Repositories.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring");
                Assert.That(count.ToList().Count, Is.EqualTo(totalExpected));

                count = uow.Repositories.Query().Skip(5).Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring");
                Assert.That(count.ToList().Count, Is.EqualTo(totalExpected - 5));

                count = uow.Repositories.Query().Skip(5).Take(6).Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) != "not-on-red-herring");
                Assert.That(count.ToList().Count, Is.EqualTo(6));

                count = uow.Repositories.Query().OrderByDescending(x => x.UtcCreated).Take(5);
                Assert.That(count.ToList().Count, Is.EqualTo(5));
            }
        }

        [Test]
        public void WhenTypedEntity_QueriedWithStringEquals_AndOrderBy_ResultsAreOrdered()
        {
            var item1Id = Guid.NewGuid();
            var item2Id = Guid.NewGuid();
            var item3Id = Guid.NewGuid();
            var parentId = Guid.NewGuid();

            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent exists for this test
            Hive.AutoCommitTo<IContentStore>(x => x.Repositories.Schemas.AddOrUpdate(new ContentRootSchema()));

            // Create schema
            var schema = Hive.Cms().NewContentType<EntitySchema, IContentStore>("withTitle")
                .Define("title", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("random", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("tag", type => type.UseExistingType("singleLineTextBox"), FixedGroupDefinitions.GeneralGroup)
                .Define("bodyText", type => type.UseExistingType("richTextEditor"), FixedGroupDefinitions.GeneralGroup)
                .Commit();

            Assert.True(schema.Success);

            var item1 = new Content();
            item1.SetupFromSchema(schema.Item);
            item1.Id = new HiveId(item1Id);
            item1["title"] = "Item1";
            item1["random"] = "Random3";
            item1["tag"] = "apple";

            var item2 = new Content();
            item2.SetupFromSchema(schema.Item);
            item2.Id = new HiveId(item2Id);
            item2["title"] = "Item2";
            item2["random"] = "Random1";
            item2["tag"] = "blueberry";

            var item3 = new Content();
            item3.SetupFromSchema(schema.Item);
            item3.Id = new HiveId(item3Id);
            item3["title"] = "Item3";
            item3["random"] = "Random2";
            item3["tag"] = "apple";

            var writerResult = Hive.AutoCommitTo<IContentStore>(x =>
                {
                    x.Repositories.AddOrUpdate(item1);
                    x.Repositories.AddOrUpdate(item2);
                    x.Repositories.AddOrUpdate(item3);
                });

            Assert.True(writerResult.WasCommitted);

            // Check can get the items normally
            using (var uow = Hive.OpenReader<IContentStore>())
            {
                Assert.True(uow.Repositories.Exists<Content>(item1.Id));
                Assert.True(uow.Repositories.Exists<Content>(item2.Id));
                Assert.True(uow.Repositories.Exists<Content>(item3.Id));
            }

            // query all with sortorder - first check is actually order of insertion anyway
            var allQuery_NaturalSort = Hive.QueryContent().OrderBy(x => x.Attribute<string>("title")).ToArray();
            Assert.That(allQuery_NaturalSort.Any());
            Assert.That(allQuery_NaturalSort[0]["title"], Is.EqualTo("Item1"));
            Assert.That(allQuery_NaturalSort[1]["title"], Is.EqualTo("Item2"));
            Assert.That(allQuery_NaturalSort[2]["title"], Is.EqualTo("Item3"));

            var allQuerySortByTag = Hive.QueryContent().OrderBy(x => x.Attribute<string>("tag")).ToArray();
            Assert.That(allQuerySortByTag.Any());
            Assert.That(allQuerySortByTag[0]["tag"], Is.EqualTo("apple"));
            Assert.That(allQuerySortByTag[0]["random"], Is.EqualTo("Random3").Or.EqualTo("Random2"));
            Assert.That(allQuerySortByTag[1]["tag"], Is.EqualTo("apple"));
            Assert.That(allQuerySortByTag[1]["random"], Is.EqualTo("Random3").Or.EqualTo("Random2"));
            Assert.That(allQuerySortByTag[2]["tag"], Is.EqualTo("blueberry"));
            Assert.That(allQuerySortByTag[2]["random"], Is.EqualTo("Random1"));

            var allQuerySortByTagThenRandom = Hive.QueryContent().OrderBy(x => x.Attribute<string>("tag")).ThenBy(x => x.Attribute<string>("random")).ToArray();
            Assert.That(allQuerySortByTagThenRandom.Any());
            Assert.That(allQuerySortByTagThenRandom[0]["tag"], Is.EqualTo("apple"));
            Assert.That(allQuerySortByTagThenRandom[0]["random"], Is.EqualTo("Random2"));
            Assert.That(allQuerySortByTagThenRandom[1]["tag"], Is.EqualTo("apple"));
            Assert.That(allQuerySortByTagThenRandom[1]["random"], Is.EqualTo("Random3"));
            Assert.That(allQuerySortByTagThenRandom[2]["tag"], Is.EqualTo("blueberry"));
            Assert.That(allQuerySortByTagThenRandom[2]["random"], Is.EqualTo("Random1"));

            // query invoking the executesingle methods
            var firstByTagDescending = Hive.QueryContent().OrderByDescending(x => x.Attribute<string>("tag")).FirstOrDefault();
            Assert.NotNull(firstByTagDescending);
            Assert.That(firstByTagDescending["tag"], Is.EqualTo("blueberry"));

            var singleByTagDescending = Hive.QueryContent().OrderByDescending(x => x.Attribute<string>("tag")).SingleOrDefault(x => x.Attribute<string>("random") == "Random2");
            Assert.NotNull(singleByTagDescending);
            Assert.That(singleByTagDescending["tag"], Is.EqualTo("apple"));
        }

        [Test]
        public void TypedEntity_ByAttributeValue_Equals_WithBoolean()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);


            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<bool>(HiveModelCreationHelper.BoolDefAlias) == true);

                // Assert
                var items = query.ToList();

                Assert.IsNotNull(items);
                Assert.AreEqual(1, items.Count);

                var firstItem = items.First();
                Assert.AreEqual(newGuid, (Guid)firstItem.Id.Value);

                // Query for the opposite (the red-herring node)
                var query_opposite = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<bool>(HiveModelCreationHelper.BoolDefAlias) == false);

                // Assert again
                var items_opposite = query_opposite.ToList();

                Assert.IsNotNull(items_opposite);
                Assert.AreEqual(1, items_opposite.Count);

                Assert.AreEqual(newGuidRedHerring, (Guid)items_opposite.First().Id.Value);
            }  
        }

        [Test]
        public void TypedEntity_Dynamic_Equals_WithBoolean()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var dynQuery = DynamicMemberMetadata.GetAsPredicate(HiveModelCreationHelper.BoolDefAlias + " == @0", true);
                var query = uow.Repositories.OfRevisionType(FixedStatusTypes.Created).Where(dynQuery).Cast<TypedEntity>();

                // Assert
                var items = query.ToList();

                Assert.IsNotNull(items);
                Assert.AreEqual(1, items.Count);

                var firstItem = items.First();
                Assert.AreEqual(newGuid, (Guid)firstItem.Id.Value);

                // Query for the opposite (the red-herring node)
                var dynQuery_opposite = DynamicMemberMetadata.GetAsPredicate(HiveModelCreationHelper.BoolDefAlias + " == @0", false);
                var query_opposite = uow.Repositories.OfRevisionType(FixedStatusTypes.Created).Where(dynQuery_opposite).Cast<TypedEntity>();

                // Assert again
                var items_opposite = query_opposite.ToList();

                Assert.IsNotNull(items_opposite);
                Assert.AreEqual(1, items_opposite.Count);

                Assert.AreEqual(newGuidRedHerring, (Guid)items_opposite.First().Id.Value);
            }
        }

        [Test]
        public void TypedEntity_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring");

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
            }
        }

        [Test]
        public void TypedEntity_ByAttributeSubValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.InnerAttribute<string>(NodeNameAttributeDefinition.AliasValue, "UrlName") == "my-test-route");

                // Assert
                var item = query.FirstOrDefault();

                Assert.IsNotNull(item);
                Assert.AreEqual(newGuid, (Guid)item.Id.Value);
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
        public void TypedEntity_ComplexBinary_IncludingSchemaAlias()
        {
            SetupContentTest();

            var schema = Hive.Cms().NewContentType("mycontent")
                .Define("text", "singleLineTextBox", "tab1")
                .Commit().Item;

            var schema2 = Hive.Cms().NewContentType("someOtherContent")
                .Define("text", "singleLineTextBox", "tab1")
                .Commit().Item;

            var revision1 = Hive.Cms().NewRevision("shouldreturn", "-", "mycontent")
                .SetValue("text", "some text")
                .Publish()
                .Commit().Item;

            var revision2 = Hive.Cms().NewRevision("shouldnotreturn", "-", "mycontent")
                .SetValue("text", "some text to be excluded")
                .Publish()
                .Commit().Item;

            var revision3 = Hive.Cms().NewRevision("shouldnotreturneither", "-", "someOtherContent")
                .SetValue("text", "some text to be excluded")
                .Publish()
                .Commit().Item;

            Assert.That(schema, Is.Not.Null);
            Assert.That(schema2, Is.Not.Null);
            Assert.That(revision1, Is.Not.Null);
            Assert.That(revision2, Is.Not.Null);
            Assert.That(revision3, Is.Not.Null);

            Assert.That(schema.Id, Is.Not.EqualTo(HiveId.Empty));
            Assert.That(schema2.Id, Is.Not.EqualTo(HiveId.Empty));
            Assert.That(revision1.Id, Is.Not.EqualTo(HiveId.Empty));
            Assert.That(revision2.Id, Is.Not.EqualTo(HiveId.Empty));
            Assert.That(revision3.Id, Is.Not.EqualTo(HiveId.Empty));

            Hive.Context.GenerationScopedCache.Clear();

            // Schema is mycontent, text starts with "some text", but id not equal to revision2
            // Should only return revision1

            var results = Hive.Query<TypedEntity, IContentStore>()
                .Where(
                    x =>
                    x.EntitySchema.Alias == "mycontent" &&
                    (x.Attribute<string>("text").StartsWith("some text") && x.Id != revision2.Id)).ToList();

            Assert.That(results.Count, Is.EqualTo(1));

            Hive.Context.GenerationScopedCache.Clear();

            // Schema is someOtherContent or the above binary combo
            results = Hive.Query<TypedEntity, IContentStore>()
                .Where(
                    x => x.EntitySchema.Alias == "someOtherContent" || (
                    x.EntitySchema.Alias == "mycontent" &&
                    (x.Attribute<string>("text").StartsWith("some text") && x.Id != revision2.Id))).ToList();

            Assert.That(results.Count, Is.EqualTo(2));

        }

        [Test]
        public void TypedEntity_Count_WithAndAlsoBinary_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext
                    .Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring" 
                        && x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                Assert.AreEqual(1, query.Count());
            }

            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext
                    .Query()
                    .InIds(entity.Id)
                    .Where(x => (x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring" && x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1")
                        && x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                Assert.AreEqual(1, query.Count());
            }
        }

        [Test]
        public void TypedEntity_Count_WithOrElseBinary_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = RepositoryGroupExtensions.Create((GroupUnitFactory)GroupUnitFactory))
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring" || x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                Assert.AreEqual(2, query.Count());
            }
        }

        [Test]
        public void TypedEntity_Count_WithComplexBinary_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            var entity = CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => (x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring" || x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1") && x.Id == (HiveId)newGuid);

                // Assert
                Assert.AreEqual(1, query.Count());
            }
        }

        [Test]
        public void TypedEntity_Count_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                // The mocked entities have attributes with alias-1 and my-test-value1, and we've added two of them in SetupTestData
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                Assert.AreEqual(2, query.Count());
            }
        }

        [Test]
        public void UsingDefaultEnumerator_OnEntityRepositoryGroup_TypedEntity_SingleOrDefault_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring");

                // Assert
                var singleOrDefault = query.SingleOrDefault();
                Assert.IsNotNull(singleOrDefault);
                Assert.AreEqual(newGuid, (Guid)singleOrDefault.Id.Value);

                // Now do another query which should return two, and ensure SingleOrDefault chucks an error our way
                var queryToFail = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                try
                {
                    var resultToFail = queryToFail.SingleOrDefault();
                    Assert.Fail("SingleOrDefault did not throw an error; result could should have been 2");
                }
                catch (InvalidOperationException)
                {
                    /* Do nothing */
                }
            }
        }

        [Test]
        public void TypedEntity_SingleOrDefault_ByAttributeValue_EqualsOperator()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();

            CreateEntityForTest(newGuid, newGuidRedHerring, ProviderSetup);

            // Act
            using (var uow = GroupUnitFactory.Create())
            {
                var query = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias2ForQuerying) == "not-on-red-herring");

                // Assert
                var singleOrDefault = query.SingleOrDefault();
                Assert.IsNotNull(singleOrDefault);
                Assert.AreEqual(newGuid, (Guid)singleOrDefault.Id.Value);

                // Now do another query which should return two, and ensure SingleOrDefault chucks an error our way
                var queryToFail = uow.Repositories.QueryContext.Query().Where(x => x.Attribute<string>(AttributeAlias1ForQuerying) == "my-test-value1");

                // Assert
                try
                {
                    var resultToFail = queryToFail.SingleOrDefault();
                    Assert.Fail("SingleOrDefault did not throw an error; result could should have been 2");
                }
                catch (InvalidOperationException)
                {
                    /* Do nothing */
                }
            }
        }

        protected abstract ProviderSetup ProviderSetup { get; }
        protected abstract GroupUnitFactory GroupUnitFactory { get; }

        private IHiveManager _hiveManager;
        protected IHiveManager Hive
        {
            get
            {
                if (_hiveManager != null) return _hiveManager;
                var wildcardUriMatch = new WildcardUriMatch(GroupUnitFactory.IdRoot);
                var abstractProviderBootstrapper = ProviderSetup.Bootstrapper ?? new NoopProviderBootstrapper();
                var providerUnitFactory = new ReadonlyProviderUnitFactory(ProviderSetup.UnitFactory.EntityRepositoryFactory);
                var readonlyProviderSetup = new ReadonlyProviderSetup(providerUnitFactory, ProviderSetup.ProviderMetadata, ProviderSetup.FrameworkContext, abstractProviderBootstrapper, 0);
                var providerMappingGroup = new ProviderMappingGroup("default", wildcardUriMatch, readonlyProviderSetup, ProviderSetup, ProviderSetup.FrameworkContext);
                _hiveManager = new HiveManager(providerMappingGroup, ProviderSetup.FrameworkContext);
                return _hiveManager;
            }
        }

        [TearDown]
        protected virtual void BaseTearDown()
        {
            if (_hiveManager != null)
            {
                _hiveManager.Dispose();
                _hiveManager = null;
            }
        }

        protected virtual TypedEntity CreateEntityForTest(Guid newGuid, Guid newGuidRedHerring, ProviderSetup providerSetup)
        {
            return HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, providerSetup);
        }

        protected virtual TypedEntity CreateContentForTest(Guid newGuid, Guid newGuidRedHerring, ProviderSetup providerSetup)
        {
            return HiveModelCreationHelper.SetupTestContentData(newGuid, newGuidRedHerring, providerSetup);
        }
    }
}