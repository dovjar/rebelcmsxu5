namespace Rebel.Framework.Persistence.NHibernate.OrmConfig
{
    #region Imports

    using System;
    using System.Collections.Generic;
    using Rebel.Framework.Diagnostics;
    using global::NHibernate;
    using global::NHibernate.Cfg;
    using global::NHibernate.Dialect;
    using global::NHibernate.Dialect.Schema;
    using global::NHibernate.Id;
    using global::NHibernate.Mapping;
    using global::NHibernate.Tool.hbm2ddl;
    using global::NHibernate.Util;

    #endregion

    /// <summary>
    /// Performs tailored checking of database schema, for example when schema changes are introduced between versions of this provider
    /// </summary>
    /// <remarks>Based on the code of SchemaValidator in the NHibernate 3.1 codebase</remarks>
    public class SchemaChangeValidator
    {
        private readonly Configuration _configuration;
        private readonly IConnectionHelper _connectionHelper;
        private readonly Dialect _dialect;

        public SchemaChangeValidator(Configuration cfg)
            : this(cfg, cfg.Properties)
        {
        }

        public SchemaChangeValidator(Configuration cfg, IDictionary<string, string> connectionProperties)
        {
            this._configuration = cfg;
            this._dialect = Dialect.GetDialect(connectionProperties);
            var cfgProperties = new Dictionary<string, string>(this._dialect.DefaultProperties);
            foreach (var keyValuePair in connectionProperties)
                cfgProperties[keyValuePair.Key] = keyValuePair.Value;
            this._connectionHelper = new ManagedProviderConnectionHelper(cfgProperties);
        }

        public ValidationResult Validate()
        {
            LogHelper.TraceIfEnabled<SchemaChangeValidator>("Running schema validator");
            try
            {
                DatabaseMetadata databaseMetadata;
                try
                {
                    LogHelper.TraceIfEnabled<SchemaChangeValidator>("Fetching db metadata");
                    this._connectionHelper.Prepare();
                    databaseMetadata = new DatabaseMetadata(this._connectionHelper.Connection, this._dialect, false);
                }
                catch (Exception ex)
                {
                    LogHelper.Error<SchemaChangeValidator>("Failed getting db metadata: " + ex.Message, ex);
                    throw;
                }
                return ValidateSchema(_dialect, databaseMetadata);
            }
            catch (Exception ex)
            {
                LogHelper.Error<SchemaChangeValidator>("Schema validation failed: " + ex.Message, ex);
                throw;
            }
            finally
            {
                try
                {
                    this._connectionHelper.Release();
                }
                catch (Exception ex)
                {
                    LogHelper.Error<SchemaChangeValidator>("Error closing connection: " + ex.Message, ex);
                }
            }
        }

        public class ValidationResult
        {
            public ValidationResult()
            {
                MissingTables = new List<Table>();
                MissingColumns = new Dictionary<Table, List<Column>>();
            }

            public ValidationResult(Exception errorWhileTrying)
            {
                ErrorWhileTrying = errorWhileTrying;
            }

            public bool IsValid { get { return !(ErrorWhileTrying != null || MissingTables.Any() || MissingColumns.Any() || InvalidColumns.Any()); } }
            public Exception ErrorWhileTrying { get; protected set; }

            public IList<Table> MissingTables { get; protected set; }
            public IDictionary<Table, List<Column>> MissingColumns { get; protected set; }
            public IDictionary<Table, List<Column>> InvalidColumns { get; protected set; }

            public void AddMissingColumn(Table table, Column column)
            {
                AddColumn(table, column, MissingColumns);
            }

            public void AddInvalidColumn(Table table, Column column)
            {
                AddColumn(table, column, InvalidColumns);
            }

            private static void AddColumn(Table table, Column column, IDictionary<Table, List<Column>> dict)
            {
                if (dict.ContainsKey(table))
                {
                    dict[table].Add(column);
                }
                else
                {
                    dict.Add(table,
                                       new List<Column>()
                                           {
                                               column
                                           });
                }
            }
        }

        public ValidationResult ValidateSchema(Dialect dialect, DatabaseMetadata databaseMetadata)
        {
            _configuration.BuildMappings();
            var mappings = _configuration.CreateMappings(_dialect);
            var mapping = _configuration.BuildMapping();

            var result = new ValidationResult();

            try
            {
                var catalog = PropertiesHelper.GetString("default_catalog", _configuration.Properties, null);
                var schema = PropertiesHelper.GetString("default_schema", _configuration.Properties, null);
                foreach (var table in mappings.IterateTables)
                {
                    if (!table.IsPhysicalTable || !Configuration.IncludeAction(table.SchemaActions, SchemaAction.Validate))
                        continue;

                    var tableMetadata = databaseMetadata.GetTableMetadata(table.Name, table.Schema ?? schema, table.Catalog ?? catalog, table.IsQuoted);
                    if (tableMetadata == null)
                    {
                        result.MissingTables.Add(table);
                    }
                    else
                    {
                        foreach (var column in table.ColumnIterator)
                        {
                            var columnMetadata = tableMetadata.GetColumnMetadata(column.Name);
                            if (columnMetadata == null)
                            {
                                result.AddMissingColumn(table, column);
                            }
                            if (!column.GetSqlType(dialect, mapping).ToLower().StartsWith(columnMetadata.TypeName.ToLower()))
                            {
                                result.AddInvalidColumn(table, column);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<SchemaChangeValidator>(ex.Message, ex);
                result = new ValidationResult(ex);
            }

            return result;
        }
    }
}