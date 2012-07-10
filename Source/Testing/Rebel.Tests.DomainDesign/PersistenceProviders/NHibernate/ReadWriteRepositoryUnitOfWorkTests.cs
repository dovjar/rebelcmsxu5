using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

using Rebel.Framework;
using Rebel.Framework.DataManagement;
using Rebel.Framework.Persistence.Model;
using Rebel.Framework.Persistence.Model.Attribution.MetaData;
using Rebel.Framework.Persistence.NHibernate;
using Rebel.Framework.Persistence.NHibernate.Config;
using Rebel.Framework.Persistence.NHibernate.DataManagement;
using Rebel.Framework.Persistence.NHibernate.DependencyDemandBuilders;
using Rebel.Framework.Persistence.ProviderSupport;
using Rebel.Framework.Persistence.RdbmsModel.Mapping;
using Rebel.Framework.ProviderSupport;
using Rebel.Framework.TypeMapping;
using Rebel.Tests.Extensions;
using Rebel.Tests.Extensions.Stubs;

namespace Rebel.Tests.DomainDesign.PersistenceProviders.NHibernate
{
    [TestClass]
    public class ReadWriteRepositoryUnitOfWorkTests
    {
        private static ISession _sessionForTest;

        private static HiveReadWriteProvider CreateReadWritter()
        {
            var builder = new NHibernateConfigBuilder("data source=:memory:", "unit-tester",
                                                      SupportedNHDrivers.SqlLite, "thread_static", false);
            var config = builder.BuildConfiguration();
            var sessionFactory = config.BuildSessionFactory();
            _sessionForTest = sessionFactory.OpenSession();
            var schemaWriter = new StringWriter(new StringBuilder());
            new SchemaExport(config).Execute(false, true, false, _sessionForTest.Connection, schemaWriter);

            var fakeFrameworkContext = new FakeFrameworkContext();
            var dataContextFactory = new DataContextFactory(fakeFrameworkContext, _sessionForTest, true);

            var readWriteRepositoryUnitOfWorkFactory = new ReadWriteUnitOfWorkFactory();
            var writer = new HiveReadWriteProvider(new HiveProviderSetup(fakeFrameworkContext, "r-unit-tester", new FakeHiveProviderBootstrapper(), null, readWriteRepositoryUnitOfWorkFactory, dataContextFactory));

            fakeFrameworkContext.TypeMappers.Add(
                new Lazy<AbstractTypeMapper, TypeMapperMetadata>(
                    () => new ManualMapper(new NhLookupHelper(dataContextFactory), writer), new TypeMapperMetadata(true)));
            
            return writer;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ReadWriteRepositoryUnitOfWorkTests_GetEntityWithNullId_ResultsInNullReferenceException()
        {
            //Arrange
            using (var writer = CreateReadWritter())
            {

                //Act

                //Assert
                using (var uow = writer.CreateReadWriteUnitOfWork())
                {
                    uow.ReadWriteRepository.GetEntity<TypedEntity>(HiveId.Empty);
                }
            }
        }

        [TestMethod]
        public void ReadWriteRepositoryUnitOfWorkTests_UncommitedUnitOfWork_DoesNotUpdateDataStore()
        {
            //arrange
            var writer = CreateReadWritter();

            //act
            var id = new HiveId(Guid.NewGuid());
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                var entity = HiveModelCreationHelper.CreateAttributeType("alias", "name", "description");
                entity.Id = id;
                uow.ReadWriteRepository.AddOrUpdate(entity);
            }

            _sessionForTest.Clear();

            //assert
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                Assert.IsNull(uow.ReadWriteRepository.GetEntity<AttributeType>(id));
            }

            writer.Dispose();
        }

        [TestMethod]
        public void ReadWriteRepositoryUnitOfWorkTests_OnceUnitOfWorkCommitted_DataStoreUpdated()
        {
            //Arrange
            var writer = CreateReadWritter();

            //Act
            HiveId id;
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                var entity = HiveModelCreationHelper.CreateAttributeType("alias", "name", "description");
                uow.ReadWriteRepository.AddOrUpdate(entity);
                uow.Commit();
                id = entity.Id;
            }

            _sessionForTest.Clear();

            //assert
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                Assert.IsNotNull(uow.ReadWriteRepository.GetEntity<AttributeType>(id));
            }

            writer.Dispose();
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectDisposedException))]
        public void ReadWriteRepositoryUnitOfWorkTests_OnceDisposed_ExceptionsThrown()
        {
            //Arrange
            var writer = CreateReadWritter();

            //Act
            var uow = writer.CreateReadWriteUnitOfWork();
            uow.Dispose();

            var entity = HiveModelCreationHelper.CreateAttributeType("alias", "name", "description");
            uow.ReadWriteRepository.AddOrUpdate(entity);

            //Assert
            try
            {
                Assert.Fail("Exception should have been raised when adding to the writer as it's disposed");
            }
            finally
            {
                writer.Dispose();
            }

        }

        [TestMethod]
        public void ReadWriteRepositoryUnitOfWorkTests_AddOneRemoveOne_EntityDoesntExist()
        {
            //Arrange
            var writer = CreateReadWritter();
            HiveId id;
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                var entity = HiveModelCreationHelper.CreateAttributeType("alias", "name", "description");
                uow.ReadWriteRepository.AddOrUpdate(entity);
                uow.Commit();
                id = entity.Id;
            }

            _sessionForTest.Clear();

            //Act

            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                uow.ReadWriteRepository.Delete<AttributeType>(id);
                uow.Commit();
            }

            //assert
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                Assert.IsNull(uow.ReadWriteRepository.GetEntity<AttributeType>(id));
            }

            writer.Dispose();
        }

        [TestMethod]
        public void ReadWriteRepositoryUnitOfWorkTests_UpdatedEntity_AccessableAcrossContext()
        {
            //Arrange
            var writer = CreateReadWritter();
            HiveId id;
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                var entity = HiveModelCreationHelper.CreateAttributeType("alias", "name", "description");
                uow.ReadWriteRepository.AddOrUpdate(entity);
                uow.Commit();
                id = entity.Id;
            }

            _sessionForTest.Clear();

            //Act
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                var entity = uow.ReadWriteRepository.GetEntity<AttributeType>(id);
                entity.Description = "foo";
                uow.ReadWriteRepository.AddOrUpdate(entity); // Note that persistence entities are not change-tracking proxies, they are disconnected from the backing store
                uow.Commit();
            }

            _sessionForTest.Clear();


            //Assert
            using (var uow = writer.CreateReadWriteUnitOfWork())
            {
                var entity = uow.ReadWriteRepository.GetEntity<AttributeType>(id);
                Assert.IsNotNull(entity);
                Assert.AreEqual("foo", (string)entity.Description);
            }

            writer.Dispose();
        }
    }
}
