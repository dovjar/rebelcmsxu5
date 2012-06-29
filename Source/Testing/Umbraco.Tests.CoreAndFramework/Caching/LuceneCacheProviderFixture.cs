namespace Umbraco.Tests.CoreAndFramework.Caching
{
    #region Imports

    using System;
    using System.IO;
    using System.Threading;
    using NUnit.Framework;
    using Umbraco.Framework.Serialization;
    using Umbraco.Framework.Testing;
    using Umbraco.Lucene;
    using Umbraco.Lucene.Caching;
    using Umbraco.Tests.Extensions;

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