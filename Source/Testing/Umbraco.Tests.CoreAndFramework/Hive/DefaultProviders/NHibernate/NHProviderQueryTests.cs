using System;
using NUnit.Framework;
using Umbraco.Hive.ProviderGrouping;
using Umbraco.Framework.Testing.PartialTrust;
using Umbraco.Tests.Extensions;
using Umbraco.Hive;

namespace Umbraco.Tests.CoreAndFramework.Hive.DefaultProviders.NHibernate
{
    using Umbraco.Framework.Persistence.NHibernate.OrmConfig;

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
            //_helper = new NhibernateTestSetupHelper(@"data source=.\sqlexpress2008;initial catalog=v5units;user id=umbraco;password=umbraco", SupportedNHDrivers.MsSql2008, "call", true, useNhProf: true);
            //_helper = new NhibernateTestSetupHelper(@"data source=C:\Users\Alex\Documents\My Web Sites\Umbraco.Cms.Web.UI-Site\App_Data\Test2.sdf", SupportedNHDrivers.MsSqlCe4, "call", true, useNhProf: true);
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
