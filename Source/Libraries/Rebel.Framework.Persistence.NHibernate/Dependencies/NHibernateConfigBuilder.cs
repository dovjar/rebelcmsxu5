using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using HibernatingRhinos.Profiler.Appender.NHibernate;
using NHibernate;
using NHibernate.ByteCode.Castle;
using NHibernate.Event;
using NHibernate.Tool.hbm2ddl;
using Rebel.Framework.DependencyManagement;
using Rebel.Framework.Diagnostics;
using Rebel.Framework.Persistence.NHibernate.Config;
using Rebel.Framework.Persistence.NHibernate.OrmConfig;
using Rebel.Framework.Persistence.NHibernate.OrmConfig.FluentMappings;
using Rebel.Framework.Persistence.NHibernate.OrmConfig.FluentMappings.DialectMitigation;
using Environment = NHibernate.Cfg.Environment;

namespace Rebel.Framework.Persistence.NHibernate.Dependencies
{
    using System.Data;
    using System.Runtime.Serialization.Formatters.Binary;
    using global::NHibernate.Cfg;

    public class NHibernateConfigBuilder : IDependencyDemandBuilder
    {
        private readonly bool _outputNhMappings = false;
        private readonly string _alias;
        private readonly SupportedNHDrivers _nhDriver;
        private readonly string _sessionContext;
        private static readonly ReaderWriterLockSlim MappingLocker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private static readonly ReaderWriterLockSlim ConfigCacheLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private bool _useNhProfiler = false;

        public NHibernateConfigBuilder(string @alias, ProviderConfigurationSection localConfig)
            : this(localConfig.GetConnectionString(), alias, localConfig.Driver, localConfig.SessionContext, localConfig.OutputNhMappings, localConfig.UseNhProf)
        {
        }

        public NHibernateConfigBuilder(string connectionString, string @alias, SupportedNHDrivers nhDriver, string sessionContext, bool outputNhMappings, bool useNhProfiler)
        {
            _sessionContext = sessionContext;
            _outputNhMappings = outputNhMappings;
            ConnectionString = connectionString;
            _alias = alias;
            _nhDriver = nhDriver;
            _useNhProfiler = useNhProfiler;
        }

        public string ConnectionString { get; set; }

        public static Assembly GetRdbmsMapsAssembly() { return typeof(NodeMap).Assembly; }

        /// <summary>
        /// Builds the dependency demands required by this implementation.
        /// </summary>
        /// <param name="containerBuilder">The <see cref="IContainerBuilder"/> .</param>
        /// <param name="builderContext">The builder context.</param>
        public void Build(IContainerBuilder containerBuilder, IBuilderContext builderContext)
        {
            Mandate.ParameterNotNull(containerBuilder, "containerBuilder");
            Mandate.ParameterNotNull(builderContext, "builderContext");

            RegisterComponents(containerBuilder);
        }

        public const string CustomConnReleaseMode = "custom_per_db_engine";

        public global::NHibernate.Cfg.Configuration BuildConfiguration(bool showSql, out NhConfigurationCacheKey configurationCacheKey, string connReleaseMode)
        {
            return CreateConfiguration(_nhDriver, ConnectionString, GetRdbmsMapsAssembly(), showSql, true, _sessionContext, _outputNhMappings, _useNhProfiler, out configurationCacheKey, connReleaseMode);
        }

        public static void AddConventions<T>(SupportedNHDrivers nhDriver, SetupConventionFinder<T> conventions)
        {
            switch (nhDriver)
            {
                case SupportedNHDrivers.SqlLite:
                case SupportedNHDrivers.MsSqlCe4:
                case SupportedNHDrivers.MySql:
                    conventions.Add(new NormalizedDateTimeUserTypeConvention(), new NormalizedNullableDateTimeUserTypeConvention());
                    break;
            }
        }

        /// <summary>
        /// This cache is used to speed up scenarios where multiple session factories are created for one application, e.g. in unit tests.
        /// Avoids creating the configuration every time for each new session factory.
        /// </summary>
        private static readonly ConcurrentDictionary<NhConfigurationCacheKey, global::NHibernate.Cfg.Configuration> ConfigurationCache = new ConcurrentDictionary<NhConfigurationCacheKey, global::NHibernate.Cfg.Configuration>();

