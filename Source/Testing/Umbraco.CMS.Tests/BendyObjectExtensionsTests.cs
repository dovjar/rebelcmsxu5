using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using NSubstitute;
using NUnit.Framework;
using Umbraco.Cms.Web;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Model;
using Umbraco.Framework;
using Umbraco.Framework.Context;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.Model.Constants.AttributeTypes;
using Umbraco.Hive;
using Umbraco.Hive.Configuration;
using Umbraco.Hive.RepositoryTypes;
using Umbraco.Tests.Extensions;

namespace Umbraco.Tests.Cms
{
    [TestFixture]
    public class BendyObjectExtensionsTests
    {
        private dynamic _root;
        private IEnumerable<dynamic> _children;

        protected IHiveManager Hive;

        #region SetUp

        private NhibernateTestSetupHelper _nhibernateTestSetup;

        [SetUp]
        public void SetUp()
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

            // Setup Schemas
            Hive.Cms()
                .UsingStore<IContentStore>()
                .NewSchema("textpageSchema")
                .Define(NodeNameAttributeDefinition.AliasValue, NodeNameAttributeType.AliasValue, "tab1")
                .Define(SelectedTemplateAttributeDefinition.AliasValue, SelectedTemplateAttributeType.AliasValue, "tab1")
                .Commit();

