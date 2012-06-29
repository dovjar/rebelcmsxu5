using System;
using System.IO;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Engine;
using NHibernate.Tool.hbm2ddl;
using Umbraco.Framework;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Attribution;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants.AttributeDefinitions;
using Umbraco.Framework.Persistence.NHibernate;
using Umbraco.Framework.Persistence.NHibernate.Dependencies;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig;
using Umbraco.Framework.Persistence.ProviderSupport._Revised;
using Umbraco.Framework.Persistence.RdbmsModel.Mapping;
using Umbraco.Framework.TypeMapping;
using Umbraco.Hive;
using Umbraco.Hive.ProviderSupport;
using Umbraco.Tests.Extensions.Stubs;

namespace Umbraco.Tests.Extensions
{
    using System.Collections.Concurrent;

    public class NhibernateTestSetupHelper : DisposableObject
    {
        /// <summary>
        /// This cache is used to speed up scenarios where multiple session factories are created for one application, e.g. in unit tests.
        /// Avoids creating the configuration every time for each new session factory.
        /// </summary>
        private static readonly ConcurrentDictionary<NhConfigurationCacheKey, ISessionFactory> SessionFactoryCache = new ConcurrentDictionary<NhConfigurationCacheKey, ISessionFactory>();

        private bool _useNhProfiler;


        public NhibernateTestSetupHelper(FakeFrameworkContext frameworkContext = null, bool useNhProf = false, string dataSource = "data source=:memory:", SupportedNHDrivers supportedNhDrivers = SupportedNHDrivers.SqlLite)
            : this(dataSource, supportedNhDrivers, "call", true, frameworkContext, useNhProf: useNhProf)
        {
        }

        public NhibernateTestSetupHelper(string dataSource, SupportedNHDrivers supportedNhDrivers, string sessionContext, bool executeSchema, FakeFrameworkContext frameworkContext = null, bool useNhProf = false, bool outputMappings = false, bool leaveSessionOpenOnRepositoryDispose = true, bool useSingleSessionForTest = true)
        {
            _useNhProfiler = useNhProf;
            var builder = new NHibernateConfigBuilder(dataSource, "unit-tester", supportedNhDrivers, sessionContext, outputMappings, _useNhProfiler);
            NhConfigurationCacheKey cacheKey;
            Config = builder.BuildConfiguration(false, out cacheKey, "on_close");
            SessionFactory = SessionFactoryCache.GetOrAdd(cacheKey, key => Config.BuildSessionFactory());
            SessionForTest = SessionFactory.OpenSession();
            var schemaWriter = new StringWriter(new StringBuilder());
            if (executeSchema) new SchemaExport(Config).Execute(false, true, false, SessionForTest.Connection, schemaWriter);

            FakeFrameworkContext = frameworkContext ?? new FakeFrameworkContext();
            
            //var dataContextFactory = new DataContextFactory(fakeFrameworkContext, SessionForTest, true);

            //var readWriteRepositoryUnitOfWorkFactory = new ReadWriteUnitOfWorkFactory();
            //var writer = new HiveReadWriteProvider(new HiveProviderSetup(fakeFrameworkContext, "r-unit-tester", new FakeHiveProviderBootstrapper(), null, readWriteRepositoryUnitOfWorkFactory, dataContextFactory));

            var providerMetadata = new ProviderMetadata("r-unit-tester", new Uri("tester://"), true, false);
            //var schemaRepositoryFactory = new NullProviderSchemaRepositoryFactory(providerMetadata, fakeFrameworkContext);
            //var revisionRepositoryFactory = new NullProviderRevisionRepositoryFactory<TypedEntity>(providerMetadata, fakeFrameworkContext);

            var revisionSchemaSessionFactory = new NullProviderRevisionRepositoryFactory<EntitySchema>(providerMetadata,
                                                                                                    FakeFrameworkContext);

            ProviderDependencyHelper helper = null;
            if (!useSingleSessionForTest)
            {
                helper = new DependencyHelper(new NhFactoryHelper(Config, null, leaveSessionOpenOnRepositoryDispose, false, FakeFrameworkContext), providerMetadata);
            }
            else
            {
                helper = new DependencyHelper(new NhFactoryHelper(null, SessionForTest, leaveSessionOpenOnRepositoryDispose, false, FakeFrameworkContext), providerMetadata);
            }

            var revisionRepositoryFactory = new RevisionRepositoryFactory(providerMetadata, FakeFrameworkContext, helper);

            var schemaRepositoryFactory = new SchemaRepositoryFactory(providerMetadata, revisionSchemaSessionFactory,
                                                                FakeFrameworkContext, helper);

            var entityRepositoryFactory = new EntityRepositoryFactory(providerMetadata, revisionRepositoryFactory,
                                                                schemaRepositoryFactory, FakeFrameworkContext,
                                                                helper);

            var readUnitFactory = new ReadonlyProviderUnitFactory(entityRepositoryFactory);
            var unitFactory = new ProviderUnitFactory(entityRepositoryFactory);
            ProviderSetup = new ProviderSetup(unitFactory, providerMetadata, FakeFrameworkContext, null, 0);
            ReadonlyProviderSetup = new ReadonlyProviderSetup(readUnitFactory, providerMetadata, FakeFrameworkContext, null, 0);

            Func<AbstractMappingEngine> addTypemapper = () => new ManualMapperv2(new NhLookupHelper(entityRepositoryFactory), providerMetadata);
            FakeFrameworkContext.TypeMappers.Add(new Lazy<AbstractMappingEngine, TypeMapperMetadata>(addTypemapper, new TypeMapperMetadata(true)));
        }

