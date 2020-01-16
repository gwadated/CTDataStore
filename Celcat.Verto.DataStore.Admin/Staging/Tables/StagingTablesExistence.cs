namespace Celcat.Verto.DataStore.Admin.Staging.Tables
{
    using System.Linq;
    using System.Reflection;
    using Celcat.Verto.Common;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;

    internal static class StagingTablesExistence
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void EnsureExist(string connectionString, int timeoutSecs)
        {
            EnsureAllStagingTablesExist(connectionString, timeoutSecs, StagingSchema.PrimaryStagingSchemaName);
            EnsureAllStagingTablesExist(connectionString, timeoutSecs, StagingSchema.SecondaryStagingSchemaName);
        }

        private static void EnsureAllStagingTablesExist(string connectionString, int timeoutSecs, string schemaName)
        {
            _log.DebugFormat("Checking existence of staging tables in schema: {0}", schemaName);

            if (!DatabaseUtils.SchemaExists(connectionString, timeoutSecs, schemaName))
            {
                DatabaseUtils.CreateSchema(connectionString, timeoutSecs, schemaName);
            }

            var b = StagingTablesBuilder.Get(schemaName);
            var tables = b.GetTables();

            var tablesInPrimaryStage = DatabaseUtils.GetTablesInSchema(
                connectionString, timeoutSecs, false, StagingSchema.PrimaryStagingSchemaName);

            var builder = new Builder();

            foreach (var t in tables)
            {
                if (!tablesInPrimaryStage.Contains(t.Name))
                {
                    builder.AddTable(t);
                }
            }

            if (builder.HasTables)
            {
                _log.WarnFormat("Recreating missing staging tables in schema: {0}", schemaName);
                builder.Execute(connectionString, timeoutSecs);
            }
        }
    }
}
