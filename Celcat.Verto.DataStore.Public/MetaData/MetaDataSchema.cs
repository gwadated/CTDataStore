namespace Celcat.Verto.DataStore.Public.MetaData
{
    using System;
    using System.Data.SqlClient;
    using System.Reflection;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Common.Schemas;
    using global::Common.Logging;

    internal class MetaDataSchema : SchemaBase
    {
        public const int LatestPublicDbVersion = 4; // the schema version of the public database
        public const string MetadataSchemaName = "METADATA";
        public const string ConfigTableName = "CONFIG";

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MetaDataSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }

        public Guid GetApplicationKey()
        {
            var result = Guid.Empty;

            _log.Debug("Getting application key guid in Public database");

            string sql = $"select app_key from {GetQualifiedTableName(ConfigTableName)}";

            var r = DatabaseUtils.ExecuteScalar(ConnectionString, sql, TimeoutSecs);
            if (r != null && r != DBNull.Value)
            {
                result = (Guid)r;
            }

            _log.DebugFormat("Application key guid = {0}", result.ToString());

            return result;
        }

        public void CreateTables(Guid appKey)
        {
            _log.DebugFormat("Creating metadata tables in public database");

            EnsureSchemaCreated();
            var b = new MetaDataTablesBuilder();
            b.Execute(ConnectionString, TimeoutSecs);

            InsertConfigRow(appKey);
        }

        private void InsertConfigRow(Guid appKey)
        {
            string configTable = GetQualifiedTableName(ConfigTableName);

            _log.DebugFormat("Inserting the single row in {0}", configTable);

            string sql =
                $"insert into {configTable} (public_database_version, app_key) values ({LatestPublicDbVersion}, @K)";

            SqlParameter[] p = { new SqlParameter("@K", appKey) };
            DatabaseUtils.ExecuteSql(ConnectionString, sql, TimeoutSecs, p);
        }

        /// <summary>
        /// Gets the public db version
        /// </summary>
        /// <returns>
        /// version
        /// </returns>
        public int GetDatabaseVersion()
        {
            int ver = 0;

            _log.Debug("Getting public db version");

            if (DatabaseUtils.TableExists(ConnectionString, TimeoutSecs, ConfigTableName, MetadataSchemaName))
            {
                string sql =
                    $"select public_database_version from {GetQualifiedTableName(ConfigTableName)}";

                try
                {
                    var result = DatabaseUtils.ExecuteScalar(ConnectionString, sql, TimeoutSecs);
                    if (result != null && result != DBNull.Value)
                    {
                        ver = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    // swallow exception
                    _log.Error("Error getting db version", ex);
                }
            }

            _log.DebugFormat("Public database version = {0}", ver);

            return ver;
        }

        protected override string SchemaName => MetadataSchemaName;
    }
}
