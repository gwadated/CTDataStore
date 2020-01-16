namespace Celcat.Verto.DataStore.Admin.History
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Celcat.Verto.Common;
    using Celcat.Verto.Common.TableDiff;
    using Celcat.Verto.DataStore.Admin.History.RhinoOperations;
    using Celcat.Verto.DataStore.Admin.Staging;
    using Celcat.Verto.DataStore.Admin.Staging.Tables;
    using Celcat.Verto.DataStore.Common.Configuration.PipelineElements;
    using Celcat.Verto.DataStore.Common.Schemas;
    using Celcat.Verto.TableBuilder;
    using global::Common.Logging;
    using Rhino.Etl.Core;

    public class HistorySchema : SchemaBase
    {
        public const string HistorySchemaName = "HISTORY";
        public const string HistoryStatusColumnName = "history_status";
        public const string HistoryStampColumnName = "history_stamp";
        public const string HistoryLogColumnName = "history_control_log_id";
        public const string HistoryFederatedColumnName = "history_federated";
        public const string HistoryAppliedColumnName = "history_applied";
        public const string HistoryStatusInsert = "I";
        public const string HistoryStatusUpdate = "U";
        public const string HistoryStatusDelete = "D";

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal HistorySchema(string connectionString, int timeoutSecs, int maxDegreeOfParallelism, Pipelines pipelineOptions)
           : base(connectionString, timeoutSecs, maxDegreeOfParallelism, pipelineOptions)
        {
        }

        public static string HistoryStatusToString(RowStatus rs)
        {
            switch (rs)
            {
                case RowStatus.Deleted:
                    return HistoryStatusDelete;
                case RowStatus.Inserted:
                    return HistoryStatusInsert;
                case RowStatus.Updated:
                    return HistoryStatusUpdate;

                default:
                    throw new ApplicationException("Unknown row status");
            }
        }

        /// <summary>
        /// Creates empty history tables (or truncates existing ones). Asssumes the database exists
        /// </summary>
        public void CreateTables()
        {
            _log.DebugFormat("Creating history tables in schema: {0}", HistorySchemaName);

            DropTablesInSchema();
            InternalCreateEmptyHistoryTables();
        }

        private void InternalCreateEmptyHistoryTables()
        {
            EnsureSchemaCreated();
            var builder = HistoryTablesBuilder.Get();
            builder.Execute(ConnectionString, TimeoutSecs);
        }

        public TimeSpan PerformDiff(Dictionary<string, PrimaryKeyInfo> pkInfo, long logId)
        {
            var stopwatch = Stopwatch.StartNew();

            _log.DebugFormat("Performing full diff on stage: {0}", DatabaseUtils.GetConnectionDescription(ConnectionString));

            var b = HistoryTablesBuilder.Get();
            DoParallelProcessing(b, pkInfo, logId);

            return stopwatch.Elapsed;
        }

        private void DoParallelProcessing(HistoryTablesBuilder b, Dictionary<string, PrimaryKeyInfo> pkInfo, long logId)
        {
            var tables = b.GetTables();

            var sb = StagingTablesBuilder.Get(StagingSchema.PrimaryStagingSchemaName);

            var pOptions = new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism };
            Parallel.ForEach(tables, pOptions, (historyTable, loopState) =>
            {
                if (!loopState.IsExceptional)
                {
                    string tableName = historyTable.Name;

                    string tableNew = DatabaseUtils.GetQualifiedTableName(StagingSchema.PrimaryStagingSchemaName, tableName);
                    string tableOld = DatabaseUtils.GetQualifiedTableName(StagingSchema.SecondaryStagingSchemaName, tableName);

                    _log.DebugFormat("Performing diff on tables {0} and {1}", tableNew, tableOld);

                    var statsNew = SourceTimetableAndRowCount.Get(
                        ConnectionString, TimeoutSecs, tableName, StagingSchema.PrimaryStagingSchemaName);

                    var statsOld = SourceTimetableAndRowCount.Get(
                        ConnectionString, TimeoutSecs, tableName, StagingSchema.SecondaryStagingSchemaName);

                    HashSet<int> srcTimetableIds = UnionSrcTimetableIds(statsNew, statsOld);

                    // we could perform the diff on the whole table but to keep memory footprint lower
                    // for large tables we diff by timetable id...
                    foreach (var timetableId in srcTimetableIds)
                    {
                        _log.DebugFormat("Performing diff for timetable Id {0}", timetableId);

                        var stagingTableColumnNames = sb.GetColumnNames(tableName);
                        EtlProcess etl;

                        if (AllNewlyInserted(timetableId, statsNew, statsOld))
                        { 
                            // optimisation here (no need to diff, just insert all into history)...
                            _log.DebugFormat("All rows are new in table {0} for timetable {1}", tableNew, timetableId);
                            
                            etl = new HistoryEtlProcessBasic(
                                ConnectionString, 
                                TimeoutSecs, 
                                historyTable, 
                                timetableId, 
                                StagingSchema.PrimaryStagingSchemaName,
                                HistoryStatusInsert, 
                                logId, 
                                PipelineOptions);
                        }
                        else if (AllNewlyDeleted(timetableId, statsNew, statsOld))
                        {
                            // optimisation here (no need to diff)...
                            _log.DebugFormat("All rows have been deleted from table {0} for timetable {1}", tableNew, timetableId);

                            etl = new HistoryEtlProcessBasic(
                                ConnectionString, 
                                TimeoutSecs, 
                                historyTable, 
                                timetableId,
                                StagingSchema.SecondaryStagingSchemaName,
                                HistoryStatusDelete, 
                                logId, 
                                PipelineOptions);
                        }
                        else
                        {
                            var pkInfoForTable = pkInfo[tableName];
                            if (pkInfoForTable == null)
                            {
                                throw new ApplicationException(
                                    $"Could not find primary key info for table: {tableName}");
                            }

                            // identityColumnCounts originate from the source timetable tables
                            // so we increment by 1 to account for the src_timetable_id column...
                            var identityColCount = pkInfoForTable.Columns.Count + 1;

                            var td = new StageTableDiffer(
                                ConnectionString, TimeoutSecs, tableOld, tableNew, timetableId, identityColCount);

                            var diff = td.Execute();

                            etl = new HistoryEtlProcessDiff(
                                ConnectionString, TimeoutSecs, historyTable, diff, stagingTableColumnNames, logId, PipelineOptions);
                        }

                        etl.Execute();

                        var errors = etl.GetAllErrors().ToArray();
                        if (errors.Any())
                        {
                            loopState.Stop();

                            string msg = "Errors occurred during execution of history process";
                            _log.Error(msg);

                            // throw the first exception
                            throw new ApplicationException(msg, errors[0]);
                        }
                    }
                }
            });
        }

        private bool AllNewlyDeleted(
            int timetableId,
            IEnumerable<SourceTimetableAndRowCount> statsNew,
            IEnumerable<SourceTimetableAndRowCount> statsOld)
        {
            return
               statsNew.FirstOrDefault(x => x.SrcTimetableId == timetableId) == null &&
               statsOld.FirstOrDefault(x => x.SrcTimetableId == timetableId) != null;
        }

        private bool AllNewlyInserted(
            int timetableId,
            IEnumerable<SourceTimetableAndRowCount> statsNew,
            IEnumerable<SourceTimetableAndRowCount> statsOld)
        {
            return
               statsNew.FirstOrDefault(x => x.SrcTimetableId == timetableId) != null &&
               statsOld.FirstOrDefault(x => x.SrcTimetableId == timetableId) == null;
        }

        private HashSet<int> UnionSrcTimetableIds(
            IEnumerable<SourceTimetableAndRowCount> stats1,
            IEnumerable<SourceTimetableAndRowCount> stats2)
        {
            var result = new HashSet<int>();

            foreach (var s in stats1)
            {
                result.Add(s.SrcTimetableId);
            }

            foreach (var s in stats2)
            {
                result.Add(s.SrcTimetableId);
            }

            return result;
        }

        protected override string SchemaName => HistorySchemaName;
    }
}
