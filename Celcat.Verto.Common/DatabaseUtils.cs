namespace Celcat.Verto.Common
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Caching;
    using global::Common.Logging;

    public static class DatabaseUtils
    {
        public const string StdSchemaName = "dbo";
        private const string DefMasterDbName = "master";
        private const int CacheLifetimeMinutes = 15;

        private static readonly MemoryCache _memoryCache = new MemoryCache("VertoDatabaseUtilsCache");
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void ResultsSetHandler(IDataRecord rec);

        public static string GetConnectionDescription(string connectionString, string schema = null)
        {
            var csb = new SqlConnectionStringBuilder(connectionString);
            var result = string.Concat(csb.DataSource, ".", csb.InitialCatalog);
            if (schema != null && !schema.Equals(StdSchemaName, StringComparison.OrdinalIgnoreCase))
            {
                return string.Concat(result, ".", schema);
            }

            return result;
        }

        /// <summary>
        /// gets a list of the tables in the specified schema
        /// </summary>
        /// <param name="connectionString">
        /// Connection string to the database
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="useCache">
        /// Determines whether the table list should be cached for 15 mins
        /// </param>
        /// <param name="schemaName">
        /// Schema name to search for
        /// </param>
        /// <returns>
        /// List of table names
        /// </returns>
        public static IReadOnlyList<string> GetTablesInSchema(
            string connectionString,
            int commandTimeoutSecs,
            bool useCache = true,
            string schemaName = StdSchemaName)
        {
            _log.DebugFormat("Getting tables in: {0}", GetConnectionDescription(connectionString, schemaName));

            var dbKey = CreateDbNameKey(connectionString);
            var key = string.Concat(dbKey, ".TableList");

            List<string> result;

            var o = useCache ? _memoryCache.Get(key) : null;
            if (o == null)
            {
                result = new List<string>();

                var sql = "select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = @S and TABLE_TYPE = 'BASE TABLE'";
                SqlParameter[] p = { new SqlParameter("@S", schemaName) };

                EnumerateResults(
                    connectionString,
                    sql,
                    commandTimeoutSecs,
                    rec => { result.Add((string)rec["TABLE_NAME"]); }, 
                    p);

                if (useCache)
                {
                    _memoryCache.Set(key, result, DateTimeOffset.UtcNow.AddMinutes(CacheLifetimeMinutes));
                }
            }
            else
            {
                _log.Debug("Using cached tables in schema");
                result = (List<string>)o;
            }

            return result;
        }
        
        /// <summary>
        /// Determines if a table exists
        /// </summary>
        /// <param name="connectionString">
        /// Database connection string
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="tableName">
        /// Name of table
        /// </param>
        /// <param name="schemaName">
        /// Name of schema to look in
        /// </param>
        /// <returns>
        /// True if the table exists
        /// </returns>
        public static bool TableExists(
            string connectionString, 
            int commandTimeoutSecs,
            string tableName, 
            string schemaName = StdSchemaName)
        {
            var table = GetTableDescription(connectionString, tableName, schemaName);
            _log.DebugFormat("Does table exist: {0}?", table);

            var sql = "select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = @S and TABLE_NAME = @T";
            SqlParameter[] p = { new SqlParameter("@S", schemaName), new SqlParameter("@T", tableName) };

            var result = ExecuteScalar(connectionString, sql, commandTimeoutSecs, p);
            var exists = result != null && result != DBNull.Value;

            _log.DebugFormat("{0}: {1}", table, BoolAsYesNo(exists));

            return exists;
        }

        public static bool TableExists(
            TransactionContext tc, 
            int commandTimeoutSecs,
            string tableName, 
            string schemaName = StdSchemaName)
        {
            var table = GetTableDescription(tc.Connection.ConnectionString, tableName, schemaName);
            _log.DebugFormat("Does table exist: {0}?", table);

            var sql = "select TABLE_NAME from INFORMATION_SCHEMA.TABLES where TABLE_SCHEMA = @S and TABLE_NAME = @T";
            SqlParameter[] p = { new SqlParameter("@S", schemaName), new SqlParameter("@T", tableName) };

            var result = ExecuteScalar(tc, sql, commandTimeoutSecs, p);
            var exists = result != null && result != DBNull.Value;

            _log.DebugFormat("{0}: {1}", table, BoolAsYesNo(exists));

            return exists;
        }

        /// <summary>
        /// Determines if a named schema exists in the specified database
        /// </summary>
        /// <param name="connectionString">
        /// Database connection string
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="schemaName">
        /// Schema to search for 
        /// </param>
        /// <returns>
        /// True if it exists
        /// </returns>
        public static bool SchemaExists(string connectionString, int commandTimeoutSecs, string schemaName)
        {
            var schema = GetConnectionDescription(connectionString, schemaName);
            _log.DebugFormat("Does schema exist: {0}?", schema);

            var sql = "select SCHEMA_NAME from information_schema.SCHEMATA where SCHEMA_NAME = @S";
            SqlParameter[] p = { new SqlParameter("@S", schemaName) };

            var result = ExecuteScalar(connectionString, sql, commandTimeoutSecs, p);
            var exists = result != null && result != DBNull.Value;

            _log.DebugFormat("{0}: {1}", schema, BoolAsYesNo(exists));

            return exists;
        }

        /// <summary>
        /// Gets a table schema
        /// </summary>
        /// <param name="connectionString">
        /// Connection string to database
        /// </param>
        /// <param name="tableName">
        /// Name of table to examine
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="useCache">
        /// Determines whether the schema should be cached for 15 mins
        /// </param>
        /// <param name="schemaName">
        /// The name of the schema to use
        /// </param>
        /// <returns>
        /// The schema DataTable
        /// </returns>
        public static IReadOnlyList<DatabaseColumnDefinition> GetSchemaForTable(
            string connectionString, 
            string tableName,
            int commandTimeoutSecs, 
            bool useCache = true, 
            string schemaName = StdSchemaName)
        {
            _log.DebugFormat("Getting schema for table: {0}", GetTableDescription(connectionString, tableName, schemaName));

            var dbKey = CreateDbNameKey(connectionString);
            var key = string.Concat(dbKey, ".TableSchema", ".", GetQualifiedTableName(schemaName, tableName));

            var result = new List<DatabaseColumnDefinition>();

            var o = _memoryCache.Get(key);
            if (o == null)
            {
                var sql = new SqlBuilder();
                sql.Append("select column_name, column_default, is_nullable, data_type, character_maximum_length");
                sql.Append("from INFORMATION_SCHEMA.COLUMNS");
                sql.Append("where table_name = @T");
                sql.Append("and table_schema = @S");
                sql.Append("order by ORDINAL_POSITION");

                SqlParameter[] sqlParameters = { new SqlParameter("@T", tableName), new SqlParameter("@S", schemaName) };

                EnumerateResults(
                    connectionString, 
                    sql.ToString(), 
                    commandTimeoutSecs, 
                    rec =>
                    {
                        var cd = new DatabaseColumnDefinition
                        {
                            Name = (string)rec["column_name"],
                            DefaultValue = (string)SafeRead(rec, "column_default", string.Empty),
                            Nullable = ((string)rec["is_nullable"]).Equals("YES", StringComparison.OrdinalIgnoreCase),
                            DataType = (string)rec["data_type"],
                            CharacterMaxLength = (int)SafeRead(rec, "character_maximum_length", 0)
                        };

                        if (cd.CharacterMaxLength == -1)
                        {
                            cd.CharacterMaxLength = int.MaxValue;
                        }

                        result.Add(cd);
                    }, 
                    sqlParameters);

                if (useCache)
                {
                    _memoryCache.Set(key, result, DateTimeOffset.UtcNow.AddMinutes(CacheLifetimeMinutes));
                }
            }
            else
            {
                _log.Debug("Using cache");
                result = (List<DatabaseColumnDefinition>)o;
            }

            return result;
        }

        public static object SafeRead(IDataRecord r, string columnName, object defaultValue)
        {
            var o = r[columnName];
            return o == DBNull.Value ? defaultValue : o;
        }

        /// <summary>
        /// Determines if a database exists
        /// </summary>
        /// <param name="connectionString">
        /// Connection string specifying server, database etc
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="masterDbName">
        /// Name of the master database
        /// </param>
        /// <returns>
        /// True if database exists
        /// </returns>
        public static bool DatabaseExists(
            string connectionString, 
            int commandTimeoutSecs,
            string masterDbName = DefMasterDbName)
        {
            var dbDesc = GetConnectionDescription(connectionString);
            _log.DebugFormat("Does database exist: {0}?", dbDesc);

            var databaseName = ExtractDatabaseName(connectionString);

            // switch to master...
            var csb = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = masterDbName };

            using (var c = CreateConnection(csb.ToString()))
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandTimeout = commandTimeoutSecs;
                cmd.CommandText = "select name from sys.databases where name = @N";

                try
                {
                    cmd.Parameters.AddWithValue("@N", databaseName);
                    var o = cmd.ExecuteScalar();
                    var exists = o != null && o != DBNull.Value;

                    _log.DebugFormat("{0}: {1}", dbDesc, BoolAsYesNo(exists));
                    return exists;
                }
                catch (Exception ex)
                {
                    _log.Error("Error checking database exists", ex);
                    _log.Error(cmd.CommandText);
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates and open a connection
        /// </summary>
        /// <param name="connectionString">
        /// The connection string
        /// </param>
        /// <returns>
        /// The opened connection
        /// </returns>
        public static SqlConnection CreateConnection(string connectionString)
        {
            var c = new SqlConnection(connectionString);
            c.Open();
            return c;
        }

        public static void EnumerateResults(
            TransactionContext tc, 
            string commandText, 
            int commandTimeoutSecs,
            ResultsSetHandler handler, 
            SqlParameter[] sqlParams = null)
        {
            if (!string.IsNullOrEmpty(commandText))
            {
                if (tc.Status == TransactionStatus.NotStarted)
                {
                    tc.Begin();
                }

                EnumerateResults(tc.Connection, tc.Transaction, commandText, commandTimeoutSecs, handler, sqlParams);
            }
        }

        public static void EnumerateResults(
            SqlConnection conn, 
            SqlTransaction tran, 
            string commandText, 
            int commandTimeoutSecs,
            ResultsSetHandler handler, 
            SqlParameter[] sqlParams = null)
        {
            using (var cmd = conn.CreateCommand())
            {
                if (tran != null)
                {
                    cmd.Transaction = tran;
                }

                cmd.CommandTimeout = commandTimeoutSecs;
                cmd.CommandText = commandText;

                try
                {
                    if (sqlParams != null)
                    {
                        cmd.Parameters.AddRange(sqlParams);
                    }

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            handler(r);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Error getting results set", ex);
                    _log.Error(cmd.CommandText);
                    throw;
                }
            }
        }

        public static void EnumerateResults(
            string connectionString, 
            string commandText, 
            int commandTimeoutSecs,
            ResultsSetHandler handler, 
            SqlParameter[] sqlParams = null)
        {
            using (var c = CreateConnection(connectionString))
            {
                EnumerateResults(c, null, commandText, commandTimeoutSecs, handler, sqlParams);
            }
        }

        public static void GetSingleResult(
            string connectionString, 
            string commandText, 
            int commandTimeoutSecs,
            ResultsSetHandler handler, 
            SqlParameter[] sqlParams = null)
        {
            using (var c = CreateConnection(connectionString))
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandTimeout = commandTimeoutSecs;
                cmd.CommandText = commandText;

                try
                {
                    if (sqlParams != null)
                    {
                        cmd.Parameters.AddRange(sqlParams);
                    }

                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            handler(r);

                            if (r.Read())
                            {
                                throw new ApplicationException($"More than 1 result row found in GetSingleResult! - {commandText}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Error getting results set", ex);
                    _log.Error(cmd.CommandText);
                    throw;
                }
            }
        }

        public static int ExecuteSql(
            string connectionString, 
            string commandText,
            int commandTimeoutSecs, 
            SqlParameter[] sqlParams = null)
        {
            int numRowsAffected = 0;

            if (!string.IsNullOrEmpty(commandText))
            {
                using (var c = CreateConnection(connectionString))
                {
                    numRowsAffected = ExecuteSql(c, null, commandText, commandTimeoutSecs, sqlParams);
                }
            }

            return numRowsAffected;
        }

        public static int ExecuteSql(
            TransactionContext t, 
            string commandText, 
            int commandTimeoutSecs, 
            SqlParameter[] sqlParams = null)
        {
            var numRowsAffected = 0;

            if (!string.IsNullOrEmpty(commandText))
            {
                if (t.Status == TransactionStatus.NotStarted)
                {
                    t.Begin();
                }

                numRowsAffected = ExecuteSql(t.Connection, t.Transaction, commandText, commandTimeoutSecs, sqlParams);
            }

            return numRowsAffected;
        }

        public static int ExecuteSql(
            SqlConnection conn, 
            SqlTransaction tran, 
            string commandText,
            int commandTimeoutSecs, 
            SqlParameter[] sqlParams = null)
        {
            var numRowsAffected = 0;

            if (!string.IsNullOrEmpty(commandText))
            {
                using (var cmd = conn.CreateCommand())
                {
                    if (tran != null)
                    {
                        cmd.Transaction = tran;
                    }

                    cmd.CommandTimeout = commandTimeoutSecs;
                    cmd.CommandText = commandText;

                    try
                    {
                        if (sqlParams != null)
                        {
                            cmd.Parameters.AddRange(sqlParams);
                        }

                        numRowsAffected = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Error executing SQL statement", ex);
                        _log.Error(cmd.CommandText);
                        throw;
                    }
                }
            }

            return numRowsAffected;
        }

        public static object ExecuteScalar(
            TransactionContext tc, 
            string commandText,
            int commandTimeoutSecs, 
            SqlParameter[] sqlParams = null)
        {
            object result = null;

            if (!string.IsNullOrEmpty(commandText))
            {
                if (tc.Status == TransactionStatus.NotStarted)
                {
                    tc.Begin();
                }

                result = ExecuteScalar(tc.Connection, tc.Transaction, commandText, commandTimeoutSecs, sqlParams);
            }

            return result;
        }

        public static object ExecuteScalar(
            string connectionString, 
            string commandText,
            int commandTimeoutSecs, 
            SqlParameter[] sqlParams = null)
        {
            using (var c = CreateConnection(connectionString))
            {
                return ExecuteScalar(c, null, commandText, commandTimeoutSecs, sqlParams);
            }
        }

        public static object ExecuteScalar(
            SqlConnection c, 
            SqlTransaction tran, 
            string commandText,
            int commandTimeoutSecs, 
            SqlParameter[] sqlParams = null)
        {
            object result;

            using (var cmd = c.CreateCommand())
            {
                if (tran != null)
                {
                    cmd.Transaction = tran;
                }

                cmd.CommandTimeout = commandTimeoutSecs;
                cmd.CommandText = commandText;

                try
                {
                    if (sqlParams != null)
                    {
                        cmd.Parameters.AddRange(sqlParams);
                    }

                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    _log.Error("Error executing SQL statement", ex);
                    _log.Error(cmd.CommandText);
                    throw;
                }
            }

            return result;
        }

        public static string GetQualifiedTableName(string schemaName, string tableName)
        {
            return string.IsNullOrEmpty(schemaName) 
                ? EscapeDbObject(tableName) 
                : $"{EscapeDbObject(schemaName)}.{EscapeDbObject(tableName)}";
        }

        public static string EscapeDbObject(string obj)
        {
            return !obj.Contains("[") 
                ? string.Concat("[", obj, "]") 
                : obj;
        }
        
        /// <summary>
        /// Truncates specified table
        /// </summary>
        /// <param name="connectionString">
        /// Connection string specifying the SQL Server, etc
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="tableName">
        /// Name of table to be truncated
        /// </param>
        /// <param name="schemaName">
        /// Name of the target schema
        /// </param>
        public static void TruncateTable(
            string connectionString, 
            int commandTimeoutSecs, 
            string tableName,
            string schemaName = StdSchemaName)
        {
            _log.DebugFormat("Truncating table: {0}", GetQualifiedTableName(schemaName, tableName));
            ExecuteSql(connectionString, $"TRUNCATE table {GetQualifiedTableName(schemaName, tableName)}", commandTimeoutSecs);
        }

        /// <summary>
        /// Empties the specified table
        /// </summary>
        /// <param name="connectionString">
        /// Connection string specifying the SQL Server, etc
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="tableName">
        /// Name of table to be emptied
        /// </param>
        /// <param name="schemaName">
        /// Name of the target schema
        /// </param>
        public static void EmptyTable(
            string connectionString, 
            int commandTimeoutSecs, 
            string tableName,
            string schemaName = StdSchemaName)
        {
            _log.DebugFormat("Emptying table: {0}", GetQualifiedTableName(schemaName, tableName));
            ExecuteSql(connectionString, $"DELETE from {GetQualifiedTableName(schemaName, tableName)}", commandTimeoutSecs);
        }

        /// <summary>
        /// Truncates all tables of the specified schema
        /// </summary>
        /// <param name="connectionString">
        /// Connection string specifying the SQL Server, etc
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="schemaName">
        /// Name of the target schema
        /// </param>
        public static void TruncateTablesInSchema(
            string connectionString, 
            int commandTimeoutSecs,
            string schemaName = StdSchemaName)
        {
            _log.DebugFormat("Truncating tables in schema: {0}", GetConnectionDescription(connectionString, schemaName));
            OperateOnSchemaTables("TRUNCATE TABLE", connectionString, commandTimeoutSecs, schemaName);
        }

        /// <summary>
        /// Empties all tables of the specified schema
        /// </summary>
        /// <param name="connectionString">
        /// Connection string specifying the SQL Server, etc
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="schemaName">
        /// Name of the target schema
        /// </param>
        public static void EmptyTablesInSchema(
            string connectionString, 
            int commandTimeoutSecs,
            string schemaName = StdSchemaName)
        {
            _log.DebugFormat("Emptying tables in schema: {0}", GetConnectionDescription(connectionString, schemaName));
            OperateOnSchemaTables("DELETE from", connectionString, commandTimeoutSecs, schemaName);
        }

        /// <summary>
        /// Drops all tables of the specified schema
        /// </summary>
        /// <param name="connectionString">
        /// Connection string specifying the SQL Server, etc
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="schemaName">
        /// Name of the target schema
        /// </param>
        public static void DropTablesInSchema(
            string connectionString, 
            int commandTimeoutSecs,
            string schemaName = StdSchemaName)
        {
            _log.DebugFormat("Dropping tables in schema: {0}", GetConnectionDescription(connectionString, schemaName));
            OperateOnSchemaTables("DROP TABLE", connectionString, commandTimeoutSecs, schemaName);
        }

        public static IEnumerable<string> GetSchemasInDatabase(string connectionString, int commandTimeoutSecs)
        {
            _log.DebugFormat("Getting schemas in database: {0}", GetConnectionDescription(connectionString));

            var result = new List<string>();

            var sql = "select distinct TABLE_SCHEMA from INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";

            EnumerateResults(connectionString, sql, commandTimeoutSecs, rec =>
            {
                result.Add((string)rec["TABLE_SCHEMA"]);
            });

            return result;
        }

        /// <summary>
        /// Moves all tables in one schema to another
        /// </summary>
        /// <param name="connectionString">
        /// Database connection string
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="oldSchema">
        /// Original schema name
        /// </param>
        /// <param name="newSchema">
        /// New schema name
        /// </param>
        /// <returns>Number of rows affected</returns>
        public static int MoveTablesToNewSchema(
            string connectionString, 
            int commandTimeoutSecs,
            string oldSchema, 
            string newSchema)
        {
            _log.DebugFormat("Moving tables in schema {0} to {1}", GetConnectionDescription(connectionString, oldSchema), newSchema);

            // clear the way...
            DropTablesInSchema(connectionString, commandTimeoutSecs, newSchema);

            var tables = GetTablesInSchema(connectionString, commandTimeoutSecs, false, oldSchema);

            var sb = new SqlBuilder();
            foreach (var table in tables)
            {
                sb.Append($"ALTER SCHEMA {newSchema} TRANSFER {oldSchema}.{table};");
            }

            return ExecuteSql(connectionString, sb.ToString(), commandTimeoutSecs);
        }

        /// <summary>
        /// Creates a named schema
        /// </summary>
        /// <param name="connectionString">
        /// The connection string specifying the target database
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="schemaName">
        /// The name of the schema to create
        /// </param>
        public static void CreateSchema(string connectionString, int commandTimeoutSecs, string schemaName)
        {
            _log.DebugFormat("Creating schema: {0}", GetConnectionDescription(connectionString, schemaName));

            using (var c = CreateConnection(connectionString))
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandTimeout = commandTimeoutSecs;
                cmd.CommandText = GenerateSchemaSql(schemaName);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _log.Error("Error creating schema", ex);
                    _log.Error(cmd.CommandText);
                    throw;
                }
            }
        }

        /// <summary>
        /// Creates a new database using default parameters
        /// </summary>
        /// <param name="connectionString">
        /// Connection string specifying the server and the database to create
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="masterDbName">
        /// Name of the server's master database
        /// </param>
        public static void CreateDatabase(string connectionString, int commandTimeoutSecs, string masterDbName = DefMasterDbName)
        {
            _log.DebugFormat("Creating database: {0}", GetConnectionDescription(connectionString));

            var databaseName = ExtractDatabaseName(connectionString);

            // switch to master db...
            var csb = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = masterDbName };

            using (var c = CreateConnection(csb.ToString()))
            using (var cmd = c.CreateCommand())
            {
                cmd.CommandTimeout = commandTimeoutSecs;
                cmd.CommandText = GenerateDatabaseCreateSql(databaseName);

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    _log.Error("Error creating database", ex);
                    _log.Error(cmd.CommandText);
                    throw;
                }
            }
        }

        /// <summary>
        /// Drops a database
        /// </summary>
        /// <param name="connectionString">
        /// Connection string specifying target server and db to drop
        /// </param>
        /// <param name="commandTimeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="silent">
        /// 'silent' mode will not throw if the database doesn't exist
        /// </param>
        /// <param name="masterDbName">
        /// Name of master database
        /// </param>
        public static void DropDatabase(
            string connectionString, 
            int commandTimeoutSecs,
            bool silent = true, 
            string masterDbName = DefMasterDbName)
        {
            _log.DebugFormat("Dropping database: {0}", GetConnectionDescription(connectionString));

            if (silent && !DatabaseExists(connectionString, commandTimeoutSecs, masterDbName))
            {
                // no-op
            }
            else
            {
                var databaseName = ExtractDatabaseName(connectionString);

                // switch to master db
                var csb = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = masterDbName };

                using (var c = CreateConnection(csb.ToString()))
                using (var cmd = c.CreateCommand())
                {
                    cmd.CommandTimeout = commandTimeoutSecs;
                    cmd.CommandText = GenerateDatabaseDropSql(databaseName);

                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Error dropping database", ex);
                        _log.Error(cmd.CommandText);
                        throw;
                    }
                }
            }
        }

        public static ConnectionStringSettings CreateConnectionStringSettings(string connectionString)
        {
            var sb = new SqlConnectionStringBuilder(connectionString);
            return new ConnectionStringSettings("Verto", sb.ToString(), "System.Data.SqlClient");
        }
        
        /// <summary>
        /// Gets information about primary keys
        /// </summary>
        /// <param name="connectionString">
        /// Database connection string
        /// </param>
        /// <param name="timeoutSecs">
        /// Command timeout
        /// </param>
        /// <param name="useCache">
        /// Whether to use a cached version
        /// </param>
        /// <returns>Primary key info</returns>
        public static Dictionary<string, PrimaryKeyInfo> GetPrimaryKeyInfo(string connectionString, int timeoutSecs, bool useCache = true)
        {
            var results = new Dictionary<string, PrimaryKeyInfo>(StringComparer.OrdinalIgnoreCase);

            var dbKey = CreateDbNameKey(connectionString);
            var key = string.Concat(dbKey, ".PrimaryKeyInfo");

            var o = useCache ? _memoryCache.Get(key) : null;
            if (o == null)
            {
                var sb = new SqlBuilder();

                sb.Append("select tc.table_name, c.column_name, c.ordinal_position");
                sb.Append("from INFORMATION_SCHEMA.table_constraints tc");
                sb.Append("inner join INFORMATION_SCHEMA.key_column_usage col");
                sb.Append("on col.constraint_name = tc.constraint_name");
                sb.Append("and col.constraint_schema = tc.constraint_schema");
                sb.Append("inner join INFORMATION_SCHEMA.COLUMNS c");
                sb.Append("on c.column_name = col.column_name");
                sb.Append("and c.table_name = tc.table_name");
                sb.Append("where tc.constraint_type = 'Primary Key'");
                sb.Append("order by table_name, ordinal_position");

                EnumerateResults(connectionString, sb.ToString(), timeoutSecs, r =>
                {
                    var tableName = (string)r["table_name"];
                    var colName = (string)r["column_name"];
                    var colPosition = (int)r["ordinal_position"];

                    if (!results.TryGetValue(tableName, out var keys))
                    {
                        keys = new PrimaryKeyInfo
                        {
                            TableName = tableName,
                            Columns = new List<ColumnNameAndPosition>()
                        };

                        results.Add(tableName, keys);
                    }

                    keys.Columns.Add(new ColumnNameAndPosition { ColumnName = colName, Position = colPosition });
                });

                if (useCache)
                {
                    _memoryCache.Set(key, results, DateTimeOffset.UtcNow.AddMinutes(CacheLifetimeMinutes));
                }
            }
            else
            {
                _log.Debug("Using cached primary key info");
                results = (Dictionary<string, PrimaryKeyInfo>)o;
            }

            return results;
        }

        /// <summary>
        /// Get foreign key info for all tables
        /// </summary>
        /// <param name="connectionString">
        /// Database connection string
        /// </param>
        /// <param name="timeoutSecs">
        /// Command timeout in secs
        /// </param>
        /// <param name="useCache">
        /// Whether to use a cached version
        /// </param>
        /// <returns>
        /// Foreign key info keyed by fk table name
        /// </returns>
        public static Dictionary<string, List<ForeignKeyDetails>> GetForeignKeyInfo(
            string connectionString, 
            int timeoutSecs, 
            bool useCache = true)
        {
            var result = new Dictionary<string, List<ForeignKeyDetails>>(StringComparer.OrdinalIgnoreCase);

            var dbKey = CreateDbNameKey(connectionString);
            var key = string.Concat(dbKey, ".ForeignKeyInfo");

            var o = useCache ? _memoryCache.Get(key) : null;
            if (o == null)
            {
                var sb = new SqlBuilder();

                sb.Append("select KCU1.CONSTRAINT_NAME as FK_CONSTRAINT_NAME,");
                sb.Append("KCU1.TABLE_NAME as FK_TABLE_NAME,");
                sb.Append("KCU1.COLUMN_NAME as FK_COLUMN_NAME,");
                sb.Append("KCU1.ORDINAL_POSITION as FK_ORDINAL_POSITION,");
                sb.Append("KCU2.CONSTRAINT_NAME as REFERENCED_CONSTRAINT_NAME,");
                sb.Append("KCU2.TABLE_NAME as REFERENCED_TABLE_NAME,");
                sb.Append("KCU2.COLUMN_NAME as REFERENCED_COLUMN_NAME,");
                sb.Append("KCU2.ORDINAL_POSITION AS REFERENCED_ORDINAL_POSITION");
                sb.Append("from INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS as RC");

                sb.Append("inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as KCU1");
                sb.Append("on KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG");
                sb.Append("and KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA");
                sb.Append("and KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME");

                sb.Append("inner join INFORMATION_SCHEMA.KEY_COLUMN_USAGE as KCU2");
                sb.Append("on KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG");
                sb.Append("and KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA");
                sb.Append("and KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME");
                sb.Append("and KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION");

                EnumerateResults(connectionString, sb.ToString(), timeoutSecs, r =>
                {
                    var rec = new ForeignKeyDetails
                    {
                        FkConstraintName = (string)r["FK_CONSTRAINT_NAME"],
                        FkTableName = (string)r["FK_TABLE_NAME"],
                        FkColumnName = (string)r["FK_COLUMN_NAME"],
                        FkPosition = (int)r["FK_ORDINAL_POSITION"],

                        ReferencedConstraintName = (string)r["REFERENCED_CONSTRAINT_NAME"],
                        ReferencedTableName = (string)r["REFERENCED_TABLE_NAME"],
                        ReferencedColumnName = (string)r["REFERENCED_COLUMN_NAME"],
                        ReferencedPosition = (int)r["REFERENCED_ORDINAL_POSITION"]
                    };

                    if (!result.TryGetValue(rec.FkTableName, out var details))
                    {
                        details = new List<ForeignKeyDetails>();
                        result.Add(rec.FkTableName, details);
                    }

                    details.Add(rec);
                });

                if (useCache)
                {
                    _memoryCache.Set(key, result, DateTimeOffset.UtcNow.AddMinutes(CacheLifetimeMinutes));
                }
            }
            else
            {
                _log.Debug("Using cached foreign key info");
                result = (Dictionary<string, List<ForeignKeyDetails>>)o;
            }

            return result;
        }

        public static void DropTablesInDatabase(string connectionString, int timeoutSecs)
        {
            _log.DebugFormat("Dropping tables in database: {0}", GetConnectionDescription(connectionString));

            if (DatabaseExists(connectionString, timeoutSecs))
            {
                DropAllForeignKeyConstraints(connectionString, timeoutSecs);

                var schemas = GetSchemasInDatabase(connectionString, timeoutSecs);
                foreach (var schema in schemas)
                {
                    OperateOnSchemaTables("DROP TABLE", connectionString, timeoutSecs, schema);
                }
            }
        }

        private static void DropAllForeignKeyConstraints(string connectionString, int timeoutSecs)
        {
            _log.DebugFormat("Dropping foreign key constraints in database: {0}", GetConnectionDescription(connectionString));

            var b = new SqlBuilder();

            b.Append("select FK.TABLE_SCHEMA, FK.TABLE_NAME, C.CONSTRAINT_NAME");
            b.Append("FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C");
            b.Append("INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK");
            b.Append("ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME");
            b.Append("INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK");
            b.Append("ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME");
            b.Append("INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU");
            b.Append("ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME");
            b.Append("INNER JOIN (");
            b.Append("SELECT i1.TABLE_NAME, i2.COLUMN_NAME");
            b.Append("FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1");
            b.Append("INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2");
            b.Append("ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME");
            b.Append("WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'");
            b.Append(") PT");
            b.Append("ON PT.TABLE_NAME = PK.TABLE_NAME;");

            EnumerateResults(connectionString, b.ToString(), timeoutSecs, r =>
            {
                var schemaName = (string)r["TABLE_SCHEMA"];
                var tableName = (string)r["TABLE_NAME"];
                var constraintName = (string)r["CONSTRAINT_NAME"];

                var sql = $"alter table {GetQualifiedTableName(schemaName, tableName)} drop constraint {constraintName}";

                ExecuteSql(connectionString, sql, timeoutSecs);
            });
        }

        private static string GetTableDescription(string connectionString, string tableName, string schema = StdSchemaName)
        {
            var result = GetConnectionDescription(connectionString, schema);
            return tableName != null ? string.Concat(result, ".", tableName) : result;
        }

        private static string BoolAsYesNo(bool val)
        {
            return val ? "Yes, exists" : "No, doesn't exist";
        }

        /// <summary>
        /// Generates a string that identifies the database specified in connectionString. Commonly used in MemoryCache
        /// </summary>
        /// <param name="connectionString">
        /// The connection string
        /// </param>
        /// <returns>
        /// The identifier
        /// </returns>
        private static string CreateDbNameKey(string connectionString)
        {
            var sb = new SqlConnectionStringBuilder(connectionString);
            return $"{sb.DataSource}.{sb.InitialCatalog}";
        }

        private static void AddSqlStatement(SqlBuilder sb, string sqlStatement)
        {
            if (!sb.Empty)
            {
                sb.Append(";");
            }

            sb.Append(sqlStatement);
        }

        private static string ExtractDatabaseName(string connectionString)
        {
            return new SqlConnectionStringBuilder(connectionString).InitialCatalog;
        }

        private static void OperateOnSchemaTables(
            string operation,
            string connectionString,
            int commandTimeoutSecs,
            string schemaName = StdSchemaName)
        {
            var tables = GetTablesInSchema(connectionString, commandTimeoutSecs, false, schemaName);

            if (tables.Any())
            {
                var sb = new SqlBuilder();

                foreach (var t in tables)
                {
                    AddSqlStatement(sb, string.Concat(operation, " ", GetQualifiedTableName(schemaName, t)));
                }

                ExecuteSql(connectionString, sb.ToString(), commandTimeoutSecs);
            }
        }

        private static string GenerateSchemaSql(string schemaName)
        {
            return $"CREATE SCHEMA {EscapeDbObject(schemaName)}";
        }

        private static string GenerateDatabaseCreateSql(string databaseName)
        {
            return $"CREATE DATABASE {EscapeDbObject(databaseName)}";
        }

        private static string GenerateDatabaseDropSql(string databaseName)
        {
            var sb = new SqlBuilder();
            sb.AppendFormat("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", EscapeDbObject(databaseName));
            sb.AppendFormat("DROP DATABASE {0}", EscapeDbObject(databaseName));
            return sb.ToString();
        }
    }
}
