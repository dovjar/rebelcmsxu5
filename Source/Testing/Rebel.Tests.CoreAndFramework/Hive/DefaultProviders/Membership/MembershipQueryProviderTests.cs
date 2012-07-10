using System;
using NUnit.Framework;
using Rebel.Framework;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Rebel.Framework.Persistence.Model.Constants.Entities;
using Rebel.Hive;
using Rebel.Hive.ProviderGrouping;

namespace Rebel.Tests.CoreAndFramework.Hive.DefaultProviders.Membership
{
    //[TestFixture]
    //public class MembershipQueryProviderTests : AbstractProviderQueryTests
    //{
    //    [SetUp]
    //    public void BeforeTest()
    //    {
    //        _setup = new MembershipWrapperTestSetupHelper();
    //        _groupUnitFactory = new GroupUnitFactory(_setup.ProviderSetup, new Uri("content://"));
    //    }

    //    private MembershipWrapperTestSetupHelper _setup;
    //    private GroupUnitFactory _groupUnitFactory;
        
    //    protected override TypedEntity CreateEntityForTest(Guid newGuid, Guid newGuidRedHerring, ProviderSetup providerSetup)
    //    {
    //        var entity = new Member()
    //            {
    //                Id = new HiveId(newGuid),
    //                Email = Guid.NewGuid().ToString("N") + "test@test.com",
    //                IsApproved = true,
    //                Name = "Test",
    //                Password = "hello",
    //                Username = "test" + Guid.NewGuid().ToString("N") //must be unique
    //            };

    //        entity.Username = "my-new-value";
    //        entity.Name = "not-on-red-herring";
    //        entity.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "my-test-route";

    //        //var redHerringEntity = HiveModelCreationHelper.MockTypedEntity();
    //        //redHerringEntity.Id = new HiveId(newGuidRedHerring);
    //        //redHerringEntity.EntitySchema.Alias = "redherring-schema";

    //        using (var uow = providerSetup.UnitFactory.Create())
    //        {
    //            uow.EntityRepository.AddOrUpdate(entity);
    //            //uow.EntityRepository.AddOrUpdate(redHerringEntity);
    //            uow.Complete();
    //        }

    //        return entity;
    //    }

    //    protected override ProviderSetup ProviderSetup
    //    {
    //        get { return _setup.ProviderSetup; }
    //    }

    //    protected override GroupUnitFactory GroupUnitFactory
    //    {
    //        get { return _groupUnitFactory; }
    //    }
    //}
}