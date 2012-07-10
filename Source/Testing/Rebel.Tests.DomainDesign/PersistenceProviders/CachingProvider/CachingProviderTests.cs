using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rebel.Framework;
using Rebel.Framework.Context;
using Rebel.Framework.Hive.CachingProvider;
using Rebel.Framework.Hive.PersistenceGovernor;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.ProviderSupport;
using Rebel.Tests.Extensions;
using Rebel.Tests.Extensions.Stubs;
using HiveReadWriteProvider = Rebel.Framework.Hive.CachingProvider.HiveReadWriteProvider;

namespace Rebel.Tests.DomainDesign.PersistenceProviders.CachingProvider
{
    [TestClass]
    public class CachingProviderTests : AbstractHivePersistenceTest
    {
        [ClassInitialize]
        public static void TestSetup(TestContext testContext)
        {
            DataHelper.SetupLog4NetForTests();
        }

        #region Overrides of DisposableObject

        private readonly IHiveReadWriteProvider _directReadWriteProvider;
        private readonly IHiveReadProvider _directReaderProvider;

        private readonly IHiveReadWriteProvider _readWriteProviderViaHiveGovernor;
        private readonly IHiveReadProvider _readerProviderViaHiveGovernor;
        private FakeFrameworkContext _fakeFrameworkContext;

        protected override IFrameworkContext FrameworkContext { get { return _fakeFrameworkContext; } }


        public CachingProviderTests()
        {
            _fakeFrameworkContext = new FakeFrameworkContext();

            var inMemory = new NHibernateInMemoryRepository(_fakeFrameworkContext);

            var dataContextFactory = new DataContextFactory();
            var readWriteUnitOfWorkFactory = new ReadWriteUnitOfWorkFactory();
            var directReaderProvider = new HiveReadWriteProvider(new HiveProviderSetup(_fakeFrameworkContext, "r-unit-tester",
                                                                  new FakeHiveProviderBootstrapper(),
                                                                  readWriteUnitOfWorkFactory, readWriteUnitOfWorkFactory,
                                                                  dataContextFactory));
            var directReadWriteProvider = directReaderProvider;

            // Create hive wrappers for the readers and writers
            var governorRUowFactory = new ReadOnlyUnitOfWorkFactoryWrapper(new[] { directReaderProvider, inMemory.HiveReadProvider });
            var governorRWUowFactory = new ReadWriteUnitOfWorkFactoryWrapper(new[] { directReadWriteProvider, inMemory.ReadWriteProvider });

            _readerProviderViaHiveGovernor = _directReaderProvider =
                new Framework.Hive.PersistenceGovernor.HiveReadProvider(new HiveProviderSetup(_fakeFrameworkContext, "r-unit-wrapper", new FakeHiveProviderBootstrapper(), governorRUowFactory, null, null), new[] { _directReaderProvider });
            _readWriteProviderViaHiveGovernor = _directReadWriteProvider =
                new Framework.Hive.PersistenceGovernor.HiveReadWriteProvider(new HiveProviderSetup(_fakeFrameworkContext, "rw-unit-wrapper", new FakeHiveProviderBootstrapper(), governorRUowFactory, governorRWUowFactory, null), new[] { _directReadWriteProvider });
        }

        /// <summary>
        /// Handles the disposal of resources. Derived from abstract class <see cref="DisposableObject"/> which handles common required locking logic.
        /// </summary>
        protected override void DisposeResources()
        {
            _directReaderProvider.Dispose();
            _directReadWriteProvider.Dispose();
        }

        #endregion

        #region Overrides of AbstractPersistenceTest

        protected override Action PostWriteCallback
        {
            get { return () => { return; }; }
        }

        protected override IHiveReadProvider DirectReaderProvider
        {
            get { return _directReaderProvider; }
        }

        protected override IHiveReadWriteProvider DirectReadWriteProvider
        {
            get { return _directReadWriteProvider; }
        }

        protected override IHiveReadProvider ReaderProviderViaHiveGovernor
        {
            get { return _readerProviderViaHiveGovernor; }
        }

        protected override IHiveReadWriteProvider ReadWriteProviderViaHiveGovernor
        {
            get { return _readWriteProviderViaHiveGovernor; }
        }

        #endregion
    }
}