        private static string GetSerializationFileName(string connectionString)
        {
            var version = typeof(NHibernateConfigBuilder).Assembly.GetName().Version.ToString();
            var lastWrite = File.GetLastWriteTimeUtc(typeof(NHibernateConfigBuilder).Assembly.Location).ToString("yyMMddHHmmss");
            return "ConfigurationCache-" + version + "-" + connectionString.GetHashCode() + "-" + lastWrite + ".bin";
        }

        private static string GetConfigurationCacheFolder()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new Uri(codeBase);
            var path = uri.LocalPath;
            var binFolder = new DirectoryInfo(Path.GetDirectoryName(path));

            string configFolder = binFolder.FullName;
            var rootFolder = string.Empty;
            if (binFolder.Name.InvariantEquals("Debug"))
            {
                rootFolder = binFolder.Parent.Parent.FullName;
            }
            else if (binFolder.Name.InvariantEquals("bin"))
            {
                rootFolder = binFolder.Parent.FullName;
            }
            if (!rootFolder.IsNullOrWhiteSpace())
            {
                configFolder = Path.Combine(rootFolder, "App_Data", "Rebel", "HiveConfig");
                if (!Directory.Exists(configFolder))
                {
                    Directory.CreateDirectory(configFolder);
                }
            }
            return configFolder;
        }

        private static void SaveConfigurationToFile(Configuration configuration, string connectionString)
        {
            var fullPath = Path.Combine(GetConfigurationCacheFolder(), GetSerializationFileName(connectionString));
            SaveConfigurationToFile(fullPath, configuration);
        }

        private static void SaveConfigurationToFile(string path, Configuration configuration)
        {
            try
            {
                using(new WriteLockDisposable(ConfigCacheLocker))
                    using (var file = File.Open(path, FileMode.OpenOrCreate))
                    {
                        var bf = new BinaryFormatter();
                        bf.Serialize(file, configuration);
                        LogHelper.TraceIfEnabled<NHibernateConfigBuilder>("Cached NHibernate configuration to " + path);
                    }
            }
            catch (Exception ex)
            {
                LogHelper.Error<NHibernateConfigBuilder>("Warning, couldn't write config cache to " + path, ex);
            }
        }

        private static bool ConfigCacheFileExists(string connectionString)
        {
            var fullPath = Path.Combine(GetConfigurationCacheFolder(), GetSerializationFileName(connectionString));

            return File.Exists(fullPath);
        }

        private static Configuration DeserializeConfig(string connectionString)
        {
            var fullPath = Path.Combine(GetConfigurationCacheFolder(), GetSerializationFileName(connectionString));

            return DeserializeConfigFromPath(fullPath);
        }

        private static Configuration DeserializeConfigFromPath(string path)
        {
            Configuration toReturn;
            try
            {
                using (new WriteLockDisposable(ConfigCacheLocker))
                    using (var file = File.Open(path, FileMode.Open, FileAccess.Read))
                    {
                        var bf = new BinaryFormatter();
                        toReturn = bf.Deserialize(file) as Configuration;
                    }
            }
            catch (Exception ex)
            {
                LogHelper.TraceIfEnabled<NHibernateConfigBuilder>("Didn't get cached NHibernate configuration from " + path + "; " + ex.Message);
                toReturn = null;
            }

            if (toReturn != null)
                LogHelper.TraceIfEnabled<NHibernateConfigBuilder>("Got gached NHibernate configuration from " + path);

            return toReturn;
        }

