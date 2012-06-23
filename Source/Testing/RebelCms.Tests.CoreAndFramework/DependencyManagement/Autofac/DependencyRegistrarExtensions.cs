using NUnit.Framework;
using RebelCms.Framework.DependencyManagement;
using RebelCms.Framework.DependencyManagement.Autofac;
using RebelCms.Framework.Testing.PartialTrust;

namespace RebelCms.Tests.CoreAndFramework.DependencyManagement.Autofac
{
    [TestFixture]
    public class DependencyRegistrarExtensions : AbstractPartialTrustFixture<DependencyRegistrarExtensions>
    {
        public class Foo
        {
        }

        [Test]
        public void DependencyRegistrarExtensions_KnwonAsSelf_RegistersTypeAsItself()
        {
            //Arrange
            var builder = new AutofacContainerBuilder();

            //Act
            builder.For(typeof(Foo))
                .KnownAsSelf();
            var resolver = builder.Build();

            //Assert
            Assert.IsNotNull(resolver.Resolve<Foo>());
        }

        /// <summary>
        /// Run once before each test in derived test fixtures.
        /// </summary>
        public override void TestSetup()
        {
            return;
        }

        /// <summary>
        /// Run once after each test in derived test fixtures.
        /// </summary>
        public override void TestTearDown()
        {
            return;
        }
    }
}
