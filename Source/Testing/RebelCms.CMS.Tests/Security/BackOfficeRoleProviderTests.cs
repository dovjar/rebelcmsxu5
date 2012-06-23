using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using RebelCms.Cms.Web.Security;
using RebelCms.Framework;
using RebelCms.Framework.Persistence.Model;
using RebelCms.Framework.Persistence.Model.Constants;
using RebelCms.Framework.Persistence.Model.Constants.Entities;
using RebelCms.Framework.Persistence.ProviderSupport._Revised;
using RebelCms.Hive;
using RebelCms.Hive.Configuration;
using RebelCms.Hive.ProviderGrouping;
using RebelCms.Hive.ProviderSupport;
using RebelCms.Hive.RepositoryTypes;
using RebelCms.Tests.Extensions.Stubs;

namespace RebelCms.Tests.Cms.Security
{
    //[TestFixture]
    //public class BackOfficeRoleProviderTests : StandardWebTest
    //{
    //    private BackOfficeRoleProvider _roleProvider;

    //    [Test]
    //    public void BackOfficeRoleProviderTests_AddUsersToRoles_Success()
    //    {
    //        //Arrange
    //        var users = new[] {"Administrator"};
    //        var roles = new[] {"Administrator", "Editor"};

    //        //Act
    //        _roleProvider.AddUsersToRoles(users, roles);

    //        //Assert

    //    }

    //    [SetUp]
    //    public void Initialize()
    //    {
    //        base.Init();

    //        var hiveManager = CreateHiveManager();

    //        _roleProvider = new BackOfficeRoleProvider(new Lazy<IHiveManager>(() => hiveManager));
    //    }
    //}
}
