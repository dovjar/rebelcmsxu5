using System;
using NUnit.Framework;
using Rebel.Framework.Diagnostics;
using Rebel.Hive;
using Rebel.Tests.Extensions;

namespace Rebel.Tests.CoreAndFramework.Hive.DefaultProviders.NHibernate
{
    using Rebel.Framework.Persistence.NHibernate.OrmConfig;

    [TestFixture]
    public class NhStandardProviderTests : AbstractProviderTests
    {
        [SetUp]
        public void BeforeTest()
        {
            SetupHelper = new NhibernateTestSetupHelper(useNhProf:true);
            //SetupHelper = new NhibernateTestSetupHelper(useNhProf:true);            
            //SetupHelper = new NhibernateTestSetupHelper(@"Data Source=c:\temp\test.s3db;Version=3;",
            //                                        SupportedNHDrivers.SqlLite,
            //                                        "call",
            //                                        true,
            //                                        useNhProf: true);
            //SetupHelper = new NhibernateTestSetupHelper(@"data source=.\sqlexpress2008;initial catalog=v5units;user id=rebel;password=rebel", SupportedNHDrivers.MsSql2008, "call", true, useNhProf: true);
            //SetupHelper = new NhibernateTestSetupHelper(@"data source=C:\Users\Alex\Documents\My Web Sites\Rebel.Cms.Web.UI-Site\App_Data\Test2.sdf", SupportedNHDrivers.MsSqlCe4, "call", true, useNhProf: true);
        }

        [TearDown]
        public void AfterTest()
        {
            SetupHelper.Dispose();
        }

        private NhibernateTestSetupHelper SetupHelper { get; set; }

        protected override Action PostWriteCallback
        {
            get
            {
                return () =>
                           {
                               LogHelper.TraceIfEnabled<NhStandardProviderTests>("Clearing SessionForTest ({0} entities)", () => SetupHelper.SessionForTest.Statistics.EntityCount);
                               SetupHelper.SessionForTest.Clear();
                               LogHelper.TraceIfEnabled<NhStandardProviderTests>("Cleared SessionForTest ({0} entities)", () => SetupHelper.SessionForTest.Statistics.EntityCount);
                           };
            }
        }

        protected override ProviderSetup ProviderSetup
        {
            get { return SetupHelper.ProviderSetup; }
        }

        protected override ReadonlyProviderSetup ReadonlyProviderSetup
        {
            get { return SetupHelper.ReadonlyProviderSetup; }
        }

        protected override void DisposeResources()
        {
            if (SetupHelper != null)
                SetupHelper.Dispose();
        }
    }
}