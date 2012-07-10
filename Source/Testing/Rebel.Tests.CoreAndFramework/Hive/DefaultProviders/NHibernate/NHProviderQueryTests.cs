using System;
using NUnit.Framework;
using Rebel.Hive.ProviderGrouping;
using Rebel.Framework.Testing.PartialTrust;
using Rebel.Tests.Extensions;
using Rebel.Hive;

namespace Rebel.Tests.CoreAndFramework.Hive.DefaultProviders.NHibernate
{
    using Rebel.Framework.Persistence.NHibernate.OrmConfig;

    [TestFixture]
    public class NhProviderQueryTests : AbstractProviderQueryTests
    {
        private NhibernateTestSetupHelper _helper;
        private GroupUnitFactory _groupUnitFactory;


        

        [SetUp]
        protected void TestSetup()
        {
            _helper = new NhibernateTestSetupHelper();
            //_helper = new NhibernateTestSetupHelper(useNhProf:true);
            //_helper = new NhibernateTestSetupHelper(@"Data Source=c:\temp\test.s3db;Version=3;",
            //                                        SupportedNHDrivers.SqlLite,
            //                                        "call",
            //                                        true,
            //                                        useNhProf: true);
            //_helper = new NhibernateTestSetupHelper(@"data source=.\sqlexpress2008;initial catalog=v5units;user id=rebel;password=rebel", SupportedNHDrivers.MsSql2008, "call", true, useNhProf: true);
            //_helper = new NhibernateTestSetupHelper(@"data source=C:\Users\Alex\Documents\My Web Sites\Rebel.Cms.Web.UI-Site\App_Data\Test2.sdf", SupportedNHDrivers.MsSqlCe4, "call", true, useNhProf: true);
            _groupUnitFactory = new GroupUnitFactory(_helper.ProviderSetup, new Uri("content://"), FakeHiveCmsManager.CreateFakeRepositoryContext(_helper.FakeFrameworkContext));
        }

        [TearDown]
        protected void TestTearDown()
        {
            _helper.Dispose();
        }

        protected override ProviderSetup ProviderSetup
        {
            get { return _helper.ProviderSetup; }
        }

        protected override GroupUnitFactory GroupUnitFactory
        {
            get { return _groupUnitFactory; }
        }
    }
}
