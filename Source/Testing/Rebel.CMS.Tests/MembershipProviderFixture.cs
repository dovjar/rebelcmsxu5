using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rebel.Tests.Cms
{
    using System.Collections.Specialized;
    using System.Web.Security;
    using NUnit.Framework;
    using Rebel.Cms.Web.DependencyManagement;
    using Rebel.Cms.Web.Security;
    using Rebel.Cms.Web.Tasks;
    using Rebel.Framework.Persistence;
    using Rebel.Framework.Security;
    using Rebel.Hive;
    using Rebel.Hive.Configuration;
    using Rebel.Tests.Extensions;

    [TestFixture]
    public class MembershipProviderFixture
    {
        private NhibernateTestSetupHelper _nhibernateTestSetup;
        protected IHiveManager Hive;
        protected MembersMembershipProvider Provider;

        [SetUp]
        public void BeforeTest()
        {
            _nhibernateTestSetup = new NhibernateTestSetupHelper(useNhProf:true);

            var storageProvider = new IoHiveTestSetupHelper(_nhibernateTestSetup.FakeFrameworkContext);


            Hive =
                new HiveManager(
                    new[]
                        {
                            new ProviderMappingGroup(
                                "test1",
                                new WildcardUriMatch("content://"),
                                _nhibernateTestSetup.ReadonlyProviderSetup,
                                _nhibernateTestSetup.ProviderSetup,
                                _nhibernateTestSetup.FakeFrameworkContext),

                            new ProviderMappingGroup(
                                "test2",
                                new WildcardUriMatch("security://members"),
                                _nhibernateTestSetup.ReadonlyProviderSetup,
                                _nhibernateTestSetup.ProviderSetup,
                                _nhibernateTestSetup.FakeFrameworkContext),
                                storageProvider.CreateGroup("uploader", "storage://")
                        },
                    _nhibernateTestSetup.FakeFrameworkContext);

            var appContext = new FakeRebelApplicationContext(Hive, true);

            AttributeTypeRegistry.SetCurrent(new CmsAttributeTypeRegistry());

            var task = new EnsureCoreDataTask(Hive.FrameworkContext,
                                              Hive,
                                              Enumerable.Empty<Lazy<Permission, PermissionMetadata>>(),
                                              null);
            task.InstallOrUpgrade();

            Provider = new MembersMembershipProvider(Hive, appContext);
            Provider.Initialize("MembersMembershipProvider", new NameValueCollection());
        }

        [Test]
        public void CanDeleteUser()
        {
            MembershipCreateStatus status;
            var user = Provider.CreateUser("test user",
                                "test password",
                                "email",
                                "question",
                                "answer",
                                true,
                                Guid.NewGuid(),
                                out status);

            Assert.That(status, Is.EqualTo(MembershipCreateStatus.Success));

            var check = Provider.GetUser("test user", false);

            Assert.That(check, Is.Not.Null);

            Provider.DeleteUser("test user", true);

            check = Provider.GetUser("test user", false);
            Assert.That(check, Is.Null);

        }

        [Test]
        public void FindUsersByEmail()
        {
            MembershipCreateStatus status;
            Provider.CreateUser("test user 1",
                                "test password",
                                "email1@thismail.com",
                                "question",
                                "answer",
                                true,
                                Guid.NewGuid(),
                                out status);

            Provider.CreateUser("test user 2",
                                "test password",
                                "email2@thismail.com",
                                "question",
                                "answer",
                                true,
                                Guid.NewGuid(),
                                out status);

            Provider.CreateUser("test user 3",
                                "test password",
                                "something@anotherdomain.com",
                                "question",
                                "answer",
                                true,
                                Guid.NewGuid(),
                                out status);

            int totalRecords = 0;
            var found = Provider.FindUsersByEmail("email1@thismail.com", 0, 10, out totalRecords);

            Assert.That(found, Is.Not.Null);
            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(totalRecords, Is.EqualTo(1));

            found = Provider.FindUsersByEmail("email*", 0, 10, out totalRecords);

            Assert.That(found, Is.Not.Null);
            Assert.That(found.Count, Is.EqualTo(2));
            Assert.That(totalRecords, Is.EqualTo(2));

            found = Provider.FindUsersByEmail("*@anotherdomain.com", 0, 10, out totalRecords);

            Assert.That(found, Is.Not.Null);
            Assert.That(found.Count, Is.EqualTo(1));
            Assert.That(totalRecords, Is.EqualTo(1));
        }

        [Test]
        public void CanUpdateMember()
        {
            MembershipCreateStatus status;
            var user = Provider.CreateUser("test user",
                                "test password",
                                "email",
                                "question",
                                "answer",
                                true,
                                Guid.NewGuid(),
                                out status);

            Assert.That(status, Is.EqualTo(MembershipCreateStatus.Success));
            user = Provider.GetUser("test user", false);
            user.Comment = "hello";
            Provider.UpdateUser(user);
        }

        [Test]
        public void CreateUserChecksForDupes()
        {
            MembershipCreateStatus status;
            var user = Provider.CreateUser("test user",
                                "test password",
                                "email",
                                "question",
                                "answer",
                                true,
                                Guid.NewGuid(),
                                out status);

            Assert.That(status, Is.EqualTo(MembershipCreateStatus.Success));

            MembershipCreateStatus status2;
            var user2 = Provider.CreateUser("test user",
                                "test password",
                                "email",
                                "question",
                                "answer",
                                true,
                                Guid.NewGuid(),
                                out status);

            Assert.That(status, Is.EqualTo(MembershipCreateStatus.DuplicateUserName));

            MembershipCreateStatus status3;
            var user3 = Provider.CreateUser("different user",
                                "test password",
                                "email",
                                "question",
                                "answer",
                                true,
                                Guid.NewGuid(),
                                out status);

            Assert.That(status, Is.EqualTo(MembershipCreateStatus.DuplicateEmail));
        }
    }
}
