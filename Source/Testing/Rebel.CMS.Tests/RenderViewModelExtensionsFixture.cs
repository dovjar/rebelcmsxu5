using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rebel.Cms.Web;
using Rebel.Cms.Web.DependencyManagement;
using Rebel.Cms.Web.Model;
using Rebel.Cms.Web.Security;
using Rebel.Framework;
using Rebel.Framework.Persistence;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Framework.Persistence.Model.Constants.Schemas;
using Rebel.Framework.Security;
using Rebel.Framework.Security.Configuration;
using Rebel.Framework.Security.Model.Entities;
using Rebel.Hive;
using Rebel.Hive.RepositoryTypes;

namespace Rebel.Tests.Cms
{
    using Rebel.Tests.Extensions;

    [TestFixture]
    public class RenderViewModelExtensionsFixture : AbstractRenderViewModelExtensionsFixture
    {
        [Test]
        public void QueryAll_WithoutCriteria_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var queryAll = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager);
            var toList = queryAll.ToList();

            // Assert
            Assert.That(toList.Count(), Is.GreaterThan(0));
        }

        [Test]
        public void QueryAll_WithEntitySchemaAliasCriteria_StaticLinq_AsTypedEntity()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var firstItem = RenderViewModelQueryExtensions.QueryAll<TypedEntity>(this.HiveManager).FirstOrDefault(x => x.EntitySchema.Alias == "redherring-schema");

            // Assert
            Assert.That(firstItem, Is.Not.Null);
        }

        [Test]
        public void Content_HasSortOrder()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var childGuid = new HiveId(Guid.NewGuid());
            var child2Guid = new HiveId(Guid.NewGuid());
            var parent = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            var child1 = this.AddChildNodeWithId(parent, childGuid, 1);
            var child2 = this.AddChildNodeWithId(parent, child2Guid, 2);

            var child1AsContent = this.HiveManager.FrameworkContext.TypeMappers.Map<Content>(child1);
            var child2AsContent = this.HiveManager.FrameworkContext.TypeMappers.Map<Content>(child2);

            Assert.NotNull(child1AsContent);
            Assert.That(child1AsContent.SortOrder, Is.EqualTo(1));
            Assert.NotNull(child2AsContent);
            Assert.That(child2AsContent.SortOrder, Is.EqualTo(2));
        }

        [Test]
        public void Content_LevelExtensionMethod()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var childGuid = new HiveId(Guid.NewGuid());
            var child1_1Guid = new HiveId(Guid.NewGuid());
            var child1_2Guid = new HiveId(Guid.NewGuid());
            var child1_3Guid = new HiveId(Guid.NewGuid());
            var child1_4Guid = new HiveId(Guid.NewGuid());
            var child1_1_1Guid = new HiveId(Guid.NewGuid());
            var child1_1_2Guid = new HiveId(Guid.NewGuid());
            var child2Guid = new HiveId(Guid.NewGuid());
            var parent = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            var child1 = this.AddChildNodeWithId(parent, childGuid, 1);
            var child1_1 = this.AddChildNodeWithId(child1, child1_1Guid, 1);
            var child1_2 = this.AddChildNodeWithId(child1, child1_2Guid, 2);
            var child1_3 = this.AddChildNodeWithId(child1, child1_3Guid, 3);
            var child1_4 = this.AddChildNodeWithId(child1, child1_4Guid, 4);
            var child1_1_1 = this.AddChildNodeWithId(child1_1, child1_1_1Guid, 1);
            var child1_1_2 = this.AddChildNodeWithId(child1_1, child1_1_2Guid, 1);
            var child2 = this.AddChildNodeWithId(parent, child2Guid, 2);

            var child1AsContent = this.HiveManager.FrameworkContext.TypeMappers.Map<Content>(child1);
            var child1_1_2AsContent = this.HiveManager.FrameworkContext.TypeMappers.Map<Content>(child1_1_2);

            var child1Path = child1AsContent.GetPath(this.HiveManager);
            Assert.That(child1Path.Count(), Is.EqualTo(2), "Path was: " + child1Path.ToString() + ", parent id is: " + parent.Id);
            Assert.That(child1Path.Level, Is.EqualTo(2));
            var child1_1_2Path = child1_1_2AsContent.GetPath(this.HiveManager);
            Assert.That(child1_1_2Path.Count(), Is.EqualTo(4), "Path was: " + child1_1_2Path.ToString() + ", parent id is: " + parent.Id);
        }

        [Test]
        [Ignore("Change test - Requires ISecurityService to build")]
        public void ParentOfT_AsTypedEntity()
        {
            //// Arrange
            //var newGuid = Guid.Parse("55598660-7AAF-4F16-89E5-1B21CE10139C");
            //var newGuidRedHerring = Guid.NewGuid();
            //var childGuid = new HiveId(Guid.Parse("4554D9CD-27B5-4E7B-BE5A-E077B2B3908A"));
            //var parent = HiveModelCreationHelper.SetupTestContentData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            //var child = this.AddChildNodeWithId(parent, childGuid);

            //// Act
            //var getParent = RenderViewModelExtensions.Parent<TypedEntity>(this.HiveManager, child.Id);
            
            //// Assert
            //Assert.That(getParent, Is.Not.Null);
        }

        [Test]
        [Ignore("Change test - Requires ISecurityService to build")]
        public void ParentOfT_AsContent()
        {
            //// Arrange
            //var newGuid = Guid.NewGuid();
            //var newGuidRedHerring = Guid.NewGuid();
            //var childGuid = new HiveId(Guid.NewGuid());
            //var parent = HiveModelCreationHelper.SetupTestContentData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            //var child = this.AddChildNodeWithId(parent, childGuid);

            //// Act
            //var getParent = RenderViewModelExtensions.Parent<Content>(this.HiveManager, child.Id);

            //// Assert
            //Assert.That(getParent, Is.Not.Null);
        }

        [Test]
        public void AncestorsOfT_AsContent()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var childGuid = new HiveId(Guid.NewGuid());
            var grandChildGuid = new HiveId(Guid.NewGuid());
            var parent = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);
            var child = this.AddChildNodeWithId(parent, childGuid);
            var grandChild = this.AddChildNodeWithId(child, grandChildGuid);

            // Act
            var getParent = RenderViewModelExtensions.GetAncestors<Content>(this.HiveManager, grandChild.Id);

            // Assert
            Assert.That(getParent, Is.Not.Null);
            Assert.That(getParent.Any(), Is.EqualTo(true));
            Assert.That(getParent.Count(), Is.EqualTo(2));
            Assert.That(getParent.First().Id, Is.EqualTo(child.Id));
            Assert.That(getParent.Skip(1).First().Id, Is.EqualTo(parent.Id));
        }

        [Test]
        public void QueryAll_WithoutCriteria_AsContent()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var queryAll = RenderViewModelQueryExtensions.QueryAll<Content>(this.HiveManager);
            var toList = queryAll.ToList();

            // Assert
            Assert.That(toList.Count(), Is.GreaterThan(0));
        }

        [Test]
        public void Field_WithComplexValue_CanAccessSpecificKey()
        {
            // Arrange
            var newGuid = Guid.NewGuid();
            var newGuidRedHerring = Guid.NewGuid();
            var entity = HiveModelCreationHelper.SetupTestData(newGuid, newGuidRedHerring, this.Setup.ProviderSetup);

            // Act
            var value2 = entity.Field<string>(NodeNameAttributeDefinition.AliasValue, "Name");
            var value3 = entity.Field<string>(NodeNameAttributeDefinition.AliasValue, "UrlName");

            // Assert
            Assert.That(value2 == "my-test-name");
            Assert.That(value3 == "my-test-route");
        }

        [Test]
        public void Can_Iterate_Over_Multi_Value_Property()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            SetupContentTest();

            // Make a new content type with a listBox
            var doctype = HiveManager.Cms<IContentStore>()
                .NewContentType("newsItem")     
                .Define("newsTypes", "listBox", "tab1")
                .Commit();
            // Make a new revision without specifying properties
            var vals = new[] {"option1", "option2"};
            var firstRev = HiveManager.Cms().NewRevision("article", "article", "newsItem")
                .SetValue("newsTypes", vals[0], "val0") //select option1
                .SetValue("newsTypes", vals[1], "val1") //select option2
                .Publish()
                .Commit();
            
            var bent = new Content(firstRev.Item).Bend(HiveManager, MembershipService, PublicAccessService);

            Assert.NotNull(bent);
            Assert.AreEqual(2, Enumerable.Count(bent.newsTypes));
            for (var i = 0; i < Enumerable.Count(bent.newsTypes); i++)
            {   
                //we don't know the order that the values get saved, so we'll just check with contains
                //Assert.Contains(vals[i], bent.newsTypes);
                var val = Enumerable.ElementAt(bent.newsTypes, i).Value;
                Assert.That(vals.Any(x => val == x));                
            }
        }

        private void SetupContentTest()
        {
            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            // Ensure parent schema and content root exists for this test
            var contentVirtualRoot = FixedEntities.ContentVirtualRoot;
            var systemRoot = new SystemRoot();
            var contentRootSchema = new ContentRootSchema();
            HiveManager.AutoCommitTo<IContentStore>(
                x =>
                {
                    x.Repositories.AddOrUpdate(systemRoot);
                    x.Repositories.AddOrUpdate(contentVirtualRoot);
                    x.Repositories.Schemas.AddOrUpdate(contentRootSchema);
                });
        }
    }
}