        public static global::NHibernate.Cfg.Configuration CreateConfiguration(SupportedNHDrivers nhDriver, string connectionString, Assembly fluentMappingsAssembly, bool showSql, bool enablePostCommitListener, string sessionContext, bool outputNhMappings, bool useNhProfiler, out NhConfigurationCacheKey configurationCacheKey, string connReleaseMode)
        {
            Mandate.ParameterNotNullOrEmpty(connectionString, "connectionString");
            Mandate.ParameterNotNull(fluentMappingsAssembly, "fluentMappingsAssembly");
            Mandate.ParameterCondition(nhDriver != SupportedNHDrivers.Unknown, "nhDriver");

            configurationCacheKey = new NhConfigurationCacheKey(
                nhDriver,
                connectionString,
                fluentMappingsAssembly,
                showSql,
                enablePostCommitListener,
                sessionContext,
                outputNhMappings);

            return ConfigurationCache.GetOrAdd(
                configurationCacheKey,
                y =>
                    {
                        using (new WriteLockDisposable(MappingLocker))
                        {
                            if (useNhProfiler)
                            {
                                try
                                {
                                    NHibernateProfiler.Initialize();
                                }
                                catch (InvalidOperationException ex)
                                {
                                    //swallow security exceptions, happens if running in Medium trust    
                                    if (!(ex.InnerException is SecurityException))
                                    {
                                        throw ex;
                                    }
                                }
                            }

                            // Check if we already have a serialized Configuration object
                            // for this assembly version and assembly last-write date
                            using (new WriteLockDisposable(ConfigCacheLocker))
                                if (ConfigCacheFileExists(connectionString))
                                {
                                    var cachedConfig = DeserializeConfig(connectionString);
                                    if (cachedConfig != null) return cachedConfig;
                                }

                            // We haven't cached config before, or couldn't load it, so dynamically create it

                            // Figure out the FluentNH persistence configurer based on the desired driver
                            var persistenceConfigurer = GetPersistenceConfigurer(nhDriver, connectionString);

                            // Figure out the connection release mode to use, because SqlCe needs "on_close" for perf reasons,
                            // whereas we should use "auto" for everything else to avoid long-running connections
                            var trueConnReleaseMode = connReleaseMode; // Could have been set already by a unit test
                            if (connReleaseMode == NHibernateConfigBuilder.CustomConnReleaseMode) // Only modify if it's a value teling us to modify
                            {
                                if (nhDriver == SupportedNHDrivers.MsSqlCe4)
                                    trueConnReleaseMode = "on_close";
                                else
                                    trueConnReleaseMode = "auto";
                            }

                            // Configure NH using FluentNH
                            var fluentConfig = Fluently.Configure().Database(persistenceConfigurer)
                                //.Cache(x =>
                                //        x.UseMinimalPuts()
                                //        .UseQueryCache()
                                //        .UseSecondLevelCache()
                                //        .ProviderClass(typeof(global::NHibernate.Caches.SysCache2.SysCacheProvider).AssemblyQualifiedName))

                                // after_transaction does not work for unit tests with Sqlite
                                .ExposeConfiguration(c => c.SetProperty(Environment.ReleaseConnections, trueConnReleaseMode)
                                    .SetProperty(Environment.CurrentSessionContextClass, sessionContext)
                                    .SetProperty(Environment.GenerateStatistics, useNhProfiler.ToString().ToLower())
                                    .SetProperty(Environment.BatchSize, "20")
                                    .SetProperty(Environment.ProxyFactoryFactoryClass, typeof(ProxyFactoryFactory).AssemblyQualifiedName))
                                .Mappings(x =>
                                                {
                                                    // Add named queries from our embedded mappings file
                                                    x.HbmMappings.AddFromAssembly(fluentMappingsAssembly);

                                                    // Add class mappings
                                                    var container = x.FluentMappings.AddFromAssembly(fluentMappingsAssembly);
                                                    AddConventions(nhDriver, container.Conventions);
                                                });

                            if (showSql) fluentConfig.ExposeConfiguration(c => c.SetProperty(Environment.ShowSql, "true"));

                            try
                            {
                                // Generate the NHibernate configuration object from FluentNH
                                var nhConfig = fluentConfig.BuildConfiguration();

                                // Add a PostCommitInsert listener which is responsible for passing generated IDs back to the 
                                // mapped entities for entity insert scenarios (since the NH mapped objects aren't passed outside 
                                // of this provider)
                                if (enablePostCommitListener)
                                {
                                    var entitySaveEventListener = new NhEventListeners();
                                    nhConfig.SetListener(ListenerType.PostInsert, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.PostUpdate, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.PostCommitInsert, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.PreDelete, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.Delete, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.Merge, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.SaveUpdate, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.Flush, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.Evict, entitySaveEventListener);
                                    nhConfig.SetListener(ListenerType.PreDelete, entitySaveEventListener);
                                }

                                // Add the Aggregate interceptor for running trigger-like actions that NH can't handle
                                // Disabled, done by event listener instead now: nhConfig.SetInterceptor(new AggregateDataInterceptor());

                                // Add the FluentNH persistence model and configure NH to use it
                                var fluentAutoModel = new AutoPersistenceModel();

                                fluentAutoModel.AddMappingsFromAssembly(fluentMappingsAssembly);
                                fluentAutoModel.BuildMappings();
                                fluentAutoModel.Configure(nhConfig);

                                if (outputNhMappings)
                                {
                                    var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                                    var uri = new Uri(codeBase);
                                    var path = uri.LocalPath;
                                    var binFolder = new DirectoryInfo(Path.GetDirectoryName(path));
                                    string nhibernateOutputFolder;
                                    if (binFolder.Name == "Debug")
                                    {
                                        nhibernateOutputFolder = Path.Combine(
                                            binFolder.Parent.Parent.FullName, "App_Data", "Logs", "NHibernateConfig");
                                    }
                                    else
                                    {
                                        //its just 'bin'
                                        nhibernateOutputFolder = Path.Combine(
                                            binFolder.Parent.FullName, "App_Data", "Logs", "NHibernateConfig");
                                    }
                                    Directory.CreateDirectory(nhibernateOutputFolder);
                                    fluentAutoModel.WriteMappingsTo(nhibernateOutputFolder);
                                }

                                SaveConfigurationToFile(nhConfig, connectionString);

                                return nhConfig;
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException("Cannot build NHibernate configuration", ex);
                            }
                        }
                    });
        }

