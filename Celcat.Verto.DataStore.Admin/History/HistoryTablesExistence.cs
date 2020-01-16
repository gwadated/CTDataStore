namespace Celcat.Verto.DataStore.Admin.History
{
    using System.Linq;
    using System.Reflection;
    using Celcat.Verto.Common;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;

    internal static class HistoryTablesExistence
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void EnsureExist(string connectionString, int timeoutSecs)
        {
            _log.DebugFormat("Checking existence of history tables in schema: {0}", HistorySchema.HistorySchemaName);

            if (!DatabaseUtils.SchemaExists(connectionString, timeoutSecs, HistorySchema.HistorySchemaName))
            {
                DatabaseUtils.CreateSchema(connectionString, timeoutSecs, HistorySchema.HistorySchemaName);
            }

            var b = HistoryTablesBuilder.Get();
            var tables = b.GetTables();

            var tablesInHistorySchema = DatabaseUtils.GetTablesInSchema(
                connectionString, timeoutSecs, false, HistorySchema.HistorySchemaName);

            var builder = new Builder();

            foreach (var t in tables)
            {
                if (!tablesInHistorySchema.Contains(t.Name))
                {
                    builder.AddTable(t);
                }
            }

            if (builder.HasTables)
            {
                _log.WarnFormat("Recreating missing history tables in schema: {0}", HistorySchema.HistorySchemaName);
                builder.Execute(connectionString, timeoutSecs);
            }
        }
    }
}
