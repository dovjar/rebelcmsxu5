using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rebel.Cms.Web;
using Rebel.Cms.Web.Model;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model.Constants;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.AttributeTypes;
using Rebel.Hive;
using Rebel.Hive.Configuration;
using Rebel.Hive.RepositoryTypes;
using Rebel.Tests.Extensions;

namespace Rebel.Tests.Cms
{
    using Rebel.Cms.Web.FluentExtensions;
    using Rebel.Framework.Persistence.Model;

    [TestFixture]
    public class CmsQueryTests
    {
        protected IHiveManager Hive;

        #region SetUp / TearDown

        private NhibernateTestSetupHelper _nhibernateTestSetup;
        private DateTime _fixedDate;
        private IContentRevisionBuilderStep<TypedEntity, IContentStore> _homePage;

        [SetUp]
        public void Setup()
        {
            // Setup Hive
            _nhibernateTestSetup = new NhibernateTestSetupHelper(useNhProf: true);

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

            // Setup Schemas
            Hive.Cms()
                .UsingStore<IContentStore>()
                .NewSchema("homepageSchema")
                .Define(NodeNameAttributeDefinition.AliasValue, NodeNameAttributeType.AliasValue, "tab1")
                .Define(SelectedTemplateAttributeDefinition.AliasValue, SelectedTemplateAttributeType.AliasValue, "tab1")
                .Define("siteName", "system-string-type", "tab1")
                .Define("siteDescription", "system-string-type", "tab1")
                .Define("bodyText", "system-long-string-type", "tab2")
                .Commit();

            Hive.Cms()
                .UsingStore<IContentStore>()
                .NewSchema("textpageSchema")
                .Define(NodeNameAttributeDefinition.AliasValue, NodeNameAttributeType.AliasValue, "tab1")
                .Define(SelectedTemplateAttributeDefinition.AliasValue, SelectedTemplateAttributeType.AliasValue, "tab1")
                .Define("bodyText", "system-long-string-type", "tab1")
                .Commit();

            Hive.Cms()
                .UsingStore<IContentStore>()
                .NewSchema("complexpageSchema")
                .Define(NodeNameAttributeDefinition.AliasValue, NodeNameAttributeType.AliasValue, "tab1")
                .Define(SelectedTemplateAttributeDefinition.AliasValue, SelectedTemplateAttributeType.AliasValue, "tab1")
                .Define("color", "system-string-type", "tab1")
                .Define("integer", "system-integer-type", "tab1")
                .Define("date", "system-date-time-type", "tab1")
                .Define("bodyText", "system-long-string-type", "tab1")
                .Commit();

            // Setup Content with a specific DateTime
            _fixedDate = DateTime.Now.Subtract(TimeSpan.FromMinutes(5));
            _homePage = Hive.Cms().NewRevision("Home", "home", "homepageSchema")
                .SetSelectedTemplate(new HiveId("storage", "templates", new HiveIdValue("/homepageTemplate.cshtml")))
                .SetValue("siteName", "Test Site Name")
                .SetValue("siteDescription", "Test Site Description")
                .SetValue("bodyText", "<p>Test Site Body Text</p>")
                .Publish();

            _homePage.Revision.Item.UtcCreated = _fixedDate;
            _homePage.Revision.MetaData.UtcStatusChanged = _fixedDate;
            _homePage.Commit();

            for (var i = 0; i < 5; i++)
            {
                // Create child
                var child = Hive.Cms().NewRevision("Child" + i, "child-" + i, "textpageSchema")
                    .SetSelectedTemplate(new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml")))
                    .SetParent(_homePage.Item)
                    .SetValue("bodyText",
                              "<p>Child Content " + i + " Revision 1 " + Guid.NewGuid().ToString() + " " +
                              Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " +
                              Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                    .Publish()
                    .Commit();

                // Create revisions
                //for (var j = 0; j < i; j++)
                //{
                //    Hive.Cms().NewRevisionOf(child.Item)
                //        .SetValue("bodyText", "<p>Child Content " + i + " Revision " + (j + 2) + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                //        .Publish()
                //        .Commit();

                //}

                //if (i < 3)
                //{
                //    for (var j = 0; j < 6; j++)
                //    {
                //        // Create grand child
                //        var grandChild = Hive.Cms().NewRevision("Grand Child" + j, "grand-child-" + j, "textpageSchema")
                //            .SetParent(child.Item)
                //            .SetValue("bodyText", "<p>Grand Child Content " + j + " Revision 1 " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                //            .Publish()
                //            .Commit();

                //        // Create revisions
                //        for (var k = 0; k < j; k++)
                //        {
                //            Hive.Cms().NewRevisionOf(grandChild.Item)
                //                .SetValue("bodyText", "<p>Grand Child Content " + j + " Revision " + (k + 2) + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                //                .Publish()
                //                .Commit();

                //        }

                //        if (j < 2)
                //        {
                //            // Create great grand children
                //            for (var k = 0; k < 10; k++)
                //            {
                //                var greatGrandChild =
                //                    Hive.Cms().NewRevision("Great Grand Child" + k, "great-grand-child-" + k,
                //                                           "complexpageSchema")
                //                        .SetParent(grandChild.Item)
                //                        .SetValue("color", "#0000" + k.ToString() + k.ToString())
                //                        .SetValue("integer", k)
                //                        .SetValue("date", DateTimeOffset.Now.AddDays(k))
                //                        .SetValue("bodyText",
                //                                  "<p>Great Grand Child Content " + k + " Revision 1 " +
                //                                  Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " +
                //                                  Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " +
                //                                  Guid.NewGuid().ToString() + " the end.</p>")
                //                        .Publish()
                //                        .Commit();

                //                // Create revisions
                //                for (var l = 0; l < k; l++)
                //                {
                //                    Hive.Cms().NewRevisionOf(greatGrandChild.Item)
                //                        .SetValue("bodyText", "<p>Great Grand Child Content " + k + " Revision " + (l + 2) + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                //                        .Publish()
                //                        .Commit();

                //                }

                //                if (k >= 8)
                //                {
                //                    // Create unpublished revisions
                //                    Hive.Cms().NewRevisionOf(greatGrandChild.Item)
                //                            .SetValue("bodyText", "<p>Great Grand Child Content " + k + " Unpublished Revision " + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                //                            .Commit();
                //                }
                //            }
                //        }
                //    }
                //}
            }

            //for(var i = 0; i < 2; i++)
            //{
            //    var trashChild = Hive.Cms().NewRevision("Trash Child" + i, "trash-child-" + i, "textpageSchema")
            //        .SetParent(FixedHiveIds.ContentRecylceBin)
            //        .SetValue("bodyText", "<p>Trash Child Content " + i + " Revision 1 " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
            //        .Publish()
            //        .Commit();
            //}
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

        #endregion

        [Test]
        public void StringQueryByName()
        {
            // Act
            var whereNodes = Hive.Cms().Content.Where<Content>("Name == @0", "Child0");
            var singleNode = Hive.Cms().Content.Single<Content>("Name == @0", "Child1");
            var singleDefaultNode = Hive.Cms().Content.SingleOrDefault<Content>("Name == @0", "Child2");
            var firstNode = Hive.Cms().Content.First<Content>("Name == @0", "Child3");
            var firstDefaultNode = Hive.Cms().Content.FirstOrDefault<Content>("Name == @0", "Child4");

            // Assert
            Assert.AreEqual(1, whereNodes.Count());
            Assert.AreEqual("Child0", whereNodes.First().Name);
            Assert.IsNotNull(singleNode);
            Assert.AreEqual("Child1", singleNode.Name);
            Assert.IsNotNull(singleDefaultNode);
            Assert.AreEqual("Child2", singleDefaultNode.Name);
            Assert.IsNotNull(firstNode);
            Assert.AreEqual("Child3", firstNode.Name);
            Assert.IsNotNull(firstDefaultNode);
            Assert.AreEqual("Child4", firstDefaultNode.Name);
        }

        [Test]
        public void ExpressionQueryByName()
        {
            // Act
            var whereNodes = Hive.Cms().Content.Where<Content>(x => x.Name == "Child0");
            var singleNode = Hive.Cms().Content.Single<Content>(x => x.Name == "Child1");
            var singleDefaultNode = Hive.Cms().Content.SingleOrDefault<Content>(x => x.Name == "Child2");
            var firstNode = Hive.Cms().Content.First<Content>(x => x.Name == "Child3");
            var firstDefaultNode = Hive.Cms().Content.FirstOrDefault<Content>(x => x.Name == "Child4");

            // Assert
            Assert.AreEqual(1, whereNodes.Count());
            Assert.AreEqual("Child0", whereNodes.First().Name);
            Assert.IsNotNull(singleNode);
            Assert.AreEqual("Child1", singleNode.Name);
            Assert.IsNotNull(singleDefaultNode);
            Assert.AreEqual("Child2", singleDefaultNode.Name);
            Assert.IsNotNull(firstNode);
            Assert.AreEqual("Child3", firstNode.Name);
            Assert.IsNotNull(firstDefaultNode);
            Assert.AreEqual("Child4", firstDefaultNode.Name);
        }

        [Test]
        public void StringQueryByUrlName()
        {
            // Act
            var whereNodes = Hive.Cms().Content.Where<Content>("UrlName == @0", "child-0");
            var singleNode = Hive.Cms().Content.Single<Content>("UrlName == @0", "child-1");
            var singleDefaultNode = Hive.Cms().Content.SingleOrDefault<Content>("UrlName == @0", "child-2");
            var firstNode = Hive.Cms().Content.First<Content>("UrlName == @0", "child-3");
            var firstDefaultNode = Hive.Cms().Content.FirstOrDefault<Content>("UrlName == @0", "child-4");

            // Assert
            Assert.AreEqual(1, whereNodes.Count());
            Assert.AreEqual("child-0", whereNodes.First().UrlName);
            Assert.IsNotNull(singleNode);
            Assert.AreEqual("child-1", singleNode.UrlName);
            Assert.IsNotNull(singleDefaultNode);
            Assert.AreEqual("child-2", singleDefaultNode.UrlName);
            Assert.IsNotNull(firstNode);
            Assert.AreEqual("child-3", firstNode.UrlName);
            Assert.IsNotNull(firstDefaultNode);
            Assert.AreEqual("child-4", firstDefaultNode.UrlName);
        }

        [Test]
        public void ExpressionQueryByUrlName()
        {
            // Act
            var whereNodes = Hive.Cms().Content.Where<Content>(x => x.UrlName == "child-0");
            var singleNode = Hive.Cms().Content.Single<Content>(x => x.UrlName == "child-1");
            var singleDefaultNode = Hive.Cms().Content.SingleOrDefault<Content>(x => x.UrlName == "child-2");
            var firstNode = Hive.Cms().Content.First<Content>(x => x.UrlName == "child-3");
            var firstDefaultNode = Hive.Cms().Content.FirstOrDefault<Content>(x => x.UrlName == "child-4");

            // Assert
            Assert.AreEqual(1, whereNodes.Count());
            Assert.AreEqual("child-0", whereNodes.First().UrlName);
            Assert.IsNotNull(singleNode);
            Assert.AreEqual("child-1", singleNode.UrlName);
            Assert.IsNotNull(singleDefaultNode);
            Assert.AreEqual("child-2", singleDefaultNode.UrlName);
            Assert.IsNotNull(firstNode);
            Assert.AreEqual("child-3", firstNode.UrlName);
            Assert.IsNotNull(firstDefaultNode);
            Assert.AreEqual("child-4", firstDefaultNode.UrlName);
        }

        [Test]
        public void StringQueryTemplate()
        {
            // Act
            var whereNodes = Hive.Cms().Content.Where<Content>("Template == @0", "textpageTemplate");
            var singleNode = Hive.Cms().Content.Single<Content>("Template == @0", "homepageTemplate");
            var singleDefaultNode = Hive.Cms().Content.SingleOrDefault<Content>("Template == @0", "homepageTemplate");
            var firstNode = Hive.Cms().Content.First<Content>("Template == @0", "textpageTemplate");
            var firstDefaultNode = Hive.Cms().Content.FirstOrDefault<Content>("Template == @0", "textpageTemplate");

            // Assert
            Assert.AreEqual(5, whereNodes.Count());
            Assert.IsNotNull(singleNode);
            Assert.AreEqual("Home", singleNode.Name);
            Assert.IsNotNull(singleDefaultNode);
            Assert.AreEqual("Home", singleDefaultNode.Name);
            Assert.IsNotNull(firstNode);
            //Assert.AreEqual("Child0", firstNode.Name);
            Assert.IsNotNull(firstDefaultNode);
            //Assert.AreEqual("Child0", firstDefaultNode.Name);

            //NB: Have commented out the specific node checks, as can't guarantee the order results will be returned
        }

        [Test]
        public void ExpressionQueryTemplate()
        {
            // Act
            var whereNodes = Hive.Cms().Content.Where<Content>(x => x.CurrentTemplate.Alias == "textpageTemplate");
            var singleNode = Hive.Cms().Content.Single<Content>(x => x.CurrentTemplate.Alias == "homepageTemplate");
            var singleDefaultNode =
                Hive.Cms().Content.SingleOrDefault<Content>(x => x.CurrentTemplate.Alias == "homepageTemplate");
            var firstNode = Hive.Cms().Content.First<Content>(x => x.CurrentTemplate.Alias == "textpageTemplate");
            var firstDefaultNode =
                Hive.Cms().Content.FirstOrDefault<Content>(x => x.CurrentTemplate.Alias == "textpageTemplate");

            // Assert
            Assert.AreEqual(5, whereNodes.Count());
            Assert.IsNotNull(singleNode);
            Assert.AreEqual("Home", singleNode.Name);
            Assert.IsNotNull(singleDefaultNode);
            Assert.AreEqual("Home", singleDefaultNode.Name);
            Assert.IsNotNull(firstNode);
            //Assert.AreEqual("Child0", firstNode.Name);
            Assert.IsNotNull(firstDefaultNode);
            //Assert.AreEqual("Child0", firstDefaultNode.Name);

            //NB: Have commented out the specific node checks, as can't guarantee the order results will be returned
        }

        [Test]
        public void StringQueryTemplateId()
        {
            var homepageTemplateId = new HiveId("storage", "templates", new HiveIdValue("/homepageTemplate.cshtml"));
            var textpageTemplateId = new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml"));

            // Act
            var whereNodes = Hive.Cms().Content.Where<Content>("TemplateId == @0", textpageTemplateId);
            var singleNode = Hive.Cms().Content.Single<Content>("TemplateId == @0", homepageTemplateId);
            var singleDefaultNode = Hive.Cms().Content.SingleOrDefault<Content>("TemplateId == @0", homepageTemplateId);
            var firstNode = Hive.Cms().Content.First<Content>("TemplateId == @0", textpageTemplateId);
            var firstDefaultNode = Hive.Cms().Content.FirstOrDefault<Content>("TemplateId == @0", textpageTemplateId);

            // Assert
            Assert.AreEqual(5, whereNodes.Count());
            Assert.IsNotNull(singleNode);
            Assert.AreEqual("Home", singleNode.Name);
            Assert.IsNotNull(singleDefaultNode);
            Assert.AreEqual("Home", singleDefaultNode.Name);
            Assert.IsNotNull(firstNode);
            //Assert.AreEqual("Child0", firstNode.Name);
            Assert.IsNotNull(firstDefaultNode);
            //Assert.AreEqual("Child0", firstDefaultNode.Name);

            //NB: Have commented out the specific node checks, as can't guarantee the order results will be returned
        }

        [Test]
        public void ExpressionQueryTemplateId()
        {
            var homepageTemplateId = new HiveId("storage", "templates", new HiveIdValue("/homepageTemplate.cshtml"));
            var textpageTemplateId = new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml"));

            // Act
            var whereNodes = Hive.Cms().Content.Where<Content>(x => x.CurrentTemplate.Id == textpageTemplateId);
            var singleNode = Hive.Cms().Content.Single<Content>(x => x.CurrentTemplate.Id == homepageTemplateId);
            var singleDefaultNode =
                Hive.Cms().Content.SingleOrDefault<Content>(x => x.CurrentTemplate.Id == homepageTemplateId);
            var firstNode = Hive.Cms().Content.First<Content>(x => x.CurrentTemplate.Id == textpageTemplateId);
            var firstDefaultNode =
                Hive.Cms().Content.FirstOrDefault<Content>(x => x.CurrentTemplate.Id == textpageTemplateId);

            // Assert
            Assert.AreEqual(5, whereNodes.Count());
            Assert.IsNotNull(singleNode);
            Assert.AreEqual("Home", singleNode.Name);
            Assert.IsNotNull(singleDefaultNode);
            Assert.AreEqual("Home", singleDefaultNode.Name);
            Assert.IsNotNull(firstNode);
            //Assert.AreEqual("Child0", firstNode.Name);
            Assert.IsNotNull(firstDefaultNode);
            //Assert.AreEqual("Child0", firstDefaultNode.Name);

            //NB: Have commented out the specific node checks, as can't guarantee the order results will be returned
        }

        [Test]
        public void ExpressionQueryCreateDate()
        {
            var whereCreateLe = Hive.Cms().Content.Where(x => x.UtcCreated <= DateTimeOffset.UtcNow);
            var whereCreateLt = Hive.Cms().Content.Where(x => x.UtcCreated < DateTimeOffset.UtcNow);
            var whereCreateEq = Hive.Cms().Content.Where(x => x.UtcCreated == _fixedDate);
            var whereCreateGt = Hive.Cms().Content.Where(x => x.UtcCreated > _fixedDate);
            var whereCreateGte = Hive.Cms().Content.Where(x => x.UtcCreated >= _fixedDate);

            // Assert based on executing "ExecuteScalar"
            Assert.AreEqual(6, whereCreateLe.Count());
            Assert.AreEqual(6, whereCreateLt.Count());
            Assert.AreEqual(1, whereCreateEq.Count());
            Assert.AreEqual(5, whereCreateGt.Count());
            Assert.AreEqual(6, whereCreateGte.Count());

            // Assert based on exeucting "ExecuteMany"
            var listLe = whereCreateLe.ToList();
            var listLt = whereCreateLt.ToList();
            var listEq = whereCreateEq.ToList();
            var listGt = whereCreateGt.ToList();
            var listGte = whereCreateGte.ToList();

            Assert.AreEqual(6, listLe.Count());
            Assert.AreEqual(6, listLt.Count());
            Assert.AreEqual(1, listEq.Count());
            Assert.AreEqual(5, listGt.Count());
            Assert.AreEqual(6, listGte.Count());
        }

        [Test]
        public void StringQueryCreateDate()
        {
            // Arrange
            var whereCreateLe = Hive.Cms().Content.Where<Content>("UtcCreated <= @0", DateTimeOffset.UtcNow);
            var whereCreateLt = Hive.Cms().Content.Where<Content>("UtcCreated < @0", DateTimeOffset.UtcNow);
            var whereCreateEq = Hive.Cms().Content.Where<Content>("UtcCreated == @0", _fixedDate);
            var whereCreateGt = Hive.Cms().Content.Where<Content>("UtcCreated > @0", _fixedDate);
            var whereCreateGte = Hive.Cms().Content.Where<Content>("UtcCreated >= @0", _fixedDate);

            // Assert based on executing "ExecuteScalar"
            Assert.AreEqual(6, whereCreateLe.Count());
            Assert.AreEqual(6, whereCreateLt.Count());
            Assert.AreEqual(1, whereCreateEq.Count());
            Assert.AreEqual(5, whereCreateGt.Count());
            Assert.AreEqual(6, whereCreateGte.Count());

            // Assert based on exeucting "ExecuteMany"
            var listLe = whereCreateLe.ToList();
            var listLt = whereCreateLt.ToList();
            var listEq = whereCreateEq.ToList();
            var listGt = whereCreateGt.ToList();
            var listGte = whereCreateGte.ToList();

            Assert.AreEqual(6, listLe.Count());
            Assert.AreEqual(6, listLt.Count());
            Assert.AreEqual(1, listEq.Count());
            Assert.AreEqual(5, listGt.Count());
            Assert.AreEqual(6, listGte.Count());
        }

        [Test]
        public void ExpressionQueryModifiedDate()
        {
            var whereCreateLe = Hive.Cms().Content.Where(x => x.UtcModified <= DateTimeOffset.UtcNow);
            var whereCreateLt = Hive.Cms().Content.Where(x => x.UtcModified < DateTimeOffset.UtcNow);
            var whereCreateEq = Hive.Cms().Content.Where(x => x.UtcModified == _fixedDate);
            var whereCreateGt = Hive.Cms().Content.Where(x => x.UtcModified > _fixedDate);
            var whereCreateGte = Hive.Cms().Content.Where(x => x.UtcModified >= _fixedDate);

            // Assert based on executing "ExecuteScalar"
            Assert.AreEqual(6, whereCreateLe.Count());
            Assert.AreEqual(6, whereCreateLt.Count());
            Assert.AreEqual(1, whereCreateEq.Count());
            Assert.AreEqual(5, whereCreateGt.Count());
            Assert.AreEqual(6, whereCreateGte.Count());

            // Assert based on exeucting "ExecuteMany"
            var listLe = whereCreateLe.ToList();
            var listLt = whereCreateLt.ToList();
            var listEq = whereCreateEq.ToList();
            var listGt = whereCreateGt.ToList();
            var listGte = whereCreateGte.ToList();

            Assert.AreEqual(6, listLe.Count());
            Assert.AreEqual(6, listLt.Count());
            Assert.AreEqual(1, listEq.Count());
            Assert.AreEqual(5, listGt.Count());
            Assert.AreEqual(6, listGte.Count());
        }

        [Test]
        public void StringQueryModifiedDate()
        {
            // Arrange
            var whereCreateLe = Hive.Cms().Content.Where<Content>("UtcModified <= @0", DateTimeOffset.UtcNow);
            var whereCreateLt = Hive.Cms().Content.Where<Content>("UtcModified < @0", DateTimeOffset.UtcNow);
            var whereCreateEq = Hive.Cms().Content.Where<Content>("UtcModified == @0", _fixedDate);
            var whereCreateGt = Hive.Cms().Content.Where<Content>("UtcModified > @0", _fixedDate);
            var whereCreateGte = Hive.Cms().Content.Where<Content>("UtcModified >= @0", _fixedDate);

            // Assert based on executing "ExecuteScalar"
            Assert.AreEqual(6, whereCreateLe.Count());
            Assert.AreEqual(6, whereCreateLt.Count());
            Assert.AreEqual(1, whereCreateEq.Count());
            Assert.AreEqual(5, whereCreateGt.Count());
            Assert.AreEqual(6, whereCreateGte.Count());

            // Assert based on exeucting "ExecuteMany"
            var listLe = whereCreateLe.ToList();
            var listLt = whereCreateLt.ToList();
            var listEq = whereCreateEq.ToList();
            var listGt = whereCreateGt.ToList();
            var listGte = whereCreateGte.ToList();

            Assert.AreEqual(6, listLe.Count());
            Assert.AreEqual(6, listLt.Count());
            Assert.AreEqual(1, listEq.Count());
            Assert.AreEqual(5, listGt.Count());
            Assert.AreEqual(6, listGte.Count());
        }

        [Test]
        public void GivenPaginationResultsArePaged()
        {
            var usingSkipTake = Hive.Cms().Content.OrderBy(x => x.UtcCreated).Skip(4).Take(2).ToList();
            var usingPaged = Hive.Cms().Content.OrderBy(x => x.UtcCreated).Paged(3, 2).ToList();

            Assert.That(usingSkipTake.Count, Is.EqualTo(2));
            Assert.That(usingPaged.Count, Is.EqualTo(2));
            CollectionAssert.AreEquivalent(usingSkipTake, usingPaged);
        }

        [Test]
        [Ignore("Waiting on impl and config options, uncomment QueryableDataSourceWrapper line 108 when ready to test")]
        public void GivenNoCriteriaOrSkipTakeCausingEnumerationThrowsException()
        {
            Assert.Throws<PaginationRequiredException>(() => Hive.Cms().Content.OrderBy(x => x.UtcCreated).ToList());
        }

        [Test]
        public void ExpressionQueryOrderByModifiedDateDescending()
        {
            var whereAscending = Hive.Cms().Content.OrderBy(x => x.UtcCreated);
            var whereDescending = Hive.Cms().Content.OrderByDescending(x => x.UtcCreated);

            var firstAscending = whereAscending.First();
            var lastAscending = whereAscending.Last();

            var firstDescending = whereDescending.First();
            var lastDescending = whereDescending.Last();

            Assert.AreEqual(firstDescending.UtcCreated, lastAscending.UtcCreated);
            Assert.AreEqual(lastDescending.UtcCreated, firstAscending.UtcCreated);
        }

        [Test]
        public void QueryTakeSkip()
        {
            var allResults = Hive.Cms().Content.OrderBy(x => x.UtcCreated).ToList();
            var filteredItems = Hive.Cms().Content.OrderBy(x => x.UtcCreated).Skip(2).Take(2).ToList();

            Assert.That(allResults.Count, Is.GreaterThan(4));
            Assert.That(filteredItems.Count, Is.EqualTo(2));

            Assert.AreEqual(allResults.ElementAt(2).Id, filteredItems.ElementAt(0).Id);
        }

        [Test]
        public void ExcludeIds()
        {
            var childOfHome = Hive.Cms().NewRevision("Child of home", "child-of-home", "textpageSchema")
                    .SetSelectedTemplate(new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml")))
                    .SetParent(_homePage.Item)
                    .SetValue("bodyText",
                              "<p>Child Content Revision 1 " + Guid.NewGuid().ToString() + " " +
                              Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " +
                              Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                    .Publish()
                    .Commit();

            var textpageItems = Hive.Cms().Content.Where(x => x.EntitySchema.Alias == "textpageSchema");
            var textpageItemsExcluding = Hive.Cms().Content.ExcludeIds(childOfHome.Item.Id).Where(x => x.EntitySchema.Alias == "textpageSchema");

            Assert.That(textpageItems.Any(), Is.True);
            Assert.That(textpageItems.Count(), Is.EqualTo(6));
            Assert.That(textpageItems.ToList().Count, Is.EqualTo(6));

            Assert.That(textpageItemsExcluding.Any(), Is.True);
            Assert.That(textpageItemsExcluding.Count(), Is.EqualTo(5));
            Assert.That(textpageItemsExcluding.ToList().Count, Is.EqualTo(5));
        }

        [Test]
        public void WithParentIds()
        {
            var childrenOfHomepage = Hive.Cms().Content.WithParentIds(_homePage.Item.Id);
            var childrenOfHomepageWithSchema = Hive.Cms().Content
                .Where(x => x.EntitySchema.Alias == "textpageSchema")
                .WithParentIds(_homePage.Item.Id)
                .OrderByDescending(x => x.UtcCreated);

            Assert.That(childrenOfHomepage.Any(), Is.True);
            Assert.That(childrenOfHomepage.Count(), Is.EqualTo(5));
            Assert.That(childrenOfHomepage.ToList().Count, Is.EqualTo(5));

            Assert.That(childrenOfHomepageWithSchema.Any(), Is.True);
            Assert.That(childrenOfHomepageWithSchema.Count(), Is.EqualTo(5));
            Assert.That(childrenOfHomepageWithSchema.ToList().Count, Is.EqualTo(5));
        }

        [Test]
        public void ExcludeParentIds()
        {
            var childOfHome = Hive.Cms().NewRevision("Child of home", "child-of-home", "textpageSchema")
                    .SetSelectedTemplate(new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml")))
                    .SetParent(_homePage.Item)
                    .SetValue("bodyText",
                              "<p>Child Content Revision 1 " + Guid.NewGuid().ToString() + " " +
                              Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " +
                              Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                    .Publish()
                    .Commit();

            for (var i = 0; i < 10; i++)
            {
                // Create child
                var child = Hive.Cms().NewRevision("Child" + i, "child-" + i, "textpageSchema")
                    .SetSelectedTemplate(new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml")))
                    .SetParent(childOfHome.Item)
                    .SetValue("bodyText",
                              "<p>Child Content " + i + " Revision 1 " + Guid.NewGuid().ToString() + " " +
                              Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " " +
                              Guid.NewGuid().ToString() + " " + Guid.NewGuid().ToString() + " the end.</p>")
                    .Publish()
                    .Commit();
            }

            var textpageItems = Hive.Cms().Content.Where(x => x.EntitySchema.Alias == "textpageSchema");
            var textpageItemsBelowHome = Hive.Cms().Content.WithParentIds(_homePage.Item.Id).Where(x => x.EntitySchema.Alias == "textpageSchema");
            var textpageItemsExcludingHome = Hive.Cms().Content.ExcludeParentIds(_homePage.Item.Id).Where(x => x.EntitySchema.Alias == "textpageSchema");

            Assert.That(textpageItems.Any(), Is.True);
            Assert.That(textpageItems.Count(), Is.EqualTo(16));
            Assert.That(textpageItems.ToList().Count, Is.EqualTo(16));

            Assert.That(textpageItemsBelowHome.Any(), Is.True);
            Assert.That(textpageItemsBelowHome.Count(), Is.EqualTo(6));
            Assert.That(textpageItemsBelowHome.ToList().Count, Is.EqualTo(6));

            Assert.That(textpageItemsExcludingHome.Any(), Is.True);
            Assert.That(textpageItemsExcludingHome.Count(), Is.EqualTo(10));
            Assert.That(textpageItemsExcludingHome.ToList().Count, Is.EqualTo(10));
        }

        [Test]
        public void ComitWithUserIdSetsCreatedByAndModifiedBy()
        {
            var childOfHome = Hive.Cms().NewRevision("Child of home", "child-of-home", "textpageSchema")
                    .SetSelectedTemplate(new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml")))
                    .SetParent(_homePage.Item)
                    .SetValue("bodyText", "<p>TEST</p>")
                    .Publish()
                    .Commit(_homePage.Item.Id);

            Assert.IsTrue(childOfHome.Success);

            using (var uow = Hive.OpenReader<IContentStore>())
            {
                var cRelations = uow.Repositories.GetParentRelations(childOfHome.Content.Id, FixedRelationTypes.CreatedByRelationType);

                Assert.AreEqual(1, cRelations.Count());
                Assert.AreEqual(_homePage.Item.Id, cRelations.First().SourceId);

                var mRelations = uow.Repositories.GetParentRelations(childOfHome.Content.Id, FixedRelationTypes.ModifiedByRelationType);

                Assert.AreEqual(1, mRelations.Count());
                Assert.AreEqual(_homePage.Item.Id, mRelations.First().SourceId);
            }
        }
    }
}