        public static IPersistenceConfigurer GetPersistenceConfigurer(SupportedNHDrivers nhDriver, string connString)
        {
            IPersistenceConfigurer persistenceConfigurer;
            switch (nhDriver)
            {
                default:
                case SupportedNHDrivers.MsSql2008:
                    persistenceConfigurer = MsSqlConfiguration
                        .MsSql2008
                        .ConnectionString(connString)
                        .DefaultSchema("dbo")
                        //.FormatSql()
                        .IsolationLevel(IsolationLevel.ReadCommitted)
                        .UseOuterJoin()
                        .UseReflectionOptimizer()
                        .Cache(x =>
                               x//.UseMinimalPuts()
                                   .UseQueryCache()
                                   .ProviderClass(
                                       typeof (global::NHibernate.Caches.SysCache2.SysCacheProvider).AssemblyQualifiedName));
                    break;
                case SupportedNHDrivers.SqlLite:
                    persistenceConfigurer = SQLiteConfiguration.Standard.ConnectionString(connString)
                        .Cache(x =>
                               x//.UseMinimalPuts()
                                   .UseQueryCache()
                                   .ProviderClass(
                                       typeof(global::NHibernate.Caches.SysCache2.SysCacheProvider).AssemblyQualifiedName));
                    break;
                case SupportedNHDrivers.MsSqlCe4:
                    var directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
                    var directoryPath = new Uri(directoryName).LocalPath;
                    var connectionString = connString.Replace("{bin}", directoryPath);

                    persistenceConfigurer = MsSqlCe4Configuration
                        .Standard
                        .ConnectionString(connectionString)
                        .AdoNetBatchSize(20)
                        .Cache(x =>
                               x//.UseMinimalPuts()
                                   .UseQueryCache()
                                   .ProviderClass(
                                       typeof(global::NHibernate.Caches.SysCache2.SysCacheProvider).AssemblyQualifiedName));

                    break;
                case SupportedNHDrivers.MySql:
                    persistenceConfigurer = MySQLConfiguration.Standard.ConnectionString(connString)
                        .Cache(x =>
                               x//.UseMinimalPuts()
                                   .UseQueryCache()
                                   .ProviderClass(
                                       typeof(global::NHibernate.Caches.SysCache2.SysCacheProvider).AssemblyQualifiedName));

                    break;
                case SupportedNHDrivers.Unknown:
                    throw new InvalidOperationException(
                        "Cannot get FluentNHibernate Persistence Configurer as the call to this method didn't specify a known driver");
            }
            return persistenceConfigurer;
        }

        public void RegisterComponents(IContainerBuilder builder)
        {
            LogHelper.TraceIfEnabled<NHibernateConfigBuilder>(
                "Registering NHibernate Configuration, ISessionFactory and ISession with IoC");

            builder
                .ForFactory(x =>
                    {
                        NhConfigurationCacheKey cacheKey;
                        return BuildConfiguration(false, out cacheKey, NHibernateConfigBuilder.CustomConnReleaseMode);
                    })
                .KnownAsSelf()
                .NamedForSelf(_alias)
                .ScopedAs.Singleton();
        }
    }
}