        public ISession SessionForTest { get; protected set; }
        public ProviderSetup ProviderSetup { get; protected set; }
        public ReadonlyProviderSetup ReadonlyProviderSetup { get; protected set; }
        public FakeFrameworkContext FakeFrameworkContext { get; protected set; }
        public Configuration Config { get; protected set; }
        public ISessionFactory SessionFactory { get; protected set; }

        protected override void DisposeResources()
        {
            if (_useNhProfiler && HibernatingRhinos.Profiler.Appender.ProfilerInfrastructure.IsInitalized)
            {
                HibernatingRhinos.Profiler.Appender.ProfilerInfrastructure.FlushAllMessages();
            }
            SessionForTest.Dispose();
            ProviderSetup.UnitFactory.Dispose();
            ReadonlyProviderSetup.ReadonlyUnitFactory.Dispose();
        }

        public static TypedEntity SetupTestData(Guid newGuid, Guid newGuidRedHerring, NhibernateTestSetupHelper repo)
        {
            var entity = HiveModelCreationHelper.MockTypedEntity();
            entity.Id = new HiveId(newGuid);
            entity.EntitySchema.Alias = "schema-alias1";

            var existingDef = entity.EntitySchema.AttributeDefinitions[0];
            var newDef = HiveModelCreationHelper.CreateAttributeDefinition("aliasForQuerying", "", "", existingDef.AttributeType, existingDef.AttributeGroup, true);
            entity.EntitySchema.AttributeDefinitions.Add(newDef);
            entity.Attributes.Add(new TypedAttribute(newDef, "my-new-value"));

            entity.Attributes[1].DynamicValue = "not-on-red-herring";
            entity.Attributes[NodeNameAttributeDefinition.AliasValue].Values["UrlName"] = "my-test-route";

            var redHerringEntity = HiveModelCreationHelper.MockTypedEntity();
            redHerringEntity.Id = new HiveId(newGuidRedHerring);
            redHerringEntity.EntitySchema.Alias = "redherring-schema";

            using (var uow = repo.ProviderSetup.UnitFactory.Create())
            {
                uow.EntityRepository.AddOrUpdate(entity);
                uow.EntityRepository.AddOrUpdate(redHerringEntity);
                uow.Complete();
            }

            return entity;
        }
    }
}