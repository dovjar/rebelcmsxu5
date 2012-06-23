using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RebelCms.Framework;
using RebelCms.Framework.Context;
using RebelCms.Framework.DataManagement;
using RebelCms.Framework.Hive.PersistenceGovernor;
using RebelCms.Framework.Persistence.ProviderSupport;
using RebelCms.Framework.Persistence.XmlStore.DataManagement;
using RebelCms.Framework.Persistence.XmlStore.DataManagement.ReadWrite;
using RebelCms.Framework.ProviderSupport;
using RebelCms.Tests.Extensions;
using RebelCms.Tests.Extensions.Stubs;
using HiveReadProvider = RebelCms.Framework.Persistence.XmlStore.HiveReadProvider;
using HiveReadWriteProvider = RebelCms.Framework.Persistence.XmlStore.HiveReadWriteProvider;

namespace RebelCms.Tests.DomainDesign.PersistenceProviders.XmlStore
{
    //NOTE: Have disabled this test as we don't use the XmlStore, if we need it, we'll re-enable and fix all the issues. SD.
    //[TestClass]

    public class XmlStoreHiveTests : AbstractHivePersistenceTest
    {
        private DataContextFactory _dataContextFactory;
        private ReadOnlyUnitOfWorkFactory _readOnlyUnitOfWorkFactory;
        private HiveReadProvider _directHiveReadProvider;
        private ReadWriteUnitOfWorkFactory _readWriteUnitOfWorkFactory;
        private HiveReadWriteProvider _writeProvider;
        private Framework.Hive.PersistenceGovernor.HiveReadProvider _hiveReadProviderViaGovernor;
        private Framework.Hive.PersistenceGovernor.HiveReadWriteProvider _hiveReadWriteProviderViaGovernor;
        private FakeFrameworkContext _fakeFrameworkContext;

        protected override IFrameworkContext FrameworkContext { get { return _fakeFrameworkContext; } }

        #region Overrides of DisposableObject

        public XmlStoreHiveTests()
        {
            using (DisposableTimer.TraceDuration<XmlStoreHiveTests>("Start setup", "End setup"))
            {
                // Create reader
                var xmlPath =
                    Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring("file:\\\\".Length + 1)),
                        "App_Data\\RebelCms.config");

                _fakeFrameworkContext = new FakeFrameworkContext();

                _dataContextFactory = new DataContextFactory(xmlPath);
                _readOnlyUnitOfWorkFactory = new ReadOnlyUnitOfWorkFactory();
                _directHiveReadProvider = new HiveReadProvider(new HiveProviderSetup(_fakeFrameworkContext, "r-unit-tester", new FakeHiveProviderBootstrapper(), _readOnlyUnitOfWorkFactory, null, _dataContextFactory));

                // Create writer
                _readWriteUnitOfWorkFactory = new ReadWriteUnitOfWorkFactory();
                _writeProvider = new HiveReadWriteProvider(new HiveProviderSetup(_fakeFrameworkContext, "rw-unit-tester", new FakeHiveProviderBootstrapper(), null, _readWriteUnitOfWorkFactory, _dataContextFactory));



                // Create hive wrappers for the readers and writers
                var governorRUowFactory = new ReadOnlyUnitOfWorkFactoryWrapper(new[] { _directHiveReadProvider });

                var governorRWUowFactory = new ReadWriteUnitOfWorkFactoryWrapper(new[] { _writeProvider });

                _hiveReadProviderViaGovernor = new Framework.Hive.PersistenceGovernor.HiveReadProvider(new HiveProviderSetup(_fakeFrameworkContext, "r-unit-tester", new FakeHiveProviderBootstrapper(), governorRUowFactory, null, null), new[] { _directHiveReadProvider });
                _hiveReadWriteProviderViaGovernor = new Framework.Hive.PersistenceGovernor.HiveReadWriteProvider(new HiveProviderSetup(_fakeFrameworkContext, "rw-unit-tester", new FakeHiveProviderBootstrapper(), null, governorRWUowFactory, null), new[] { _writeProvider });
            }
        }

        protected override void DisposeResources()
        {
            _directHiveReadProvider.Dispose();
            _writeProvider.Dispose();
        }

        #endregion

        #region Overrides of AbstractPersistenceTest

        protected override Action PostWriteCallback
        {
            get { return () => { /* do nothing */ }; }
        }

        protected override IHiveReadProvider DirectReaderProvider
        {
            get { return _directHiveReadProvider; }
        }

        protected override IHiveReadWriteProvider DirectReadWriteProvider
        {
            get { return _writeProvider; }
        }

        protected override IHiveReadProvider ReaderProviderViaHiveGovernor
        {
            get { return _hiveReadProviderViaGovernor; }
        }

        protected override IHiveReadWriteProvider ReadWriteProviderViaHiveGovernor
        {
            get { return _hiveReadWriteProviderViaGovernor; }
        }

        #endregion
    }
}