            // Setup Content
            var root = Hive.Cms().NewRevision("Root", "root", "textpageSchema")
                .SetSelectedTemplate(new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml")))
                .Publish()
                .Commit();

            // Avoid type scanning in a unit test runner by passing in the known assembly under test
            var dynamicExtensionAssemblies = typeof (RenderViewModelExtensions).Assembly.AsEnumerableOfOne();

            _root = new Content(root.Item).Bend(Hive, null, null, dynamicExtensionAssemblies);

            var children = new List<dynamic>();

            for (var i = 0; i < 10; i++)
            {
                // Create child
                var child = Hive.Cms().NewRevision("Child" + i, "child-" + i, "textpageSchema")
                    .SetSelectedTemplate(new HiveId("storage", "templates", new HiveIdValue("/textpageTemplate.cshtml")))
                    .SetParent(root.Item)
                    .Publish()
                    .Commit();

                children.Add(new Content(child.Item).Bend(Hive, null, null, dynamicExtensionAssemblies));
            }

            _children = children;

            // Setup dependency resolver
            var dependencyResolver = Substitute.For<IDependencyResolver>();
            dependencyResolver.GetService(typeof(IHiveManager)).Returns(Hive);

            DependencyResolver.SetResolver(dependencyResolver);
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

        #region IsHelper Methods

        [Test]
        public void BendyObjectExtensionsTests_IsFirst_Success()
        {
            var firstChild = _children.First();
            var secondChild = _children.ElementAt(1);

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(firstChild.IsFirst());
            Assert.IsFalse(secondChild.IsFirst());

            Assert.AreEqual(trueValue, firstChild.IsFirst(trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, secondChild.IsFirst(trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, firstChild.IsFirst(trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, secondChild.IsFirst(trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsNotFirst_Success()
        {
            var firstChild = _children.First();
            var secondChild = _children.ElementAt(1);

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsFalse(firstChild.IsNotFirst());
            Assert.IsTrue(secondChild.IsNotFirst());

            Assert.AreEqual(string.Empty, firstChild.IsNotFirst(trueValue).ToHtmlString());
            Assert.AreEqual(trueValue, secondChild.IsNotFirst(trueValue).ToHtmlString());

            Assert.AreEqual(falseValue, firstChild.IsNotFirst(trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(trueValue, secondChild.IsNotFirst(trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsLast_Success()
        {
            var lastChild = _children.Last();
            var secondChild = _children.ElementAt(1);

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(lastChild.IsLast());
            Assert.IsFalse(secondChild.IsLast());

            Assert.AreEqual(trueValue, lastChild.IsLast(trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, secondChild.IsLast(trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, lastChild.IsLast(trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, secondChild.IsLast(trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsNotLast_Success()
        {
            var lastChild = _children.Last();
            var secondChild = _children.ElementAt(1);

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsFalse(lastChild.IsNotLast());
            Assert.IsTrue(secondChild.IsNotLast());

            Assert.AreEqual(string.Empty, lastChild.IsNotLast(trueValue).ToHtmlString());
            Assert.AreEqual(trueValue, secondChild.IsNotLast(trueValue).ToHtmlString());

            Assert.AreEqual(falseValue, lastChild.IsNotLast(trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(trueValue, secondChild.IsNotLast(trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsIndex_Success()
        {
            var firstChild = _children.First();
            var thirdChild = _children.ElementAt(2);
            var lastChild = _children.Last();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(thirdChild.IsIndex(2));
            Assert.IsFalse(firstChild.IsIndex(2));
            Assert.IsFalse(firstChild.IsIndex(20)); // Test out of bounds index
            Assert.IsFalse(firstChild.IsIndex(-2)); // Test negative bounds
            Assert.IsTrue(lastChild.IsIndex(9)); // Test array edge

            Assert.AreEqual(trueValue, thirdChild.IsIndex(2, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, firstChild.IsIndex(2, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, thirdChild.IsIndex(2, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, firstChild.IsIndex(2, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsNotIndex_Success()
        {
            var firstChild = _children.First();
            var thirdChild = _children.ElementAt(2);
            var lastChild = _children.Last();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(firstChild.IsNotIndex(2));
            Assert.IsFalse(thirdChild.IsNotIndex(2));
            Assert.IsTrue(thirdChild.IsNotIndex(20)); // Test out of bounds index
            Assert.IsTrue(thirdChild.IsNotIndex(-2)); // Test negative bounds
            Assert.IsFalse(lastChild.IsNotIndex(9)); // Test array edge

            Assert.AreEqual(string.Empty, thirdChild.IsNotIndex(2, trueValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsNotIndex(2, trueValue).ToHtmlString());

            Assert.AreEqual(falseValue, thirdChild.IsNotIndex(2, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsNotIndex(2, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsPosition_Success()
        {
            var firstChild = _children.First();
            var thirdChild = _children.ElementAt(2);
            var lastChild = _children.Last();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(thirdChild.IsPosition(3));
            Assert.IsFalse(firstChild.IsPosition(3));
            Assert.IsFalse(firstChild.IsPosition(20)); // Test out of bounds index
            Assert.IsFalse(firstChild.IsPosition(-2)); // Test negative bounds
            Assert.IsTrue(lastChild.IsPosition(10)); // Test array edge

            Assert.AreEqual(trueValue, thirdChild.IsPosition(3, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, firstChild.IsPosition(3, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, thirdChild.IsPosition(3, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, firstChild.IsPosition(3, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsNotPosition_Success()
        {
            var firstChild = _children.First();
            var thirdChild = _children.ElementAt(2);
            var lastChild = _children.Last();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(firstChild.IsNotPosition(3));
            Assert.IsFalse(thirdChild.IsNotPosition(3));
            Assert.IsTrue(thirdChild.IsNotPosition(20)); // Test out of bounds index
            Assert.IsTrue(thirdChild.IsNotPosition(-2)); // Test negative bounds
            Assert.IsFalse(lastChild.IsNotPosition(10)); // Test array edge

            Assert.AreEqual(string.Empty, thirdChild.IsNotPosition(3, trueValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsNotPosition(3, trueValue).ToHtmlString());

            Assert.AreEqual(falseValue, thirdChild.IsNotPosition(3, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsNotPosition(3, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        [ExpectedException(typeof(ObsoleteException))]
        public void BendyObjectExtensionsTests_IsModZero_Success()
        {
            var thirdChild = _children.ElementAt(2);

            Assert.IsTrue(thirdChild.IsModZero(3));
        }

        [Test]
        [ExpectedException(typeof(ObsoleteException))]
        public void BendyObjectExtensionsTests_IsNotModZero_Success()
        {
            var thirdChild = _children.ElementAt(2);

            Assert.IsFalse(thirdChild.IsNotModZero(3));
        }

        [Test]
        public void BendyObjectExtensionsTests_IsPositionDivisibleBy_Success()
        {
            var firstChild = _children.First();
            var thirdChild = _children.ElementAt(2);
            var lastChild = _children.Last();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(thirdChild.IsPositionDivisibleBy(3));
            Assert.IsFalse(firstChild.IsPositionDivisibleBy(3));
            Assert.IsFalse(firstChild.IsPositionDivisibleBy(20)); // Test out of bounds index
            Assert.IsFalse(firstChild.IsPositionDivisibleBy(-2)); // Test negative bounds
            Assert.IsTrue(lastChild.IsPositionDivisibleBy(10)); // Test array edge

            Assert.AreEqual(trueValue, thirdChild.IsPositionDivisibleBy(3, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, firstChild.IsPositionDivisibleBy(3, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, thirdChild.IsPositionDivisibleBy(3, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, firstChild.IsPositionDivisibleBy(3, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsPositionIndivisibleBy_Success()
        {
            var firstChild = _children.First();
            var thirdChild = _children.ElementAt(2);
            var lastChild = _children.Last();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsFalse(thirdChild.IsPositionIndivisibleBy(3));
            Assert.IsTrue(firstChild.IsPositionIndivisibleBy(3));
            Assert.IsTrue(firstChild.IsPositionIndivisibleBy(20)); // Test out of bounds index
            Assert.IsTrue(firstChild.IsPositionIndivisibleBy(-2)); // Test negative bounds
            Assert.IsFalse(lastChild.IsPositionIndivisibleBy(10)); // Test array edge

            Assert.AreEqual(string.Empty, thirdChild.IsPositionIndivisibleBy(3, trueValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsPositionIndivisibleBy(3, trueValue).ToHtmlString());

            Assert.AreEqual(falseValue, thirdChild.IsPositionIndivisibleBy(3, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsPositionIndivisibleBy(3, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsEven_Success()
        {
            var firstChild = _children.First();
            var secondChild = _children.ElementAt(1);
            var thirdChild = _children.ElementAt(2);
            var forthChild = _children.ElementAt(3);

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsFalse(firstChild.IsEven());
            Assert.IsTrue(secondChild.IsEven());
            Assert.IsFalse(thirdChild.IsEven());
            Assert.IsTrue(forthChild.IsEven());

            Assert.AreEqual(trueValue, secondChild.IsEven(trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, firstChild.IsEven(trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, secondChild.IsEven(trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, firstChild.IsEven(trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsOdd_Success()
        {
            var firstChild = _children.First();
            var secondChild = _children.ElementAt(1);
            var thirdChild = _children.ElementAt(2);
            var forthChild = _children.ElementAt(3);

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(firstChild.IsOdd());
            Assert.IsFalse(secondChild.IsOdd());
            Assert.IsTrue(thirdChild.IsOdd());
            Assert.IsFalse(forthChild.IsOdd());

            Assert.AreEqual(trueValue, firstChild.IsOdd(trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, secondChild.IsOdd(trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, firstChild.IsOdd(trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, secondChild.IsOdd(trueValue, falseValue).ToHtmlString());
        }

        [Test]
        [ExpectedException(typeof(ObsoleteException))]
        public void BendyObjectExtensionsTests_IsEqual_Success()
        {
            var firstChild = _children.First();
            var firstChildContent = firstChild.__OriginalItem;
            Assert.IsTrue(firstChild.IsEqual(firstChildContent));
        }

        [Test]
        public void BendyObjectExtensionsTests_IsEqualTo_Success()
        {
            var firstChild = _children.First();
            var secondChild = _children.ElementAt(1);

            var firstChildContent = firstChild.__OriginalItem;
            var secondChildContent = secondChild.__OriginalItem;

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            // Content based

            Assert.IsTrue(firstChild.IsEqualTo(firstChildContent));
            Assert.IsFalse(secondChild.IsEqualTo(firstChildContent));
            Assert.IsTrue(secondChild.IsEqualTo(secondChildContent));
            Assert.IsFalse(firstChild.IsEqualTo(secondChildContent));

            Assert.AreEqual(trueValue, secondChild.IsEqualTo(secondChildContent, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, firstChild.IsEqualTo(secondChildContent, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, secondChild.IsEqualTo(secondChildContent, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, firstChild.IsEqualTo(secondChildContent, trueValue, falseValue).ToHtmlString());

            // Dynamic based
            Assert.IsTrue(firstChild.IsEqualTo(firstChild));
            Assert.IsFalse(secondChild.IsEqualTo(firstChild));
            Assert.IsTrue(secondChild.IsEqualTo(secondChild));
            Assert.IsFalse(firstChild.IsEqualTo(secondChild));

            Assert.AreEqual(trueValue, secondChild.IsEqualTo(secondChild, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, firstChild.IsEqualTo(secondChild, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, secondChild.IsEqualTo(secondChild, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, firstChild.IsEqualTo(secondChild, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        public void BendyObjectExtensionsTests_IsNotEqualTo_Success()
        {
            var firstChild = _children.First();
            var secondChild = _children.ElementAt(1);

            var firstChildContent = firstChild.__OriginalItem;
            var secondChildContent = secondChild.__OriginalItem;

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            // Content based

            Assert.IsFalse(firstChild.IsNotEqualTo(firstChildContent));
            Assert.IsTrue(secondChild.IsNotEqualTo(firstChildContent));
            Assert.IsFalse(secondChild.IsNotEqualTo(secondChildContent));
            Assert.IsTrue(firstChild.IsNotEqualTo(secondChildContent));

            Assert.AreEqual(string.Empty, secondChild.IsNotEqualTo(secondChildContent, trueValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsNotEqualTo(secondChildContent, trueValue).ToHtmlString());

            Assert.AreEqual(falseValue, secondChild.IsNotEqualTo(secondChildContent, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsNotEqualTo(secondChildContent, trueValue, falseValue).ToHtmlString());

            // Dynamic based
            Assert.IsFalse(firstChild.IsNotEqualTo(firstChild));
            Assert.IsTrue(secondChild.IsNotEqualTo(firstChild));
            Assert.IsFalse(secondChild.IsNotEqualTo(secondChild));
            Assert.IsTrue(firstChild.IsNotEqualTo(secondChild));

            Assert.AreEqual(string.Empty, secondChild.IsNotEqualTo(secondChild, trueValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsNotEqualTo(secondChild, trueValue).ToHtmlString());

            Assert.AreEqual(falseValue, secondChild.IsNotEqualTo(secondChild, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(trueValue, firstChild.IsNotEqualTo(secondChild, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        [ExpectedException(typeof(ObsoleteException))]
        public void BendyObjectExtensionsTests_IsDescendant_Success()
        {
            var firstChild = _children.First();

            Assert.IsTrue(firstChild.IsDescendant(_root));
        }

        [Test]
        public void BendyObjectExtensionsTests_IsDescendantOf_Success()
        {
            var firstChild = _children.First();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(firstChild.IsDescendantOf(_root));
            Assert.IsFalse(_root.IsDescendantOf(firstChild));
            Assert.IsFalse(firstChild.IsDescendantOf(firstChild)); // Test self reference

            Assert.AreEqual(trueValue, firstChild.IsDescendantOf(_root, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, _root.IsDescendantOf(firstChild, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, firstChild.IsDescendantOf(_root, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, _root.IsDescendantOf(firstChild, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        [ExpectedException(typeof(ObsoleteException))]
        public void BendyObjectExtensionsTests_IsDescendantOrSelf_Success()
        {
            var firstChild = _children.First();

            Assert.IsTrue(firstChild.IsDescendantOrSelf(_root));
        }

        [Test]
        public void BendyObjectExtensionsTests_IsDescendantOfOrEqualTo_Success()
        {
            var firstChild = _children.First();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(firstChild.IsDescendantOfOrEqualTo(_root));
            Assert.IsFalse(_root.IsDescendantOfOrEqualTo(firstChild));
            Assert.IsTrue(firstChild.IsDescendantOfOrEqualTo(firstChild)); // Test self reference

            Assert.AreEqual(trueValue, firstChild.IsDescendantOfOrEqualTo(_root, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, _root.IsDescendantOfOrEqualTo(firstChild, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, firstChild.IsDescendantOfOrEqualTo(_root, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, _root.IsDescendantOfOrEqualTo(firstChild, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        [ExpectedException(typeof(ObsoleteException))]
        public void BendyObjectExtensionsTests_IsAncestor_Success()
        {
            var firstChild = _children.First();

            Assert.IsTrue(_root.IsAncestor(firstChild));
        }

        [Test]
        public void BendyObjectExtensionsTests_IsAncestorOf_Success()
        {
            var firstChild = _children.First();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(_root.IsAncestorOf(firstChild));
            Assert.IsFalse(firstChild.IsAncestorOf(_root));
            Assert.IsFalse(firstChild.IsAncestorOf(firstChild)); // Test self reference

            Assert.AreEqual(trueValue, _root.IsAncestorOf(firstChild, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, firstChild.IsAncestorOf(_root, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, _root.IsAncestorOf(firstChild, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, firstChild.IsAncestorOf(_root, trueValue, falseValue).ToHtmlString());
        }

        [Test]
        [ExpectedException(typeof(ObsoleteException))]
        public void BendyObjectExtensionsTests_IsAncestorOrSelf_Success()
        {
            var firstChild = _children.First();

            Assert.IsTrue(_root.IsAncestorOrSelf(firstChild));
        }

        [Test]
        public void BendyObjectExtensionsTests_IsAncestorOfOrEqualTo_Success()
        {
            var firstChild = _children.First();

            var trueValue = "<p>trueValue</p>";
            var falseValue = "<p>trueValue</p>";

            Assert.IsTrue(_root.IsAncestorOfOrEqualTo(firstChild));
            Assert.IsFalse(firstChild.IsAncestorOfOrEqualTo(_root));
            Assert.IsTrue(firstChild.IsAncestorOfOrEqualTo(firstChild)); // Test self reference

            Assert.AreEqual(trueValue, _root.IsAncestorOfOrEqualTo(firstChild, trueValue).ToHtmlString());
            Assert.AreEqual(string.Empty, firstChild.IsAncestorOfOrEqualTo(_root, trueValue).ToHtmlString());

            Assert.AreEqual(trueValue, _root.IsAncestorOfOrEqualTo(firstChild, trueValue, falseValue).ToHtmlString());
            Assert.AreEqual(falseValue, firstChild.IsAncestorOfOrEqualTo(_root, trueValue, falseValue).ToHtmlString());
        }

        #endregion
    }
}
