using System;
using System.Configuration;
using System.Data.SqlServerCe;
using System.Xml.Linq;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Umbraco.Framework.Configuration;
using Umbraco.Framework.Dynamics;
using Umbraco.Framework.Persistence.NHibernate.Config;
using Umbraco.Framework.Persistence.NHibernate.OrmConfig;
using Umbraco.Framework.ProviderSupport;
using AbstractProviderBootstrapper = Umbraco.Hive.AbstractProviderBootstrapper;
using Environment = NHibernate.Cfg.Environment;

namespace Umbraco.Framework.Persistence.NHibernate
{
    using System.Linq;
    using System.Threading;
    using Umbraco.Framework.Diagnostics;
    using Umbraco.Framework.Persistence.NHibernate.OrmConfig.FluentMappings;

    public class ProviderBootstrapper : AbstractProviderBootstrapper
    {

        private readonly global::NHibernate.Cfg.Configuration _configuration;
        private readonly ProviderConfigurationSection _localConfig;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderBootstrapper"/> class if sufficient configuration information has been supplied by the user.
        /// </summary>        
        /// <param name="configuration">The configuration.</param>
        /// <param name="localConfig">The existing config.</param>
        /// <remarks></remarks>
        public ProviderBootstrapper(global::NHibernate.Cfg.Configuration configuration, ProviderConfigurationSection localConfig)
        {
            _localConfig = localConfig;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates any necessary configuration files/transforms for the provider to operate
        /// </summary>
        /// <param name="providerKey">The provider key for the provider that is being configured</param>
        /// <param name="configXml">The configuration xml file that needs to be written to</param>
        /// <param name="installParams">TODO: This is only a temporary way of passing arbitrary parameters to a provider to create its configuration,
        /// we need to allow hive providers to return a model for which we display a form/installer for and then pass in that
        /// model to the installParams</param>
        public override void ConfigureApplication(string providerKey, XDocument configXml, BendyObject installParams)
        {
            dynamic dynamicParams = installParams;
            string dbType = dynamicParams.DatabaseType.ToString();

            var connectionString = "";
            var providerName = "";
            var nhDriver = "";
            //we need to create the connection strings if it's not custom

            switch (dbType)
            {
                case "MSSQL":
                    connectionString = string.Format("Data Source={0}; Initial Catalog={1};User Id={2};Password={3}",
                                                     dynamicParams.Server, dynamicParams.DatabaseName, dynamicParams.Username, dynamicParams.Password);
                    providerName = "System.Data.SqlClient";
                    nhDriver = "MsSql2008";
                    break;
                case "MySQL":
                    connectionString = string.Format("Server={0}; Database={1};Uid={2};Pwd={3}",
                                                     dynamicParams.Server, dynamicParams.DatabaseName, dynamicParams.Username, dynamicParams.Password);
                    providerName = "MySql.Data.MySQLClient";
                    nhDriver = "MySql";
                    break;
                case "SQLCE":
                    connectionString = "Data Source=|DataDirectory|Umbraco.sdf";
                    providerName = "System.Data.SqlServerCe.4.0";
                    nhDriver = "MsSqlCe4";
                    break;
                case "Custom":
                    //limiting to MS SQL atm 
                    connectionString = dynamicParams.ConnectionString;
                    providerName = "System.Data.SqlClient";
                    nhDriver = "MsSql2008";
                    break;
            }

            var connstringKey = "";

            var hiveElement = new ProviderConfigurationSection()
                       {
                           ConnectionStringKey = "{0}.ConnString",
                           Driver = SupportedNHDrivers.MsSqlCe4,
                           SessionContext = "web"
                       };

            var elementName = providerKey;
    
            hiveElement.DriverAsString = nhDriver;
            connstringKey = string.Format(hiveElement.ConnectionStringKey, providerKey);
            hiveElement.ConnectionStringKey = connstringKey;

            DeepConfigManager.SerializeProviderConfigSection(configXml, hiveElement, "umbraco/persistenceProviderSettings/" + elementName, true);

            //add the connection strings
            var connStrings = new ConnectionStringsSection();
            connStrings.ConnectionStrings.Add(new ConnectionStringSettings(connstringKey, connectionString, providerName));
            //now serialize the connection strings to the config
            var connectionStringElement = DeepConfigManager.SerializeProviderConfigSection(configXml, connStrings, "connectionStrings", false);
            var newConnString = new XElement("add");
            DeepConfigManager.AddPropertiesToElement(connStrings.ConnectionStrings[0], newConnString);
            connectionStringElement.Add(newConnString);
            
            // The following is superceded by the above to support multiple "add" references: DeepConfigManager.SerializeProviderConfigSection(configXml, connStrings.ConnectionStrings[0], "connectionStrings/add", false);
        }

        public override InstallStatus GetInstallStatus()
        {

            if (_configuration == null || _localConfig == null)
            {
                return new InstallStatus(InstallStatusType.RequiresConfiguration);
            }

            var installStatus = new InstallStatus(InstallStatusType.Pending);
            //now, let check if we've actually been installed
            try
            {
                var isValid = ValidateSchema(_configuration);
                if (isValid)
                {
                    installStatus = new InstallStatus(InstallStatusType.Completed);
                }
            }
            catch (Exception ex)
            {
                //if an exception is thrown other than an HibernateException then its probably attempted an install and failed
                //such as invalid connection details
                installStatus = new InstallStatus(InstallStatusType.TriedAndFailed, ex);
            }

            return installStatus;
        }

        public override InstallStatus TryInstall()
        {
            switch (_localConfig.Driver)
            {
                case SupportedNHDrivers.MsSqlCe4:
                    using (new WriteLockDisposable(SchemaValidationLocker))
                    {
                        using (var sqlCeEngine = new SqlCeEngine(_configuration.Properties[Environment.ConnectionString]))
                        {
                            if (!sqlCeEngine.Verify())
                            {
                                sqlCeEngine.CreateDatabase();
                            }
                        }
                    }
                    break;
            }

            InstallStatus installStatus;
            try
            {
                var schemaAlreadyValid = ValidateSchema(_configuration);
                if (schemaAlreadyValid) 
                    return new InstallStatus(InstallStatusType.Completed);
                UpdateSchema(_configuration);
                installStatus = new InstallStatus(InstallStatusType.Completed);
            }
            catch (Exception ex)
            {
                installStatus = new InstallStatus(InstallStatusType.TriedAndFailed, ex);
            }
            return installStatus;
        }

        private static readonly ReaderWriterLockSlim SchemaValidationLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public bool ValidateSchema(global::NHibernate.Cfg.Configuration configuration)
        {
            using (new WriteLockDisposable(SchemaValidationLocker))
            {
                var myvalidator = new SchemaValidator(configuration);
                try
                {
                    myvalidator.Validate();
                    return true;
                }
                catch (HibernateException ex)
                {
                    /* SchemaValidator.Validate() returns void - FFS */
                    LogHelper.Error<ProviderBootstrapper>("While running SchemaValidator: " + ex.Message, ex);

                    // New in 5.2 (limited support for schema upgrades - pending full migration support)
                    // Use our own validator to actually get back metadata about the missing tables rather than
                    // just an exception of the first failure
                    // Then if it's something we're OK to try to handle, try to handle it...
                    if (_localConfig.AutoUpdateDbSchema)
                    {
                        var customValidator = new SchemaChangeValidator(configuration);
                        var result = customValidator.Validate();
                        if (!result.IsValid)
                        {
                            // 5.2: Just check for whether AggregateNodeStatus is the only missing one
                            if (result.MissingTables.Any())
                            {
                                var missingTables = string.Join(", ", result.MissingTables.Select(x => x.Name));
                                LogHelper.Warn<ProviderBootstrapper>("The following tables are missing from the database: {0}", missingTables);

                                var agg = result.MissingTables.FirstOrDefault(x => x.Name == typeof(AggregateNodeStatus).Name);
                                if (agg != null && result.MissingTables.Count == 1)
                                {
                                    // It's the only missing table, so we've already done one install
                                    LogHelper.Warn<ProviderBootstrapper>("Automatically attempting to update the database schema to add the following missing tables: {0}. You can prevent this behaviour by setting '{1}' to false in the configuration for this provider", missingTables, ProviderConfigurationSection.XAutoUpdateSchema);
                                    try
                                    {
                                        UpdateSchema(configuration);
                                        // Everything went OK, so can return true since we're now "valid"
                                        return true;
                                    }
                                    catch (Exception updateEx)
                                    {
                                        LogHelper.Error<ProviderBootstrapper>("Auto-update of db schema failed. Does the db user have the correct permissions? If you need to manually run the update script, the script should be in the logfile preceding this entry", updateEx);
                                    }
                                }
                            }
                        }
                    }
                    return false;
                }
            }
        }

        public static void UpdateSchema(global::NHibernate.Cfg.Configuration configuration)
        {
            using (new WriteLockDisposable(SchemaValidationLocker))
            {
                using (DisposableTimer.TraceDuration<ProviderBootstrapper>("Begin db schema update", "End db schema update"))
                {
                    var schema = new SchemaUpdate(configuration);
                    schema.Execute(x => LogHelper.TraceIfEnabled<ProviderBootstrapper>("NHibernate generated the following update Sql: \n" + x), true);
                }
            }
        }
    }
}