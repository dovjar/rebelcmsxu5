namespace Rebel.Tests.CoreAndFramework.Caching
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading;
    using NUnit.Framework;
    using Rebel.Framework;
    using Rebel.Framework.Caching;
    using Rebel.Framework.Data;
    using Rebel.Framework.Diagnostics;
    using Rebel.Framework.Linq.CriteriaGeneration.Expressions;
    using Rebel.Framework.Linq.QueryModel;
    using Rebel.Framework.Persistence.Model;
    using Rebel.Framework.Persistence.Model.Constants;
    using Rebel.Hive.Caching;
    using Rebel.Tests.Extensions;

    public abstract class AbstractCacheProviderFixture
    {
        protected AbstractCacheProvider CacheProvider { get; set; }

        protected bool CacheIsAsync { get; set; }
        [Test]
        public void SlidingExpiredItemIsRemoved()
        {
            var stringKey = CacheKey.Create<string>("hello");
            var myObject1 = CacheProvider.GetOrCreate(stringKey,
                () => new CacheValueOf<TestCacheObject>(new TestCacheObject("hello-1"), new StaticCachePolicy(TimeSpan.FromSeconds(1))));

            Assert.NotNull(myObject1);

            Assert.That(myObject1.AlreadyExisted, Is.False);

            Assert.That(myObject1.WasInserted, Is.True);

            Assert.NotNull(myObject1.Value);

            if (CacheIsAsync) Thread.Sleep(500);

            Assert.NotNull(CacheProvider.GetValue<TestCacheObject>(stringKey));

            Assert.NotNull(CacheProvider.Get<TestCacheObject>(stringKey));

            Thread.Sleep(TimeSpan.FromSeconds(1.2d));

            Assert.Null(CacheProvider.Get<TestCacheObject>(stringKey));
        }

        [Test]
        public void CanAddAtLeast300ItemsPerSecond()
        {
            var watch = new Stopwatch();
            TypedEntity[] itemsToAdd = new TypedEntity[500];
            for (int i = 0; i < 300; i++)
            {
                var item = HiveModelCreationHelper.MockTypedEntity(true);
                itemsToAdd[i] = item;
            }

            watch.Start();
            for (int i = 0; i < 300; i++)
            {
                CacheProvider.AddOrChangeValue(CacheKey.Create("Item " + i), itemsToAdd[i]);
            }
            watch.Stop();
            var writeElapsed = watch.Elapsed;

            watch.Reset();
            watch.Start();
            for (int i = 0; i < 300; i++)
            {
                var cacheKey = CacheKey.Create("Item " + i);
                var item = CacheProvider.Get<TypedEntity>(cacheKey);
                Assert.That(item, Is.Not.Null);
            }
            watch.Stop();
            var readElapsed = watch.Elapsed;

            LogHelper.TraceIfEnabled<AbstractCacheProviderFixture>("Write Took (s): " + writeElapsed.TotalSeconds);
            LogHelper.TraceIfEnabled<AbstractCacheProviderFixture>("Read Took (s): " + readElapsed.TotalSeconds);
            Assert.That(writeElapsed, Is.LessThan(TimeSpan.FromSeconds(1)));
            Assert.That(readElapsed, Is.LessThan(TimeSpan.FromSeconds(1)));
        }

        [Test]
        public void CanAddItemToCacheWithComplexKey()
        {
            var myObject1 = new TestCacheObject("test-1");
            var myObject2 = new TestCacheObject("test-2");
            var myObject3 = new TestCacheObject("test-3");

            Assert.IsNull(CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<string>("ah")));
            Assert.IsNull(CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<string>("ah")));

            CacheProvider.AddOrChangeValue(CacheKey.Create("my-1"), myObject1);

            if (CacheIsAsync) Thread.Sleep(500);

            var retrieve1typed = CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<string>("my-1"));
            Assert.NotNull(retrieve1typed);

            var retrieve1 = CacheProvider.Get<TestCacheObject>(CacheKey.Create("my-1"));
            Assert.That(retrieve1.Item, Is.EqualTo(myObject1));


            CacheProvider.AddOrChangeValue(CacheKey.Create("my-2"), myObject2);
            CacheProvider.AddOrChangeValue(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob"), myObject3);

            if (CacheIsAsync) Thread.Sleep(500);

            var retrieve2 = CacheProvider.GetValue<TestCacheObject>(CacheKey.Create("my-2"));
            var retrieve3 = CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob"));

            
            Assert.That(retrieve1typed.Text, Is.EqualTo(myObject1.Text));
            Assert.That(retrieve2, Is.EqualTo(myObject2));
            Assert.That(retrieve3, Is.EqualTo(myObject3));
        }

        [Test]
        public void CanRemoveItemFromCacheWithDelegate()
        {
            var myObject1 = new TestCacheObject("test-1");
            var myObject2 = new TestCacheObject("bob");
            var myObject3 = new TestCacheObject("frank");

            CacheProvider.AddOrChangeValue(CacheKey.Create<string>("my-1"), myObject1);
            CacheProvider.AddOrChangeValue(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob"), myObject2);
            CacheProvider.AddOrChangeValue(CacheKey.Create<StrongClassKey>(x => x.MyName = "frank"), myObject3);

            if (CacheIsAsync) Thread.Sleep(500);

            var resultFilterClause = new ResultFilterClause(typeof(string), ResultFilterType.Any, 0);
            var scopeStartId = new HiveId(Guid.NewGuid());
            var fromClause = new FromClause(scopeStartId.AsEnumerableOfOne(), HierarchyScope.AncestorsOrSelf, FixedStatusTypes.Published);
            var fieldPredicateExpression = new FieldPredicateExpression("title", ValuePredicateType.Equal, "blah");


            //var key = new HiveQueryCacheKey(new QueryDescription(resultFilterClause, fromClause, fieldPredicateExpression, Enumerable.Empty<SortClause>()));
            var key = CacheKey.Create(new HiveQueryCacheKey(new QueryDescription(resultFilterClause, fromClause, fieldPredicateExpression, Enumerable.Empty<SortClause>())));
            CacheProvider.AddOrChangeValue(key, myObject3);

            if (CacheIsAsync) Thread.Sleep(500);

            Assert.NotNull(CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<string>("my-1")));
            Assert.NotNull(CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob")));
            Assert.NotNull(CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<string>("my-1")));
            Assert.NotNull(CacheProvider.GetValue<TestCacheObject>(key));

            CacheProvider.RemoveWhereKeyMatches<string>(x => x == "my-1");
            CacheProvider.RemoveWhereKeyMatches<StrongClassKey>(x => x.MyName == "bob");
            CacheProvider.RemoveWhereKeyMatches<HiveQueryCacheKey>(x => x.From.HierarchyScope == HierarchyScope.AncestorsOrSelf);

            // No check for async as removals should be instant

            Assert.Null(CacheProvider.Get<TestCacheObject>(CacheKey.Create<string>("my-1")));
            Assert.Null(CacheProvider.GetValue<TestCacheObject>(CacheKey.Create<string>("my-1")));
            Assert.Null(CacheProvider.Get<TestCacheObject>(CacheKey.Create<StrongClassKey>(x => x.MyName = "bob")));
            Assert.NotNull(CacheProvider.Get<TestCacheObject>(CacheKey.Create<StrongClassKey>(x => x.MyName = "frank")));
        }

        [TestFixtureSetUp]
        public virtual void SetUp()
        {
            TestHelper.SetupLog4NetForTests();
        }

        [TestFixtureTearDown]
        public virtual void TearDown()
        {
            CacheProvider.Dispose();
        }

        [SetUp]
        public virtual void TestSetUp()
        {
            
        }

        [TearDown]
        public virtual void TestTearDown()
        {
            
        }

        [Test]
        public void WhenAddingNonExistantItemResultShowsItemWasCreated()
        {
            bool check = false;

            var result = CacheProvider.GetOrCreate(
                CacheKey.Create<string>("hello"),
                () =>
                {
                    check = true;
                    return new CacheValueOf<TestCacheObject>(new TestCacheObject("whatever"));
                });

            Assert.That(check, Is.True);
            Assert.That(result.AlreadyExisted, Is.False);
            Assert.That(result.WasInserted, Is.True);
            Assert.That(result.WasUpdated, Is.False);
            Assert.That(result.ExistsButWrongType, Is.False);
            Assert.That(result.Value.Item.Text, Is.EqualTo("whatever"));
        }

        public class StrongClassKey : AbstractEquatableObject<StrongClassKey>
        {
            public string MyName { get; set; }

            /// <summary>
            /// Gets the natural id members.
            /// </summary>
            /// <returns></returns>
            /// <remarks></remarks>
            protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
            {
                yield return this.GetPropertyInfo(x => x.MyName);
            }
        }

        private class TestCacheObject : AbstractEquatableObject<TestCacheObject>
        {
            public TestCacheObject(string text)
            {
                Date = DateTime.Now;
                Text = text;
            }

            public DateTime Date { get; set; }
            public string Text { get; set; }
            protected override IEnumerable<PropertyInfo> GetMembersForEqualityComparison()
            {
                yield return this.GetPropertyInfo(x => x.Date);
                yield return this.GetPropertyInfo(x => x.Text);
            }
        }
    }
}