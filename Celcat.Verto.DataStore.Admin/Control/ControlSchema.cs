namespace Celcat.Verto.DataStore.Admin.Control
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Reflection;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Models;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Common.Schemas;
    using global::Common.Logging;

    internal class ControlSchema : SchemaBase
    {
        public const int LatestAdminDbVersion = 3; // the schema version of the admin database
        public const string ConfigTableName = "CONFIG";
        public const string LogTableName = "LOG";
        public const string ControlSchemaName = "CONTROL";
        public const string SrcTimetableName = "SOURCE_TIMETABLE";

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal ControlSchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }

        protected override string SchemaName => ControlSchemaName;

        /// <summary>
        /// creates table in the Admin db, CONTROL schema
        /// </summary>
        /// <returns>
        /// App key guid
        /// </returns>
        public Guid CreateTables()
        {
            _log.DebugFormat("Creating control tables in admin database");

            EnsureSchemaCreated();
            var b = new ControlTablesBuilder();
            b.Execute(ConnectionString, TimeoutSecs);

            return InsertConfigRow();
        }

        private Guid InsertConfigRow()
        {
            string configTable = GetQualifiedTableName(ConfigTableName);

            Guid appKey = Guid.NewGuid();

            _log.DebugFormat("Inserting the single row in {0}", configTable);

            string sql =
                $"insert into {configTable} (admin_database_version, app_key) values ({LatestAdminDbVersion}, @K)";

            SqlParameter[] p = { new SqlParameter("@K", appKey) };
            DatabaseUtils.ExecuteSql(ConnectionString, sql, TimeoutSecs, p);

            return appKey;
        }

        /// <summary>
        /// Gets the application Guid - a value that ties Admin and Public databases
        /// together as part of the same application.
        /// </summary>
        /// <returns></returns>
        public Guid GetApplicationKey()
        {
            var result = Guid.Empty;

            if (DatabaseUtils.DatabaseExists(ConnectionString, TimeoutSecs) &&
                DatabaseUtils.TableExists(ConnectionString, TimeoutSecs, ConfigTableName, SchemaName))
            {
                _log.Debug("Getting application key guid");

                var sql = $"select app_key from {GetQualifiedTableName(ConfigTableName)}";

                var r = DatabaseUtils.ExecuteScalar(ConnectionString, sql, TimeoutSecs);
                if (r != null && r != DBNull.Value)
                {
                    result = (Guid)r;
                }

                _log.DebugFormat("Application key guid = {0}", result.ToString());
            }

            return result;
        }

        private SourceTimetableRecord GetSourceTimetableRecord(string identifierColName, object identifierValue)
        {
            var sb = new SqlBuilder();
            sb.Append("select src_timetable_id, timetable_name, server_name, database_name, schema_version, guid");
            sb.AppendFormat("from {0} where {1} = @ID", GetQualifiedTableName(SrcTimetableName), identifierColName);

            SqlParameter[] sqlParameters = { new SqlParameter("@ID", identifierValue) };

            SourceTimetableRecord result = null;

            DatabaseUtils.GetSingleResult(
                ConnectionString, 
                sb.ToString(), 
                TimeoutSecs, 
                r =>
                {
                    result = new SourceTimetableRecord
                    {
                        Id = (int)r["src_timetable_id"],
                        Name = (string)r["timetable_name"],
                        SqlServerName = (string)r["server_name"],
                        DatabaseName = (string)r["database_name"],
                        SchemaVersion = (int)r["schema_version"],
                        Identifier = (Guid)r["guid"]
                    };
                }, 
                sqlParameters);

            return result;
        }

        public SourceTimetableRecord GetSourceTimetableRecord(Guid identifier)
        {
            _log.DebugFormat("Getting source timetable record by guid: {0}", identifier.ToString());
            return GetSourceTimetableRecord("guid", identifier);
        }

        public IReadOnlyList<SourceTimetableRecord> GetSourceTimetableRecords()
        {
            var sb = new SqlBuilder();
            sb.Append("select src_timetable_id, timetable_name, server_name, database_name, schema_version, guid");
            sb.AppendFormat("from {0}", GetQualifiedTableName(SrcTimetableName));

            var result = new List<SourceTimetableRecord>();

            DatabaseUtils.EnumerateResults(ConnectionString, sb.ToString(), TimeoutSecs, r =>
            {
                var rec = new SourceTimetableRecord
                {
                    Name = (string)r["timetable_name"],
                    SqlServerName = (string)r["server_name"],
                    DatabaseName = (string)r["database_name"],
                    SchemaVersion = (int)r["schema_version"],
                    Identifier = (Guid)r["guid"]
                };

                result.Add(rec);
            });

            return result;
        }

        /// <summary>
        /// Gets the admin db version
        /// </summary>
        /// <returns>
        /// version
        /// </returns>
        public int GetDatabaseVersion()
        {
            int ver = 0;

            _log.Debug("Getting admin db version");

            if (DatabaseUtils.TableExists(ConnectionString, TimeoutSecs, ConfigTableName, SchemaName))
            {
                var sql = $"select admin_database_version from {GetQualifiedTableName(ConfigTableName)}";
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

            _log.DebugFormat("Admin database version = {0}", ver);

            return ver;
        }

        /// <summary>
        /// Inserts an entry in the log
        /// </summary>
        /// <param name="task">
        /// Task summary
        /// </param>
        /// <param name="description">
        /// More details of the task
        /// </param>
        /// <returns>
        /// log id (BigInt)
        /// </returns>
        public long Log(string task, string description)
        {
            var sql =
                $"insert into {GetQualifiedTableName(LogTableName)} (task, description) values (@T, @D); select @@IDENTITY";

            SqlParameter[] p = { new SqlParameter("@T", task), new SqlParameter("@D", description) };

            return Convert.ToInt64(DatabaseUtils.ExecuteScalar(ConnectionString, sql, TimeoutSecs, p));
        }
    }
}
