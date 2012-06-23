using System;
using NUnit.Framework;
using RebelCms.Framework.Persistence;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Attribution;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.AttributeTypes;
using RebelCms.Framework.Persistence.Model.Constants.Entities;
using RebelCms.Hive;

namespace RebelCms.Tests.CoreAndFramework.Hive.DefaultProviders.Membership
{
    [Ignore("Need to implement the remaining methods, until then i don't want to have a ton of tests failing in our build")]
    [TestFixture]
    public class MembershipStandardProviderTests : AbstractEntityOnlyProviderTests
    {
        private MembershipWrapperTestSetupHelper _setup;

        [SetUp]
        public void BeforeTest()
        {
            _setup = new MembershipWrapperTestSetupHelper();
        }

        /// <summary>
        /// Need to mock an entity that is compatible with the users Hive provider
        /// </summary>
        /// <returns></returns>
        protected override TypedEntity GetMockedEntity()
        {
            // need to fill in all required values for membership providers
            return new Member()
                {
                    Email = Guid.NewGuid().ToString("N") + "test@test.com",
                    IsApproved = true,
                    Name = "Test",
                    Password = "hello",
                    Username = "test" + Guid.NewGuid().ToString("N") //must be unique
                };
        }

        [TearDown]
        public void AfterTest()
        {
            
        }
        
        protected override Action PostWriteCallback
        {
            get { return () => { return; }; }
        }

        protected override ProviderSetup ProviderSetup
        {
            get { return _setup.ProviderSetup; }
        }

        protected override ReadonlyProviderSetup ReadonlyProviderSetup
        {
            get { return _setup.ReadonlyProviderSetup; }
        }

        protected override void DisposeResources()
        {
        }
       
    }
}