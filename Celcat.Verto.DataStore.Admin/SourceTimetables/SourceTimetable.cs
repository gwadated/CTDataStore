namespace Celcat.Verto.DataStore.Admin.SourceTimetables
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Celcat.Verto.Common;
    using Celcat.Verto.DataStore.Admin.Staging;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common.Columns;
    using global::Common.Logging;

    internal class SourceTimetable
    {
        private readonly string _connectionString;
        private readonly string _connectionDescription;
        private readonly int _timeoutSecs;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public SourceTimetable(string connectionString, int timeoutSecs)
        {
            _connectionString = connectionString;
            _connectionDescription = DatabaseUtils.GetConnectionDescription(connectionString);
            _timeoutSecs = timeoutSecs;
        }

        public bool DatabaseExists()
        {
            return DatabaseUtils.DatabaseExists(_connectionString, _timeoutSecs);
        }

        public int GetSchemaVersion()
        {
            var result = DatabaseUtils.ExecuteScalar(_connectionString, "select version from CT_CONFIG", _timeoutSecs);

            var ver = 0;
            if (result != null && result != DBNull.Value)
            {
                ver = Convert.ToInt32(result);
            }

            return ver;
        }

        public IReadOnlyList<string> GetTables()
        {
            return DatabaseUtils.GetTablesInSchema(_connectionString, _timeoutSecs);
        }

        public IReadOnlyList<DatabaseColumnDefinition> GetSchemaForTable(string tableName)
        {
            return DatabaseUtils.GetSchemaForTable(_connectionString, tableName, _timeoutSecs);
        }

        // is the source timetable schema compatible with CTDS (i.e. with the staging area)?
        public SourceCompatibilityReport GetCompatibilityWithStagingSchema()
        {
            _log.DebugFormat("Generating compatibility report for source timetable: {0}", _connectionDescription);

            var result = new SourceCompatibilityReport(_connectionDescription);

            var latestSchema = StagingTablesBuilder.Get(StagingSchema.PrimaryStagingSchemaName);

            // first check for the existence of all tables...
            var tablesInStage = latestSchema.GetTableNames();
            var tablesInSource = GetTables();

            GetMissingTablesAndColumns(result, latestSchema, tablesInStage, tablesInSource);
            GetExtraTablesAndColumns(result, latestSchema, tablesInStage, tablesInSource);

            return result;
        }

        private void GetExtraTablesAndColumns(
            SourceCompatibilityReport report, 
            StagingTablesBuilder latestSchema,
            IReadOnlyList<string> tablesInStage, 
            IReadOnlyList<string> tablesInSource)
        {
            foreach (var srcTable in tablesInSource)
            {
                if (!tablesInStage.Contains(srcTable, StringComparer.OrdinalIgnoreCase))
                {
                    _log.DebugFormat("Extra table {0}", srcTable);
                    report.AddExtraTable(srcTable);
                }
                else
                {
                    // any extra columns in source?
                    var srcSchema = GetSchemaForTable(srcTable);
                    var columnsInSource = latestSchema.GetColumns(srcTable);

                    foreach (var col in columnsInSource)
                    {
                        var srcCol = srcSchema.FirstOrDefault(x => x.Name.Equals(col.Name, StringComparison.OrdinalIgnoreCase));
                        if (srcCol == null)
                        {
                            _log.DebugFormat("Extra column {0}.{1}", srcTable, col.Name);
                            report.AddExtraColumn(srcTable, col.Name);
                        }
                    }
                }
            }
        }

        private void GetMissingTablesAndColumns(
            SourceCompatibilityReport report, 
            StagingTablesBuilder latestSchema,
            IReadOnlyList<string> tablesInStage, 
            IReadOnlyList<string> tablesInSource)
        {
            foreach (var stageTable in tablesInStage)
            {
                if (!stageTable.Equals(StagingTablesBuilder.PseudoRegisterMarkTable, StringComparison.OrdinalIgnoreCase)) 
                {
                    // this table is not actually a member of the CT7 schema
                    if (!tablesInSource.Contains(stageTable, StringComparer.OrdinalIgnoreCase))
                    {
                        _log.ErrorFormat("Missing table {0}", stageTable);
                        report.AddMissingTable(stageTable);
                    }
                    else
                    {
                        // now check that the source columns exist...
                        var srcSchema = GetSchemaForTable(stageTable);
                        var columnsInStage = latestSchema.GetColumns(stageTable);

                        foreach (var col in columnsInStage)
                        {
                            if (!col.Name.Equals(ColumnConstants.SrcTimetableIdColumnName, StringComparison.OrdinalIgnoreCase) &&
                               !col.Name.Equals(ColumnConstants.RegistersReqResolvedColumnName, StringComparison.OrdinalIgnoreCase))
                            {
                                var srcCol =
                                   srcSchema.FirstOrDefault(x => x.Name.Equals(col.Name, StringComparison.OrdinalIgnoreCase));
                                if (srcCol == null)
                                {
                                    _log.ErrorFormat("Missing column {0}.{1}", stageTable, col.Name);
                                    report.AddMissingColumn(stageTable, col.Name);
                                }
                                else if (!DbTypeMatching.
                                   MatchingDataTypes(srcCol.DataType, srcCol.CharacterMaxLength, col.SqlDbType, col.Length))
                                {
                                    _log.ErrorFormat("Incompatible data type for column {0}.{1}", stageTable, col.Name);
                                    report.BadDataType(stageTable, col.Name);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
