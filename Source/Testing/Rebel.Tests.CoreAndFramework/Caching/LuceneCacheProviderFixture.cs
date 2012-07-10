namespace Rebel.Tests.CoreAndFramework.Caching
{
    #region Imports

    using System;
    using System.IO;
    using System.Threading;
    using NUnit.Framework;
    using Rebel.Framework.Serialization;
    using Rebel.Framework.Testing;
    using Rebel.Lucene;
    using Rebel.Lucene.Caching;
    using Rebel.Tests.Extensions;

    #endregion

    [TestFixture]
    public class LuceneCacheProviderFixture : AbstractCacheProviderFixture
    {
        private IndexConfiguration _configuration;
        private FakeFrameworkContext _frameworkContext;

        [TestFixtureSetUp]
        public override void SetUp()
        {
            TestHelper.SetupLog4NetForTests();
            CacheIsAsync = true;
        }

        public override void TestSetUp()
        {
            var serializer = new ServiceStackSerialiser();

            _frameworkContext = new FakeFrameworkContext(serializer);
            _configuration = new IndexConfiguration(GetPathForTest());
            var controller = new IndexController(_configuration, _frameworkContext);

            CacheProvider = new CacheProvider(controller, false);
        }

        public override void TestTearDown()
        {
            CacheProvider.Dispose();
            _frameworkContext.ScopedFinalizer.FinalizeScope();

            if (Directory.Exists(_configuration.BuildLocation))
                Directory.Delete(_configuration.BuildLocation, true);

            base.TestTearDown();
        }

        private string GetPathForTest()
        {
            var indexPath = Common.MapPathForTest("~/IndexTest-" + Thread.CurrentThread.ManagedThreadId + TestContext.CurrentContext.Test.Name);
            return indexPath;
        }
    }
}